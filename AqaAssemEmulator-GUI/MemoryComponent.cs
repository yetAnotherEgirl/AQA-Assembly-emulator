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
        private int address;
        public Memory RAM;

        public MemoryComponent(int address, long data, Point location, ref Memory ram)
        {
            this.address = address;
            AddressLabel = new Label();
            Value = new TextBox();
            InitializeComponent(address, data, location, ref ram);
        }
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

            Point AddressOffset = new Point(-1, 0);
            Point ValueOffset = new Point(-1,35);

            AddressLabel.Location = AddressOffset;
            Value.Location = ValueOffset;

            Size size = new Size(150, 70);
            this.Size = size;
            Value.Size = size;
            AddressLabel.Size = size;

        /*  this.Address.Location = new Point(location.X + AddressOffset.X,
         *                                    location.Y + AddressOffset.Y);
         *
         *  this.Value.Location = new Point(location.X + ValueOffset.X,
         *                                  location.Y + ValueOffset.Y);
         *                                  
         *  this caused wacky shit to happen
        */
            
            this.BorderStyle = BorderStyle.FixedSingle;

            Controls.Add(Value);
            Controls.Add(AddressLabel);

//            Value.TextChanged += Value_TextChanged;
        }
 
//       fucks up UpdateValue(), find new method for input validation
//
//       private void Value_TextChanged(object sender, EventArgs e)
//       {
//           if (address == -1) return;
//
//           if (Value.Text.Length == 0) Value.Text = data.ToString();
//
//           if (!int.TryParse(Value.Text, out int result)) Value.Text = data.ToString();
//
//           data = result;
//
//           long addressLong = (long)address;
//           long valueLong = (long)data;
//
//           RAM.SetAddress(addressLong, valueLong);
//       }

        public void UpdateValue()
        {
            data = RAM.QuereyAddress(address);
            Value.Text = data.ToString();
        }
    }
}
