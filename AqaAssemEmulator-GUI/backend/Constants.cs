namespace AqaAssemEmulator_GUI.backend;

internal static class Constants
{
    public const char commentChar = ';';
    public const char preProcessorIndicator = '*';
    public const char decimalChar = '#';
    public const int decimalIndicator = 1;
    public const char registerChar = 'r';
    public const int registerIndicator = 2;
    public const int addressIndicator = 0;

    public const int bitsPerNibble = 4;
    public const int opCodeOffset = 14;
    public const int signBitOffset = 11;
    public const int registerOffset = 12;
}


enum CPSRFlags
{
    Zero = 0,
    Negative = 1,
    Overflow = 2,
    None = 3
}
