using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AqaAssemEmulator_GUI
{
    internal abstract class ErrorDisplay : Form
    {
        protected Button OkButton;
        protected TextBox ErrorTextBox;
        
        protected bool IsFatal;
        protected Button IgnoreButton;

        public bool IgnoreErrors { 
            get;
            protected set;
        }

        protected ErrorDisplay()
        {
            OkButton = new Button();
            ErrorTextBox = new TextBox();
            IgnoreButton = new Button();
        }

        protected void InitializeComponent()
        {
            //ToDo these sizes are all fucked up
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

            Controls.Add(OkButton);
            Controls.Add(ErrorTextBox);

            if(!IsFatal) Controls.Add(IgnoreButton);

            ResumeLayout(false);
        }

        protected void OkButton_Click(object? sender, EventArgs e)
        {
            Close();
        }

        public abstract void IgnoreButton_Click(object? sender, EventArgs e);
    }
}
