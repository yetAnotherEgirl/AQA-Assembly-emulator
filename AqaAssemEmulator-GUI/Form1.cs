using AqaAssemEmulator_GUI.backend;
using System.Text.RegularExpressions;

namespace AqaAssemEmulator_GUI
{
    /* ToDo:
     * make a class to display Errors
     * add syntax highlighting to editor
     * add page with instructionset
     */

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

            Point RamLabelPosition = new(ShowRam.Location.X + (ShowRam.Size.Width / 2) - (RamLabel.Size.Width / 2),
                             ShowRam.Location.Y + (ShowRam.Size.Height / 2) + 40);

            RamLabel.Location = RamLabelPosition;
            RamLabel.BackColor = Color.Transparent;

            RamLabel.Click += ShowRam_Click;

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
            TraceTableDepthInput.Text = 30.ToString();
            TraceTableDepthInput_Enter(sender, e);
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
            CompileAssembly(AssemblyTextBox.Text);
        }
        #endregion editor buttons

        

        async void CompileAssembly(string assembly)
        {
            /*x
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
              */
            Task assemble = Task.Run(() => Assembler.AssembleFromString(assembly));
            Task resetRam = Task.Run(() => RAM.Reset());
            Task.WaitAll(assemble, resetRam);
            List<AssemblerError> errors = Assembler.GetCompilationErrors();

            bool failedTocompile = IsFailure(errors);

            if (failedTocompile)
            {
                AssemblerErrorDisplay errorDisplay = new(errors);
                errorDisplay.Show();

                return;
            }

            //this will only be true if the code has errors but they are none fatal
            if(errors.Count != 0)
            {
                Task t = Task.Run(() => {
                    AssemblerErrorDisplay errorDisplay = new(errors);
                    errorDisplay.Show();
                });
                await t;
                return;
            }

            RAM.LoadMachineCode(Assembler.GetMachineCode());
            UpdateSystemInfomation();
        }

        static bool IsFailure(List<AssemblerError> errors)
        {
            bool failedToCompile = false;
            if (errors.Count != 0)
            {
                foreach (AssemblerError error in errors)
                {
                    if (error.IsFatal)
                    {
                        failedToCompile = true;
                        break;
                    }
                }
            }
            return failedToCompile;
        }

        void UpdateSystemInfomation()
        {
            CpuInfo.UpdateRegisters();
            RamGrid.UpdateGrid();
        }


        private async void RunProgram()
        {
            //TraceTable.Clear();
            try
            {
                Cpu.halted = false;
                CpuInfo.UpdateRegisters();
                while (Cpu.halted == false)
                {
                    if (Cpu.GetProgramCounter() >= RAM.Length())
                    {
                        MessageBox.Show("Program counter out of bounds",
                                        "Runtime Error",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                        return;
                    }

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
        private void ShowRam_Click(object? sender, EventArgs e)
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

                    CompileAssembly(AssemblyTextBox.Text);
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
            /* this is a bad way to do this, it makes it a pain for the user to type in a number
             * the cursor will jump to the beginning of the string every time a character is entered
             */ 
           /*x
             * CPUDelayInput.Text = Regex.Replace(CPUDelayInput.Text, "[^0-9]", "");
             * if (CPUDelayInput.Text == "") CPUDelayInput.Text = "0";
             * CpuDelayInMs = int.Parse(CPUDelayInput.Text);
             * Cpu.UpdateDelay(CpuDelayInMs);
            */
        }

        private void CPUDelayInput_KeyDown(object sender, KeyEventArgs e)
        {
            //account for the fact that the user will expect the enter key to set the value
            if (e.KeyCode == Keys.Enter)
            {
                CPUDelayInput_Leave(sender, e);
            }
        }

        private void CPUDelayInput_Leave(object sender, EventArgs e)
        {
            CPUDelayInput.Text = Regex.Replace(CPUDelayInput.Text, "[^0-9]", "");
            if (CPUDelayInput.Text == "") CPUDelayInput.Text = Cpu.GetDelayInMs().ToString();
            CpuDelayInMs = int.Parse(CPUDelayInput.Text);
            Cpu.UpdateDelay(CpuDelayInMs);
        }

        //BAD!!!
        private void TraceTableDepthInput_TextChanged(object sender, EventArgs e)
        {
            /* this was done the same way as CPUDelayInput_TextChanged, the same explanation applies:
             * 
             * this is a bad way to do this, it makes it a pain for the user to type in a number
             * the cursor will jump to the beginning of the string every time a character is entered
             */
           /*x
             * TraceTableDepthInput.Text = Regex.Replace(TraceTableDepthInput.Text, "[^0-9]", "");
             * if (TraceTableDepthInput.Text == "") TraceTableDepthInput.Text = "10";
             */
        }

        //x BAD!!!
        private void TraceTableDepthInput_Enter(object sender, EventArgs e)
        {   /* this was another bad idea, however this code is still useful
             * It just needs to be called from somewhere else
             */
          /*x
             * if (!Cpu.halted)
             * {
             *     MessageBox.Show("Cannot change trace table depth while CPU is running",
             *                     "Runtime Error",
             *                     MessageBoxButtons.OK,
             *                     MessageBoxIcon.Error);
             * 
             *     TraceTableDepthInput.Text = TraceTable.GetDepth().ToString();
             * 
             *     return;
             * }
            */
        }

        private void TraceTableDepthInput_Leave(object sender, EventArgs e)
        {
            if (!Cpu.halted)
            {
                MessageBox.Show("Cannot change trace table depth while CPU is running",
                                "Runtime Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);

                TraceTableDepthInput.Text = TraceTable.GetDepth().ToString();

                return;
            }
            TraceTableDepthInput.Text = Regex.Replace(TraceTableDepthInput.Text, "[^0-9]", "");
            if (TraceTableDepthInput.Text == "") TraceTableDepthInput.Text = TraceTable.GetDepth().ToString();
            TraceTable.UpdateDepth(int.Parse(TraceTableDepthInput.Text) + 1);   //+1 because the first row is the header
        }

        private void TraceTableDepthInput_KeyDown(object sender, KeyEventArgs e)
        {
            //account for the fact that the user will expect the enter key to set the value
            if (e.KeyCode == Keys.Enter)
            {
                TraceTableDepthInput_Leave(sender, e);
            }
        }

        #endregion settings



    }
}
