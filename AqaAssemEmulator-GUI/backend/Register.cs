namespace AqaAssemEmulator_GUI.backend;

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
        //return the value in hexadecimal format as this will be useful for debugging
        return value.ToString("X");
    }

    public void UpdateDelay(int delayInMs)
    {
        DelayInMs = delayInMs;
    }
}
