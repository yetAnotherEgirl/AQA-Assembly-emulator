using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AqaAssemEmulator_GUI
{
    internal abstract class ErrorDisplay<T> : Form
    {
        protected Button OkButton;
        protected TextBox ErrorTextBox;
        
        protected bool IsFatal;
        protected Button IgnoreButton;

        public event EventHandler IgnoreButtonClicked;

        protected static List<T> Errors = [];

        protected ErrorDisplay()
        {
            OkButton = new Button();
            ErrorTextBox = new TextBox();
            IgnoreButton = new Button();
        }

        protected void InitializeComponent()
        {
            SuspendLayout();

            Size = new Size(600, 450);
            MaximizeBox = false;
            MinimizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            StartPosition = FormStartPosition.CenterScreen;
            MaximumSize = Size;
            MinimumSize = Size;

            OkButton.Location = new Point(419, 322);
            OkButton.Size = new Size(150, 50);
            OkButton.Text = "OK";
            OkButton.Click += OkButton_Click;

            IgnoreButton.Location = new Point(269 - 6, 322);
            IgnoreButton.Size = new Size(150, 50);
            IgnoreButton.Text = "Ignore";
            IgnoreButton.Click += IgnoreButton_Click;

            ErrorTextBox.Location = new Point(6, 6);
            ErrorTextBox.Size = new Size(562, 310);
            ErrorTextBox.Multiline = true;
            ErrorTextBox.ReadOnly = true;
            ErrorTextBox.BackColor = Color.White;
            ErrorTextBox.ScrollBars = ScrollBars.Vertical;

            this.FormClosing += OnClosing;

            Controls.Add(OkButton);
            Controls.Add(ErrorTextBox);

            // this line doesnt affect the code as we check this in the SetErrors method
            //x if(!IsFatal) Controls.Add(IgnoreButton);

            ResumeLayout(false);
        }

        public void SetErrors(List<T> errors)
        {
            SuspendLayout();

            Controls.Remove(IgnoreButton);

            Errors = errors;
            IsFatal = IsFailure();
            string[] errorText = GetErrors();
            ErrorTextBox.Clear();

            foreach (string error in errorText)
            {
                ErrorTextBox.AppendText(error + Environment.NewLine + Environment.NewLine);
            }
            if (!IsFatal) Controls.Add(IgnoreButton);

            ResumeLayout(false);
        }

        protected abstract bool IsFailure();

        protected abstract string[] GetErrors();

        protected void OkButton_Click(object? sender, EventArgs e)
        {
            Close();
        }

        protected void IgnoreButton_Click(object? sender, EventArgs e)
        {
            IgnoreButtonClicked?.Invoke(this, e);
            Close();
        }

        protected void OnClosing(object? sender, EventArgs e)
        {
            Errors.Clear();
           
            ErrorTextBox.Clear();
            this.Hide();
        }
    }
}
