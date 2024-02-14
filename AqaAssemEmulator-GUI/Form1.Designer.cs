namespace AqaAssemEmulator_GUI
{
    partial class Window
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            SaveAssembly = new SaveFileDialog();
            OpenAssembly = new OpenFileDialog();
            Settings = new TabPage();
            TraceTableDepthInput = new RichTextBox();
            TraceTableDepthLabel = new Label();
            CPUDelayInput = new RichTextBox();
            CPUDelayLabel = new Label();
            Editor = new TabPage();
            LoadButton = new Button();
            OpenButton = new Button();
            AssemblyTextBox = new RichTextBox();
            SaveButton = new Button();
            TraceTblTab = new TabPage();
            Hardware = new TabPage();
            ResetButton = new Button();
            HaltButton = new Button();
            LoadFileButton = new Button();
            RunButton = new Button();
            ShowRam = new Button();
            Tabs = new TabControl();
            HowToTab = new TabPage();
            RamLabel = new Label();
            Settings.SuspendLayout();
            Editor.SuspendLayout();
            Hardware.SuspendLayout();
            Tabs.SuspendLayout();
            SuspendLayout();
            // 
            // SaveAssembly
            // 
            SaveAssembly.DefaultExt = "aqa";
            SaveAssembly.ShowHiddenFiles = true;
            SaveAssembly.Title = "Save Assembly";
            // 
            // OpenAssembly
            // 
            OpenAssembly.DefaultExt = "aqa";
            // 
            // Settings
            // 
            Settings.Controls.Add(TraceTableDepthInput);
            Settings.Controls.Add(TraceTableDepthLabel);
            Settings.Controls.Add(CPUDelayInput);
            Settings.Controls.Add(CPUDelayLabel);
            Settings.Location = new Point(8, 46);
            Settings.Name = "Settings";
            Settings.Padding = new Padding(3);
            Settings.Size = new Size(1134, 1016);
            Settings.TabIndex = 3;
            Settings.Text = "Settings";
            Settings.UseVisualStyleBackColor = true;
            // 
            // TraceTableDepthInput
            // 
            TraceTableDepthInput.Location = new Point(565, 55);
            TraceTableDepthInput.Multiline = false;
            TraceTableDepthInput.Name = "TraceTableDepthInput";
            TraceTableDepthInput.RightToLeft = RightToLeft.No;
            TraceTableDepthInput.Size = new Size(200, 40);
            TraceTableDepthInput.TabIndex = 3;
            TraceTableDepthInput.Text = "30";
            TraceTableDepthInput.TextChanged += TraceTableDepthInput_TextChanged;
            TraceTableDepthInput.Enter += TraceTableDepthInput_Enter;
            TraceTableDepthInput.KeyDown += TraceTableDepthInput_KeyDown;
            TraceTableDepthInput.Leave += TraceTableDepthInput_Leave;
            // 
            // TraceTableDepthLabel
            // 
            TraceTableDepthLabel.AutoSize = true;
            TraceTableDepthLabel.Location = new Point(6, 55);
            TraceTableDepthLabel.Name = "TraceTableDepthLabel";
            TraceTableDepthLabel.Size = new Size(204, 32);
            TraceTableDepthLabel.TabIndex = 2;
            TraceTableDepthLabel.Text = "Trace Table Depth";
            // 
            // CPUDelayInput
            // 
            CPUDelayInput.Location = new Point(565, 9);
            CPUDelayInput.Multiline = false;
            CPUDelayInput.Name = "CPUDelayInput";
            CPUDelayInput.RightToLeft = RightToLeft.No;
            CPUDelayInput.Size = new Size(200, 40);
            CPUDelayInput.TabIndex = 1;
            CPUDelayInput.Text = "100";
            CPUDelayInput.TextChanged += CPUDelayInput_TextChanged;
            CPUDelayInput.KeyDown += CPUDelayInput_KeyDown;
            CPUDelayInput.Leave += CPUDelayInput_Leave;
            // 
            // CPUDelayLabel
            // 
            CPUDelayLabel.AutoSize = true;
            CPUDelayLabel.Location = new Point(6, 9);
            CPUDelayLabel.Name = "CPUDelayLabel";
            CPUDelayLabel.Size = new Size(201, 32);
            CPUDelayLabel.TabIndex = 0;
            CPUDelayLabel.Text = "CPU delay (in ms)";
            // 
            // Editor
            // 
            Editor.Controls.Add(LoadButton);
            Editor.Controls.Add(OpenButton);
            Editor.Controls.Add(AssemblyTextBox);
            Editor.Controls.Add(SaveButton);
            Editor.Location = new Point(8, 46);
            Editor.Name = "Editor";
            Editor.Padding = new Padding(3);
            Editor.Size = new Size(1134, 1016);
            Editor.TabIndex = 2;
            Editor.Text = "Editor";
            Editor.UseVisualStyleBackColor = true;
            // 
            // LoadButton
            // 
            LoadButton.Location = new Point(926, 6);
            LoadButton.Name = "LoadButton";
            LoadButton.Size = new Size(202, 46);
            LoadButton.TabIndex = 3;
            LoadButton.Text = "Load to RAM";
            LoadButton.UseVisualStyleBackColor = true;
            LoadButton.Click += LoadButton_Click;
            // 
            // OpenButton
            // 
            OpenButton.Location = new Point(162, 6);
            OpenButton.Name = "OpenButton";
            OpenButton.Size = new Size(150, 46);
            OpenButton.TabIndex = 2;
            OpenButton.Text = "Open";
            OpenButton.UseVisualStyleBackColor = true;
            OpenButton.Click += OpenButton_Click;
            // 
            // AssemblyTextBox
            // 
            AssemblyTextBox.AcceptsTab = true;
            AssemblyTextBox.BorderStyle = BorderStyle.FixedSingle;
            AssemblyTextBox.Cursor = Cursors.IBeam;
            AssemblyTextBox.Font = new Font("Cascadia Code SemiBold", 10.875F, FontStyle.Bold, GraphicsUnit.Point, 0);
            AssemblyTextBox.Location = new(6, 58);
            AssemblyTextBox.Name = "AssemblyTextBox";
            AssemblyTextBox.Size = new Size(1122, 952);
            AssemblyTextBox.TabIndex = 1;
            AssemblyTextBox.Text = "";
            // 
            // SaveButton
            // 
            SaveButton.Location = new Point(6, 6);
            SaveButton.Name = "SaveButton";
            SaveButton.Size = new Size(150, 46);
            SaveButton.TabIndex = 0;
            SaveButton.Text = "Save";
            SaveButton.UseVisualStyleBackColor = true;
            SaveButton.Click += SaveButton_Click;
            // 
            // TraceTblTab
            // 
            TraceTblTab.Location = new Point(8, 46);
            TraceTblTab.Name = "TraceTblTab";
            TraceTblTab.Padding = new Padding(3);
            TraceTblTab.Size = new Size(1134, 1016);
            TraceTblTab.TabIndex = 1;
            TraceTblTab.Text = "Trace table";
            TraceTblTab.UseVisualStyleBackColor = true;
            // 
            // Hardware
            // 
            Hardware.Controls.Add(RamLabel);
            Hardware.Controls.Add(ResetButton);
            Hardware.Controls.Add(HaltButton);
            Hardware.Controls.Add(LoadFileButton);
            Hardware.Controls.Add(RunButton);
            Hardware.Controls.Add(ShowRam);
            Hardware.Location = new Point(8, 46);
            Hardware.Name = "Hardware";
            Hardware.Padding = new Padding(3);
            Hardware.Size = new Size(1134, 1016);
            Hardware.TabIndex = 0;
            Hardware.Text = "Hardware view";
            Hardware.UseVisualStyleBackColor = true;
            // 
            // ResetButton
            // 
            ResetButton.Location = new Point(512, 889);
            ResetButton.Name = "ResetButton";
            ResetButton.Size = new Size(300, 120);
            ResetButton.TabIndex = 4;
            ResetButton.Text = "Reset System";
            ResetButton.UseVisualStyleBackColor = true;
            ResetButton.Click += ResetButton_Click;
            // 
            // HaltButton
            // 
            HaltButton.Location = new Point(828, 889);
            HaltButton.Name = "HaltButton";
            HaltButton.Size = new Size(300, 120);
            HaltButton.TabIndex = 3;
            HaltButton.Text = "Manual Halt";
            HaltButton.UseVisualStyleBackColor = true;
            HaltButton.Click += HaltButton_Click;
            // 
            // LoadFileButton
            // 
            LoadFileButton.Location = new Point(259, 890);
            LoadFileButton.Name = "LoadFileButton";
            LoadFileButton.Size = new Size(247, 120);
            LoadFileButton.TabIndex = 2;
            LoadFileButton.Text = "Load from file";
            LoadFileButton.UseVisualStyleBackColor = true;
            LoadFileButton.Click += LoadFileButton_Click;
            // 
            // RunButton
            // 
            RunButton.Location = new Point(6, 889);
            RunButton.Name = "RunButton";
            RunButton.Size = new Size(247, 120);
            RunButton.TabIndex = 1;
            RunButton.Text = "Run Program";
            RunButton.UseVisualStyleBackColor = true;
            RunButton.Click += RunButton_Click;
            // 
            // ShowRam
            // 
            ShowRam.Font = new Font("Segoe UI", 20F);
            ShowRam.Location = new Point(828, 6);
            ShowRam.Name = "ShowRam";
            ShowRam.Size = new Size(300, 800);
            ShowRam.TabIndex = 0;
            ShowRam.Text = "RAM";
            ShowRam.UseVisualStyleBackColor = true;
            ShowRam.Click += ShowRam_Click;
            // 
            // Tabs
            // 
            Tabs.Controls.Add(Hardware);
            Tabs.Controls.Add(TraceTblTab);
            Tabs.Controls.Add(Editor);
            Tabs.Controls.Add(Settings);
            Tabs.Controls.Add(HowToTab);
            Tabs.Location = new Point(12, 12);
            Tabs.Name = "Tabs";
            Tabs.SelectedIndex = 0;
            Tabs.Size = new Size(1150, 1070);
            Tabs.TabIndex = 0;
            // 
            // HowToTab
            // 
            HowToTab.Location = new Point(8, 46);
            HowToTab.Name = "HowToTab";
            HowToTab.Padding = new Padding(3);
            HowToTab.Size = new Size(1134, 1016);
            HowToTab.TabIndex = 4;
            HowToTab.Text = "How to use";
            HowToTab.UseVisualStyleBackColor = true;
            // 
            // RamLabel
            // 
            RamLabel.AutoSize = true;
            RamLabel.Location = new Point(905, 452);
            RamLabel.Name = "RamLabel";
            RamLabel.Size = new Size(165, 32);
            RamLabel.TabIndex = 5;
            RamLabel.Text = "(click to show)";
            // 
            // Window
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1174, 1094);
            Controls.Add(Tabs);
            Name = "Window";
            Text = "AQA assembly Emulator";
            Load += Form1_Load;
            Settings.ResumeLayout(false);
            Settings.PerformLayout();
            Editor.ResumeLayout(false);
            Hardware.ResumeLayout(false);
            Hardware.PerformLayout();
            Tabs.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private SaveFileDialog SaveAssembly;
        private OpenFileDialog OpenAssembly;
        private TabPage Settings;
        private TabPage Editor;
        private Button OpenButton;
        private RichTextBox AssemblyTextBox;
        private Button SaveButton;
        private TabPage TraceTblTab;
        private TabPage Hardware;
        private TabControl Tabs;
        private Button LoadButton;
        private Button ShowRam;
        private Button RunButton;
        private Button HaltButton;
        private Button LoadFileButton;
        private Button ResetButton;
        private Label CPUDelayLabel;
        private RichTextBox CPUDelayInput;
        private RichTextBox TraceTableDepthInput;
        private Label TraceTableDepthLabel;
        private TabPage HowToTab;
        private Label RamLabel;
    }
}
