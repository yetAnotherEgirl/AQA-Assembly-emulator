using AqaAssemEmulator_GUI.backend;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AqaAssemEmulator_GUI
{
    //! 

    public partial class Window : Form
    {
        static CPU Cpu;
        Memory RAM;
        Assembler Assembler;

        CpuInfoComponent CpuInfo;
        RamGrid RamGrid;

        PictureBox CPUtoRAMarrow;
        PictureBox RAMtoCPUarrow;

        int CpuDelayInMs;

        TraceTable TraceTable;

        AssemblerErrorDisplay assemblerErrorDisplay;
        EmulatorErrorDisplay emulatorErrorDisplay;

        readonly TestingModePopout testingModePopout = new();

        static ManualResetEventSlim ErrorRecieved = new(false);

        

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

            PopulateHowToTextbox();

            this.ResumeLayout(false);

            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CpuDelayInMs = int.Parse(CPUDelayInput.Text);
            InitializeHardware();

            assemblerErrorDisplay = new AssemblerErrorDisplay();
            assemblerErrorDisplay.IgnoreButtonClicked += AssemblerIgnoreButtonClicked;

            emulatorErrorDisplay = new EmulatorErrorDisplay();
            emulatorErrorDisplay.IgnoreButtonClicked += EmulatorIgnoreButtonClicked;
            emulatorErrorDisplay.OkButtonClicked += EmulatorOkButtonClicked;

            TraceTable = new TraceTable(Cpu, ref RAM);
            this.TraceTblTab.Controls.Add(TraceTable);
            TraceTableDepthInput.Text = TraceTable.GetDepth().ToString();
            TraceTableDepthInput_Enter(sender, e);
            TraceTable.Show();
            CpuInfo.Show();

            TestingModeCheckBox.CheckedChanged += TestingModeCheckBox_CheckedChanged;
        }

        void InitializeHardware()
        {
            RAM = new Memory(200);
            Cpu = new CPU(ref RAM, CpuDelayInMs, 13);

            Cpu.EmulatorErrorOccured += CPU_EmulatorErrorOccurred;

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
            testingModePopout.UpdateAssembly(AssemblyTextBox.Lines);
            Task assemble = Task.Run(() => Assembler.AssembleFromString(assembly));
            Task resetRam = Task.Run(() => RAM.Reset());
            Task.WaitAll(assemble, resetRam);
            List<AssemblerError> errors = Assembler.GetCompilationErrors();

            if (errors.Count == 0)
            {
                RAM.LoadMachineCode(Assembler.GetMachineCode());
                UpdateSystemInfomation();
                try
                {
                    if(TraceTable.TestingMode == false) TraceTable.UpdateTable(Assembler.GetVariables());
                }
                catch (ArgumentException e)
                {
                    emulatorErrorDisplay.SetErrors([new EmulatorError(e.Message, -1, true)]);
                }
                return;
            }

            assemblerErrorDisplay.SetErrors(errors);
            assemblerErrorDisplay.ShowDialog();
        }

        void AssemblerIgnoreButtonClicked(object? sender, EventArgs e)
        {
            RAM.LoadMachineCode(Assembler.GetMachineCode());
            UpdateSystemInfomation();
            TraceTable.UpdateTable(Assembler.GetVariables());
        }

        void EmulatorIgnoreButtonClicked(object? sender, EventArgs e)
        {
            ErrorRecieved = new(false);
        }

        void EmulatorOkButtonClicked(object? sender, EventArgs e)
        {
            Cpu.halted = true;
            UpdateSystemInfomation();
        }

        //no longer needed
        /*x
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
         */

        void UpdateSystemInfomation()
        {
            if (ErrorRecieved.IsSet) return;
            CpuInfo.UpdateRegisters();
            RamGrid.UpdateGrid();
        }


        private async void RunProgram()
        {
            //TraceTable.UpdateTable(Assembler.GetVariables());
            Cpu.halted = false;
            CpuInfo.UpdateRegisters();
            while (Cpu.halted == false)
            {
                if (Cpu.GetProgramCounter() >= RAM.GetLength())
                {
                    MessageBox.Show("Program counter out of bounds",
                                    "Runtime Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    return;
                }

                await Task.Run(() =>
                {
                    if (!ErrorRecieved.IsSet)
                        Cpu.Fetch();
                });

                UpdateSystemInfomation();

                await Task.Run(() =>
                {
                    if (!ErrorRecieved.IsSet)
                        Cpu.Decode();

                });

                UpdateSystemInfomation();

                await Task.Run(() =>
                {
                    if (!ErrorRecieved.IsSet)
                        Cpu.Execute();
                });

                TraceTable.UpdateTable(Assembler.GetVariables());
            }

            UpdateSystemInfomation();
            TraceTable.RemoveExtraLines();
        }

        private void CPU_EmulatorErrorOccurred(object? sender, EmulatorErrorEventArgs e)
        {
            ErrorRecieved.Set();
            emulatorErrorDisplay.SetErrors(e.Errors);
            emulatorErrorDisplay.ShowDialog();
        }

        #region hardware buttons
        private void ShowRam_Click(object? sender, EventArgs e)
        {
            RamGrid.Show();
        }

        private async void RunButton_Click(object sender, EventArgs e)
        {
           

            if (RAM.IsEmpty)
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
            if (RAM.IsEmpty) return;
            Cpu.Reset();
            RunProgram();
        }

        private void LoadFileButton_Click(object sender, EventArgs e)
        {
            if (Cpu.halted == false)
            {
                MessageBox.Show("Cannot load from file while CPU is running",
                                "Runtime Error",
                                 MessageBoxButtons.OK,
                                 MessageBoxIcon.Error);
                return;
            }

            OpenAssembly.Filter = "Assembly Files (*.aqa)|*.aqa";
            OpenAssembly.DefaultExt = "aqa";
            OpenAssembly.Title = "Open Assembly";
            OpenAssembly.ShowDialog();

            if (OpenAssembly.FileName != "")
            {
                string assembly = File.ReadAllText(OpenAssembly.FileName);
                testingModePopout.UpdateAssembly(assembly.Split('\n'));
                AssemblyTextBox.Text = assembly;
                if(TraceTable.TestingMode) testingModePopout.Show();
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
            Task ResetCpuTask = Task.Run(Cpu.Reset);
            Task ResetRamTask = Task.Run(RAM.Reset);
            TraceTable.Clear();
            ErrorRecieved = new(false);
            Task.WaitAll(ResetCpuTask, ResetRamTask);

            UpdateSystemInfomation();
            TraceTable.Clear();
             
        }
        #endregion hardware buttons

        #region settings
        private void TestingModeCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (Cpu.halted == false)
            {
                MessageBox.Show("Cannot change testing mode while CPU is running",
                                "Runtime Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                TestingModeCheckBox.Checked = !TestingModeCheckBox.Checked;
                return;
            }

            TraceTable.TestingMode = TestingModeCheckBox.Checked;
            if (Assembler.GetVariables().Count != 0)
            {
                TraceTable.UpdateTable(Assembler.GetVariables());
            }

            if (TestingModeCheckBox.Checked)
            {
                testingModePopout.Show();
                testingModePopout.UpdateAssembly(AssemblyTextBox.Lines);
            }
            else
            {
                testingModePopout.Hide();
            }
        }


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
            try
            {
                CpuDelayInMs = int.Parse(CPUDelayInput.Text);
                Cpu.UpdateDelay(CpuDelayInMs);
            }
            catch (Exception)
            {
                CPUDelayInput.Text = Cpu.GetDelayInMs().ToString();
            }
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
            try
            {
                TraceTable.UpdateDepth(int.Parse(TraceTableDepthInput.Text) + 1);   //+1 because the first row is the header
            }
            catch (Exception)
            {
                TraceTableDepthInput.Text = TraceTable.GetDepth().ToString();
            }
            
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

        void Tabs_TabIndexChanged(object? sender, EventArgs e)
        {
            if (Tabs.SelectedTab == Editor)
            {
                testingModePopout.Hide();
            }
            else
            {
                if(TraceTable.TestingMode)
                {
                    testingModePopout.Show();
                    testingModePopout.UpdateAssembly(AssemblyTextBox.Lines);
                }
            }
        }

        void PopulateHowToTextbox()
        {
            Font title = new("Arial", 20);
            Font header = new("Arial", 16);
            Font body = new("Arial", 12);

            HowToTextbox.SelectionFont = title;
            HowToTextbox.AppendText("AQA assembly Emulator - made by Emily\r\n\r\n");
            HowToTextbox.SelectionFont = header;
            HowToTextbox.AppendText("Quick start: \r\n");
            HowToTextbox.SelectionFont = body;
            HowToTextbox.AppendText("programs can be created in the editor tab. To run a program,  " +
                "press \"Load to RAM\", then go to the hardware tab, and press run\r\n\r\n\r\n\r\n");
            HowToTextbox.SelectionFont = header;
            HowToTextbox.AppendText("Hardware view:\r\n");
            HowToTextbox.SelectionFont = body;
            HowToTextbox.AppendText("here the registers in the CPU can be seen, one additional register " +
                "not present in the AQA standard is the current program status register, this stores flags " +
                "about a numerical operation each time CMP is called. while the CPU is running (not halted) the " +
                "CPU diagram label will turn grey.\r\n\r\n" +
                "the RAM diagram can be pressed to see the data currently stored in RAM (the first few addresses " +
                "will be the program information so make sure not to write over this).\r\n\r\n" +
                "Run Program starts the CPU and begins executing the instructions in RAM from address 0, if " +
                "there is no program currently loaded into RAM it will attempt to load from the editor.\r\n\r\n" +
                "Load from file will \"compile\" any .aqa file and load it into RAM.\r\n\r\n" +
                "Reset system will wipe all data from the CPU, RAM, and trace Table, if the CPU is running it" +
                " will halt the CPU.\r\n\r\nManual halt will halt the CPU, this is useful if you have accidentally " +
                "created an infinite loop.\r\n\r\n\r\n\r\n");
            HowToTextbox.SelectionFont = header;
            HowToTextbox.AppendText("Trace table:\r\n");
            HowToTextbox.SelectionFont = body;
            HowToTextbox.AppendText("once a program is loaded, the variables used in the program will be added as " +
                "columns in the trace table. \r\nthe trace table will update with each CPU cycle, if there aren't " +
                "enough lines on the trace table for all the data, it will refresh.\r\nthe trace table \"Depth\" " +
                "(number of rows) can be configured in settings\r\n\r\n\r\n\r\n");
            HowToTextbox.SelectionFont = header;
            HowToTextbox.AppendText("Editor:\r\n");
            HowToTextbox.SelectionFont = body;
            HowToTextbox.AppendText("Save and Open buttons can be used to read/write .aqa files to storage.\r\n\r\n" +
                "Load to RAM will \"compile\" the program and move it to RAM, ready to be run from " +
                "the hardware tab.\r\n\r\n" +
                "AQA assembly can be written in the text box on this tab.\r\n\r\n\r\n\r\n");
            HowToTextbox.SelectionFont = header;
            HowToTextbox.AppendText("Settings:\r\n");
            HowToTextbox.SelectionFont = body;
            HowToTextbox.AppendText("CPU delay (in ms) is the amount of time the CPU thread will sleep between " +
                "each step of the cycle (ie a delay of 100 would look like: fetch, sleep 100, decode, " +
                "sleep 100, execute, sleep 100).\r\n\r\n" +
                "Trace Table Depth Step is the number of rows the trace table will add each time it fills up, " +
                "low values will cause lag so keep this over 30, this cannot be changed while the CPU is running.\r\n\r\n\r\n\r\n");
            HowToTextbox.SelectionFont = header;
            HowToTextbox.AppendText("language documentation:\r\n");
            HowToTextbox.SelectionFont = body;
            HowToTextbox.AppendText("All programs must have a HALT instruction in the program to \"compile\".\r\n\r\n" +
                "unlike in the official AQA assembly, operands do not have to be separated by commas " +
                "(however they still can be to maintain full parity).\r\n\r\n" +
                "comments are made in the program with a semicolon.\r\n\r\n" +
                "several pre-processor flags can be passed, all pre-processor flags are " +
                "indicated with an asterisk.\r\n\r\n\r\n");
            HowToTextbox.SelectionFont = header;
            HowToTextbox.AppendText("Preprocessor flags:\r\n");
            HowToTextbox.SelectionFont = body;
            HowToTextbox.AppendText("\"*EXTENDED\"\r\n - this enables the extended instruction set which can " +
                "be found further below\r\n\r\n" +
                "\"*INCLUDE <path/to/someFile> <where>\"\r\n - this \"pastes\" the content of another assembly" +
                " file (said file being \"someFile\"), <where> can either be\"FIRST\", to paste the file at the " +
                "start, \"LAST\", to paste the file at the end, or \"HERE\" to include it where the line " +
                "is\r\n\r\n\r\n");
            HowToTextbox.SelectionFont = header;
            HowToTextbox.AppendText("definitions for reading the instruction set:\r\n");
            HowToTextbox.SelectionFont = body;
            HowToTextbox.AppendText("\"Rn\" means register n (there are 13 registers being 0 up to and including " +
                "12), instructions involving multiple registers may also use \"Rd\" " +
                "which means register d.\r\n\r\n" +
                "\"<opperand>\" can either be a register see above, an address in memory to read from," +
                " or a number constant indicated with a hashtag (eg \"#3\").\r\n\r\n\"<memory ref>\" means " +
                "memory reference, it is a pointer to an address in memory.\r\n\r\n\"<label>\" is a header " +
                "which can be branched to, labels are 1 word and marked with a colon\r\n(eg:\r\n\"somelabel:\r\n;" +
                " do something\r\nB somelabel\").\r\n\r\n\"CPSR\" is the current program status register, it " +
                "is different from standard registers as instead of storing a number it stores a CPSRflag.\r\n\r\n\"" +
                "CPSRflag\" is a flag that is stored in the current program status register, it can only " +
                "be \"Zero\", \"Negative\", \"Overflow\" or \"None\"\r\n\r\n\r\n");
            HowToTextbox.SelectionFont = header;
            HowToTextbox.AppendText("The standard instruction set for the AQA assembly reference language:\r\n");
            HowToTextbox.SelectionFont = body;
            HowToTextbox.AppendText("\"LDR Rn <memory ref>\"\r\n - loads the value in <memory ref> into" +
                " Rn\r\n\"STR Rn <memory ref>\"\r\n - stores the value of Rn into <memory ref>\r\n\"ADD Rn Rd " +
                "<opperand>\" \r\n - adds the value of <opperand> to the value of Rd and stores the result in" +
                " Rn\r\n\"SUB Rn Rd <opperand>\"\r\n - subtracts the value of <opperand> from the value of Rd and " +
                "stores the result in Rn\r\n\"MOV Rn <opperand>\"\r\n - moves the value of <opperand> into Rn, MOV " +
                "can be used in the same situations LDR is used in however this is considered bad practice as it is" +
                " less efficient\r\n\"CMP Rn <opperand>\"\r\n - compares Rn with the value of <opperand> to set " +
                "the CPSR, this is done by subtracting Rn from <opperand>, if the result is 0 the CPSR is set to " +
                "Zero, if it is below 0 the CPSR is set to negative, if an overflow occurs set the CPSR to " +
                "Overflow\r\n\"B <label>\"\r\n - branches to the given label\r\n\"BEQ <label>\" \r\n- branches to " +
                "the given label if the CPSR is set to Zero\r\n\"BNE <label>\"\r\n - branches to the given label " +
                "if the CPSR is not set to zero\r\n\"BGT <label>\"\r\n - branches to the given label if the CPSR " +
                "is set to Negative\r\n\"BLT <label>\"\r\n - branches to the given label if the CPSR is not " +
                "negative and not Zero\r\n\"AND Rn Rd <opperand>\"\r\n - performs the logical operation \"and\" " +
                "between the value of Rd and the value of <opperand>, the result is stored in Rn\r\n\"ORR Rn " +
                "Rd <opperand>\"\r\n - performs the logical operation \"or\" between the value of Rd and the " +
                "value of <opperand>, the result is stored in Rn\r\n\"EOR Rn Rd <opperand>\"\r\n - performs the" +
                " logical operation \"xor\" between the value of Rd and the value of <opperand>, the result is " +
                "stored in Rn\r\n\"MVN Rn <opperand>\"\r\n - performs the logical operation \"not\" on the " +
                "value stored in <opperand> and stores the result in Rn\r\n\"LSL Rn Rd <opperand>\"\r\n - " +
                "shifts the bits of Rd left by the value of <opperand>, storing the result in Rn\r\n\"LSR " +
                "Rn Rd <opperand>\" \r\n - shifts the bits of Rd left by the value of <opperand>, storing the " +
                "result in Rn\r\n\"HALT\" \r\n - stops execution of the program and halts the CPU\r\n\r\n\r\n");
            HowToTextbox.SelectionFont = header;
            HowToTextbox.AppendText("\r\nThe extended instruction Set (the extended pre-processor " +
                "flag must be passed to use these instructions):\r\n");
            HowToTextbox.SelectionFont = body;
            HowToTextbox.AppendText("\"INPUT Rn\" \r\n - opens a dialog box for the user to input a number, " +
                "this is stored in Rn\r\n\"OUTPUT Rn\"\r\n - opens a dialog box displaying the value of Rn " +
                "to the user\r\n\"DUMP <dump type>\"\r\n - <dump type> can be either \"memory\", \"registers\", " +
                "or \"all\", depending on what is passed as <dump type, these are then saved as a .Dump file " +
                "located in ./Dumps/\r\n\"JMP Rn\" \r\n- sets the program counter to the value stored in Rn, " +
                "useful for creating a call stack\r\n\"CDP Rn\" \r\n - stores the current value of the program " +
                "counter in Rn, you will typically want to add 2 to this value so if you return to it with JMP " +
                "you do not get stuck in a loop");
        }

        
    }
}
