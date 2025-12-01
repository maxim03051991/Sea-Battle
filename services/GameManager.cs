using Sea_Battle.model;
using System.Windows;

namespace Sea_Battle.services
{
    public class GameManager //управляет логикой игры
    {
        // игровые поля игрока и компьютера
        public GameBoard PlayerBoard { get; private set; }
        public GameBoard ComputerBoard { get; private set; }

        //указывает, чей сейчас ход
        public bool IsPlayerTurn { get; private set; }

        //текущее состояние игры
        public GameState State { get; private set; }

        //событие для обновления UI
        public event Action GameStateChanged;

        // НОВОЕ: Событие для сообщений игроку
        public event Action<string> GameMessageChanged;

        // Флаг выбора клетки после попадания в мину
        public bool IsSelectingCellForMine { get; private set; }

        // Клетки, которые игрок показал компьютеру
        private List<Cell> _playerRevealedCells = new List<Cell>();

        // Клетки, которые компьютер показал игроку
        private List<Cell> _computerRevealedCells = new List<Cell>();

        private Random _rand;
        private bool _isNewRulesMode;
        private int _sizeBoard; //Размер игрового поля
        private List<Cell> _computerKnownCells; // Клетки, которые компьютер узнал от мин
        private List<Cell> _playerKnownCells;   // Клетки, которые игрок узнал от мин
        private Mine _lastMineHit;
        private GameBoard _lastMineHitBoard;

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
            IsSelectingCellForMine = false;
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
            _sizeBoard = SizeBoard;
            _computerKnownCells.Clear();
            _playerKnownCells.Clear();
            _playerRevealedCells.Clear();
            _computerRevealedCells.Clear();
            IsSelectingCellForMine = false;

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

            foreach (int size in shipSizes)
            {
                bool placed = false;
                int attempts = 0;

                while (!placed && attempts < 100)
                {
                    int row = _rand.Next(0, _sizeBoard);
                    int col = _rand.Next(0, _sizeBoard);
                    bool horizontal = _rand.Next(0, 2) == 0;

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
            if (!IsPlayerTurn || State != GameState.Playing || IsSelectingCellForMine)
                return false;

            var result = ComputerBoard.Shoot(row, col);

            if (result.CellState == CellState.Hit)
            {
                if (ComputerBoard.AllShipsSunk())
                {
                    State = GameState.PlayerWon;
                }
                // При попадании ход остается у игрока
            }
            else if (result.CellState == CellState.MineHit)
            {
                // Попадание в мину - игрок должен выбрать клетку для показа
                GameMessageChanged?.Invoke("Вы попали на мину! Выберите одну клетку своего корабля для показа компьютеру.");
                IsSelectingCellForMine = true;

                // Сохраняем информацию о мине
                _lastMineHit = result.Mine;
                _lastMineHitBoard = ComputerBoard;
            }
            else if (result.CellState == CellState.Miss)
            {
                IsPlayerTurn = false;
                Task.Delay(1000).ContinueWith(_ => Application.Current.Dispatcher.Invoke(ComputerShoot));
            }
            else if (result.CellState == CellState.RevealedShip && result.IsRevealedHit)
            {
                // Меткий выстрел по подсвеченной клетке - игрок получает еще один ход
                if (ComputerBoard.AllShipsSunk())
                {
                    State = GameState.PlayerWon;
                }
                // Ход остается у игрока (повторный ход)
            }

            GameStateChanged?.Invoke();
            return true;
        }

        // НОВЫЙ МЕТОД: игрок выбирает клетку для показа после попадания в мину
        public bool PlayerSelectCellForMine(int row, int col)
        {
            if (!IsSelectingCellForMine)
            {
                GameMessageChanged?.Invoke("Сейчас не время выбирать клетку для показа");
                return false;
            }

            var cell = PlayerBoard.Cells[row, col];

            // Проверяем, что клетка принадлежит неповрежденному кораблю
            if (cell.State != CellState.Ship)
            {
                GameMessageChanged?.Invoke("Выберите неповрежденную клетку своего корабля!");
                return false;
            }

            // Подсвечиваем клетку
            cell.State = CellState.RevealedShip;
            _playerRevealedCells.Add(cell);

            // Отмечаем мину как использованную
            if (_lastMineHit != null)
            {
                _lastMineHit.IsUsed = true;
                _lastMineHit.Cell.State = CellState.MineUsed;
                _lastMineHit.RevealedCell = cell;
            }

            IsSelectingCellForMine = false;
            GameMessageChanged?.Invoke($"Вы показали клетку [{row},{col}] компьютеру. Теперь его ход.");

            // Ход переходит к компьютеру
            IsPlayerTurn = false;
            Task.Delay(1000).ContinueWith(_ => Application.Current.Dispatcher.Invoke(() =>
            {
                ComputerShootAtRevealedCell(cell);
            }));

            GameStateChanged?.Invoke();
            return true;
        }

        // Компьютер стреляет по подсвеченной клетке
        private void ComputerShootAtRevealedCell(Cell revealedCell)
        {
            // Стреляем по подсвеченной клетке
            var result = PlayerBoard.Shoot(revealedCell.Row, revealedCell.Column);

            if (result.CellState == CellState.Hit)
            {
                GameMessageChanged?.Invoke("Компьютер попал по подсвеченной клетке!");

                // Проверяем, потоплен ли корабль
                if (PlayerBoard.AllShipsSunk())
                {
                    State = GameState.ComputerWon;
                }
                else
                {
                    // Компьютер получает повторный ход
                    Task.Delay(1000).ContinueWith(_ => Application.Current.Dispatcher.Invoke(ComputerShoot));
                }
            }

            GameStateChanged?.Invoke();
        }

        // Выстрел компьютера
        private void ComputerShoot()
        {
            if (State != GameState.Playing || IsPlayerTurn)
                return;

            int row, col;
            ShootResult result;

            // Сначала проверяем, есть ли подсвеченные клетки от игрока
            if (_playerRevealedCells.Any(cell => cell.State == CellState.RevealedShip))
            {
                var revealedCell = _playerRevealedCells.First(cell => cell.State == CellState.RevealedShip);
                row = revealedCell.Row;
                col = revealedCell.Column;
                _playerRevealedCells.Remove(revealedCell);
            }
            else
            {
                // Обычная логика выстрела
                do
                {
                    row = _rand.Next(0, _sizeBoard);
                    col = _rand.Next(0, _sizeBoard);
                } while (PlayerBoard.Cells[row, col].State == CellState.Miss ||
                         PlayerBoard.Cells[row, col].State == CellState.Hit ||
                         PlayerBoard.Cells[row, col].State == CellState.MineHit ||
                         PlayerBoard.Cells[row, col].State == CellState.MineUsed);
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
                    // Компьютер продолжает ход при попадании
                    Task.Delay(800).ContinueWith(_ => Application.Current.Dispatcher.Invoke(ComputerShoot));
                }
            }
            else if (result.CellState == CellState.MineHit)
            {
                // Компьютер попал в мину - он должен показать клетку
                var mine = result.Mine;
                var availableCells = ComputerBoard.Ships
                    .SelectMany(ship => ship.Cells)
                    .Where(cell => cell.State == CellState.Ship)
                    .ToList();

                if (availableCells.Any())
                {
                    var randomCell = availableCells[_rand.Next(availableCells.Count)];
                    randomCell.State = CellState.RevealedShip;
                    _computerRevealedCells.Add(randomCell);

                    // Отмечаем мину как использованную
                    mine.IsUsed = true;
                    mine.Cell.State = CellState.MineUsed;
                    mine.RevealedCell = randomCell;

                    GameMessageChanged?.Invoke($"Компьютер попал на вашу мину! Он показал клетку: [{randomCell.Row},{randomCell.Column}]");

                    // Ход переходит к игроку, который может выстрелить по подсвеченной клетке
                    IsPlayerTurn = true;
                }
            }
            else if (result.CellState == CellState.Miss)
            {
                IsPlayerTurn = true;
                GameMessageChanged?.Invoke("Ваш ход");
            }
            else if (result.CellState == CellState.RevealedShip && result.IsRevealedHit)
            {
                // Меткий выстрел по подсвеченной клетке
                GameMessageChanged?.Invoke("Компьютер попал по подсвеченной клетке!");
                // Компьютер получает повторный ход
                Task.Delay(800).ContinueWith(_ => Application.Current.Dispatcher.Invoke(ComputerShoot));
            }

            GameStateChanged?.Invoke();
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