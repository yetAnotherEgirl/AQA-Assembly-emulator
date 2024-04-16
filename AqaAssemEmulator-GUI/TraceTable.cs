using AqaAssemEmulator_GUI.backend;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AqaAssemEmulator_GUI
{
    internal class D : UserControl
    {
        private CPU CPU;
        private Memory RAM;
        private List<string> trackedVariables = [];

        Size TableSize = new Size(1134 - 6, 1016 - 6);

        int TableDepthStep;
        int currentRow;

        public bool TestingMode = false;


        TextBox[,] TraceTableEntries;
        string[,] TraceTableData;

        public D(CPU CPU, ref Memory ram, int TableDepth = 50)
        {
            RAM = ram;
            this.TableDepthStep = TableDepth;
            this.CPU = CPU;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Size = TableSize;
            this.Location = new Point(3, 3);
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Hide();
            this.AutoScroll = true;

            //originally the trace table entries were created here, but this caused the problems
            //when the size of the table was changed, so this was moved to UpdateTable(), and later
            //to InitializeTableData(). leading to better maintainability and readability

            this.ResumeLayout(false);
        }

        public void UpdateTable(List<string> Inputvariables)
        {
            this.Show();
            List<string> variables;
            if (TestingMode)
            {
                //when the input variables are passed in, the first 5 variables are always the specific registers,
                //so we can skip them and only take the universal registers and memory addresses
                int varIndex = 5;
                int varCount = Inputvariables.Count - varIndex;
                variables = Inputvariables.GetRange(varIndex, varCount);
            }
            else
            {
                variables = Inputvariables;
            }
            if (variables.Count == 0)
            {
                this.Hide();
                return;
            }

            //if the tracked variables have changed, re-initialise the table
            if(!variables.SequenceEqual(trackedVariables))
            {
                trackedVariables = variables;
                InitializeTableData(variables);
            }


            //if the table is full, scroll down, "- 1" is to account for the header row
            if (currentRow == TraceTableData.GetLength(1) - 1)
            {
                Scrolldown();
            }
            
            //this should never occur, scrolldown should be called before this, however this is a safety check
            //so the program doesnt crash on the user if an error is encountered
            if (currentRow > TraceTableData.GetLength(1) + 1)
            {
                return;
            }
            currentRow++;


            //get the values for the specific registers, universal registers, and memory addresses
            List<string> values = new List<string>();
            if (!TestingMode) values.AddRange(GetSpecificRegisters());
            values.AddRange(GetUniversalRegisters());
            values.AddRange(GetMemory());


            //add the values to the table
            for (int i = 0; i < values.Count; i++)
            {
                TraceTableData[i, currentRow] = values[i];
                if (!TestingMode) TraceTableEntries[i, currentRow].Text = TraceTableData[i, currentRow];
            }

            
        }

        private List<string> GetSpecificRegisters()
        {
            /* due to the way the Assembler generates the list of variables, the first 5 
             * variables will always be "PC", "MAR", "MDR", "ACC", and "CPSR" therefore we
             * can assume that the first data in the row will always be in this order
             * (and therefore we dont need to consider which register is 
             * which or what order to add the variables)
             */
            string PC = CPU.GetProgramCounter().ToString();
            string MAR = CPU.GetMemoryAddressRegister().ToString();
            string MDR = CPU.GetMemoryDataRegister().ToString();
            string ACC = CPU.GetACC().ToString();
            string CPSR = CPU.GetCPSR().ToString();

            List<string> SpecificRegisters = [PC, MAR, MDR, ACC, CPSR];

            return SpecificRegisters;
        }

        private List<string> GetUniversalRegisters()
        {
            List<int> wantedRegisters = trackedVariables                //get all the registers
                .Where(x => x.StartsWith(Constants.registerChar) &&     //see if string x starts with the register char and...
                int.TryParse(x.Substring(1), out _))                    //...see if the rest of the string is a number, out _ is a discard
                .Select(x => int.Parse(x.Substring(1)))                 //after filtering, parse the string to an int
                .ToList();

            List<string> UniversalRegisters = new List<string>();

            foreach (int register in wantedRegisters)
            {
                UniversalRegisters.Add(CPU.GetRegister(register).ToString());
            }

            return UniversalRegisters;
        }

        private List<string> GetMemory()
        {
            List<int> wantedMemory = trackedVariables   //get all the memory addresses
                .Where(x => int.TryParse(x, out _))     //see if string x is a number, out _ is a discard
                .Select(x => int.Parse(x))              //after filtering, parse the string to an int
                .ToList();

            List<string> Memory = new List<string>();

            foreach (int address in wantedMemory)
            {
                Memory.Add(RAM.QuereyAddress((long)address).ToString());
            }
            return Memory;
        }

        public void InitializeTableData(List<string> variables)
        {
            currentRow = 0;

            this.SuspendLayout();
            this.Controls.Clear();
            TraceTableEntries = new TextBox[trackedVariables.Count, TableDepthStep];
            TraceTableData = new string[trackedVariables.Count, TableDepthStep];

            // remove the "- 34" if less than 25 entries, as the scrollbar will not be shown
            int scrollBarWidth = 34;
            if(TraceTableEntries.GetLength(0) < 25) scrollBarWidth = 1;
            int EntrySizeX = (TableSize.Width - scrollBarWidth) / TraceTableEntries.GetLength(0);
            int EntrySizeY = 40;

            Size EntrySize = new Size(EntrySizeX, EntrySizeY);


            //create the header row
            for (int i = 0; i < TraceTableEntries.GetLength(0); i++)
            {
                TextBox headerTextbox = new TextBox();
                headerTextbox.ReadOnly = true;
                headerTextbox.Size = EntrySize;
                headerTextbox.Location = new Point(i * EntrySize.Width, 0);
                headerTextbox.BackColor = System.Drawing.SystemColors.Control;
                headerTextbox.BorderStyle = BorderStyle.FixedSingle;
                headerTextbox.Text = variables[i];

                TraceTableEntries[i, 0] = headerTextbox;
                this.Controls.Add(headerTextbox);
            }

            //create the rest of the table
            for (int y = 1; y < TraceTableEntries.GetLength(1); y++)
            {
                for (int x = 0; x < TraceTableEntries.GetLength(0); x++)
                {
                    TextBox entryTextbox = new()
                    {
                        ReadOnly = !TestingMode,
                        Size = EntrySize,
                        Location = new Point(x * EntrySize.Width, y * EntrySize.Height),
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = System.Drawing.SystemColors.Window
                    };
                    TraceTableEntries[x, y] = entryTextbox;
                    if (TestingMode) TraceTableEntries[x, y].KeyDown += TraceTableEntry_OnKeyDown;
                    if (TestingMode) TraceTableEntries[x, y].Leave += TraceTableEntry_OnLeave;
                    this.Controls.Add(TraceTableEntries[x, y]);

                    TraceTableData[x, y] = "";
                }
            }

            this.ResumeLayout(false);
        }
    
        //trace table depth can be changed by the user, this function updates the depth
        public void UpdateDepth(int depth)
        {
            TableDepthStep = depth;
            if (TraceTableEntries == null)
            {
                return;
            }
            InitializeTableData(trackedVariables);
        }

        public void Clear()
        {

            trackedVariables.Clear();
            currentRow = 0;

            //retturn early if the table has not been initialised,
            //why waste computational power on something that has not been used
            if (TraceTableEntries == null)
            {
                ResumeLayout(false);
                return;
            }

            SuspendLayout();
            Controls.Clear();
            TraceTableEntries = new TextBox[1,1];
            TraceTableData = new string[1,1];
            ResumeLayout(false);
            InitializeComponent();
        }

        void Scrolldown()
        {
            //copy the old data to a new array, this way we can modify the arrays without losing data
            //this may be memory inefficient, but it is the easiest way to do this
            TextBox[,] oldTraceTableEntries = TraceTableEntries;    
            string[,] oldTraceTableData = TraceTableData;

            //increase the size of the arrays
            TraceTableEntries = new TextBox[oldTraceTableEntries.GetLength(0),
                oldTraceTableEntries.GetLength(1) + TableDepthStep];
            TraceTableData = new string[oldTraceTableData.GetLength(0) , oldTraceTableData.GetLength(1) + TableDepthStep];

            int EntrySizeX = (TableSize.Width - 34) / TraceTableEntries.GetLength(0);
            int EntrySizeY = 40;

            Size EntrySize = new(EntrySizeX, EntrySizeY);

            SuspendLayout();

            //remove the old entries from the form
            foreach (TextBox t in Controls)
            {
                Controls.Remove(t);
            }

            //the next 2 for loops do the same as InitializeTableData(), but also copy the old data into the new arrays
            for (int y = 0; y < oldTraceTableEntries.GetLength(1); y++)
            {
                for (int x = 0; x < oldTraceTableEntries.GetLength(0); x++)
                {
                    TraceTableEntries[x, y] = oldTraceTableEntries[x, y];
                    TraceTableData[x, y] = oldTraceTableData[x, y];
                    Controls.Add(TraceTableEntries[x, y]);
                }
            }

            for (int y = oldTraceTableEntries.GetLength(1); y < TraceTableEntries.GetLength(1); y++)
            {
                for (int x = 0; x < TraceTableEntries.GetLength(0); x++)
                {
                    TextBox entryTextbox = new()
                    {
                        ReadOnly = !TestingMode,
                        Size = EntrySize,
                        Location = new Point(x * EntrySize.Width, y * EntrySize.Height),
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = System.Drawing.SystemColors.Window
                    };
                    TraceTableEntries[x, y] = entryTextbox;

                    this.Controls.Add(TraceTableEntries[x, y]);
                    if (TestingMode) TraceTableEntries[x, y].KeyDown += TraceTableEntry_OnKeyDown;
                    if (TestingMode) TraceTableEntries[x, y].Leave += TraceTableEntry_OnLeave;
                    TraceTableData[x, y] = "";
                }
            }

            ResumeLayout(false);
        }


        //the user will expect pressing enter to cause the trace table entry to be checked,
        //TraceTableEntry_OnLeave() is the function that does this, so we call it here
        void TraceTableEntry_OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TraceTableEntry_OnLeave(sender, e);
            }
        }

        void TraceTableEntry_OnLeave(object? sender, EventArgs e)
        {
            //this code is not easy to read, but it is the most efficient way to do this,
            //a new possibly null textbox is created, and then the sender is cast to a textbox,
            //if the sender is null, an exception is thrown, if not, the textbox is assigned to the new textbox
            TextBox? textBox = (TextBox?)sender ?? throw new ArgumentNullException(nameof(sender));


            //these 2 for loops iterate through the entire TraceTableEntries array
            for(int y = 0; y < TraceTableEntries.GetLength(1); y++)
            {
                for (int x = 0; x < TraceTableEntries.GetLength(0); x++)
                {
                    //if the textbox is not the one we are looking for, skip to the next iteration
                    if (TraceTableEntries[x, y] != textBox)
                    {
                        continue;
                    }

                    //if the textbox is empty, set the background to the default colour and return,
                    //users may find it annoying if they accidentally press enter on an empty textbox and
                    //the background changes colour
                    if (TraceTableData[x, y] == "")
                    {
                        TraceTableEntries[x, y].BackColor = System.Drawing.SystemColors.Window;
                        TraceTableEntries[x, y].Text = "";
                        return;
                    }
                    //if the textbox text is the same as the data in the table, set the background to a nice green colour
                    if (textBox.Text == TraceTableData[x, y])
                    {
                        TraceTableEntries[x, y].BackColor = Color.FromArgb(171, 233, 179);
                        return;
                    }
                    //if the textbox text is not the same as the data in the table, set the background to a nice red colour
                    else
                    {
                        TraceTableEntries[x, y].BackColor = Color.FromArgb(242, 143, 173);
                        return;
                    }
                }
            }
        }

        //this function removes the extra lines from the trace table, this is used in testing mode,
        //this function works by removing all empty textboxes from the form controls
        public void RemoveExtraLines()
        {
            if (!TestingMode) return;
            for(int y = 0; y < TraceTableEntries.GetLength(1); y++)
            {
                for(int x = 0; x < TraceTableEntries.GetLength(0); x++)
                {
                    if (TraceTableData[x, y] == "") Controls.Remove(TraceTableEntries[x, y]);
                }
            }


        }


        //this function returns the depth of the trace table
        public int GetDepth()
        {
            return TableDepthStep;
        }
    }


    
}
