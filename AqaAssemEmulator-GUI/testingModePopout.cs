using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AqaAssemEmulator_GUI
{
    internal class TestingModePopout : Form
    {
        readonly RichTextBox assembly = new();

        public TestingModePopout()
        {

            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            AutoScaleDimensions = new SizeF(6F, 13F);
            AutoScaleMode = AutoScaleMode.Font;
            Name = "TestingModePopout";
            Text = "Assembly Code";
            Size = new Size(400, 450);
            FormClosing += TestingModePopout_FormClosing;


            assembly.Location = new Point(0, 0);
            assembly.Size = new Size(400, 450);
            assembly.BorderStyle = BorderStyle.FixedSingle;
            assembly.Cursor = Cursors.IBeam;
            assembly.Font = new Font("Cascadia Code SemiBold", 10.875F, FontStyle.Bold, GraphicsUnit.Point, 0);
            assembly.ReadOnly = true;
            assembly.Multiline = true;

            Controls.Add(assembly);
            ResumeLayout(false);
        }

        //dont let the user close the form by clicking the x button as it will dispose the form and we need to keep it alive
        private void TestingModePopout_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;

                Hide();
            }
        }

        public void UpdateAssembly(string[] assemblyCode)
        {
            string assemblystring = string.Join("\n", assemblyCode);
            assembly.Text = assemblystring;
        }


    }
}
