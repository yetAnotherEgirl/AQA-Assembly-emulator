namespace AqaAssemEmulator_GUI.backend;

//https://filestore.aqa.org.uk/resources/computing/AQA-75162-75172-ALI.PDF


internal class Assembler
{
    // this number is caused by the fact Array.IndexOf returns -1 if the element is not found, however
    // we are converting this to a long, for some reason this results in the number 576460752303423487
    // this is the hexadecimal equivalent of 0x7FFFFFFFFFFFFFFF indicating the sign bit is being
    // incorrectly read as a 1
    private const long INVALID_LABEL_LOCATION = 576460752303423487;

    
    private const int INVALID_OPCODE = -1;

    public static string[] instructionSet =
    ["LDR",
        "STR",
        "ADD",
        "SUB",
        "MOV",
        "CMP",
        "B",
        "BEQ",
        "BNE",
        "BGT",
        "BLT",
        "AND",
        "ORR",
        "EOR",
        "MVN",
        "LSL",
        "LSR",
        "HALT"];

    public static string[] extendedInstructionSet = ["INPUT", "OUTPUT", "DUMP", "JMP", "CDP"];

    bool extendedInstructionSetEnabled = false;

    List<long> machineCode = [];

    List<string> Variables = [];

    //used for fetching line numbers for errors
    List<string> UncompiledCode = [];

    List<string> assemblyLineList = [];

    List<AssemblerError> Errors = [];

    public Assembler()
    {

    }

    public void AssembleFromString(string assembly)
    {
        extendedInstructionSetEnabled = false;
        Errors.Clear();
        machineCode.Clear();
        Variables.Clear();
        assemblyLineList = assembly.Split('\n').ToList();
        UncompiledCode = assemblyLineList;
        assemblyLineList = assemblyLineList.Where(x => x != "").ToList();

        PreProcessAssembly(ref assemblyLineList);

        if (Array.IndexOf(assemblyLineList.ToArray(), "HALT") == -1)
        {
            assemblyLineList.Add("HALT");
            Errors.Add(new("HALT instruction missing, assuming halt at end of assembly", 
                AssemblerError.NoLineNumber, false));
        }

        //remove leading and trailing whitespace
        assemblyLineList = assemblyLineList.Select(x => x.TrimEnd()).ToList();

        foreach (string assemblyLine in assemblyLineList)
        {
            machineCode.Add(CompileAssemblyLine(ref assemblyLineList, assemblyLine));
        }

        Variables = Variables.Distinct().ToList();
        List<string> SpecificRegisterVariables = ["PC", "MAR", "MDR", "ACC", "CPSR"];
        SpecificRegisterVariables.AddRange(Variables);
        Variables = SpecificRegisterVariables;
        Variables = Variables.Where(x => !x.Contains(Constants.decimalChar)).ToList();

        bool failedToCompile = false;
        foreach (AssemblerError error in Errors)
        {
            if (error.IsFatal)
            {
                failedToCompile = true;
                break;
            }
        }
        if (failedToCompile)
        {
            machineCode.Clear();
            Variables.Clear();
        }
    }

    public void PreProcessAssembly(ref List<string> assemblyLineList)
    {
        //remove comments
        for (int i = 0; i < assemblyLineList.Count; i++)
        {
            string assemblyLine = assemblyLineList[i];

            int commentStart = assemblyLine.IndexOf(Constants.commentChar);
            if (commentStart != -1) assemblyLine = assemblyLine.Substring(0, commentStart);

            assemblyLineList[i] = assemblyLine;
        }

        List<string> preProcessorList = assemblyLineList.Where(x => x.Contains(Constants.preProcessorIndicator)).ToList();
        foreach (string preProcessorInstruction in preProcessorList)
        {
            string instruction = preProcessorInstruction[1..];
            string[] splitInstruction = instruction.Split(' ');
            if (Array.IndexOf(splitInstruction, "") != -1) splitInstruction = splitInstruction.Where(x => x != "").ToArray();

            if (splitInstruction[0] == "EXTENDED")
            {
                if (splitInstruction.Length != 1)
                {
                    AddError("invalid EXTENDED instruction, *EXTENDED takes no arguments", preProcessorInstruction);
                    break;
                };
                extendedInstructionSetEnabled = true;
            }

            if (splitInstruction[0] == "INCLUDE")
            {
                if (splitInstruction.Length != 3)
                {
                    AddError("invalid INCLUDE instruction, " +
                             "'*INCLUDE </path/to/file(.aqa)> <FIRST / LAST / HERE>'",
                             preProcessorInstruction);
                    break;
                }

                
                //x handle errors if the file doesn't exist <-- Done

                string path = "";
                string assembly = "";
                try
                {
                    path = Path.GetFullPath(splitInstruction[1] + ".aqa");
                    assembly = File.ReadAllText(path);
                }
                catch (Exception)
                {
                    AddError("file not found", preProcessorInstruction);
                    break;
                }
                

                List<string> assemblyList = assembly.Split('\n').ToList();
                assemblyList = assemblyList.Where(x => x != "").ToList();

                if (splitInstruction[2] == "FIRST")
                {
                    assemblyLineList.InsertRange(0, assemblyList);

                }
                else if (splitInstruction[2] == "LAST")
                {
                    assemblyLineList.AddRange(assemblyList);
                }
                else if (splitInstruction[2] == "HERE")
                {
                    int index = assemblyLineList.IndexOf(preProcessorInstruction);
                    assemblyLineList.InsertRange(index, assemblyList);
                }
                else
                {
                    AddError("invalid INCLUDE instruction, " +
                             "'*INCLUDE </path/to/file(.aqa)> <FIRST / LAST / HERE>'",
                             preProcessorInstruction);
                }
            }
        }

        assemblyLineList = assemblyLineList.Where(x => !x.Contains(Constants.preProcessorIndicator)).ToList();

        //it is necessary to do a second pass to remove comments that may have been added by the preprocessor from included files
        for (int i = 0; i < assemblyLineList.Count; i++)
        {
            string assemblyLine = assemblyLineList[i];

            int commentStart = assemblyLine.IndexOf(Constants.commentChar);
            if (commentStart != -1) assemblyLine = assemblyLine.Substring(0, commentStart);

            assemblyLineList[i] = assemblyLine;
        }

        assemblyLineList = assemblyLineList.Where(x => x != "").ToList();
    }

    public long CompileAssemblyLine(ref List<string> assemblyLineList, string assemblyLine)
    {
        //LineNumber++;
        //if (string.IsNullOrEmpty(assemblyLine)) throw new ArgumentException("empty string passed to assembleLine");
        assemblyLine = assemblyLine.Replace(",", "");

        if (string.IsNullOrEmpty(assemblyLine)) return 0;
        string[] splitLine = assemblyLine.Split(' ');
        if (Array.IndexOf(splitLine, "") != -1) splitLine = splitLine.Where(x => x != "").ToArray();


        long output = 0;
        int opCode = 0;
        output += AssembleOpCode(splitLine, ref opCode);

        switch (opCode)
        {
            default:
                throw new ArgumentException("invalid operation");
            case INVALID_OPCODE:
                // a case of -1 indicates that the operation is invalid, so we return 0. as the error is fatal
                // it doesn't matter what the output is, as it will be cleared later
                return 0;
            case 0: //label
                const string errorText = "invalid label, labels must be 1 word and are followed by a colon";
                if (splitLine.Length > 1) AddError(errorText, assemblyLine);
                int colonIndex = assemblyLine.IndexOf(":");
                if (colonIndex == -1) AddError(errorText, assemblyLine);
                break;
            case 1: //LDR
                if (splitLine.Length != 3) { AddError("LDR takes 2 arguments", assemblyLine); break; }
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleMemoryReference(splitLine[2], assemblyLine);
                break;
            case 2: //STR
                if (splitLine.Length != 3) { AddError("STR takes 2 arguments", assemblyLine); break; }
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleMemoryReference(splitLine[2], assemblyLine);
                break;
            case 3: //ADD
                if (splitLine.Length != 4) { AddError("ADD takes 3 arguments", assemblyLine); break; }
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleRegister(splitLine[2], assemblyLine, 1);
                output += AssembleOpperand(splitLine[3], assemblyLine);
                break;
            case 4: //SUB
                if (splitLine.Length != 4) { AddError("SUB takes 3 arguments", assemblyLine); break; }
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleRegister(splitLine[2], assemblyLine, 1);
                output += AssembleOpperand(splitLine[3], assemblyLine);
                break;
            case 5: //MOV
                if (splitLine.Length != 3) { AddError("MOV takes 2 arguments", assemblyLine); break; }
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleOpperand(splitLine[2], assemblyLine);
                char[] opperand = splitLine[2].ToCharArray();
                if (opperand[0] != Constants.registerChar && opperand[0] != Constants.decimalChar)
                {
                    AddError("MOV has been used like a LDR, consider using LDR instead", assemblyLine, false);
                }
                break;
            case 6: //CMP
                if (splitLine.Length != 3) { AddError("CMP takes 2 arguments", assemblyLine); break; }
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleOpperand(splitLine[2], assemblyLine);
                break;
            case 7: //B
                if (splitLine.Length != 2) { AddError("B takes 1 argument", assemblyLine); break; }
                output += AssembleLabel(ref assemblyLineList, splitLine[1]);
                if (output == -1) AddError("invalid label", assemblyLine);
                break;
            case 8: //BEQ
                if (splitLine.Length != 2) { AddError("BEQ takes 1 argument", assemblyLine); break; }
                output += AssembleLabel(ref assemblyLineList, splitLine[1]);
                if (output == INVALID_LABEL_LOCATION) AddError("invalid label", assemblyLine);
                break;
            case 9: //BNE
                if (splitLine.Length != 2) { AddError("BNE takes 1 argument", assemblyLine); break; }
                output += AssembleLabel(ref assemblyLineList, splitLine[1]);
                if (output == INVALID_LABEL_LOCATION) AddError("invalid label", assemblyLine);
                break;
            case 10: //BGT
                if (splitLine.Length != 2) { AddError("BGT takes 1 argument", assemblyLine); break; }
                output += AssembleLabel(ref assemblyLineList, splitLine[1]);
                if (output == INVALID_LABEL_LOCATION) AddError("invalid label", assemblyLine);
                break;
            case 11: //BLT
                if (splitLine.Length != 2) { AddError("BLT takes 1 argument", assemblyLine); break; }
                output += AssembleLabel(ref assemblyLineList, splitLine[1]);
                if (output == INVALID_LABEL_LOCATION) AddError("invalid label", assemblyLine);
                break;
            case 12: //AND
                if (splitLine.Length != 4) { AddError("AND takes 3 arguments", assemblyLine); break; }
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleRegister(splitLine[2], assemblyLine, 1);
                output += AssembleOpperand(splitLine[3], assemblyLine);
                break;
            case 13: //ORR
                if (splitLine.Length != 4) { AddError("ORR takes 3 arguments", assemblyLine); break; }
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleRegister(splitLine[2], assemblyLine, 1);
                output += AssembleOpperand(splitLine[3], assemblyLine);
                break;
            case 14: //EOR
                if (splitLine.Length != 4) { AddError("EOR takes 3 arguments", assemblyLine); break; }
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleRegister(splitLine[2], assemblyLine, 1);
                output += AssembleOpperand(splitLine[3], assemblyLine);
                break;
            case 15: //MVN
                if (splitLine.Length != 3) { AddError("MVN takes 2 arguments", assemblyLine); break; }
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleOpperand(splitLine[2], assemblyLine);
                break;
            case 16: //LSL
                if (splitLine.Length != 4) { AddError("LSL takes 3 arguments", assemblyLine); break; }
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleRegister(splitLine[2], assemblyLine, 1);
                output += AssembleOpperand(splitLine[3], assemblyLine);
                break;
            case 17: //LSR
                if (splitLine.Length != 4) { AddError("LSR takes 3 arguments", assemblyLine); break; }
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleRegister(splitLine[2], assemblyLine, 1);
                output += AssembleOpperand(splitLine[3], assemblyLine);
                break;
            case 18: //HALT
                if (splitLine.Length != 1) { AddError("HALT takes no arguments", assemblyLine); break; }
                break;
            case 19: //INPUT
                if (splitLine.Length != 2) { AddError("INPUT takes 1 argument", assemblyLine); break; }
                output += AssembleRegister(splitLine[1], assemblyLine) + 1;
                break;
            case 20: //OUTPUT
                if (splitLine.Length != 2) { AddError("OUTPUT takes 1 argument", assemblyLine); break; }
                output += AssembleRegister(splitLine[1], assemblyLine) + 1;
                break;
            case 21: //DUMP
                if (splitLine.Length != 2) { AddError("DUMP takes 1 argument", assemblyLine); break; }
                output += AssembleDumpMode(splitLine[1], assemblyLine);
                break;
            case 22: //JMP
                if (splitLine.Length != 2) { AddError("JMP takes 1 argument", assemblyLine); break; }
                output += AssembleOpperand(splitLine[1], assemblyLine);
                break;
            case 23: //CDP
                if (splitLine.Length != 2) { AddError("CDP takes 1 argument", assemblyLine); break; }
                output += AssembleRegister(splitLine[1], assemblyLine);
                break;


        }
        return output;
    }

    public long AssembleOpCode(string[] line, ref int opCode)
    {
        opCode = Array.IndexOf(instructionSet, line[0]) + 1;
        if (extendedInstructionSetEnabled && opCode == 0)
        {
            opCode = Array.IndexOf(extendedInstructionSet, line[0]) + 1;
            if (opCode != 0) opCode += instructionSet.Length;
        }
        if (opCode == 0)
        {

            if (line.Length > 1)
            {
                if (Array.IndexOf(extendedInstructionSet, line[0]) != -1)
                {
                    AddError("extended instruction set used without preprocessor flag", string.Join(" ", line));
                    opCode = INVALID_OPCODE;
                }
                else
                {
                    AddError("invalid operation", string.Join(" ", line));
                    opCode = INVALID_OPCODE;
                    return 0;
                }

            };
            //x Console.WriteLine("label recognised");
            return 0;
        }


        long output = (long)opCode << Constants.opCodeOffset * Constants.bitsPerNibble;

        return output;
    }

    public long AssembleRegister(string register, string line, int registerOffsetIndex = 0)
    {
        Variables.Add(register);
        if (register[0] != Constants.registerChar)
            AddError($"expected a register, use {Constants.registerChar}n to define a register", line);
        int registerAddress = 0;
        try
        {
            if (register.Length == 1) throw new FormatException();
            registerAddress = int.Parse(register.Substring(1));
        }
        catch (FormatException)
        {
            AddError("invalid register address", line);
        }
        catch (OverflowException)
        {
            AddError("invalid register address, " +
                     "im not sure what CPU would have enough registers " +
                     "to cause an overflow exception but the emulated one certainly doesnt", line);
        }
        int CurrentRegisterOffset = Constants.registerOffset + registerOffsetIndex;
        if (registerAddress < 0) AddError("registers are all positive integers", line);
        long output = (long)registerAddress << CurrentRegisterOffset * Constants.bitsPerNibble;

        if (registerAddress > 12)
        {
            AddError("invalid register address, the emulated CPU only" +
                " has 13 registers (R0 up to and including R12)", line);
        }

        return output;
    }

    public long AssembleOpperand(string opperand, string line)
    {


        long output = Constants.addressIndicator;

        Variables.Add(opperand);

        if (opperand[0] == Constants.decimalChar)
        {
            output = (long)Constants.decimalIndicator << Constants.signBitOffset * Constants.bitsPerNibble;
            try
            {
                if (opperand.Length == 1) throw new FormatException();
                output += long.Parse(opperand.Substring(1));
            }
            catch (FormatException)
            {
                AddError("invalid decimal literal", line);
            }
            catch (OverflowException)
            {
                AddError("invalid decimal, decimal literals should be between 0 and 2^32", line);
            }
        }
        else if (opperand[0] == Constants.registerChar)
        {
            output = (long)Constants.registerIndicator << Constants.signBitOffset * Constants.bitsPerNibble;
            try
            {
                output += long.Parse(opperand.Substring(1));
            }
            catch (FormatException)
            {
                AddError("invalid register address", line);
            }
            catch (OverflowException)
            {
                AddError("invalid register address, " +
                         "im not sure what CPU would have enough registers " +
                         "to cause an overflow exception but the emulated one certainly doesnt", line);
            }
        }
        else
        {
            output <<= Constants.signBitOffset * Constants.bitsPerNibble;
            if (opperand[0] == Constants.registerChar) opperand = opperand.Substring(1);
            try
            {
                if (opperand.Length == 1) throw new FormatException();
                output += long.Parse(opperand);
            }
            catch (FormatException)
            {
                AddError("invalid opperand", line);
            }
            catch (OverflowException)
            {
                AddError("opperands must be between 0 and 2^32", line);
            }
        }




        return output;
    }

    public long AssembleMemoryReference(string memory, string line)
    {
        long memoryReference = -1;
        memoryReference = AssembleOpperand(memory, line);
        return memoryReference;
    }

    public long AssembleLabel(ref List<string> line, string label)
    {
        label += ":";
        //0x7FFFFFFFFFFFFFFF is the hexadecimal equivalent of -1
        long output = Array.IndexOf(line.ToArray(), label);
        return output;
    }

    public long AssembleDumpMode(string mode, string line)
    {
        long output = 0;
        switch (mode)
        {
            case "memory":
                output = 0;
                break;
            case "registers":
                output = 1;
                break;
            case "all":
                output = 2;
                break;
            default:
                AddError("invalid dump mode, mode must be 'memory', 'registers' or 'all'", line);
                break;
        }
        return output;
    }

    public List<long> GetMachineCode()
    {
        return machineCode;
    }

    public List<string> GetVariables()
    {
        return Variables;
    }

    public List<AssemblerError> GetCompilationErrors()
    {
        return Errors;
    }

    private void AddError(string message, string line, bool isFatal = true)
    {
        int lineNumber = UncompiledCode.IndexOf(line) + 1;
        if (lineNumber == -1)
        {
            lineNumber = assemblyLineList.IndexOf(line);
            if (lineNumber == -1)
            {
                lineNumber = AssemblerError.NoLineNumber;
            }
            else
            {
                lineNumber = AssemblerError.ErrorInIncludedFile;
            }
        }
        AssemblerError error = new(message, lineNumber, isFatal);
        Errors.Add(error);
    }
}

