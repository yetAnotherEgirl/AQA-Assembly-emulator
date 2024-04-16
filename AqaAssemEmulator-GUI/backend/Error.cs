﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AqaAssemEmulator_GUI.backend
{
    abstract class Error
    {
        public readonly string Message;
        public readonly bool IsFatal;
        protected Error(string message, bool isFatal)
        { 
            Message = message;
            IsFatal = isFatal;
        }

        public abstract new string ToString(); //all children must override this method as it is used to display the error message
    }

    internal class AssemblerError : Error
    {
        public readonly int LineNumber;

        //these are to be used as line numbers when there is no actual line number present
        public const int NoLineNumber = -1;
        public const int ErrorInIncludedFile = -2;

        public AssemblerError(string message, int lineNumber, bool isFatal = true) : base(message, isFatal)
        {
            LineNumber = lineNumber;
        }

        public override string ToString()
        {
            return Message;
        }
    }

    internal class EmulatorError : Error
    {
        public readonly int ProgramCounter;
        public EmulatorError(string message, int instructionNumber, bool isFatal = true) : base(message, isFatal)
        {
            ProgramCounter = instructionNumber;
        }

        public override string ToString()
        {
            return Message;
        }
    }
}
