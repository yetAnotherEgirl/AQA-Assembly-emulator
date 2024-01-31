using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AqaAssemEmulator_GUI
{
    public partial class InputDialog : Form
    {
        string InputText = "";
        Label InputTextLabel;
        RichTextBox InputTextBox;
        Button OkButton;
        int Input;

        bool InputValid = false;
        
        public InputDialog(string inputText)
        {
            InitializeComponent();

            this.SuspendLayout();
            this.Size = new Size(500, 300);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = this.Size;
            this.MaximumSize = this.Size;
            this.Text = "INPUT";
            this.FormClosing += InputDialog_FormClosing;

            InputTextLabel = new Label();
            InputTextLabel.Text = inputText;
            InputTextLabel.Location = new Point(20, 20);
            InputTextLabel.Size = new Size(440, 60);
            Controls.Add(InputTextLabel);

            InputTextBox = new RichTextBox();
            InputTextBox.Location = new Point(20, 80);
            InputTextBox.Size = new Size(440, 60);
            InputTextBox.KeyDown += InputTextBox_KeyDown;
            Controls.Add(InputTextBox);

            OkButton = new Button();
            OkButton.Text = "OK";
            OkButton.Location = new Point(400, 160);
            OkButton.Size = new Size(60, 60);
            OkButton.Click += OkButton_Click;
            Controls.Add(OkButton);

            InputText = inputText;
            this.ResumeLayout(false);
        }

        private void InputDialog_Load(object sender, EventArgs e)
        {
            
        }

        public static int GetInput(string inputText)
        {
            InputDialog inputDialog = new InputDialog(inputText);
            inputDialog.ShowDialog();
            return inputDialog.Input;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            try
            {
                Input = int.Parse(InputTextBox.Text);
                InputValid = true;
                this.Close();
            }
            catch (Exception)
            {
                Task.Run(() => WrongDialogEntered());
            }
        }

        private void InputDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!InputValid)
            {
                e.Cancel = true;
                WrongDialogEntered();
                return;
            }
        }

        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                OkButton_Click(sender, e);
            }
        }

        private async void WrongDialogEntered()
        {
            InputTextLabel.Text = "Please enter an Integer!";
            await Task.Delay(2000);
            InputTextLabel.Text = InputText;
        }
    }
}
