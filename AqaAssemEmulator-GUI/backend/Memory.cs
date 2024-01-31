using System.Reflection.Emit;

namespace AqaAssemEmulator_GUI.backend;

internal class Memory
{
    private long[] memory;

    public Memory(int Size)
    {
        memory = new long[Size];
    }

    public int Length()
    {
        return memory.Length;
    }

    public long QuereyAddress(long address)
    {
        if (address > memory.Length)
            throw new ArgumentException("address out of bounds");
        return memory[address];
    }

    public void SetAddress(long address, long value)
    {
        if (address > memory.Length) throw new ArgumentException("address out of bounds");
        if (memory[address] != 0) Console.WriteLine("warning: overwriting memory");
        if (memory[address] > Constants.opCodeOffset * Constants.bitsPerNibble) Console.WriteLine("warning: overwtiting what appears to be machine code");
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

    public void Reset()
    {
        for (int i = 0; i < memory.Length; i++)
        {
            memory[i] = 0;
        }
    }
}

class Register
{
    private long value;
    int DelayInMs;

    public Register(int delayInMs)
    {
        value = 0;
        DelayInMs = delayInMs;
    }

    public void SetRegister(long val)
    {
        Thread.Sleep(DelayInMs);
        value = val;
    }

    public long GetRegister()
    {
        return value;
    }

    public void Reset()
    {
        value = 0;
    }

    public string DumpRegister()
    {
        return value.ToString("X");
    }

    public void UpdateDelay(int delayInMs)
    {
        DelayInMs = delayInMs;
    }
}
