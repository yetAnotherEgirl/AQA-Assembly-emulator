using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AqaAssemEmulator_GUI.backend
{
    internal class GenericRegisters
    {
        private Register[] registers;
        public event EventHandler<MemoryErrorEventArgs> RegisterError;

        public int Count
        {
            get { return registers.Length; }
        }

        public GenericRegisters(int delayInMs, int numRegisters)
        {
            registers = new Register[numRegisters];
            for (int i = 0; i < numRegisters; i++)
            {
                registers[i] = new Register(delayInMs);
            }
            
        }

        public void SetRegister(int register, long val)
        {
            if(register < 0 || register >= Count)
            { 
                MemoryErrorEventArgs e = new($"invalid register: R{register}");
                OnRegisterError(e);
            }
            registers[register].SetRegister(val);
        }

        public long GetRegister(int register)
        {
            if (register < 0 || register >= Count)
            {
                MemoryErrorEventArgs e = new($"invalid register: R{register}");
                OnRegisterError(e);
                return -1;
            }
            return registers[register].GetRegister();
        }

        //account for the fact that the registers can be indexed by longs
        public long GetRegister(long register)
        {
            if (register < 0 || register >= Count)
            {
                MemoryErrorEventArgs e = new($"invalid register: R{register}");
                OnRegisterError(e);
                return -1;
            }
            return registers[register].GetRegister();
        }

        public void Reset()
        {
            for (int i = 0; i < Count; i++)
            {
                registers[i].Reset();
            }
        }

        //this returns a string array of the register values, this should only be called from
        //the CPU so the strings can be included in a larger dump file
        public string[] DumpRegisters()
        {
            string[] dump = new string[Count];
            for (int i = 0; i < Count; i++)
            {
                dump[i] = $"Register {i}: {registers[i].GetRegister()}";
            }
            return dump;
        }

        public void UpdateDelay(int delayInMs)
        {
            for (int i = 0; i < Count; i++)
            {
                registers[i].UpdateDelay(delayInMs);
            }
        }

        //raise the event for a memory error for the CPU to handle
        protected virtual void OnRegisterError(MemoryErrorEventArgs e)
        {
            RegisterError?.Invoke(this, e);
        }
    }
}
