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
            Tabs = new TabControl();
            Hardware = new TabPage();
            TraceTblTab = new TabPage();
            Editor = new TabPage();
            OpenButton = new Button();
            AssemblyTextBox = new RichTextBox();
            SaveButton = new Button();
            Settings = new TabPage();
            SaveAssembly = new SaveFileDialog();
            OpenAssembly = new OpenFileDialog();
            Tabs.SuspendLayout();
            Editor.SuspendLayout();
            SuspendLayout();
            // 
            // Tabs
            // 
            Tabs.Controls.Add(Hardware);
            Tabs.Controls.Add(TraceTblTab);
            Tabs.Controls.Add(Editor);
            Tabs.Controls.Add(Settings);
            Tabs.Location = new Point(12, 12);
            Tabs.Name = "Tabs";
            Tabs.SelectedIndex = 0;
            Tabs.Size = new Size(1150, 1070);
            Tabs.TabIndex = 0;
            // 
            // Hardware
            // 
            Hardware.Location = new Point(8, 46);
            Hardware.Name = "Hardware";
            Hardware.Padding = new Padding(3);
            Hardware.Size = new Size(1134, 1016);
            Hardware.TabIndex = 0;
            Hardware.Text = "Hardware view";
            Hardware.UseVisualStyleBackColor = true;
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
            // Editor
            // 
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
            AssemblyTextBox.DetectUrls = false;
            AssemblyTextBox.Font = new Font("Cascadia Code SemiBold", 10.875F, FontStyle.Bold, GraphicsUnit.Point, 0);
            AssemblyTextBox.Location = new Point(6, 58);
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
            // Settings
            // 
            Settings.Location = new Point(8, 46);
            Settings.Name = "Settings";
            Settings.Padding = new Padding(3);
            Settings.Size = new Size(1134, 1016);
            Settings.TabIndex = 3;
            Settings.Text = "Settings";
            Settings.UseVisualStyleBackColor = true;
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
            // Window
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1174, 1094);
            Controls.Add(Tabs);
            Name = "Window";
            Text = "AQA assembly Emulator";
            Load += Form1_Load;
            Tabs.ResumeLayout(false);
            Editor.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TabControl Tabs;
        private TabPage Hardware;
        private TabPage TraceTblTab;
        private TabPage Editor;
        private TabPage Settings;
        private SaveFileDialog SaveAssembly;
        private Button SaveButton;
        private RichTextBox AssemblyTextBox;
        private Button OpenButton;
        private OpenFileDialog OpenAssembly;
    }
}
