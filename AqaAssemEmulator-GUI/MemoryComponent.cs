using AqaAssemEmulator_GUI.backend;
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
        private Label AddressLabel;
        private TextBox Value;
        public long data;
        private readonly int address;
        public Memory RAM;

        //this constructor is used to create a memory component pointing to a specific memory address
        public MemoryComponent(int address, long data, Point location, ref Memory ram)
        {
            this.address = address;
            AddressLabel = new Label();
            Value = new TextBox();
            InitializeComponent(address, data, location, ref ram);
        }

        //this constructor is used to create a blank memory component that is not pointing to any memory address
        public MemoryComponent(Point location, ref Memory ram)
        {
            this.address = -1;
            AddressLabel = new Label();
            Value = new TextBox();
            InitializeComponent(0, 0, location, ref ram);
            AddressLabel.Text = "";
            Value.Text = "";
            Value.Enabled = false;
        }

        private void InitializeComponent(int address, long data, Point location, ref Memory ram)
        {
            RAM = ram;

            this.AddressLabel = new Label();
            AddressLabel.Text = address.ToString();
            AddressLabel.BackColor = System.Drawing.Color.LightGray;

            this.Value = new TextBox();
            Value.Text = data.ToString();

            this.Location = location;

            Point AddressOffset = new(-1, 0);
            Point ValueOffset = new(-1,35);

            AddressLabel.Location = AddressOffset;
            Value.Location = ValueOffset;

            Size size = new Size(150, 70);
            this.Size = size;
            Value.Size = size;
            AddressLabel.Size = size;
            
            this.BorderStyle = BorderStyle.FixedSingle;

            Controls.Add(Value);
            Controls.Add(AddressLabel);
        }

        public void UpdateValue()
        {
            data = RAM.QuereyAddress(address);
            Value.Text = data.ToString();
        }
    }
}
