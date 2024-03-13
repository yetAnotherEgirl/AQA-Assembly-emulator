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
    internal class TraceTable : UserControl
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

        public TraceTable(CPU CPU, ref Memory ram, int TableDepth = 50)
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

            /*x            Evil 
             *            for (int i = 0; i < TraceTableEntries.GetLength(0); i++)
             *            {
             *                RichTextBox headerTextbox = new RichTextBox();
             *                headerTextbox.ReadOnly = true;
             *                headerTextbox.Size = EntrySize;
             *                headerTextbox.Location = new Point(i * EntrySize.Width, 0);
             *                headerTextbox.BackColor = System.Drawing.SystemColors.Control;
             *                headerTextbox.BorderStyle = BorderStyle.FixedSingle;
             *
             *                TraceTableEntries[i, 0] = headerTextbox;
             *                this.Controls.Add(headerTextbox);
             *            }   
             *
             *            for (int y = 1; y < TraceTableEntries.GetLength(1); y++)
             *            {
             *                for (int x = 0; x < TraceTableEntries.GetLength(0); x++)
             *                {
             *                    RichTextBox entryTextbox = new RichTextBox();
             *                    entryTextbox.ReadOnly = true;
             *                    entryTextbox.Size = EntrySize;
             *                    entryTextbox.Location = new Point(x * EntrySize.Width, y * EntrySize.Height);
             *                    entryTextbox.BorderStyle = BorderStyle.FixedSingle;
             *
             *                    TraceTableEntries[x, y] = entryTextbox;
             *                    this.Controls.Add(entryTextbox);
             *                }
             *            }
             *            int sizeX = (TraceTableEntries.GetLength(0) * EntrySize.Width) + 2;
             *            int sizeY = (TraceTableEntries.GetLength(1) * EntrySize.Height) + 2;
             *            this.Size = new Size(sizeX, sizeY);
             */

            this.ResumeLayout(false);
        }

        public void UpdateTable(List<string> Inputvariables)
        {
            this.Show();
            List<string> variables;
            if (TestingMode)
            {
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

            
            if(!variables.SequenceEqual(trackedVariables))
            {
                trackedVariables = variables;
                InitializeTableData(variables);
            }

            if (currentRow == TraceTableData.GetLength(1) - 1)
            {
                Scrolldown();
            }
            
            if (currentRow > TraceTableData.GetLength(1) + 1)
            {
                return;
            }
            currentRow++;

            List<string> values = new List<string>();
            if (!TestingMode) values.AddRange(GetSpecificRegisters());
            values.AddRange(GetUniversalRegisters());
            values.AddRange(GetMemory());

            for (int i = 0; i < values.Count(); i++)
            {
                
                try
                {
                    TraceTableData[i, currentRow] = values[i];
                    if (!TestingMode) TraceTableEntries[i, currentRow].Text = TraceTableData[i, currentRow];
                }
                catch (Exception)
                {

                    throw;
                }
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

            // remove the "- 34" if less than 25 entries
            int scrollBarWidth = 34;
            if(TraceTableEntries.GetLength(0) < 25) scrollBarWidth = 1;
            int EntrySizeX = (TableSize.Width - scrollBarWidth) / TraceTableEntries.GetLength(0);
            int EntrySizeY = 40;

            Size EntrySize = new Size(EntrySizeX, EntrySizeY);

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

            //x int width = EntrySizeX * TraceTableEntries.GetLength(0) + 2;
            //x this.Size = new Size(width, TableSize.Height);

            this.ResumeLayout(false);
        }
    
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
           /*
            //if (TraceTableEntries == null)
            //{
            //    return;
            //}
            //TextBox[] headers = new TextBox[TraceTableEntries.GetLength(0)];
            //for (int i = 0; i < TraceTableEntries.GetLength(0); i++)
            //{
            //    headers[i] = TraceTableEntries[i, 0];
            //}


            //currentRow = 0;
            //if (TraceTableEntries == null)
            //{
            //    return;
            //}


            //for (int y = 0; y < TraceTableEntries.GetLength(1); y++)
            //{
            //    for (int x = 0; x < TraceTableEntries.GetLength(0); x++)
            //    {
            //        TraceTableData[x, y] = "";
            //        TraceTableEntries[x, y].BackColor = System.Drawing.SystemColors.Window;
            //        TraceTableEntries[x, y].Text = "";
            //    }
            //}

            //for (int i = 0; i < TraceTableData.GetLength(0); i++)
            //{
            //    TraceTableEntries[i, 0] = headers[i];
            //}
            
            //foreach (Control c in Controls)
            //{
            //    Controls.Remove(c);
            //}
            */

            trackedVariables.Clear();
            currentRow = 0;
            if (TraceTableEntries == null)
            {
                return;
            }

            SuspendLayout();
            Controls.Clear();
            TraceTableEntries = new TextBox[1,1];
            TraceTableData = new string[1,1];
            ResumeLayout(false);
            InitializeComponent();

            //UpdateTable(trackedVariables);
        }

        void Scrolldown()
        {

            //string[] headers = new string[TraceTableEntries.GetLength(0)];

            //for (int i = 0; i < TraceTableEntries.GetLength(0); i++)
            //{
            //    headers[i] = TraceTableEntries[i, 0].Text;
            //}
            //Clear();
            //UpdateTable(trackedVariables);
            //for (int i = 0; i < TraceTableEntries.GetLength(0); i++)
            //{
            //    TraceTableEntries[i, 0].Text = headers[i];
            //}

            TextBox[,] oldTraceTableEntries = TraceTableEntries;    
            string[,] oldTraceTableData = TraceTableData;

            TraceTableEntries = new TextBox[oldTraceTableEntries.GetLength(0),
                oldTraceTableEntries.GetLength(1) + TableDepthStep];
            TraceTableData = new string[oldTraceTableData.GetLength(0) , oldTraceTableData.GetLength(1) + TableDepthStep];

            int EntrySizeX = (TableSize.Width - 34) / TraceTableEntries.GetLength(0);
            int EntrySizeY = 40;

            Size EntrySize = new(EntrySizeX, EntrySizeY);

            SuspendLayout();
            foreach (TextBox t in Controls)
            {
                Controls.Remove(t);
            }

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

        void TraceTableEntry_OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TraceTableEntry_OnLeave(sender, e);
            }
        }

        void TraceTableEntry_OnLeave(object? sender, EventArgs e)
        {
            TextBox? textBox = (TextBox?)sender ?? throw new ArgumentNullException(nameof(sender));
            for(int y = 0; y < TraceTableEntries.GetLength(1); y++)
            {
                for (int x = 0; x < TraceTableEntries.GetLength(0); x++)
                {
                    if (TraceTableEntries[x, y] != textBox)
                    {
                        continue;
                    }
                    if (TraceTableData[x, y] == "")
                    {
                        TraceTableEntries[x, y].BackColor = System.Drawing.SystemColors.Window;
                        TraceTableEntries[x, y].Text = "";
                        return;
                    }
                    if (textBox.Text == TraceTableData[x, y])
                    {
                        TraceTableEntries[x, y].BackColor = Color.FromArgb(171, 233, 179);
                        return;
                    }
                    else
                    {
                        TraceTableEntries[x, y].BackColor = Color.FromArgb(242, 143, 173);
                        return;
                    }
                }
            }
        }

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

        public int GetDepth()
        {
            return TableDepthStep;
        }
    }


    
}
