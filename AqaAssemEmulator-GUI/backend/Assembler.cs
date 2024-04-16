namespace AqaAssemEmulator_GUI.backend;

//https://filestore.aqa.org.uk/resources/computing/AQA-75162-75172-ALI.PDF


internal class Assembler
{
    //see AssembleLabel() for an explanation of where this comes from
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

    //this function is used to assemble a string of assembly code into machine code, it is the main function of the class
    //it is called by the GUI to compile the assembly code
    public void AssembleFromString(string assembly)
    {
        //reset all variables
        extendedInstructionSetEnabled = false;
        Errors.Clear();
        machineCode.Clear();
        Variables.Clear();
        assemblyLineList = assembly.Split('\n').ToList();
        UncompiledCode = assemblyLineList;
        assemblyLineList = assemblyLineList.Where(x => x != "").ToList();

        //preprocess the assembly code, this is its own function to make the code more readable
        PreProcessAssembly(ref assemblyLineList);

        //check if the assembly code contains a HALT instruction, if not add one
        if (Array.IndexOf(assemblyLineList.ToArray(), "HALT") == -1)
        {
            assemblyLineList.Add("HALT");
            Errors.Add(new("HALT instruction missing, assuming halt at end of assembly", 
                AssemblerError.NoLineNumber, false));
        }

        //remove leading and trailing whitespace
        assemblyLineList = assemblyLineList.Select(x => x.TrimEnd()).ToList();

        //compile the assembly code line by line
        foreach (string assemblyLine in assemblyLineList)
        {
            machineCode.Add(CompileAssemblyLine(ref assemblyLineList, assemblyLine));
        }

        //remove any duplicate variables and add the specific register variables, decimal literals are also removed
        Variables = Variables.Distinct().ToList();
        List<string> SpecificRegisterVariables = ["PC", "MAR", "MDR", "ACC", "CPSR"];
        SpecificRegisterVariables.AddRange(Variables);
        Variables = SpecificRegisterVariables;
        Variables = Variables.Where(x => !x.Contains(Constants.decimalChar)).ToList();

        //check if the assembly code compiled successfully, if not clear the machine code and variables
        //so the faulty machine code cannot be run
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

    //this function is used to preprocess the assembly code, it is called before the assembly code is compiled
    public void PreProcessAssembly(ref List<string> assemblyLineList)
    {
        //remove comments from the assembly code
        for (int i = 0; i < assemblyLineList.Count; i++)
        {
            string assemblyLine = assemblyLineList[i];

            int commentStart = assemblyLine.IndexOf(Constants.commentChar);
            if (commentStart != -1) assemblyLine = assemblyLine.Substring(0, commentStart);

            assemblyLineList[i] = assemblyLine;
        }

        //store each preprocessor flag in a list and iterate through them
        List<string> preProcessorList = assemblyLineList.Where(x => x.Contains(Constants.preProcessorIndicator)).ToList();
        foreach (string preProcessorInstruction in preProcessorList)
        {
            //remove the preprocessor indicator from the instruction and split the instruction into its components
            string instruction = preProcessorInstruction[1..];
            string[] splitInstruction = instruction.Split(' ');
            if (Array.IndexOf(splitInstruction, "") != -1) splitInstruction = splitInstruction.Where(x => x != "").ToArray();


            //enable the extended instriuction set if the flag is passed
            if (splitInstruction[0] == "EXTENDED")
            {
                if (splitInstruction.Length != 1)
                {
                    AddError("invalid EXTENDED instruction, *EXTENDED takes no arguments", preProcessorInstruction);
                    break;
                };
                extendedInstructionSetEnabled = true;
            }

            //include a file in the assembly code if the flag is passed
            if (splitInstruction[0] == "INCLUDE")
            {
                if (splitInstruction.Length != 3)
                {
                    AddError("invalid INCLUDE instruction, " +
                             "'*INCLUDE </path/to/file(.aqa)> <FIRST / LAST / HERE>'",
                             preProcessorInstruction);
                    break;
                }

                //atwempt to read the file and add its contents to the assembly code, if the file is not found throw an assembler error
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

                //determine where to add the included file in the assembly code
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

        //remove the preprocessor flags from the assembly code
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

    //this function is used to compile a single line of assembly code into machine code, it is called iteratively on each line,
    //the current assembly is also passed by reference (to reduce memory usage) so that the line number can be determined for 
    //compiling branch operations
    public long CompileAssemblyLine(ref List<string> assemblyLineList, string assemblyLine)
    {
        //in AQA assembly code, variables are seperated by commas, this removes them
        assemblyLine = assemblyLine.Replace(",", "");

        //skip empty lines, this really shouldn't happen as the preprocessor has been modified to remove empty lines,
        //however this was done after the initial implementation of the assembler, it is still a good idea to check though as
        //a failsafe
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
                // it doesn't matter what the output is, as it will be cleared later when we check if compilation
                //was successful
                return 1; //1 is the standard error code for C so i have decided to use that :)
            case 0: //label
                //this error will be the same for all the variations so we can make 1 constant string used every time
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
                    //MOV is a slower operation than LDR so we should discourage users from doing this,
                    //it is however still valid AQA assembly so we cannot throw a fatal error here
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

    //assemble the opcode of the current line, an integer called opcode is passed by reference,
    //this is because the function returns the opcode after being bit shifted to the right position whereas the
    //opcode before being bitshifted is needed elsewhere in the program.
    //if i were to redesign this program i would  make this function return a Tuple containing both values,
    //or move the bit shifting logic out of this function however this works fine
    public long AssembleOpCode(string[] line, ref int opCode)
    {
        //set opCode to be the index of the instruction plus one, we add one as returning an opcode of 0 represents a label,
        //the if statement is to handle how the instruction may instead be in the extended instructionset which is stored
        //as a seperate list
        opCode = Array.IndexOf(instructionSet, line[0]) + 1;
        if (extendedInstructionSetEnabled && opCode == 0)
        {
            opCode = Array.IndexOf(extendedInstructionSet, line[0]) + 1;
            if (opCode != 0) opCode += instructionSet.Length;
        }
        
        //this will get called if the opcode hasnt been found (meaning it wasnt in the instructionset, and the line
        //was not a label (as labels would have a length of 1)
        if (opCode == 0 && line.Length > 1)
        {
            //check the if extended instructionset has been used without the preprocessor flag
            if (Array.IndexOf(extendedInstructionSet, line[0]) != -1)
            {
                AddError("extended instruction set used without preprocessor flag", string.Join(" ", line));
                opCode = INVALID_OPCODE;
                return 0;
            }
            else
            {
                //if none of the above if statements trigger this means the assembler wasnt able to find the opcode in
                //the instructionset or the extended instructionset, it also means the offending line was not a label,
                //as we cannot accurately determine what caused the error we should just say the operation was invalid
                AddError("invalid operation", string.Join(" ", line));
                opCode = INVALID_OPCODE;
                return 0;
            }
        }

        //bitshift the opcode to the correct location in the machinecode line
        long output = (long)opCode << Constants.opCodeOffset * Constants.bitsPerNibble;

        return output;
    }

    //this is used to assemble the register of the current line, it is called by the CompileAssemblyLine function,
    //an optional offset can be passed to this function, this is used to determine the offset of the register
    //in the machine code if a function has multiple registers (not to be confused with registers passed as operands)
    public long AssembleRegister(string register, string line, int registerOffsetIndex = 0)
    {
        Variables.Add(register); //add the register to the list of variables

        //check if the register is indicated with the register key character, if not add an error
        if (register[0] != Constants.registerChar)
            AddError($"expected a register, use {Constants.registerChar}n to define a register", line);
        int registerAddress = 0;
        try
        {
            if (register.Length == 1) throw new FormatException(); //thrown if register = "R"
            registerAddress = int.Parse(register[1..]);
        }
        catch (FormatException)
        {
            AddError("invalid register address", line);
        }
        catch (OverflowException)
        {
            //what CPU has  2,147,483,648 registers??
            AddError("invalid register address, the emulated CPU only" +
                " has 13 registers (R0 up to and including R12)", line);
        }

        //check if the register address is valid, if not add an error
        if (registerAddress > 12 || registerAddress < 0)
        {
            AddError("invalid register address, the emulated CPU only" +
                " has 13 registers (R0 up to and including R12)", line);
        }

        //bitshift the register address to the correct location in the machine code and return it
        int CurrentRegisterOffset = Constants.registerOffset + registerOffsetIndex;
        long output = (long)registerAddress << CurrentRegisterOffset * Constants.bitsPerNibble;

        return output;
    }

    //this function is used to assemble the opperand of the current line, the opcode takes up the largest amount of bits
    public long AssembleOpperand(string opperand, string line)
    {
        //addresses arent indicated with a key character, so we start by assuming that the inputed opperand is an address,
        //and then check if it is in fact a decimal literal or a register later. this makes the program logic simpler
        //however it is less understandable on first glance

        //here we set the output to be the address indicator, this is then bitshifted to the sign bit location
        // later in the program, this output variable is what is returned at the end of the function
        long output = Constants.addressIndicator;

        Variables.Add(opperand); //add the opperand to the list of variables

        //check if the opperand is a decimal literal or a register, if neither our assumption that it is an address is correct
        if (opperand[0] == Constants.decimalChar)
        {
            //redefine the output to be the decimal indicator, this is then bitshifted to the sign bit location
            output = (long)Constants.decimalIndicator << Constants.signBitOffset * Constants.bitsPerNibble;
            try
            {
                if (opperand.Length == 1) throw new FormatException(); //thrown if opperand = "#"
                output += long.Parse(opperand[1..]); //parse the decimal literal and add to the output
            }
            catch (FormatException)
            {
                AddError("invalid decimal literal", line);
            }
            catch (OverflowException)
            {
                AddError("invalid decimal, decimal literals should be between 0 and 2^32 - 1", line);
            }
        }
        else if (opperand[0] == Constants.registerChar)
        {
            //like before we redefine the output to be the register indicator, this is then bitshifted to the sign bit location
            output = (long)Constants.registerIndicator << Constants.signBitOffset * Constants.bitsPerNibble;
            try
            {
                if (opperand.Length == 1) throw new FormatException(); //thrown if opperand = "R"
                output += long.Parse(opperand[1..]);
                if (long.Parse(opperand[1..]) > 12 || long.Parse(opperand[1..]) < 0) //check if the register address is valid
                    throw new OverflowException();                                   //this will prevent runtime errors later
            }                                                                        //which are harder to debug
            catch (FormatException)
            {
                AddError("invalid register address", line);
            }
            catch (OverflowException)
            {
                AddError("invalid register address, the emulated CPU only" +
                " has 13 registers (R0 up to and including R12)", line);
            }
        }
        else 
        {
            //here our previous assumption that the opperand is an address is correct, so we bitshift the
            //address indicator to the sign bit location
            output <<= Constants.signBitOffset * Constants.bitsPerNibble;
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

    //this function does the same as the AssembleOpperand function, however it is used specifically for memory references,
    //this is because memory references are not allowed to be decimal literals
    public long AssembleMemoryReference(string memory, string line)
    {
        if (memory[0] == Constants.decimalChar)
        {
            AddError("memory references cannot be decimal literals", line);
        }
        long memoryReference = AssembleOpperand(memory, line);
        return memoryReference;
    }

    //this function is used to assemble a pointer to a label, it is used by the B, BEQ, BNE, BGT and BLT instructions,
    public long AssembleLabel(ref List<string> line, string label)
    {
        /* WHERE INVALID_LABEL_LOCATION COMES FROM:
         * when Array.IndexOf is called on a list and the element is not found, it returns -1, this is stored as an int
         * which is made up of 32 bits using twos compliment signed binary. this means our -1 gets stored in memory as
         * 0x7FFFFFFFFFFFFFFF.
         * the issue occurs when we convert this into a long, while longs are also signed using twos compliment binary,
         * they are made of 64 bits rather than 32, due to the implimentation of doing "long x = someInt;" the bytes are
         * copied one to one into output, meaning output now looks like 0x00000000000000007FFFFFFFFFFFFFFF. as you can
         * see the sign bit has been intepreted as part of the number, meaning the returned value is now equal to
         * 576460752303423487
         */

        label += ":";
        long output = Array.IndexOf(line.ToArray(), label);
        return output;
    }

    //this is used when a DUMP instruction is processed
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

    #region Getters
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
    #endregion Getters

    //this takes in an error message that should be shown and the line it occured on, the line number is then found
    //in the uncompiled code (unless there isnt a line number) and also displayed to the user. an optional boolean is
    //also used to determine whether the error was fatal, at the end the error is added to the list of errors
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

