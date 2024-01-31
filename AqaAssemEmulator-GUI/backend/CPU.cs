using System.Windows.Forms;

namespace AqaAssemEmulator_GUI.backend;

internal class CPU
{
    public bool halted = true;

    int ProgramCounter = 0;

    long ALU = 0;

    CPSRFlags CPSR = CPSRFlags.None;
    Register MemoryAddressRegister;
    Register memoryDataRegister;
    machineCodeLine instructionRegister;
    Register[] registers;

    Memory RAM;
    int DelayInMs;

    public CPU(ref Memory ram, int delayInMs, int registerCount)
    {
        RAM = ram;
        registers = new Register[registerCount];
        DelayInMs = delayInMs;
        for (int i = 0; i < registers.Length; i++)
        {
            registers[i] = new Register(DelayInMs);
        }
        MemoryAddressRegister = new Register(DelayInMs);
        memoryDataRegister = new Register(DelayInMs);
        instructionRegister = new machineCodeLine();
    }



    public void Fetch()
    {
        MemoryAddressRegister.SetRegister(ProgramCounter);
        long x = RAM.QuereyAddress(MemoryAddressRegister.GetRegister());
        memoryDataRegister.SetRegister(x);
        ProgramCounter++;
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
                throw new ArgumentException("invalid instruction passed to execute");
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
            throw new ArgumentException("CPU is not in address mode when reading address");
        MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
        memoryDataRegister.SetRegister(RAM.QuereyAddress(MemoryAddressRegister.GetRegister()));
        registers[instructionRegister.arguments[1]].SetRegister(memoryDataRegister.GetRegister());
    }
    void STR()
    {
        if (!(instructionRegister.AddressMode == Constants.addressIndicator))
            throw new ArgumentException("CPU is not in address mode when writing address");

        MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
        memoryDataRegister.SetRegister(registers[instructionRegister.arguments[0]].GetRegister());
        RAM.SetAddress(MemoryAddressRegister.GetRegister(), memoryDataRegister.GetRegister());
    }
    void ADD()
    {
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(MemoryAddressRegister.GetRegister()));
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers[MemoryAddressRegister.GetRegister()].GetRegister());
        }
        else
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        ALU = memoryDataRegister.GetRegister();
        memoryDataRegister.SetRegister(registers[instructionRegister.arguments[1]].GetRegister());
        ALU += memoryDataRegister.GetRegister();
        registers[instructionRegister.arguments[0]].SetRegister(ALU);
    }
    void SUB()
    {
        MemoryAddressRegister.SetRegister(instructionRegister.arguments[1]);
        memoryDataRegister.SetRegister(registers[MemoryAddressRegister.GetRegister()].GetRegister());
        ALU = memoryDataRegister.GetRegister();
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(MemoryAddressRegister.GetRegister()));
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers[MemoryAddressRegister.GetRegister()].GetRegister());
        }
        else
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        ALU -= memoryDataRegister.GetRegister();
        registers[instructionRegister.arguments[0]].SetRegister(ALU);
    }
    void MOV()
    {
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(MemoryAddressRegister.GetRegister()));
            Console.WriteLine("MOV command used like LDR, consider using LDR instead");
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers[MemoryAddressRegister.GetRegister()].GetRegister());
        }
        else
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        registers[instructionRegister.arguments[0]].SetRegister(memoryDataRegister.GetRegister());
    }
    void CMP()
    {
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(MemoryAddressRegister.GetRegister()));
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers[MemoryAddressRegister.GetRegister()].GetRegister());
        }
        else
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        ALU = memoryDataRegister.GetRegister();
        MemoryAddressRegister.SetRegister(instructionRegister.arguments[1]);
        memoryDataRegister.SetRegister(registers[MemoryAddressRegister.GetRegister()].GetRegister());
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
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(MemoryAddressRegister.GetRegister()));
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers[MemoryAddressRegister.GetRegister()].GetRegister());
        }
        else
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        ALU = memoryDataRegister.GetRegister();
        memoryDataRegister.SetRegister(registers[instructionRegister.arguments[1]].GetRegister());
        ALU &= memoryDataRegister.GetRegister();

        registers[instructionRegister.arguments[0]].SetRegister(ALU);
    }
    void ORR()
    {
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(MemoryAddressRegister.GetRegister()));
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers[MemoryAddressRegister.GetRegister()].GetRegister());
        }
        else
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        ALU = memoryDataRegister.GetRegister();
        memoryDataRegister.SetRegister(registers[instructionRegister.arguments[1]].GetRegister());
        ALU |= memoryDataRegister.GetRegister();

        registers[instructionRegister.arguments[0]].SetRegister(ALU);
    }
    void EOR()
    {
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(MemoryAddressRegister.GetRegister()));
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers[MemoryAddressRegister.GetRegister()].GetRegister());
        }
        else
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        ALU = memoryDataRegister.GetRegister();
        memoryDataRegister.SetRegister(registers[instructionRegister.arguments[1]].GetRegister());
        ALU ^= memoryDataRegister.GetRegister();

        registers[instructionRegister.arguments[0]].SetRegister(ALU);
    }
    void MVN()
    {
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(MemoryAddressRegister.GetRegister()));
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers[MemoryAddressRegister.GetRegister()].GetRegister());
        }
        else
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        ALU = ~memoryDataRegister.GetRegister();
        registers[instructionRegister.arguments[0]].SetRegister(ALU);
    }
    void LSL()
    {
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(MemoryAddressRegister.GetRegister()));
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers[MemoryAddressRegister.GetRegister()].GetRegister());
        }
        else
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        ALU = memoryDataRegister.GetRegister();
        memoryDataRegister.SetRegister(registers[instructionRegister.arguments[1]].GetRegister());
        ALU = (int)ALU << (int)memoryDataRegister.GetRegister();

        registers[instructionRegister.arguments[0]].SetRegister(ALU);
    }
    void LSR()
    {
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(MemoryAddressRegister.GetRegister()));
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers[MemoryAddressRegister.GetRegister()].GetRegister());
        }
        else
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        ALU = memoryDataRegister.GetRegister();
        memoryDataRegister.SetRegister(registers[instructionRegister.arguments[1]].GetRegister());
        ALU = (int)ALU >> (int)memoryDataRegister.GetRegister();

        registers[instructionRegister.arguments[0]].SetRegister(ALU);
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
        registers[instructionRegister.arguments[0]].SetRegister(Convert.ToInt32(input));
    }
    void OUTPUT()
    {
        MessageBox.Show($"Register {instructionRegister.arguments[0]} contains " +
                        $"#{registers[instructionRegister.arguments[0]].GetRegister()}",
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
                throw new ArgumentException("invalid dump command");
        }
    }
    
    void JMP()
    {
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            MemoryAddressRegister.SetRegister(instructionRegister.arguments[1]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(MemoryAddressRegister.GetRegister()));
            ProgramCounter = (int)memoryDataRegister.GetRegister();
        }
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            ProgramCounter = (int)registers[instructionRegister.arguments[1]].GetRegister();
        }
        else
        {
            ProgramCounter = instructionRegister.arguments[1];
        }
    }

    void CDP()
    {
        registers[instructionRegister.arguments[0]].SetRegister(ProgramCounter); 
    }
    #endregion extended instruction set

    public void Reset()
    {
        halted = true;
        ProgramCounter = 0;
        ALU = 0;
        CPSR = CPSRFlags.None;
        MemoryAddressRegister.Reset();
        memoryDataRegister.Reset();
        for (int i = 0; i < registers.Length; i++)
        {
            registers[i].Reset();
        }
    }

    public void DumpRegisters(string fileName, string DumpPath = "dumps")
    {
        string[] memoryDump = new string[registers.Length + 5];

        for (int i = 0; i < registers.Length; i++)
        {
            memoryDump[i] = $" register {i}: {registers[i].DumpRegister()}";
        }
        memoryDump[registers.Length] = $" PC: {ProgramCounter}";
        memoryDump[registers.Length + 1] = $" ALU: {ALU}";
        memoryDump[registers.Length + 2] = $" MAR: {MemoryAddressRegister.DumpRegister()}";
        memoryDump[registers.Length + 3] = $" MDR: {memoryDataRegister.DumpRegister()}";
        memoryDump[registers.Length + 4] = $" CPSR: {CPSR}";

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
        return MemoryAddressRegister.GetRegister();
    }

    public long GetMemoryDataRegister()
    {
        return memoryDataRegister.GetRegister();
    }

    public long GetRegister(int register)
    {
        return registers[register].GetRegister();
    }

    public int GetRegisterCount()
    {
        return registers.Length;
    }

    #endregion getters

    public void UpdateDelay(int delayInMs)
    {
        DelayInMs = delayInMs;
        for (int i = 0; i < registers.Length; i++)
        {
            registers[i].UpdateDelay(delayInMs);
        }
        MemoryAddressRegister.UpdateDelay(delayInMs);
        memoryDataRegister.UpdateDelay(delayInMs);
    }
}