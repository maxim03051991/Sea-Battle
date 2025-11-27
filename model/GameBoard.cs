using System.Linq;

namespace Sea_Battle.model
{
    public class GameBoard
    {
        public Cell[,] Cells { get; private set; } //Двумерный массив ячеек игрового поля 
        public List<Ship> Ships { get; private set; } = new List<Ship>(); //Список всех кораблей на доске
        public int SizeBoard { get; private set; } = 10; //Размер игрового поля
        public bool IsPlayerBoard { get; set; } //Флаг указывающий принадлежность доски

        public GameBoard(bool isPlayerBoard = true) //Конструктор и инициализация
        {
            IsPlayerBoard = isPlayerBoard;
            InitializeBoard();
        }
        // Создает пустое игровое поле и для каждой ячейки устанавливает координаты и принадлежность доски
        private void InitializeBoard() 
        {
            Cells = new Cell[SizeBoard, SizeBoard];
            for (int i = 0; i < SizeBoard; i++)
            {
                for (int j = 0; j < SizeBoard; j++)
                {
                    Cells[i, j] = new Cell
                    {
                        Row = i,
                        Column = j,
                        IsPlayerBoard = IsPlayerBoard
                    };
                }
            }
        }
        // метод размещения корблей
        public bool PlaceShip(int startRow, int startCol, int size, bool isHorizontal)
        { //проверка на возможность постановки корабря
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
        // метод проверки возможности установления корабля
        private bool CanPlaceShip(int startRow, int startCol, int size, bool isHorizontal)
        {
            for (int i = 0; i < size; i++)
            {
                int row = isHorizontal ? startRow : startRow + i;
                int col = isHorizontal ? startCol + i : startCol;

                if (row >= SizeBoard || col >= SizeBoard) return false;

                for (int r = row - 1; r <= row + 1; r++)
                {
                    for (int c = col - 1; c <= col + 1; c++)
                    {
                        if (r >= 0 && r < SizeBoard && c >= 0 && c < SizeBoard && Cells[r, c].State == CellState.Ship)
                            return false;
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
        //метод для проверки, все ли корабли потоплены
        public bool AllShipsSunk() => Ships.All(ship => ship.IsSunk);
    }
}