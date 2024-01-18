using AqaAssemEmulator_GUI.backend;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AqaAssemEmulator_GUI
{
    public partial class RamGrid : Form
    {
        Memory RAM;
        MemoryGrid grid;

        internal RamGrid(ref Memory ram)
        {
            Point gridLocation = new Point(-1, 0);

            RAM = ram;
            grid = new MemoryGrid(ref RAM, gridLocation);
            InitializeComponent();
            this.FormClosing += RamGrid_FormClosing;
        }

        private void RamGrid_Load(object sender, EventArgs e)
        {
            this.Controls.Add(grid);

            Size gridSize = new Size (grid.Size.Width + 25, grid.Size.Height + 70);
            this.Size = gridSize;

            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximumSize = gridSize;
            this.MinimumSize = gridSize;

            this.Text = "RAM";

            this.BringToFront();
        }

        public void UpdateGrid()
        {
            grid.UpdateMemory();
        }

        private void RamGrid_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;

                this.Hide();
            }
        }
        

        private void RamGrid_OnShown(object sender, EventArgs e)
        {
            this.BringToFront();
        }
    }
}
