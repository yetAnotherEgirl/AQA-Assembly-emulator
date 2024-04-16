namespace AqaAssemEmulator_GUI
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            //one potential idea is to have the option to pass a path 
            //to a .aqa file as an argument to the program
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            ApplicationConfiguration.Initialize();
            Application.Run(new Window());
        }
    }
}