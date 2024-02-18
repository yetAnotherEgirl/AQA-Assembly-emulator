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
                int x = i % GridWidth;
                int y = i / GridWidth;
                Point location = new Point(Location.X + (x * 150), Location.Y + (y * 70));
                if (i < memory.GetLength())
                {
                    MemoryComponents[x, y] = new MemoryComponent(i, memory.QuereyAddress(i), location, ref memory);
                }
                else
                {
                    MemoryComponents[x, y] = new MemoryComponent(location, ref memory);
                }
                Controls.Add(MemoryComponents[x, y]);
            }

            this.Size = new Size(GridWidth * 150, GridHieght * 70);
        }

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
