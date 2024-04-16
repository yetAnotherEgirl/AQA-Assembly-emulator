using System.Windows.Forms;

namespace AqaAssemEmulator_GUI.backend;

internal class CPU
{
    public bool halted = true;

    #region Registers
    int ProgramCounter = 0;

    long Accumulator = 0;

    CPSRFlags CPSR = CPSRFlags.None;
    Register memoryAddressRegister;
    Register memoryDataRegister;
    MachineCodeLine instructionRegister;
    GenericRegisters registers;
    #endregion Registers

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
        instructionRegister = new MachineCodeLine();

        RAM.InvalidMemoryAccess += Memory_InvalidMemoryAccess;
        RAM.PossibleProgramOverwrite += Memory_PossibleProgramOverwrite;

        registers.RegisterError += Registers_RegisterError;
    }


    //PC -> MAR -> RAM -> MDR
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

    //MDR -> IR
    public void Decode()
    {
        instructionRegister = new();
        //extract the instruction from the memory data register by shifting the data to the right by the offset of the opcode
        instructionRegister.instruction = (int)(memoryDataRegister.GetRegister() >> Constants.opCodeOffset * Constants.bitsPerNibble);

        //extract the register values from the memory data register by shifting the data to the right by the offset of the register values,
        //then masking the data to get 2 nibbles storing the register values
        int registerValues = (int)(memoryDataRegister.GetRegister() >> Constants.registerOffset * Constants.bitsPerNibble);
        registerValues &= 0xFF;

        //apply 2 more masks to get the individual register values (while shifting the data to the right by the offset of the register values)
        instructionRegister.arguments.Add(registerValues & 0x0F);
        instructionRegister.arguments.Add((registerValues & 0xF0) >> 1 * Constants.bitsPerNibble);

        //extract the address mode from the memory data register by shifting the data to the right by the offset of the address mode,
        //then masking the data to get 1 nibble storing the address mode
        int signBit = (int)(memoryDataRegister.GetRegister() >> Constants.signBitOffset * Constants.bitsPerNibble) & 0xF;
        instructionRegister.AddressMode = signBit;

        //create a mask to get the operand from the IR, this mask is created by shifting 1 to the left by the offset of the sign bit
        //then subtracting 1 from the result. for example, if the sign bit offset is 4 the following calculation is done:
        //mask = 1b2 << 4 = 10000b2
        //mask = mask - 1 = 1111b2
        //if we apply this mask to the IR we will get the operand as it will be the data that is on the right of the sign bit
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
    /* in the following sections the flow of data is written as comments in the following format:
     * "=>" indicates that the data is being moved from the left to the right
     * "@" indicates the index of the register / memory address that data is being moved to / read from
     *    (e.g. MDR => Rn @ IR means the MDR is being stored
     *    in a register whos index is stored in the IR)
     * "+>" indicates that the data is being added to the right
     * "->" indicates that the data is being subtracted from the right
     * "&>" indicates that the data is being anded with the right
     * "|>" indicates that the data is being ored with the right
     * "^>" indicates that the data is being xored with the right
     * "!>" indicates that the data is being notted with the right
     * "//>" indicates that the data on the right is being shifted to the right by the data on the left
     * "\\>" indicates that the data on the right is being shifted to the left by the data on the left
     * "..." indicates that the data is being passed to the next step
     */

    //IR => MAR, RAM @ MAR => MDR => Rn @ IR
    void LDR()
    {
        if (!(instructionRegister.AddressMode == Constants.addressIndicator))
            AddError("CPU is not in address mode when reading address");
        memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);

        memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
        registers.SetRegister(instructionRegister.arguments[1], memoryDataRegister.GetRegister());  
    }

    //IR => MAR, Rn @ IR => MDR => RAM @ MAR
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
        //IR => MAR, RAM @ MAR => MDR ...
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {

            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));

        }
        //IR => MAR, Rn @ MAR => MDR ...
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));

        }
        //IR => MDR ...
        else //this will be a decimal value
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        //... => ACC, Rn @ IR => MAR +> ACC => Rn @ IR
        Accumulator = memoryDataRegister.GetRegister();
        memoryDataRegister.SetRegister(registers.GetRegister(instructionRegister.arguments[1]));
        Accumulator += memoryDataRegister.GetRegister();
        
        registers.SetRegister(instructionRegister.arguments[0], Accumulator);

    }

    //IR => MAR, RAM @ MAR => MDR => ACC, ...
    void SUB()
    {
        memoryAddressRegister.SetRegister(instructionRegister.arguments[1]);
        memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));

        Accumulator = memoryDataRegister.GetRegister();

        //IR => MAR, RAM @ MAR => MDR ...
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
        }
        //IR => MAR, Rn @ MAR => MDR ...
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));

        }
        //IR => MDR ...
        else //this will be a decimal value
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }

        //... -> ACC => Rn @ IR
        Accumulator -= memoryDataRegister.GetRegister();
        registers.SetRegister(instructionRegister.arguments[0], Accumulator);

    }

    void MOV()
    {
        //IR => MAR, RAM @ MAR => MDR ...
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
            AddError("MOV command used like LDR, consider using LDR instead", false); //warn user as this operation is slower than LDR
        }
        //IR => MAR, Rn @ MAR => MDR ...
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));
        }
        //IR => MDR ...
        else //this will be a decimal value
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        //... => Rn @ IR
        registers.SetRegister(instructionRegister.arguments[0], memoryDataRegister.GetRegister());
    }
    void CMP()
    {
        //IR => MAR, RAM @ MAR => MDR ...
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
        }
        //IR => MAR, Rn @ MAR => MDR ...
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));
        }
        //IR => MDR ...
        else //this will be a decimal value
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }

        //... => ACC, Rn @ IR => MAR, Rn @ MAR => MDR -> ACC => CPSR

        //worded description of the operation:
        //the 2 values are loaded into the accumulator and the memory data register
        //the accumulator is then subtracted from the memory data register and one of the following flags is set:
        //if the result is 0 the zero flag is set
        //if the result is negative the negative flag is set
        //if the result overflows the overflow flag is set

        Accumulator = memoryDataRegister.GetRegister();
        memoryAddressRegister.SetRegister(instructionRegister.arguments[0]);
        memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));
        try
        {
            Accumulator -= memoryDataRegister.GetRegister();
        }
        catch (OverflowException)
        {
            CPSR = CPSRFlags.Overflow;
            Accumulator += 1 << 32;
            Accumulator -= memoryDataRegister.GetRegister();
        }
        if (Accumulator == 0) CPSR = CPSRFlags.Zero;
        if (Accumulator < 0) CPSR = CPSRFlags.Negative;
    }

    //IR => PC
    void B()
    {
        ProgramCounter = instructionRegister.arguments[2];
    }

    //IR => PC (if CPSR == Zero)
    void BEQ()
    {
        if (CPSR == CPSRFlags.Zero) ProgramCounter = instructionRegister.arguments[2];
    }

    //IR => PC (if CPSR != Zero)
    void BNE()
    {
        if (CPSR != CPSRFlags.Zero) ProgramCounter = instructionRegister.arguments[2];
    }

    //IR => PC (if CPSR == Negative)
    void BGT()
    {
        if (CPSR == CPSRFlags.Negative) ProgramCounter = instructionRegister.arguments[2];
    }

    //IR => PC (if CPSR has no flags)
    void BLT()
    {
        if (CPSR != CPSRFlags.Negative && CPSR != CPSRFlags.Zero) ProgramCounter = instructionRegister.arguments[2];
    }

    void AND()
    {
        //IR => MAR, RAM @ MAR => MDR ...
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
        }
        //IR => MAR, Rn @ MAR => MDR ...
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));
        }
        //IR => MDR ...
        else //this will be a decimal value
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        //... => ACC, Rn @ IR => MAR &> ACC => Rn @ IR
        Accumulator = memoryDataRegister.GetRegister();
        memoryDataRegister.SetRegister(registers.GetRegister(instructionRegister.arguments[1]));
        Accumulator &= memoryDataRegister.GetRegister();

        registers.SetRegister(instructionRegister.arguments[0], Accumulator);
    }
    void ORR()
    {
        //IR => MAR, RAM @ MAR => MDR ...
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
        }
        //IR => MAR, Rn @ MAR => MDR ...
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));
        }
        //IR => MDR ...
        else //this will be a decimal value
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        //... => ACC, Rn @ IR => MAR |> ACC => Rn @ IR
        Accumulator = memoryDataRegister.GetRegister();
        memoryDataRegister.SetRegister(registers.GetRegister(instructionRegister.arguments[1]));
        Accumulator |= memoryDataRegister.GetRegister();

        registers.SetRegister(instructionRegister.arguments[0], Accumulator);
    }
    void EOR()
    {
        //IR => MAR, RAM @ MAR => MDR ...
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
        }
        //IR => MAR, Rn @ MAR => MDR ...
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));
        }
        //IR => MDR ...
        else //this will be a decimal value
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        //... => ACC, Rn @ IR => MAR ^> ACC => Rn @ IR
        Accumulator = memoryDataRegister.GetRegister();
        memoryDataRegister.SetRegister(registers.GetRegister(instructionRegister.arguments[1]));
        Accumulator ^= memoryDataRegister.GetRegister();

        registers.SetRegister(instructionRegister.arguments[0], Accumulator);
    }
    void MVN()
    {
        //IR => MAR, RAM @ MAR => MDR ...
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
        }
        //IR => MAR, Rn @ MAR => MDR ...
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));
        }
        //IR => MDR ...
        else //this will be a decimal value
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        //... !> ACC => Rn @ IR
        Accumulator = ~memoryDataRegister.GetRegister();
        registers.SetRegister(instructionRegister.arguments[0], Accumulator);
    }
    void LSL()
    {
        //IR => MAR, RAM @ MAR => MDR ...
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
        }
        //IR => MAR, Rn @ MAR => MDR ...
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));
        }
        //IR => MDR ...
        else //this will be a decimal value
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }
        //... => ACC, Rn @ IR => MAR \\> ACC => Rn @ IR
        Accumulator = memoryDataRegister.GetRegister();
        memoryDataRegister.SetRegister(registers.GetRegister(instructionRegister.arguments[1]));
        Accumulator = (int)Accumulator << (int)memoryDataRegister.GetRegister();

        registers.SetRegister(instructionRegister.arguments[0], Accumulator);
    }
    void LSR()
    {
        //IR => MAR, RAM @ MAR => MDR ...
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
        }
        //IR => MAR, Rn @ MAR => MDR ...
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[2]);
            memoryDataRegister.SetRegister(registers.GetRegister(memoryAddressRegister.GetRegister()));
        }
        //IR => MDR ...
        else //this will be a decimal value
        {
            memoryDataRegister.SetRegister(instructionRegister.arguments[2]);
        }

        //... => ACC, Rn @ IR => MAR //> ACC => Rn @ IR
        Accumulator = memoryDataRegister.GetRegister();
        memoryDataRegister.SetRegister(registers.GetRegister(instructionRegister.arguments[1]));
        Accumulator = (int)memoryDataRegister.GetRegister() >> (int)Accumulator;

        registers.SetRegister(instructionRegister.arguments[0], Accumulator);
    }

    //this is a special instruction that will halt the CPU
    void HALT()
    {
        halted = true;
    }
    #endregion standard instruction set

    #region extended instruction set
    void INPUT()
    {
        //prompt the user for input and store the value in the specified register
        int input = InputDialog.GetInput($"Enter a value for register {instructionRegister.arguments[0]}");
        registers.SetRegister(instructionRegister.arguments[0], Convert.ToInt32(input));
    }
    void OUTPUT()
    {
        //display the value of the specified register using the message box function built into the windows forms library
        //one potential improvement would be to use a custom dialog box to display the output
        MessageBox.Show($"Register {instructionRegister.arguments[0]} contains " +
                        $"#{registers.GetRegister(instructionRegister.arguments[0])}",
                                   "Output",
                                   MessageBoxButtons.OK
                                   );
       }
    void DUMP()
    {
        //this instruction will dump the registers, memory, or both to a file by interpreting the dump type
        //and calling the appropriate dump function
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
        //IR => MAR, RAM @ MAR => MDR => PC
        if (instructionRegister.AddressMode == Constants.addressIndicator)
        {
            memoryAddressRegister.SetRegister(instructionRegister.arguments[1]);
            memoryDataRegister.SetRegister(RAM.QuereyAddress(memoryAddressRegister.GetRegister()));
            ProgramCounter = (int)memoryDataRegister.GetRegister();
        }
        //Rn @ IR => PC
        else if (instructionRegister.AddressMode == Constants.registerIndicator)
        {
            ProgramCounter = (int)registers.GetRegister(instructionRegister.arguments[2]);
        }
        //IR => PC
        else //this will be a decimal value
        {
            ProgramCounter = instructionRegister.arguments[1];
        }
    }

    //PC => Rn @ IR
    void CDP()
    {
        registers.SetRegister(instructionRegister.arguments[0], ProgramCounter); 
    }
    #endregion extended instruction set

    //this will reset all the registers in the CPU
    public void Reset()
    {
        halted = true;
        ProgramCounter = 0;
        Accumulator = 0;
        CPSR = CPSRFlags.None;
        memoryAddressRegister.Reset();
        memoryDataRegister.Reset();
        registers.Reset();
    }

    //this will dump the registers to a file with the specified name, it should only be called by the DUMP instruction
    //one potential idea was to dump the registers if a runtime error occurs however this would be a bad idea as the
    //dump folder could be filled with useless dumps, or a previous dump could be overwritten if the user was not expecting it
    void DumpRegisters(string fileName, string DumpPath = "dumps")
    {
        string[] memoryDump = new string[registers.Count + 5];

        string[] registerDump = registers.DumpRegisters();

        for (int i = 0; i < registers.Count; i++)
        {
            memoryDump[i] = registerDump[i];
        }
        
        memoryDump[registers.Count] = $" PC: {ProgramCounter}";
        memoryDump[registers.Count + 1] = $" ALU: {Accumulator}";
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

    public long GetACC()
    {
        return Accumulator;
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
        if (register > GetRegisterCount())
        {
            AddError("Register out of bounds");
            return 0;
        }
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

    //this will update the delay of the CPU and all the components that require a delay,
    //the CPU updating the delay of all the components itself means that when the user changes the delay only
    //the CPU and RAM need to be updated
    public void UpdateDelay(int delayInMs)
    {
        DelayInMs = delayInMs;
        registers.UpdateDelay(delayInMs);
        memoryAddressRegister.UpdateDelay(delayInMs);
        memoryDataRegister.UpdateDelay(delayInMs);
    }


    //this function gets called when an error occurs, it will add the error to the
    //error list and halt the CPU if the error is fatal, an event is then raised to notify the GUI
    void AddError(string message, bool isFatal = true)
    {
        Errors.Add(new EmulatorError(message, ProgramCounter, isFatal));
        if (isFatal) halted = true;
        EmulatorErrorEventArgs e = new(Errors);
        OnEmulatorErrorOccured(e);
    }

    //this function will be called when the RAM raises an invalid memory access event,
    //having the CPU handle the event means that the GUI does not need to track the CPU and RAM for errors,
    //the CPU will handle all the errors and raise an event to notify the GUI
    private void Memory_InvalidMemoryAccess(object? sender, MemoryErrorEventArgs e)
    {
        AddError(e.ErrorMessage);
    }

    //much like the previous function this will handle the registers raising an error event, this will be
    //raised by the generic registers as the internal registers do not have an error event (as they should not have errors)
    private void Registers_RegisterError(object? sender, MemoryErrorEventArgs e)
    {
        AddError(e.ErrorMessage);
    }

    //this function will be called when the RAM raises a possible program overwrite event, this will be raised
    //when the RAM believes that the memory being written to is part of the program, 
    //this is marked as none fatal as the program overwrite could be intentional or a false positive
    //much like the previous functions the CPU will handle the event and raise an event to notify the GUI
    private void Memory_PossibleProgramOverwrite(object? sender, MemoryErrorEventArgs e)
    {
        AddError(e.ErrorMessage, false);
    }


    //this event will be raised when an error occurs, the GUI will subscribe to this event to display the error
    protected virtual void OnEmulatorErrorOccured(EmulatorErrorEventArgs e)
    {
        EmulatorErrorOccured?.Invoke(this, e);
    }
}