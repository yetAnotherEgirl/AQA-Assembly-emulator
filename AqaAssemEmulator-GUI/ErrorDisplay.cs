using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AqaAssemEmulator_GUI
{
    internal abstract class ErrorDisplay<T> : Form
    {
        /* this is an abstract forms class used to display errors of type T to the user
         * it defines the basic layout of the form and the basic functionality of the buttons
         */

        protected Button OkButton;
        protected TextBox ErrorTextBox;

        
        protected bool IsFatal;
        protected Button IgnoreButton;

        public event EventHandler IgnoreButtonClicked;
        public event EventHandler OkButtonClicked;

        protected static List<T> Errors = [];

        protected ErrorDisplay()
        {
            OkButton = new Button();
            ErrorTextBox = new TextBox();
            IgnoreButton = new Button();
        }

        //this is protected so that the children classes can call it in their constructors
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

        //these 2 methods are abstract so that the children classes can implement them, 
        //as the errors are stored in different ways in each class
        protected abstract bool IsFailure();

        protected abstract string[] GetErrors();

        protected void OkButton_Click(object? sender, EventArgs e)
        {
            OkButtonClicked?.Invoke(this, e);
            Close(); //this should also invoke the OnClosing method
        }

        protected void IgnoreButton_Click(object? sender, EventArgs e)
        {
            IgnoreButtonClicked?.Invoke(this, e);
            Close(); //this should also invoke the OnClosing method
        }

        //this is to prevent the form the form from being disposed when the user closes it
        //as it is reused, instead it is hidden
        protected void OnClosing(object? sender, EventArgs e)
        {
            Errors.Clear();
           
            ErrorTextBox.Clear();
            Hide();
        }
    }
}
