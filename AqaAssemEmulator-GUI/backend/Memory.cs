namespace AqaAssemEmulator_GUI.backend;

internal class Memory
{
    private long[] memory;
    public event EventHandler<MemoryErrorEventArgs> InvalidMemoryAccess;
    public event EventHandler<MemoryErrorEventArgs> possibleProgramOverwrite;

    public Memory(int Size)
    {
        memory = new long[Size];
    }

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
        //if (memory[address] != 0) Console.WriteLine("warning: overwriting memory");
        //if (memory[address] > Constants.opCodeOffset * Constants.bitsPerNibble) throw new AccessViolationException("overwtiting what appears to be machine code");
        if (memory[address] > Constants.opCodeOffset * Constants.bitsPerNibble) 
            OnPossibleProgramOverwrite(new($"Possible program overwrite at address {address}"));
        memory[address] = value;
    }

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
        if (address + code.Count > memory.Length) throw new ArgumentException("code too large for memory");
        if (address + code.Count > memory.Length - 3) Console.WriteLine("warning: code with variables may be too large for memory");
        for (int i = address; i < code.Count; i++)
        {
            memory[i] = code[i];
        }
    }


    // awful!!! add threading if you have time
    public void Reset()
    {
        for (int i = 0; i < memory.Length; i++)
        {
            memory[i] = 0;
        }
    }

    protected virtual void OnInvalidMemoryAccess(MemoryErrorEventArgs e)
    {
        InvalidMemoryAccess?.Invoke(this, e);
    }

    protected virtual void OnPossibleProgramOverwrite(MemoryErrorEventArgs e)
    {
        possibleProgramOverwrite?.Invoke(this, e);
    }
}


