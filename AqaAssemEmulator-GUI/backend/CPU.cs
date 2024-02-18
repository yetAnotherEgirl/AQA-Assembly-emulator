using System.Windows.Forms;

namespace AqaAssemEmulator_GUI.backend;

internal class CPU
{
    public bool halted = true;

    int ProgramCounter = 0;

    long ALU = 0;

    CPSRFlags CPSR = CPSRFlags.None;
    Register memoryAddressRegister;
    Register memoryDataRegister;
    machineCodeLine instructionRegister;
    GenericRegisters registers;

    Memory RAM;

    int DelayInMs;

    List<EmulatorError> Errors = [];
    public event EventHandler<EmulatorErrorEventArgs> EmulatorErrorOccured;

    public CPU(ref Memory ram, int delayInMs, int registerCount)
    {
        RAM = ram;
        registers = new GenericRegisters(delayInMs, registerCount);
        DelayInMs = delayInMs;
        memoryAddressRegister = new Register(DelayInMs);
        memoryDataRegister = new Register(DelayInMs);
        instructionRegister = new machineCodeLine();

        RAM.InvalidMemoryAccess += Memory_InvalidMemoryAccess;
        RAM.possibleProgramOverwrite += Memory_PossibleProgramOverwrite;

        registers.RegisterError += Registers_RegisterError;
    }



    public void Fetch()
    {
        memoryAddressRegister.SetRegister(ProgramCounter);
        long x = RAM.QuereyAddress(memoryAddressRegister.GetRegister());
        memoryDataRegister.SetRegister(x);
        ProgramCounter++;
        if(ProgramCounter >= RAM.GetLength())
        {
            AddError("Program counter out of bounds, ignore reset to 0", false);
            ProgramCounter = 0;
            
        }
        Thread.Sleep(DelayInMs);
    }

    public void Decode()
    {
        instructionRegister = new machineCodeLine();
        instructionRegister.instruction = (int)(memoryDataRegister.GetRegister() >> Constants.opCodeOffset * Constants.bitsPerNibble);

        int registerValues = (int)(memoryDataRegister.GetRegister() >> Constants.registerOffset * Constants.bitsPerNibble);
        registerValues &= 0xFF;

        instructionRegister.arguments.Add(registerValues & 0x0F);
        instructionRegister.arguments.Add(registerValues & 0xF0);

        int signBit = (int)(memoryDataRegister.GetRegister() >> Constants.signBitOffset * Constants.bitsPerNibble) & 0xF;
        instructionRegister.AddressMode = signBit;

        long mask = (1L << Constants.signBitOffset * Constants.bitsPerNibble) - 1;
        instructionRegister.arguments.Add((int)(memoryDataRegister.GetRegister() & mask));
    }

    public void Execute()
    {
        switch (instructionRegister.instruction)
        {
            default:
                AddError("Invalid instruction");
                break;
            case 0: //label, do nothing
                break;
            case 1:
                LDR();
                break;
            case 2:
                STR();
                break;
            case 3:
                ADD();
                break;
            case 4:
                SUB();
                break;
            case 5:
                MOV();
                break;
            case 6:
                CMP();
                break;
            case 7:
                B();
                break;
            case 8:
                BEQ();
                break;
            case 9:
                BNE();
                break;
            case 10:
                BGT();
                break;
            case 11:
                BLT();
                break;
            case 12:
                AND();
                break;
            case 13:
                ORR();
                break;
            case 14:
                EOR();
                break;
            case 15:
                MVN();
                break;
            case 16:
                LSL();
                break;
            case 17:
                LSR();
                break;
            case 18:
                HALT();
                break;
            case 19:
                INPUT();
                break;
            case 20:
                OUTPUT();
                break;
            case 21:
                DUMP();
                break;
            case 22:
                JMP();
                break;
            case 23:
                CDP();
                break;

        }
    }

    #region standard instruction set
    void LDR()
    {
        if (!(instructionRegister.AddressMode == Constants.addressIndicator))
            AddError("CPU is not in address mode when reading address");
        memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
        
        memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
        registers.SetRegister(instructionRegister.arguments[1], memoryDataRegister.GetRegister());
        
            //registers[instructionRegister.arguments[1]].SetRegister(memoryDataRegister.GetRegister());
         
    }
    void STR()
    {
        if (!(instructionRegister.AddressMode == Constants.addressIndicator))
            AddError("CPU is not in address mode when writing to address");

        memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
        memoryDataRegister.SetRegister(registers.GetRegister(instructionRegister.arguments[0]));
        RAM.SetAddress(memoryAddressRegister.GetRegister(), memoryDataRegister.GetRegister());
    }
    void ADD()
    {
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {

            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));

        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));

        }
        else
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        ALU = memoryDataRegister.GetRegister();
        memoryDataRegister.SetRegister(registers.GetRegister(instructionRegister.arguments[1]));
        ALU += memoryDataRegister.GetRegister();
        
        registers.SetRegister(instructionRegister.arguments[0], ALU);

    }
    void SUB()
    {
        memoryAddressRegister.SetRegister(instructionRegister.arguments[1]);
        memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));

        ALU = memoryDataRegister.GetRegister();
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));

        }
        else
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        ALU -= memoryDataRegister.GetRegister();
        registers.SetRegister(instructionRegister.arguments[0], ALU);

    }
    void MOV()
    {
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
            AddError("MOV command used like LDR, consider using LDR instead", false);
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));
        }
        else
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        registers.SetRegister(instructionRegister.arguments[0], memoryDataRegister.GetRegister());
    }
    void CMP()
    {
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));
        }
        else
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        ALU = memoryDataRegister.GetRegister();
        memoryAddressRegister.SetRegister(instructionRegister.arguments[1]);
        memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));
        try
        {
            ALU -= memoryDataRegister.GetRegister();
        }
        catch (OverflowException)
        {
            CPSR = CPSRFlags.Overflow;
            ALU += 1 << 32;
            ALU -= memoryDataRegister.GetRegister();
        }
        if (ALU == 0) CPSR = CPSRFlags.Zero;
        if (ALU < 0) CPSR = CPSRFlags.Negative;
    }
    void B()
    {
        ProgramCounter = instructionRegister.arguments[2];
    }
    void BEQ()
    {
        if (CPSR == CPSRFlags.Zero) ProgramCounter = instructionRegister.arguments[2];
    }
    void BNE()
    {
        if (CPSR != CPSRFlags.Zero) ProgramCounter = instructionRegister.arguments[2];
    }
    void BGT()
    {
        if (CPSR == CPSRFlags.Negative) ProgramCounter = instructionRegister.arguments[2];
    }
    void BLT()
    {
        if (CPSR != CPSRFlags.Negative && CPSR != CPSRFlags.Zero) ProgramCounter = instructionRegister.arguments[2];
    }
    void AND()
    {
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));
        }
        else
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        ALU = memoryDataRegister.GetRegister();
        memoryDataRegister.SetRegister(registers.GetRegister(instructionRegister.arguments[1]));
        ALU &= memoryDataRegister.GetRegister();

        registers.SetRegister(instructionRegister.arguments[0], ALU);
    }
    void ORR()
    {
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));
        }
        else
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        ALU = memoryDataRegister.GetRegister();
        memoryDataRegister.SetRegister(registers.GetRegister(instructionRegister.arguments[1]));
        ALU |= memoryDataRegister.GetRegister();

        registers.SetRegister(instructionRegister.arguments[0], ALU);
    }
    void EOR()
    {
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));
        }
        else
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        ALU = memoryDataRegister.GetRegister();
        memoryDataRegister.SetRegister(registers.GetRegister(instructionRegister.arguments[1]));
        ALU ^= memoryDataRegister.GetRegister();

        registers.SetRegister(instructionRegister.arguments[0], ALU);
    }
    void MVN()
    {
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));
        }
        else
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        ALU = ~memoryDataRegister.GetRegister();
        registers.SetRegister(instructionRegister.arguments[0], ALU);
    }
    void LSL()
    {
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));
        }
        else
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        ALU = memoryDataRegister.GetRegister();
        memoryDataRegister.SetRegister(registers.GetRegister(instructionRegister.arguments[1]));
        ALU = (int)ALU << (int)memoryDataRegister.GetRegister();

        registers.SetRegister(instructionRegister.arguments[0], ALU);
    }
    void LSR()
    {
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));
        }
        else
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        ALU = memoryDataRegister.GetRegister();
        memoryDataRegister.SetRegister(registers.GetRegister(instructionRegister.arguments[1]));
        ALU = (int)ALU >> (int)memoryDataRegister.GetRegister();

        registers.SetRegister(instructionRegister.arguments[0], ALU);
    }
    void HALT()
    {
        halted = true;
    }
    #endregion standard instruction set

    #region extended instruction set
    void INPUT()
    {
        int input = InputDialog.GetInput($"Enter a value for register {instructionRegister.arguments[0]}");
        registers.SetRegister(instructionRegister.arguments[0], Convert.ToInt32(input));
    }
    void OUTPUT()
    {
        MessageBox.Show($"Register {instructionRegister.arguments[0]} contains " +
                        $"#{registers.GetRegister(instructionRegister.arguments[0])}",
                                   "Output",
                                   MessageBoxButtons.OK
                                   );
       }
    void DUMP()
    {
        switch (instructionRegister.arguments[2])
        {
            case 0:
                RAM.DumpMemory("memory");
                break;
            case 1:
                DumpRegisters("registers");
                break;
            case 2:
                DumpRegisters("registers");
                RAM.DumpMemory("memory");
                break;
            default:
                AddError("Invalid dump type");
                break;
        }
    }
    
    void JMP()
    {
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[1]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
            ProgramCounter = (int)memoryDataRegister.GetRegister();
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            ProgramCounter = (int)registers.GetRegister(instructionRegister.arguments[1]);
        }
        else
        {
            ProgramCounter = instructionRegister.arguments[1];
        }
    }

    void CDP()
    {
        registers.SetRegister(instructionRegister.arguments[0], ProgramCounter); 
    }
    #endregion extended instruction set

    public void Reset()
    {
        halted = true;
        ProgramCounter = 0;
        ALU = 0;
        CPSR = CPSRFlags.None;
        memoryAddressRegister.Reset();
        memoryDataRegister.Reset();
        registers.Reset();
    }

    public void DumpRegisters(string fileName, string DumpPath = "dumps")
    {
        string[] memoryDump = new string[registers.Count + 5];

        string[] registerDump = registers.DumpRegisters();

        for (int i = 0; i < registers.Count; i++)
        {
            memoryDump[i] = registerDump[i];
        }
        
        memoryDump[registers.Count] = $" PC: {ProgramCounter}";
        memoryDump[registers.Count + 1] = $" ALU: {ALU}";
        memoryDump[registers.Count + 2] = $" MAR: {memoryAddressRegister.DumpRegister()}";
        memoryDump[registers.Count + 3] = $" MDR: {memoryDataRegister.DumpRegister()}";
        memoryDump[registers.Count + 4] = $" CPSR: {CPSR}";

        Directory.CreateDirectory($"./{DumpPath}");
        fileName = DumpPath + "/" + fileName + ".Dump";
        File.WriteAllLines(fileName, memoryDump);
    }

    #region getters
    public int GetProgramCounter()
    {
        return ProgramCounter; 
    }

    public long GetALU()
    {
        return ALU;
    }

    public int GetDelayInMs()
    {
        return DelayInMs;
    }

    public CPSRFlags GetCPSR()
    {
        return CPSR;
    }

    public long GetMemoryAddressRegister()
    {
        return memoryAddressRegister.GetRegister();
    }

    public long GetMemoryDataRegister()
    {
        return memoryDataRegister.GetRegister();
    }

    public long GetRegister(int register)
    {
        return registers.GetRegister(register);
    }

    public int GetRegisterCount()
    {
        return registers.Count;
    }

    public List<EmulatorError> GetErrors()
    {
        return Errors;
    }

    #endregion getters

    public void UpdateDelay(int delayInMs)
    {
        DelayInMs = delayInMs;
        registers.UpdateDelay(delayInMs);
        memoryAddressRegister.UpdateDelay(delayInMs);
        memoryDataRegister.UpdateDelay(delayInMs);
    }

    void AddError(string message, bool isFatal = true)
    {
        Errors.Add(new EmulatorError(message, ProgramCounter, isFatal));
        if (isFatal) halted = true;
        EmulatorErrorEventArgs e = new(Errors);
        OnEmulatorErrorOccured(e);
    }

    private void Memory_InvalidMemoryAccess(object? sender, MemoryErrorEventArgs e)
    {
        AddError(e.ErrorMessage);
    }

    private void Registers_RegisterError(object? sender, MemoryErrorEventArgs e)
    {
        AddError(e.ErrorMessage);
    }

    private void Memory_PossibleProgramOverwrite(object? sender, MemoryErrorEventArgs e)
    {
        AddError(e.ErrorMessage, false);
    }

    protected virtual void OnEmulatorErrorOccured(EmulatorErrorEventArgs e)
    {
        EmulatorErrorOccured?.Invoke(this, e);
    }
}