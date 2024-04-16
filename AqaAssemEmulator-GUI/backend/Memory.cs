namespace AqaAssemEmulator_GUI.backend;

internal class Memory
{
    private long[] memory;
    public event EventHandler<MemoryErrorEventArgs> InvalidMemoryAccess;
    public event EventHandler<MemoryErrorEventArgs> PossibleProgramOverwrite;

    public Memory(int Size)
    {
        memory = new long[Size];
    }

    //a better way to do this would be to use a property like in the Generic register class
    //however this will also work so I will leave it as is
    public int GetLength()
    {
        return memory.Length;
    }

    public long QuereyAddress(long address)
    {
        if ( address < 0 || address > memory.Length) 
        {
            MemoryErrorEventArgs e = new($"Invalid memory address: {address}");
            OnInvalidMemoryAccess(e);
            return -1;
        }
        return memory[address];
    }

    public void SetAddress(long address, long value)
    {
        if (address < 0 || address > memory.Length)
        {
            MemoryErrorEventArgs e = new($"Invalid memory address: {address}");
            OnInvalidMemoryAccess(e);
        }
        //this is a simple check to see if the address is in the range of the opcodes,
        //if it is then it is likely that the current program is being overwritten
        if (memory[address] > Constants.opCodeOffset * Constants.bitsPerNibble) 
            OnPossibleProgramOverwrite(new($"Possible program overwrite at address {address}"));
        memory[address] = value;
    }

    //this will be called by the CPU when a dump instruction is executed
    public void DumpMemory(string fileName, string DumpPath = "dumps")
    {
        string[] memoryDump = new string[memory.Length];

        for (int i = 0; i < memory.Length; i++)
        {
            string memoryInHex = memory[i].ToString("X");
            memoryDump[i] = $" address {i}: {memoryInHex}";
        }
        Directory.CreateDirectory($"./{DumpPath}");
        fileName = DumpPath + "/" + fileName + ".Dump";
        File.WriteAllLines(fileName, memoryDump);
    }

    public void LoadMachineCode(List<long> code, int address = 0)
    {
        if (address + code.Count > memory.Length)
        {
            MemoryErrorEventArgs e = new($"Program too large for memory: {code.Count} lines in wheras " +
                $"there are only {GetLength()} addresses available");
            OnInvalidMemoryAccess(e);
        }
        if (address + code.Count > memory.Length - 3) 
        { 
            MemoryErrorEventArgs e = new($"Program possibly too large for memory: {code.Count} lines in wheras " +
                               $"there are only {GetLength()} addresses available, data may be overwritten as the program runs");

            OnPossibleProgramOverwrite(e);
        }
        for (int i = address; i < code.Count; i++)
        {
            memory[i] = code[i];
        }
    }


    //an improvement here would be to implement threading to speed up the process
    //(i.e. divide the memory into chunks and have each thread read a chunk)
    public bool IsEmpty
    {
        get
        {
            bool isEmpty = true;
            foreach (long value in memory)
            {
                if (value != 0)
                {
                    isEmpty = false;
                    break;
                }
            }

            return isEmpty;
        }
    }


    //much like above, this could be improved with threading, the time this takes is noticable
    //to the user, however it is not significant enough to warrant the extra complexity
    public void Reset()
    {
        for (int i = 0; i < memory.Length; i++)
        {
            memory[i] = 0;
        }
    }

    //this event is used to notify the CPU that an invalid memory access has been attempted
    protected virtual void OnInvalidMemoryAccess(MemoryErrorEventArgs e)
    {
        InvalidMemoryAccess?.Invoke(this, e);
    }

    //this event is used to notify the CPU that a program may be overwritten, it is marked as a none 
    //fatal error in the CPUclass
    protected virtual void OnPossibleProgramOverwrite(MemoryErrorEventArgs e)
    {
        PossibleProgramOverwrite?.Invoke(this, e);
    }
}


