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
        private List<string> trackedVariables = new List<string>();

        Size TableSize = new Size(1134 - 6, 1016 - 6);

        int TableDepth;
        int currentRow;

        RichTextBox[,] TraceTableEntries;

        public TraceTable(CPU CPU,  int TableDepth = 30)
        {
            this.TableDepth = TableDepth;
            InitializeComponent(CPU);
        }

        private void InitializeComponent(CPU CPU)
        {
            this.SuspendLayout();
            this.CPU = CPU;
            this.Size = TableSize;
            this.Location = new Point(3, 3);
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Hide();
            this.AutoScroll = true;

/*            Evil fuckery 
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
        


        public void UpdateTable(List<string> variables)
        {
            if (variables.Count == 0)
            {
                return;
            }
            if(trackedVariables != variables)
            {
                InitializeTableData(variables);
            }
            if (currentRow > TraceTableEntries.GetLength(1) + 1)
            {
                return;
            }
            currentRow++;

            List<string> values = new List<string>();
            values.AddRange(GetSpecificRegisters());
            values.AddRange(GetUniversalRegisters());
            values.AddRange(GetMemory());

            if (values.Count != TraceTableEntries.GetLength(0)) 
                throw new System.Exception("something broke in the trace table");

            for (int i = 0; i < values.Count(); i++)
            {
                TraceTableEntries[i, currentRow].Text = values[i];
            }
        }

        private List<string> GetSpecificRegisters()
        {
            /* due to the way the Assembler generates the list of variables, the first 5 
             * variables will always be "PC", "MAR", "MDR", "ALU", and "CPSR" therefore we
             * can assume that the first data in the row will always be in this order
             * (and therefore we dont need to consider which register is 
             * which or what order to add the variables)
             */
            string PC = CPU.GetProgramCounter().ToString();
            string MAR = CPU.GetMemoryAddressRegister().ToString();
            string MDR = CPU.GetMemoryDataRegister().ToString();
            string ALU = CPU.GetALU().ToString();
            string CPSR = CPU.GetCPSR().ToString();

            List<string> SpecificRegisters = [PC, MAR, MDR, ALU, CPSR];

            return SpecificRegisters;
        }

        private List<string> GetUniversalRegisters()
        {
            List<int> wantedRegisters = trackedVariables    //get all the registers
                .Where(x => x.StartsWith("r") &&            //see if string x starts with "r" and...
                int.TryParse(x.Substring(1), out _))        //...see if the rest of the string is a number, out _ is a discard
                .Select(x => int.Parse(x.Substring(1)))     //after filtering, parse the string to an int
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

        private void InitializeTableData(List<string> variables)
        {
            currentRow = 0;

            this.SuspendLayout();
            this.Controls.Clear();
            trackedVariables = variables;
            TraceTableEntries = new RichTextBox[variables.Count, TableDepth];

            int EntrySizeX = (TableSize.Width - 31) / TraceTableEntries.GetLength(0);
            int EntrySizeY = 40;

            Size EntrySize = new Size(EntrySizeX, EntrySizeY);

            for (int i = 0; i < TraceTableEntries.GetLength(0); i++)
            {
                RichTextBox headerTextbox = new RichTextBox();
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
                    RichTextBox entryTextbox = new RichTextBox();
                    entryTextbox.ReadOnly = true;
                    entryTextbox.Size = EntrySize;
                    entryTextbox.Location = new Point(x * EntrySize.Width, y * EntrySize.Height);
                    entryTextbox.BorderStyle = BorderStyle.FixedSingle;
                    entryTextbox.BackColor = System.Drawing.SystemColors.Window;
                    TraceTableEntries[x, y] = entryTextbox;
                    this.Controls.Add(entryTextbox);
                }
            }

            //int width = EntrySizeX * TraceTableEntries.GetLength(0) + 2;
            //this.Size = new Size(width, TableSize.Height);

            this.ResumeLayout(false);
        }
    
        public void UpdateDepth(int depth)
        {
            TableDepth = depth;
        }

        public void Clear()
        {
            currentRow = 0;
            foreach (RichTextBox dataBox in TraceTableEntries)
            {
                if(dataBox.Text == "")
                {
                    break;
                }

                dataBox.Text = "";

                UpdateTable(trackedVariables);
            }
        }
    }
}
