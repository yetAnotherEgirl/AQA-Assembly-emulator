using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AqaAssemEmulator_GUI
{
    internal class MemoryComponent : UserControl
    {
        private Label Address;
        private TextBox Value;
        public int data;

        public MemoryComponent(int address, ref int data, Point location)
        {
            InitializeComponent(address, ref data, location);
        }
        public MemoryComponent(Point location)
        {
            int x = 0;
            InitializeComponent(0, ref x, location);
            Address.Text = "";
            Value.Text = "";
            Value.Enabled = false;
        }

        private void InitializeComponent(int address, ref int data, Point location)
        {
            this.Address = new Label();
            Address.Text = address.ToString();
            Address.BackColor = System.Drawing.Color.LightGray;

            this.Value = new TextBox();
            Value.Text = data.ToString();

            this.Location = location;

            Point AddressOffset = new Point(-1, 0);
            Point ValueOffset = new Point(-1,35);
            Size size = new Size(150, 70);
            this.Size = size;
            Value.Size = size;
            Address.Size = size;

            this.Address.Location = new Point(location.X + AddressOffset.X,
                                              location.Y + AddressOffset.Y);

            this.Value.Location = new Point(location.X + ValueOffset.X,
                                            location.Y + ValueOffset.Y);

            this.BorderStyle = BorderStyle.FixedSingle;

            Controls.Add(this.Value);
            Controls.Add(this.Address);
        }
    }
}
