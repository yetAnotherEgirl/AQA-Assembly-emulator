
# AQA assembly emulator

an emulator for the AQA reference assembly language used in the AQA computer science A-level


## Instruction set

the standard AQA instructionset can be found 
[here](https://filestore.aqa.org.uk/resources/computing/AQA-75162-75172-ALI.PDF) however the emulator supports an extended instruction set provided the preprossesor flag is given

unlike the assembly often used in exams, all programs are required to have a `HALT` instruction

`<operand2>` can now also be a memory reference

### extended instruction Set

| command | instruction |
| --------|--------------|
| `INPUT Rn` | ask the user to enter an integer to be stored in register n |
|`OUTPUT Rn` | outputs the integer stored in register n to the user |
| `DUMP <dump mode>` | dumps the contents of either the registers or RAM (or both) to a file, `<dump mode>` can be either "memory", "registers" or "all" |
|`JMP <operand2>` | sets the program counter to `<operand2>`, useful for returning after a branch operation or creating a stack |
| `CDP Rn` | stores the program counters current value into register n, useful before a JUMP operation  |

### preprossesor flags

all preprossesor flags are indicated with a `*` , eg:
`*EXTENDED`

| flag | meaning |
|------|---------|
| `EXTENDED` | enables the use of the extended instruction set |
| `INCLUDE <path/to/file.aqa> <where>` |  include another .aqa file as part of the compiled machinecode, the position of the inserted code is determined by `<where>`, it can either be `FIRST` (at the start of the code), `HERE` (where the preprossesor flag is placed), or `LAST` (at the end of the code)



