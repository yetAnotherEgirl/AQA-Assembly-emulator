using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Error = AqaAssemEmulator_GUI.backend.AssemblerError;

namespace AqaAssemEmulator_GUI
{
    internal class AssemblerErrorDisplay : ErrorDisplay
    {
        static List<Error> Errors = [];
        

        public AssemblerErrorDisplay(List<Error> errors) 
        {
            Name = "Assembler Error";
            Text = "Assembler Error";
            Errors = errors;
            IsFatal = IsFailure();

            InitializeComponent();

            string[] errorText = GetErrors();

            foreach (string error in errorText)
            {
                ErrorTextBox.AppendText(error + Environment.NewLine + Environment.NewLine);
            }
        }

        static bool IsFailure()
        {
            bool failedToCompile = false;
            if (Errors.Count != 0)
            {
                foreach (Error error in Errors)
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

        static string[] GetErrors()
        {
            string[] errors = new string[Errors.Count];

            for(int i = 0; i < Errors.Count; i++)
            {
                Error error = Errors[i];
                string errorString = error.ToString();


                if (error.LineNumber == Error.ErrorInIncludedFile)
                {
                    errorString += ", error found in included file";
                }
                else if (error.LineNumber >= 0)
                {
                    errorString += $", occured on ln {error.LineNumber}";
                }
                
                if(!error.IsFatal)
                {
                    errorString += ", (none fatal)";
                }
                errorString += ".";
                errors[i] = errorString;
            }

            return errors;
        }

        public override void IgnoreButton_Click(object? sender, EventArgs e)
        {
            IgnoreErrors = true;
        }
    }
}
