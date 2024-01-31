using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AqaAssemEmulator_GUI.backend
{
    internal class AssemblerError
    {
        public readonly string Message;
        public readonly int LineNumber;
        public readonly bool IsFatal;

        public const int NoLineNumber = -1;
        public const int ErrorInIncludedFile = -2;

        public AssemblerError(string message, int lineNumber, bool isFatal = true)
        {
            Message = message;
            LineNumber = lineNumber;
            IsFatal = isFatal;
        }
    }
}
