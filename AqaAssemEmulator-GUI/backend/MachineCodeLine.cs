namespace AqaAssemEmulator_GUI.backend;

internal class MachineCodeLine
{
    /* this class is used to represent the decoded machine code line
     * that is inside the instruction register. it is not related to the
     * output of the assembler, which just outputs the machine code 
     * as a list of longs
     * 
     * a potential refactor would be to have this as a struct instead
     * however this is not necessary to the functionality of the program
     */

    public int instruction;
    public List<int> arguments;
    public int AddressMode;

    public MachineCodeLine(int instruction, List<int> arguments, int AddressMode)
    {
        this.instruction = instruction;
        this.arguments = arguments;
        this.AddressMode = AddressMode;
    }

    public MachineCodeLine()
    {
        instruction = 0;
        arguments = [];
        AddressMode = 0;
    }

}