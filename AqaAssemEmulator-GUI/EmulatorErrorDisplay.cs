using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Error = AqaAssemEmulator_GUI.backend.EmulatorError;

namespace AqaAssemEmulator_GUI
{
    internal class EmulatorErrorDisplay : ErrorDisplay<Error>
    {
        public EmulatorErrorDisplay()
        {
            Name = "Emulator Error";
            Text = "Emulator Error";

            InitializeComponent();
        }

        override protected bool IsFailure()
        {
            bool CausedCrash = false; //causedCrash means that the error is fatal
            if (Errors.Count != 0)
            {
                foreach (Error error in Errors)
                {
                    if (error.IsFatal)
                    {
                        CausedCrash = true;
                        break;
                    }
                }
            }
            return CausedCrash;
        }

        override protected string[] GetErrors()
        {
            string[] errors = new string[Errors.Count];

            for (int i = 0; i < Errors.Count; i++)
            {
                Error error = Errors[i];
                string errorString = error.ToString();

                if (error.ProgramCounter >= 0)
                {
                    errorString += $", occured at program counter: {error.ProgramCounter}";
                }

                if (!error.IsFatal)
                {
                    errorString += ", (none fatal)";
                }

                errors[i] = errorString + ".";
            }

            return errors;
        }
    }
}
