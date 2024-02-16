using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Error = AqaAssemEmulator_GUI.backend.AssemblerError;

namespace AqaAssemEmulator_GUI
{
    internal class AssemblerErrorDisplay : ErrorDisplay<Error>
    { 
        

        public AssemblerErrorDisplay() 
        {
            Name = "Assembler Error";
            Text = "Assembler Error";

            InitializeComponent();
        }

        override protected bool IsFailure()
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

       override protected string[] GetErrors()
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
    }
}
