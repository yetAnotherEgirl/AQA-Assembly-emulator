namespace AqaAssemEmulator_GUI.backend;

class machineCodeLine
{
    public int instruction;
    public List<int> arguments;
    public int AddressMode;

    public machineCodeLine(int instruction, List<int> arguments, int AddressMode)
    {
        this.instruction = instruction;
        this.arguments = arguments;
        this.AddressMode = AddressMode;
    }

    public machineCodeLine()
    {
        instruction = 0;
        arguments = new List<int>();
        AddressMode = 0;
    }

    public override string ToString()
    {
        string[] fullInstructionSet = Assembler.instructionSet.Concat(Assembler.extendedInstructionSet).ToArray();
        string output = fullInstructionSet[instruction];
        
        if (arguments.Count > 0)
        {
            output += " ";
            for (int i = 0; i < arguments.Count; i++)
            {
                output += arguments[i].ToString();
                if (i < arguments.Count - 1) output += ", ";
            }
        }
        return output;
    }


}