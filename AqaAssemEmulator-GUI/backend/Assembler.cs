namespace AqaAssemEmulator_GUI.backend;

//https://filestore.aqa.org.uk/resources/computing/AQA-75162-75172-ALI.PDF


internal class Assembler
{

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

    int LineNumber = 0;

    List<AssemblerError> Errors = [];

    public Assembler()
    {

    }

    public void AssembleFromString(string assembly)
    {
        Errors.Clear();
        machineCode.Clear();
        Variables.Clear();
        assemblyLineList = assembly.Split('\n').ToList();
        UncompiledCode = assemblyLineList;
        assemblyLineList = assemblyLineList.Where(x => x != "").ToList();

        PreProcessAssembly(ref assemblyLineList);

        if (Array.IndexOf(assemblyLineList.ToArray(), "HALT") == -1)
        {
            // pass an empty string for the line parameter as the error is the line is missing
            Errors.Add(new("HALT instruction missing", AssemblerError.NoLineNumber, true));
        }


        foreach (string assemblyLine in assemblyLineList)
        {
            machineCode.Add(CompileAssemblyLine(ref assemblyLineList, assemblyLine));
        }

        Variables = Variables.Distinct().ToList();
        List<string> SpecificRegisterVariables = ["PC", "MAR", "MDR", "ALU", "CPSR"];
        SpecificRegisterVariables.AddRange(Variables);
        Variables = SpecificRegisterVariables;
        Variables = Variables.Where(x => !x.Contains('#')).ToList();

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
        List<string> preProcessorList = assemblyLineList.Where(x => x.Contains(Constants.preProcessorIndicator)).ToList();
        foreach (string preProcessorInstruction in preProcessorList)
        {
            string instruction = preProcessorInstruction.Substring(1);
            string[] splitInstruction = instruction.Split(' ');
            if (Array.IndexOf(splitInstruction, "") != -1) splitInstruction = splitInstruction.Where(x => x != "").ToArray();

            if (splitInstruction[0] == "EXTENDED")
            {
                if (splitInstruction.Length != 1)
                {
                    AddError("invalid EXTENDED instruction, *EXTENDED takes no arguments", preProcessorInstruction);
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
                }

                //  done
                //x handle errors if the file doesn't exist

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
    }

    public long CompileAssemblyLine(ref List<string> assemblyLineList, string assemblyLine)
    {
        LineNumber++;
        if (string.IsNullOrEmpty(assemblyLine)) throw new ArgumentException("empty string passed to assembleLine");

        assemblyLine = assemblyLine.Replace(",", "");

        {
            int commentStart = assemblyLine.IndexOf(Constants.commentChar);
            if (commentStart != -1) assemblyLine = assemblyLine.Substring(0, commentStart);
        }
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
            case 0: //label
                const string errorText = "invalid label, labels must be 1 word and are followed by a colon";
                if (splitLine.Length > 1) AddError(errorText, assemblyLine);
                int colonIndex = assemblyLine.IndexOf(":");
                if (colonIndex == -1) AddError(errorText, assemblyLine);
                break;
            case 1: //LDR
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleMemoryReference(splitLine[2], assemblyLine);
                break;
            case 2: //STR
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleMemoryReference(splitLine[2], assemblyLine);
                break;
            case 3: //ADD
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleRegister(splitLine[2], assemblyLine, 1);
                output += AssembleOpperand(splitLine[3], assemblyLine);
                break;
            case 4: //SUB
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleRegister(splitLine[2], assemblyLine, 1);
                output += AssembleOpperand(splitLine[3], assemblyLine);
                break;
            case 5: //MOV
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleOpperand(splitLine[2], assemblyLine);
                char[] opperand = splitLine[2].ToCharArray();
                if (opperand[0] != Constants.registerChar && opperand[0] != Constants.decimalChar)
                {
                    AddError("MOV has been used like a LDR, consider using LDR instead", assemblyLine, false);
                }
                break;
            case 6: //CMP
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleOpperand(splitLine[2], assemblyLine);
                break;
            case 7: //B
                output += AssembleLabel(ref assemblyLineList, splitLine[1]);
                break;
            case 8: //BEQ
                output += AssembleLabel(ref assemblyLineList, splitLine[1]);
                break;
            case 9: //BNE
                output += AssembleLabel(ref assemblyLineList, splitLine[1]);
                break;
            case 10: //BGT
                output += AssembleLabel(ref assemblyLineList, splitLine[1]);
                break;
            case 11: //BLT
                output += AssembleLabel(ref assemblyLineList, splitLine[1]);
                break;
            case 12: //AND
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleRegister(splitLine[2], assemblyLine, 1);
                output += AssembleOpperand(splitLine[3], assemblyLine);
                break;
            case 13: //ORR
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleRegister(splitLine[2], assemblyLine, 1);
                output += AssembleOpperand(splitLine[3], assemblyLine);
                break;
            case 14: //EOR
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleRegister(splitLine[2], assemblyLine, 1);
                output += AssembleOpperand(splitLine[3], assemblyLine);
                break;
            case 15: //MVN
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleOpperand(splitLine[2], assemblyLine);
                break;
            case 16: //LSL
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleRegister(splitLine[2], assemblyLine, 1);
                output += AssembleOpperand(splitLine[3], assemblyLine);
                break;
            case 17: //LSR
                output += AssembleRegister(splitLine[1], assemblyLine);
                output += AssembleRegister(splitLine[2], assemblyLine, 1);
                output += AssembleOpperand(splitLine[3], assemblyLine);
                break;
            case 18: //HALT
                break;
            case 19: //INPUT    
                output += AssembleRegister(splitLine[1], assemblyLine) + 1;
                break;
            case 20: //OUTPUT
                output += AssembleRegister(splitLine[1], assemblyLine) + 1;
                break;
            case 21: //DUMP
                output += AssembleDumpMode(splitLine[1], assemblyLine);
                break;
            case 22: //JMP
                output += AssembleOpperand(splitLine[1], assemblyLine);
                break;
            case 23: //CDP
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
                }
                else
                {
                    AddError("invalid operation", string.Join(" ", line));
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

