using System.Collections.Generic;
using System.Linq;

namespace Sea_Battle.model
{
    public class Ship
    {
        public int Size { get; set; }
        public List<Cell> Cells { get; set; } = new List<Cell>();
        public bool IsSunk => Cells.All(c => c.State == CellState.Hit);

        public Ship(int size)
        {
            Size = size;
        }
    }
}