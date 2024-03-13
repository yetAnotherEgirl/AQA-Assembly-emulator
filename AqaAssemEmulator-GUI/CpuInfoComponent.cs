using AqaAssemEmulator_GUI.backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AqaAssemEmulator_GUI
{
    internal class CpuInfoComponent : UserControl
    {

        internal CPU Cpu { get; set; }

        RichTextBox Header;
        RichTextBox ProgramCounter;
        RichTextBox MemoryAddressRegister;
        RichTextBox MemoryDataRegister;
        RichTextBox Accumulator;
        RichTextBox CPSRflag;

        RichTextBox[] GeneralRegisters;

        PictureBox PCtoMARarrow;
        PictureBox MDRtoALUarrow;

        int padding = 10;

        internal CpuInfoComponent(ref CPU cpu)
        {
            Cpu = cpu;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.Name = "CpuInfoComponent";
            this.Size = new Size(500, 800);
            this.Location = new Point(6, 6);
            this.BorderStyle = BorderStyle.FixedSingle;

            #region define header 
            Header = new RichTextBox();
            Size headerSize = new Size(125, 60);
            Point headerLocation = new Point(250 - headerSize.Width / 2, padding);

            Header.Location = headerLocation;
            Header.Size = headerSize;
            Header.Text = "CPU";
            Header.ReadOnly = true;
            Header.Font = new Font("Arial", 20, FontStyle.Bold);
            Header.BackColor = Color.White;
            Header.ScrollBars = RichTextBoxScrollBars.None;
            Header.BorderStyle = BorderStyle.FixedSingle;

            this.Controls.Add(Header);
            #endregion define header

            #region define program counter
            ProgramCounter = new RichTextBox();
            Point programCounterLocation = new Point(padding, 100);
            Size programCounterSize = new Size(200, 40);

            ProgramCounter.Location = programCounterLocation;
            ProgramCounter.Size = programCounterSize;
            ProgramCounter.Text = "PC: " + Cpu.GetProgramCounter().ToString();
            ProgramCounter.ReadOnly = true;
            ProgramCounter.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            ProgramCounter.BackColor = Color.White;
            ProgramCounter.ScrollBars = RichTextBoxScrollBars.None;
            ProgramCounter.BorderStyle = BorderStyle.FixedSingle;

            this.Controls.Add(ProgramCounter);
            #endregion define program counter

            #region define MAR
            MemoryAddressRegister = new RichTextBox();
            Size MemoryAddressRegisterSize = new Size(200, 40);
            Point MemoryAddressRegisterLocation = new Point(
                this.Size.Width - (padding + MemoryAddressRegisterSize.Width), 100);

            MemoryAddressRegister.Location = MemoryAddressRegisterLocation;
            MemoryAddressRegister.Size = MemoryAddressRegisterSize;
            MemoryAddressRegister.Text = "MAR: " + Cpu.GetMemoryAddressRegister().ToString();
            MemoryAddressRegister.ReadOnly = true;
            MemoryAddressRegister.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            MemoryAddressRegister.BackColor = Color.White;
            MemoryAddressRegister.ScrollBars = RichTextBoxScrollBars.None;
            MemoryAddressRegister.BorderStyle = BorderStyle.FixedSingle;

            this.Controls.Add(MemoryAddressRegister);
            #endregion define MAR

            #region define PC to MAR arrow
            PCtoMARarrow = new PictureBox();
            Size PCtoMARarrowSize = new Size(80, 40);
            PCtoMARarrow.Image = Image.FromFile("Assets/SmallArrowRight.png");
            PCtoMARarrow.Location = new Point(programCounterLocation.X + programCounterSize.Width, 100);

            this.Controls.Add(PCtoMARarrow);
            #endregion define PC to MAR arrow

            #region define MemoryDataRegister
            MemoryDataRegister = new RichTextBox();
            Point MemoryDataRegisterLocation = new Point(MemoryAddressRegisterLocation.X, 200);
            Size MemoryDataRegisterSize = new Size(200, 40);

            MemoryDataRegister.Location = MemoryDataRegisterLocation;
            MemoryDataRegister.Size = MemoryDataRegisterSize;
            MemoryDataRegister.Text = "MDR: " + Cpu.GetMemoryDataRegister().ToString();
            MemoryDataRegister.ReadOnly = true;
            MemoryDataRegister.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            MemoryDataRegister.BackColor = Color.White;
            MemoryDataRegister.ScrollBars = RichTextBoxScrollBars.None;
            MemoryDataRegister.BorderStyle = BorderStyle.FixedSingle;

            this.Controls.Add(MemoryDataRegister);
            #endregion define MemoryDataRegister

            #region define ArithmaticLogicUnit
            Accumulator = new RichTextBox();
            Point AccumulatorLocation = new Point(padding, 200);
            Size ArithmaticLogicUnitSize = new Size(200, 40);

            Accumulator.Location = AccumulatorLocation;
            Accumulator.Size = ArithmaticLogicUnitSize;
            Accumulator.Text = "ACC: " + Cpu.GetACC().ToString();
            Accumulator.ReadOnly = true;
            Accumulator.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            Accumulator.BackColor = Color.White;
            Accumulator.ScrollBars = RichTextBoxScrollBars.None;
            Accumulator.BorderStyle = BorderStyle.FixedSingle;

            this.Controls.Add(Accumulator);
            #endregion define ArithmaticLogicUnit

            #region define MDR to ALU arrow
            MDRtoALUarrow = new PictureBox();
            Size MDRtoALUarrowSize = new Size(80, 40);
            MDRtoALUarrow.Image = Image.FromFile("Assets/SmallArrowLeft.png");
            MDRtoALUarrow.Location = new Point(MemoryDataRegisterLocation.X - MDRtoALUarrowSize.Width, 200);

            this.Controls.Add(MDRtoALUarrow);
            #endregion define MDR to ALU arrow

            #region define general purpose registers
            int registerCount = Cpu.GetRegisterCount();
            int offset = (int)(this.Size.Height - Math.Ceiling((double)registerCount / 2) * 50) - 50;

            GeneralRegisters = new RichTextBox[registerCount];
            for (int i = 0; i < registerCount; i++)
            {
                GeneralRegisters[i] = new RichTextBox();
                

                int locationX = padding;
                if (i % 2 == 1) locationX = MemoryAddressRegisterLocation.X;
                
                int locationY = offset + (i / 2) * 50;

                Point GeneralRegisterLocation = new Point(locationX, locationY);

                Size GeneralRegisterSize = new Size(200, 40);

                GeneralRegisters[i].Location = GeneralRegisterLocation;
                GeneralRegisters[i].Size = GeneralRegisterSize;
                GeneralRegisters[i].Text = "R" + i.ToString() + ": " + Cpu.GetRegister(i).ToString();
                GeneralRegisters[i].ReadOnly = true;
                GeneralRegisters[i].Font = new Font("Segoe UI", 10, FontStyle.Regular);
                GeneralRegisters[i].BackColor = Color.White;
                GeneralRegisters[i].ScrollBars = RichTextBoxScrollBars.None;
                GeneralRegisters[i].BorderStyle = BorderStyle.FixedSingle;

                this.Controls.Add(GeneralRegisters[i]);
            }

            #endregion define general purpose registers

            #region define CPSR flag
            CPSRflag = new RichTextBox();
            Point CPSRflagLocation = new Point(padding, this.Size.Height - 50);
            Size CPSRflagSize = new Size(200, 40);

            CPSRflag.Location = CPSRflagLocation;
            CPSRflag.Size = CPSRflagSize;
            CPSRflag.Text = "flags: " + Cpu.GetCPSR().ToString();
            CPSRflag.ReadOnly = true;
            CPSRflag.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            CPSRflag.BackColor = Color.White;
            CPSRflag.ScrollBars = RichTextBoxScrollBars.None;
            CPSRflag.BorderStyle = BorderStyle.FixedSingle;

            this.Controls.Add(CPSRflag);
            #endregion define CPSR flag

            this.ResumeLayout(false);
        }

        public void UpdateRegisters()
        {
            this.SuspendLayout();
            ProgramCounter.Text = "PC: " + Cpu.GetProgramCounter().ToString();
            MemoryAddressRegister.Text = "MAR: " + Cpu.GetMemoryAddressRegister().ToString();
            MemoryDataRegister.Text = "MDR: " + Cpu.GetMemoryDataRegister().ToString();
            Accumulator.Text = "ACC: " + Cpu.GetACC().ToString();
            CPSRflag.Text = "flags: " + Cpu.GetCPSR().ToString();
            for (int i = 0; i < Cpu.GetRegisterCount(); i++)
            {
                GeneralRegisters[i].Text = "R" + i.ToString() + ": " + Cpu.GetRegister(i).ToString();
            }

            if (Cpu.halted)
            {
                Header.BackColor = Color.White;
            }
            else
            {
                Header.BackColor = Color.LightGray;
            }

            this.ResumeLayout(false);
        }
    }
}
