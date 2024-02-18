using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AqaAssemEmulator_GUI.backend;


internal class EmulatorErrorEventArgs : EventArgs
{
    public List<EmulatorError> Errors { get; }

    public EmulatorErrorEventArgs(List<EmulatorError> errors)
    {
        Errors = errors;
    }
}

internal class MemoryErrorEventArgs : EventArgs
{
    public string ErrorMessage { get; }

    public MemoryErrorEventArgs(string errorMessage)
    {
        ErrorMessage = errorMessage; 
    }
}
