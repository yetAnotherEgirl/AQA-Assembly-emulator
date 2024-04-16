using AqaAssemEmulator_GUI.backend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AqaAssemEmulator_GUI
{
    internal class MemoryGrid : UserControl
    {
        private Memory memory;
        private int GridWidth;
        private int GridHieght;

        private MemoryComponent[,] MemoryComponents;

        public MemoryGrid(ref Memory memory, Point Location)
        {
            InitializeComponent(ref memory, Location);
        }

        private void InitializeComponent(ref Memory memory, Point Location)
        {
            this.memory = memory;
            GetDimensions();

            MemoryComponents = new MemoryComponent[GridWidth, GridHieght];
            int GridSize = GridWidth * GridHieght;

            for (int i = 0; i < GridSize; i++)
            {
                //split i into x and y coordinates
                int x = i % GridWidth;
                int y = i / GridWidth;

                Point location = new(Location.X + (x * 150), Location.Y + (y * 70));

                if (i < memory.GetLength())
                {
                    //this will show a real memory address
                    MemoryComponents[x, y] = new MemoryComponent(i, memory.QuereyAddress(i), location, ref memory);
                }
                else
                {
                    //this will just be a blank placeholder
                    MemoryComponents[x, y] = new MemoryComponent(location, ref memory);
                }
                Controls.Add(MemoryComponents[x, y]);
            }

            this.Size = new Size(GridWidth * 150, GridHieght * 70);
        }

        //this calculates the dimensions of the grid, rather than returning the dimensions
        private void GetDimensions()
        {
            int size = memory.GetLength();
            GridWidth = (int)Math.Ceiling(Math.Sqrt(size));
            GridHieght = (int)Math.Ceiling((double)size / GridWidth);
        }

        public void UpdateMemory()
        {
            for (int i = 0; i < memory.GetLength(); i++)
            {
                int x = i % GridWidth;
                int y = i / GridWidth;
                MemoryComponents[x, y].UpdateValue();
            }
        }
    }
}
