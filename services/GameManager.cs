using Sea_Battle.model;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Sea_Battle.services
{
    public class GameManager //управляет логикой игры
    { // игровые поля игрока и компьютера
        public GameBoard PlayerBoard { get; private set; }
        public GameBoard ComputerBoard { get; private set; }
        //указывает, чей сейчас ход
        public bool IsPlayerTurn { get; private set; }
        //текущее состояние игры
        public GameState State { get; private set; }
        //событие для обновления UI
        public event Action GameStateChanged;

        private Random _rand;
        private bool _isNewRulesMode;
        private int _sizeBoard; //Размер игрового поля
        private List<Cell> _computerKnownCells; // Клетки, которые компьютер узнал от мин
        private List<Cell> _playerKnownCells;   // Клетки, которые игрок узнал от мин

        public GameManager(bool isNewRulesMode, int SizeBoard) //конструктор
        {
            _isNewRulesMode = isNewRulesMode;
            _sizeBoard = SizeBoard;
            _rand = new Random();
            _computerKnownCells = new List<Cell>();
            _playerKnownCells = new List<Cell>();

            PlayerBoard = new GameBoard(_sizeBoard, true);  // Доска игрока
            ComputerBoard = new GameBoard(_sizeBoard, false); // Доска компьютера
            State = GameState.Setup;
            GameStateChanged?.Invoke();
        }

        // ДОБАВЛЕНО: Свойство для получения размера доски
        public int BoardSize => _sizeBoard;

        // начало игры
        public void StartGame()
        {
            // Автоматически расставляем корабли только для компьютера
            if (_isNewRulesMode)
            {
                AutoPlaceCurvedShips(ComputerBoard);
                AutoPlaceMines(ComputerBoard);
                AutoPlaceMines(PlayerBoard);
            }
            else
            {
                AutoPlaceShips(ComputerBoard);
                // В классическом режиме мины не расставляем
            }
            IsPlayerTurn = true;
            State = GameState.Playing;
            GameStateChanged?.Invoke();
        }

        //перезапустить игру
        public void ResetGame(bool isNewRulesMode, int SizeBoard)
        {
            _isNewRulesMode = isNewRulesMode;
            _sizeBoard = SizeBoard; // ИСПРАВЛЕНО: обновляем размер доски
            _computerKnownCells.Clear();
            _playerKnownCells.Clear();

            PlayerBoard = new GameBoard(_sizeBoard, true);
            ComputerBoard = new GameBoard(_sizeBoard, false);
            State = GameState.Setup;
            IsPlayerTurn = false;
            GameStateChanged?.Invoke();
        }

        // авто расстановка классических кораблей для компьютера
        private void AutoPlaceShips(GameBoard board)
        {
            board.Ships.Clear();
            ClearBoard(board);

            int[] shipSizes = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };
            Random rand = new Random();

            foreach (int size in shipSizes)
            {
                bool placed = false;
                int attempts = 0;

                while (!placed && attempts < 100)
                {
                    int row = rand.Next(0, _sizeBoard);
                    int col = rand.Next(0, _sizeBoard);
                    bool horizontal = rand.Next(0, 2) == 0;

                    placed = board.PlaceShip(row, col, size, horizontal);
                    attempts++;
                }
            }
        }


        // Авто-расстановка изогнутых кораблей для нового режима
        public void AutoPlaceCurvedShips(GameBoard board)
        {
            board.ClearBoard();

            var curvedShipTypes = new List<CurvedShipType>
    {
        CurvedShipType.Square,
        CurvedShipType.LShape,
        CurvedShipType.TShape,
        CurvedShipType.ZShape
    };

            foreach (var shipType in curvedShipTypes)
            {
                bool placed = false;
                int attempts = 0;

                while (!placed && attempts < 100)
                {
                    int row = _rand.Next(0, _sizeBoard - 3);
                    int col = _rand.Next(0, _sizeBoard - 3);

                    List<Cell> shipCells = null;
                    switch (shipType)
                    {
                        case CurvedShipType.Square:
                            shipCells = board.GetSquareCells(row, col);
                            break;
                        case CurvedShipType.LShape:
                            shipCells = board.GetLShapeCells(row, col);
                            break;
                        case CurvedShipType.TShape:
                            shipCells = board.GetTShapeCells(row, col);
                            break;
                        case CurvedShipType.ZShape:
                            shipCells = board.GetZShapeCells(row, col);
                            break;
                    }

                    if (shipCells != null && board.CanPlaceCurvedShip(shipCells))
                    {
                        board.PlaceCurvedShip(shipCells);
                        placed = true;
                    }
                    attempts++;
                }
            }
        }



        // Авто-расстановка мин для компьютера
        private void AutoPlaceMines(GameBoard board)
        {
            int minesToPlace = 3;
            int attempts = 0;

            while (minesToPlace > 0 && attempts < 100)
            {
                int row = _rand.Next(0, _sizeBoard);
                int col = _rand.Next(0, _sizeBoard);

                if (board.CanPlaceMine(row, col))
                {
                    var mine = new Mine();
                    board.PlaceMine(row, col, mine);
                    minesToPlace--;
                }
                attempts++;
            }
        }










        //очистка доски
        private void ClearBoard(GameBoard board)
        {
            for (int i = 0; i < _sizeBoard; i++)
            {
                for (int j = 0; j < _sizeBoard; j++)
                {
                    if (board.Cells[i, j].State == CellState.Ship || board.Cells[i, j].State == CellState.Mine)
                    {
                        board.Cells[i, j].State = CellState.Empty;
                        board.Cells[i, j].Ship = null;
                        board.Cells[i, j].Mine = null;
                    }
                }
            }
        }
        // выстрел игрока
        public bool PlayerShoot(int row, int col)
        {
            if (!IsPlayerTurn || State != GameState.Playing)
                return false;

            var result = ComputerBoard.Shoot(row, col);

            if (result.CellState == CellState.Hit)
            {
                if (ComputerBoard.AllShipsSunk())
                {
                    State = GameState.PlayerWon;
                }
                // При попадании в корабль ход остается у игрока
            }
            else if (result.CellState == CellState.MineHit)
            {
                // Попадание в мину - игрок сообщает компьютеру одну клетку своего корабля
                RevealCellToComputer();
                IsPlayerTurn = false; // Ход переходит к компьютеру
                Task.Delay(1000).ContinueWith(_ => Application.Current.Dispatcher.Invoke(ComputerShoot));
            }
            else if (result.CellState == CellState.Miss)
            {
                IsPlayerTurn = false;
                Task.Delay(1000).ContinueWith(_ => Application.Current.Dispatcher.Invoke(ComputerShoot));
            }

            GameStateChanged?.Invoke();
            return true;
        }
        // Игрок сообщает компьютеру одну клетку своего корабля
        private void RevealCellToComputer()
        {
            // Находим все неподбитые клетки кораблей игрока
            var availableCells = PlayerBoard.Ships
                .SelectMany(ship => ship.Cells)
                .Where(cell => cell.State == CellState.Ship)
                .ToList();

            if (availableCells.Any())
            {
                var randomCell = availableCells[_rand.Next(availableCells.Count)];
                _computerKnownCells.Add(randomCell);
            }
        }




        // Выстрел компьютера
        private void ComputerShoot()
        {
            if (State != GameState.Playing || IsPlayerTurn)
                return;

            int row, col;
            ShootResult result;

            // Сначала стреляем по известным клеткам от мин
            if (_computerKnownCells.Any())
            {
                var knownCell = _computerKnownCells.First();
                _computerKnownCells.Remove(knownCell);
                row = knownCell.Row;
                col = knownCell.Column;
            }
            else
            {
                // Случайный выстрел
                do
                {
                    row = _rand.Next(0, _sizeBoard);
                    col = _rand.Next(0, _sizeBoard);
                } while (PlayerBoard.Cells[row, col].State == CellState.Miss ||
                         PlayerBoard.Cells[row, col].State == CellState.Hit ||
                         PlayerBoard.Cells[row, col].State == CellState.MineHit);
            }

            result = PlayerBoard.Shoot(row, col);

            if (result.CellState == CellState.Hit)
            {
                if (PlayerBoard.AllShipsSunk())
                {
                    State = GameState.ComputerWon;
                }
                else
                {
                    // Компьютер продолжает ход при попадании в корабль
                    Task.Delay(800).ContinueWith(_ => Application.Current.Dispatcher.Invoke(ComputerShoot));
                }
            }
            else if (result.CellState == CellState.MineHit)
            {
                // Попадание в мину игрока - компьютер сообщает игроку одну клетку своего корабля
                RevealCellToPlayer();
                IsPlayerTurn = true; // Ход переходит к игроку
            }
            else if (result.CellState == CellState.Miss)
            {
                IsPlayerTurn = true;
            }

            GameStateChanged?.Invoke();
        }
        // Компьютер сообщает игроку одну клетку своего корабля
        private void RevealCellToPlayer()
        {
            // Находим все неподбитые клетки кораблей компьютера
            var availableCells = ComputerBoard.Ships
                .SelectMany(ship => ship.Cells)
                .Where(cell => cell.State == CellState.Ship)
                .ToList();

            if (availableCells.Any())
            {
                var randomCell = availableCells[_rand.Next(availableCells.Count)];
                _playerKnownCells.Add(randomCell);

                // Можно добавить визуальное выделение этой клетки на поле компьютера
                // Например, установить специальное состояние клетки
                randomCell.State = CellState.Forbidden; // Временно помечаем как запрещенную для визуализации
            }
        }

       
    }
    //состояния игры
    public enum GameState
    {
        Setup,
        Playing,
        PlayerWon,
        ComputerWon
    }
}