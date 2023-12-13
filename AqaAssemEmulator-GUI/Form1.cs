namespace AqaAssemEmulator_GUI
{
    public partial class Window : Form
    {

        // readonly Size size = new Size(600, 600) * 2;

        public int[] fakeRam = FakeArray(200);
        Tuple<int, int> RamTblSize = new Tuple<int, int>(0, 0);

        public Window()
        {
            InitializeComponent();
            Size currentSize = this.Size;
            this.MaximumSize = currentSize;
            this.MinimumSize = currentSize;
            RamTblSize = GetRamTblSize(fakeRam.Length);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private Tuple<int, int> GetRamTblSize(int size)
        {
            Tuple<int, int> result = new Tuple<int, int>(0, 0);

            int width = (int)Math.Ceiling(Math.Sqrt(size));
            int height = (int)Math.Ceiling((double)size / width);

            result = new Tuple<int, int>(width, height);
            return result;
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (AssemblyTextBox.Text == "")
            {
                MessageBox.Show("Assembly cannot be empty",
                                "Save Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                return;
            }

            SaveAssembly.Filter = "Assembly Files (*.aqa)|*.aqa";
            SaveAssembly.DefaultExt = "aqa";
            SaveAssembly.Title = "Save Assembly";
            SaveAssembly.ShowDialog();

            if (SaveAssembly.FileName != "")
            {
                File.WriteAllText(SaveAssembly.FileName, AssemblyTextBox.Text);
            }
            MessageBox.Show("Assembly saved successfully",
                                           "Save Success",
                                           MessageBoxButtons.OK,
                                           MessageBoxIcon.Information);
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            OpenAssembly.Filter = "Assembly Files (*.aqa)|*.aqa";
            OpenAssembly.DefaultExt = "aqa";
            OpenAssembly.Title = "Open Assembly";
            OpenAssembly.ShowDialog();

            if (OpenAssembly.FileName != "")
            {
                AssemblyTextBox.Text = File.ReadAllText(OpenAssembly.FileName);
            }
        }

        //remove when backend added
        public static int[] FakeArray(int size)
        {
            int[] result = new int[size];
            for (int i = 0; i < size; i++)
            {
                result[i] = i;
            }
            return result;
        }
    }
}
