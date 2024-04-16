using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AqaAssemEmulator_GUI.backend;

/* these classes are used to pass error messages from the emulator to the GUI.
 * 
 * EmulatorErrorEventArgs is used to pass a list of EmulatorError objects to the GUI, 
 * which can then be displayed to the user.
 * 
 * MemoryErrorEventArgs is used to pass a single error message from the emulated RAM
 * to the emulated CPU. the GUI should never be subscribed to this event as it is 
 * handled internally by the emulator and raised as an emulator error in an EmulatorErrorEventArgs event.
 */

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
