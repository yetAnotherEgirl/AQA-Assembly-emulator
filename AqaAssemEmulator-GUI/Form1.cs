using AqaAssemEmulator_GUI.backend;
using System.Text.RegularExpressions;

namespace AqaAssemEmulator_GUI
{
    public partial class Window : Form
    {
        CPU Cpu;
        Memory RAM;
        Assembler Assembler;

        CpuInfoComponent CpuInfo;
        RamGrid RamGrid;

        PictureBox CPUtoRAMarrow;
        PictureBox RAMtoCPUarrow;

        int CpuDelayInMs;

        TraceTable TraceTable;

        public Window()
        {
            this.SuspendLayout();
            InitializeComponent();
            Size currentSize = this.Size;
            this.MaximizeBox = false;
            this.MaximumSize = currentSize;
            this.MinimumSize = currentSize;

            CPUtoRAMarrow = new PictureBox();
            CPUtoRAMarrow.Image = Image.FromFile("Assets/LongArrowRight.png");
            CPUtoRAMarrow.Size = new Size(330, 65);
            CPUtoRAMarrow.Location = new Point(501, 94);
            this.Hardware.Controls.Add(CPUtoRAMarrow);

            int secondArrowOffset = 40;

            RAMtoCPUarrow = new PictureBox();
            RAMtoCPUarrow.Image = Image.FromFile("Assets/LongArrowLeft.png");
            RAMtoCPUarrow.Size = new Size(330, 65);
            RAMtoCPUarrow.Location = new Point(501, 154 + secondArrowOffset);
            this.Hardware.Controls.Add(RAMtoCPUarrow);

            this.ResumeLayout(false);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CpuDelayInMs = int.Parse(CPUDelayInput.Text);
            InitializeHardware();

            TraceTable = new TraceTable(Cpu);
            this.TraceTblTab.Controls.Add(TraceTable);
            TraceTable.Show();
            CpuInfo.Show();
        }

        void InitializeHardware()
        {
            RAM = new Memory(200);
            Cpu = new CPU(ref RAM, CpuDelayInMs, 13);

            Assembler = new Assembler();

            CpuInfo = new CpuInfoComponent(ref Cpu);
            RamGrid = new RamGrid(ref RAM);

            this.Hardware.Controls.Add(CpuInfo);
        }

        #region editor buttons
        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (AssemblyTextBox.Text == "")
            {
                System.Media.SystemSounds.Beep.Play();
                MessageBox.Show("Assembly cannot be empty",
                                "Save Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            SaveAssembly.Filter = "Assembly Files (*.aqa)|*.aqa";
            SaveAssembly.DefaultExt = "aqa";
            SaveAssembly.Title = "Save Assembly";
            SaveAssembly.ShowDialog();

            if (SaveAssembly.FileName != "")
            {
                File.WriteAllText(SaveAssembly.FileName, AssemblyTextBox.Text);
            }
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            OpenAssembly.Filter = "Assembly Files (*.aqa)|*.aqa";
            OpenAssembly.DefaultExt = "aqa";
            OpenAssembly.Title = "Open Assembly";
            OpenAssembly.ShowDialog();

            if (OpenAssembly.FileName != "")
            {
                AssemblyTextBox.Text = File.ReadAllText(OpenAssembly.FileName);
            }
        }

        private async void LoadButton_Click(object sender, EventArgs e)
        {
            if (AssemblyTextBox.Text == "")
            {
                System.Media.SystemSounds.Beep.Play();
                MessageBox.Show("Assembly cannot be empty",
                                 "Load Error",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Error);
                return;
            }
            try
            {
                CompileAssembly(AssemblyTextBox.Text);
            }
            catch (Exception)
            {
                return;
            }
        }
        #endregion editor buttons

        void CompileAssembly(string assembly)
        {
            try
            {
                Task assemble = Task.Run(() => Assembler.AssembleFromString(assembly));
                Task resetRam = Task.Run(() => RAM.Reset());
                Task.WaitAll(assemble, resetRam);
                RAM.LoadMachineCode(Assembler.GetMachineCode());
                MessageBox.Show("Assembly loaded successfully",
                                "Load Success",
                                MessageBoxButtons.OK
                                );
                UpdateSystemInfomation();
                TraceTable.UpdateTable(Assembler.GetVariables());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message,
                                "Compile Error",
                                MessageBoxButtons.OK
                                );
                throw;
            }
        }

        void UpdateSystemInfomation()
        {
            CpuInfo.UpdateRegisters();
            RamGrid.UpdateGrid();
        }


        private async void RunProgram()
        {
            try
            {
                Cpu.halted = false;
                CpuInfo.UpdateRegisters();
                while (Cpu.halted == false)
                {
                    await Task.Run(() => Cpu.Fetch());
                    UpdateSystemInfomation();
                    await Task.Run(() => Cpu.Decode());
                    UpdateSystemInfomation();
                    await Task.Run(() => Cpu.Execute());
                    UpdateSystemInfomation();
                    TraceTable.UpdateTable(Assembler.GetVariables());
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message,
                                "Runtime Error",
                                MessageBoxButtons.OK
                                                                                                                             );
                RAM.DumpMemory("memory.Dump");
                Cpu.DumpRegisters("registers.Dump");
            }
        }

        #region hardware buttons
        private void ShowRam_Click(object sender, EventArgs e)
        {
            RamGrid.Show();
        }

        private async void RunButton_Click(object sender, EventArgs e)
        {
            if (RAM.QuereyAddress(0) == (long)0)
            {
                if (AssemblyTextBox.Text != "")
                {
                    MessageBox.Show("No program loaded, loading from Editor",
                                    "Runtime warning",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Warning);

                    try
                    {
                        CompileAssembly(AssemblyTextBox.Text);
                    }
                    catch (Exception)
                    {
                        return;
                    }
                }
                else
                {
                    System.Media.SystemSounds.Beep.Play();
                    MessageBox.Show("No program loaded, Editor empty",
                                    "Runtime Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return;
                }
            }
            if (!Cpu.halted)
            {
                System.Media.SystemSounds.Beep.Play();
                MessageBox.Show("CPU is already running, press 'Manual halt' to stop",
                                "Runtime Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            Cpu.Reset();
            RunProgram();
        }

        private void LoadFileButton_Click(object sender, EventArgs e)
        {
            OpenAssembly.Filter = "Assembly Files (*.aqa)|*.aqa";
            OpenAssembly.DefaultExt = "aqa";
            OpenAssembly.Title = "Open Assembly";
            OpenAssembly.ShowDialog();

            if (OpenAssembly.FileName != "")
            {
                string assembly = File.ReadAllText(OpenAssembly.FileName);

                CompileAssembly(assembly);

            }
        }

        private void HaltButton_Click(object sender, EventArgs e)
        {
            Cpu.halted = true;
            UpdateSystemInfomation();
        }

        private async void ResetButton_Click(object sender, EventArgs e)
        {
            Task ResetCpuTask = Task.Run(() => Cpu.Reset());
            Task ResetRamTask = Task.Run(() => RAM.Reset());

            Task.WaitAll(ResetCpuTask, ResetRamTask);

            UpdateSystemInfomation();
            TraceTable.Clear();
        }
        #endregion hardware buttons

        #region settings
        private void CPUDelayInput_TextChanged(object sender, EventArgs e)
        {
            CPUDelayInput.Text = Regex.Replace(CPUDelayInput.Text, "[^0-9]", "");
            if (CPUDelayInput.Text == "") CPUDelayInput.Text = "0";
            CpuDelayInMs = int.Parse(CPUDelayInput.Text);
            Cpu.UpdateDelay(CpuDelayInMs);
        }

        private void TraceTableDepthInput_TextChanged(object sender, EventArgs e)
        {
            TraceTableDepthInput.Text = Regex.Replace(TraceTableDepthInput.Text, "[^0-9]", "");
            if (TraceTableDepthInput.Text == "") TraceTableDepthInput.Text = "10";
        }

        private void TraceTableDepthInput_Enter(object sender, EventArgs e)
        {
            if (!Cpu.halted)
            {
                MessageBox.Show("Cannot change trace table depth while CPU is running",
                                "Runtime Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }
            TraceTable.UpdateDepth(int.Parse(TraceTableDepthInput.Text));
        }

        #endregion settings



    }
}
