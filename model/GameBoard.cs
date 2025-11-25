using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sea_Battle.model
{
    // Объявление публичного класса GameBoard, представляющего игровое поле
    public class GameBoard
    {
        public Cell[,] Cells { get; private set; }
        public List<Ship> Ships { get; private set; } = new List<Ship>();
        public int Size { get; private set; } = 10;

        // Свойство для определения, чье это поле (игрока или компьютера)
        public bool IsPlayerBoard { get; set; }

        public GameBoard(bool isPlayerBoard = true)
        {
            IsPlayerBoard = isPlayerBoard;
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            Cells = new Cell[Size, Size];
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    Cells[i, j] = new Cell
                    {
                        Row = i,
                        Column = j,
                        IsPlayerBoard = this.IsPlayerBoard
                    };
                }
            }
        }

        // Остальные методы остаются без изменений...
        public bool PlaceShip(int startRow, int startCol, int size, bool isHorizontal)
        {
            if (!CanPlaceShip(startRow, startCol, size, isHorizontal))
                return false;

            var ship = new Ship(size);

            for (int i = 0; i < size; i++)
            {
                int row = isHorizontal ? startRow : startRow + i;
                int col = isHorizontal ? startCol + i : startCol;

                var cell = Cells[row, col];
                cell.State = CellState.Ship;
                cell.Ship = ship;
                ship.Cells.Add(cell);
            }

            Ships.Add(ship);
            return true;
        }

        private bool CanPlaceShip(int startRow, int startCol, int size, bool isHorizontal)
        {
            for (int i = 0; i < size; i++)
            {
                int row = isHorizontal ? startRow : startRow + i;
                int col = isHorizontal ? startCol + i : startCol;

                if (row >= Size || col >= Size) return false;

                for (int r = row - 1; r <= row + 1; r++)
                {
                    for (int c = col - 1; c <= col + 1; c++)
                    {
                        if (r >= 0 && r < Size && c >= 0 && c < Size)
                        {
                            if (Cells[r, c].State == CellState.Ship)
                                return false;
                        }
                    }
                }
            }
            return true;
        }

        public CellState Shoot(int row, int col)
        {
            var cell = Cells[row, col];

            if (cell.State == CellState.Ship)
            {
                cell.State = CellState.Hit;
                return CellState.Hit;
            }
            else if (cell.State == CellState.Empty)
            {
                cell.State = CellState.Miss;
                return CellState.Miss;
            }

            return cell.State;
        }

        public bool AllShipsSunk()
        {
            return Ships.All(ship => ship.IsSunk);
        }
    }
}