using System.Linq;
using System.Windows.Shapes;

namespace Sea_Battle.model
{
    public class GameBoard
    {
    
        public Cell[,] Cells { get; private set; } //Двумерный массив ячеек игрового поля 
        public List<Ship> Ships { get; private set; } = new List<Ship>(); //Список всех кораблей на доске
        public List<Mine> Mines { get; private set; } = new List<Mine>();// Мины
        public bool IsPlayerBoard { get; set; } //Флаг указывающий принадлежность доски

        private int _sizeBoard; //Размер игрового поля

       
        public GameBoard(int SizeBoard, bool isPlayerBoard = true) //Конструктор и инициализация
        {
            _sizeBoard = SizeBoard;
            IsPlayerBoard = isPlayerBoard;
            InitializeBoard();
        }
        // Создает пустое игровое поле и для каждой ячейки устанавливает координаты и принадлежность доски
        private void InitializeBoard() 
        {
            Cells = new Cell[_sizeBoard, _sizeBoard];
            for (int i = 0; i < _sizeBoard; i++)
            {
                for (int j = 0; j < _sizeBoard; j++)
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

                if (row >= _sizeBoard || col >= _sizeBoard) return false;

                for (int r = row - 1; r <= row + 1; r++)
                {
                    for (int c = col - 1; c <= col + 1; c++)
                    {
                        if (r >= 0 && r < _sizeBoard && c >= 0 && c < _sizeBoard && Cells[r, c].State == CellState.Ship)
                            return false;
                    }
                }
            }
            return true;
        }
        //зазмещение изогнутого корабя
        public bool PlaceCurvedShip(List<Cell> shipCells)
        {
            if (!CanPlaceCurvedShip(shipCells))
                return false;

            var ship = new Ship(shipCells.Count);
            foreach (var cell in shipCells)
            {
                cell.State = CellState.Ship;
                cell.Ship = ship;
                ship.Cells.Add(cell);
            }

            Ships.Add(ship);
            return true;
        }
        //можно ли разместить изогнутый корабль
        public bool CanPlaceCurvedShip(List<Cell> shipCells)
        {
            foreach (var cell in shipCells)
            {
                if (cell.Row >= _sizeBoard || cell.Column >= _sizeBoard || cell.State != CellState.Empty)
                    return false;

                for (int r = cell.Row - 1; r <= cell.Row + 1; r++)
                {
                    for (int c = cell.Column - 1; c <= cell.Column + 1; c++)
                    {
                        if (r >= 0 && r < _sizeBoard && c >= 0 && c < _sizeBoard)
                        {
                            var nearbyCell = Cells[r, c];
                            if ((nearbyCell.State == CellState.Ship || nearbyCell.State == CellState.Mine) && !shipCells.Contains(nearbyCell))
                                return false;
                        }
                    }
                }
            }
            return true;
        }
        // размещение мины
        public bool PlaceMine(int row, int col, Mine mine)
        {
            if (!CanPlaceMine(row, col))
                return false;

            var cell = Cells[row, col];
            cell.State = CellState.Mine;
            cell.Mine = mine;
            mine.Cell = cell;
            Mines.Add(mine);
            return true;
        }
        //можно ли разместить мину
        public bool CanPlaceMine(int row, int col)
        {
            if (row >= _sizeBoard || col >= _sizeBoard || Cells[row, col].State != CellState.Empty)
                return false;

            for (int r = row - 1; r <= row + 1; r++)
            {
                for (int c = col - 1; c <= col + 1; c++)
                {
                    if (r >= 0 && r < _sizeBoard && c >= 0 && c < _sizeBoard)
                    {
                        var cell = Cells[r, c];
                        if (cell.State == CellState.Ship || cell.State == CellState.Mine)
                            return false;
                    }
                }
            }
            return true;
        }
        // результат выстрела
        public ShootResult Shoot(int row, int col)
        {
            var cell = Cells[row, col];
            var result = new ShootResult();

            if (cell.State == CellState.Ship)
            {
                cell.State = CellState.Hit;
                result.CellState = CellState.Hit;
                result.Ship = cell.Ship;

                if (cell.Ship.IsSunk)
                {
                    result.IsShipSunk = true;
                }
            }
            else if (cell.State == CellState.Mine)
            {
                cell.State = CellState.MineHit;
                result.CellState = CellState.MineHit;
                result.Mine = cell.Mine;
                cell.Mine.IsUsed = true;
            }
            else if (cell.State == CellState.Empty)
            {
                cell.State = CellState.Miss;
                result.CellState = CellState.Miss;
            }
            else
            {
                result.CellState = cell.State;
            }

            return result;
        }

                //метод для проверки, все ли корабли потоплены
        public bool AllShipsSunk() => Ships.All(ship => ship.IsSunk);


        //очистка доски
        public void ClearBoard()
        {
            // Очищаем корабли
            foreach (var ship in Ships.ToList())
            {
                foreach (var cell in ship.Cells)
                {
                    cell.State = CellState.Empty;
                    cell.Ship = null;
                }
            }
            Ships.Clear();

            // Очищаем мины
            foreach (var mine in Mines.ToList())
            {
                if (mine.Cell != null)
                {
                    mine.Cell.State = CellState.Empty;
                    mine.Cell.Mine = null;
                    mine.Cell = null;
                }
                mine.IsActive = true;
                mine.IsUsed = false;
            }
            Mines.Clear();
        }

        //******************виды изогунтых кораблей**************************
        // квадратный корабль
        public List<Cell> GetSquareCells(int startRow, int startCol)
        {
            var cells = new List<Cell>();
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    int row = startRow + i;
                    int col = startCol + j;
                    if (row < _sizeBoard && col < _sizeBoard)
                        cells.Add(Cells[row, col]);
                    else
                        return null;
                }
            }
            return cells;
        }

        public List<Cell> GetLShapeCells(int startRow, int startCol)
        {
            var cells = new List<Cell>();

            // Г-образная форма (3 клетки вниз, 1 вправо)
            int[,] pattern = {
            {0, 0}, {1, 0}, {2, 0}, {0, 1}
        };

            for (int i = 0; i < pattern.GetLength(0); i++)
            {
                int row = startRow + pattern[i, 0];
                int col = startCol + pattern[i, 1];
                if (row < _sizeBoard && col < _sizeBoard)
                    cells.Add(Cells[row, col]);
                else
                    return null;
            }
            return cells;
        }

        public List<Cell> GetTShapeCells(int startRow, int startCol)
        {
            var cells = new List<Cell>();

            // T-образная форма
            int[,] pattern = {
            {2, 1}, {1, 0}, {1, 1}, {1, 2},
        };

            for (int i = 0; i < pattern.GetLength(0); i++)
            {
                int row = startRow + pattern[i, 0];
                int col = startCol + pattern[i, 1];
                if (row < _sizeBoard && col < _sizeBoard)
                    cells.Add(Cells[row, col]);
                else
                    return null;
            }
            return cells;
        }

        public List<Cell> GetZShapeCells(int startRow, int startCol)
        {
            var cells = new List<Cell>();

            // Z-образная форма
            int[,] pattern = {
            {0, 0}, {0, 1}, {1, 1}, {1, 2},
        };

            for (int i = 0; i < pattern.GetLength(0); i++)
            {
                int row = startRow + pattern[i, 0];
                int col = startCol + pattern[i, 1];
                if (row < _sizeBoard && col < _sizeBoard)
                    cells.Add(Cells[row, col]);
                else
                    return null;
            }
            return cells;
        }





    }
    public class ShootResult
    {
        public CellState CellState { get; set; }
        public Ship Ship { get; set; }
        public Mine Mine { get; set; }
        public bool IsShipSunk { get; set; }
    }
}