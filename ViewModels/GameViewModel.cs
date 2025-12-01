using Sea_Battle.model;
using Sea_Battle.services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Sea_Battle.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private GameManager _gameManager;
        private ManualShipPlacer _shipPlacer;
        private string _statusText;
        private string _startButtonText;
        private bool _isManualPlacement;
        private bool _isNewRulesMode;
        private int _sizeBoard;
        public bool IsPlayerBoardInteractive => IsManualPlacement ||
                                       (_gameManager.State == GameState.Playing &&
                                        _gameManager.IsSelectingCellForMine);

        public GameViewModel(bool isNewRulesMode)
        {
            _isNewRulesMode = isNewRulesMode;
            _sizeBoard = _isNewRulesMode ? 15 : 10;
            _gameManager = new GameManager(_isNewRulesMode, _sizeBoard);

            _shipPlacer = new ManualShipPlacer(_isNewRulesMode, _gameManager.PlayerBoard);

            // Подписываемся на события
            _gameManager.GameStateChanged += OnGameStateChanged;
            _gameManager.GameMessageChanged += OnGameMessageChanged;

            StatusText = "Расставьте корабли и начните игру";
            StartButtonText = "Начать игру";
            IsManualPlacement = true;

            InitializeCommands();
            UpdateBoards();
        }

        private void OnGameMessageChanged(string message)
        {
            // Устанавливаем сообщение от GameManager
            StatusText = message;
        }

        public string StartButtonText
        {
            get => _startButtonText;
            set
            {
                _startButtonText = value;
                OnPropertyChanged();
            }
        }

        public bool IsManualPlacement
        {
            get => _isManualPlacement;
            set
            {
                _isManualPlacement = value;
                OnPropertyChanged();
            }
        }

        public bool IsNewRulesMode
        {
            get => _isNewRulesMode;
            set
            {
                _isNewRulesMode = value;
                OnPropertyChanged();
            }
        }

        public int SizeBoard
        {
            get => _sizeBoard;
            set
            {
                if (!_isNewRulesMode)
                    _sizeBoard = 10;
                else
                    _sizeBoard = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(AvailableBoardSizes));
            }
        }

        public List<int> AvailableBoardSizes => new List<int> { 15, 16, 17, 18, 19, 20 };

        public ManualShipPlacer ShipPlacer => _shipPlacer;
        public ObservableCollection<Cell> PlayerCells { get; } = new ObservableCollection<Cell>();
        public ObservableCollection<Cell> ComputerCells { get; } = new ObservableCollection<Cell>();

        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        public ICommand StartGameCommand { get; private set; }
        public ICommand ComputerCellClickCommand { get; private set; }
        public ICommand PlayerCellClickCommand { get; private set; }
        public ICommand SelectShipCommand { get; private set; }
        public ICommand RotateShipCommand { get; private set; }
        public ICommand ChangeBoardSizeCommand { get; private set; }

        private void InitializeCommands()
        {
            StartGameCommand = new RelayCommand(StartGame);
            ComputerCellClickCommand = new RelayCommand<Cell>(ComputerCellClick);
            PlayerCellClickCommand = new RelayCommand<Cell>(PlayerCellClick);
            SelectShipCommand = new RelayCommand<ShipTemplate>(SelectShip);
            RotateShipCommand = new RelayCommand(RotateShip);
            ChangeBoardSizeCommand = new RelayCommand<object>(ChangeBoardSize);
        }

        private void ChangeBoardSize(object size)
        {
            int newSize;

            if (size is int)
                newSize = (int)size;
            else if (size is string str)
            {
                if (!int.TryParse(str, out newSize))
                {
                    return;
                }
            }
            else
                return;

            SizeBoard = newSize;
            ResetGame();
        }

        private void StartGame()
        {
            if (_gameManager.State != GameState.Setup)
            {
                ResetGame();
                return;
            }

            if (IsManualPlacement && !AllShipsPlaced())
            {
                StatusText = "Сначала расставьте все корабли!";
                return;
            }

            _gameManager.StartGame();
            IsManualPlacement = false;
            UpdateStatus();
            UpdateBoards();
            UpdateShipCounts();
            StartButtonText = "Начать новую игру";
        }

        private void ResetGame()
        {
            _gameManager.ResetGame(_isNewRulesMode, _sizeBoard);
            _shipPlacer = new ManualShipPlacer(_isNewRulesMode, _gameManager.PlayerBoard);
            OnPropertyChanged(nameof(ShipPlacer));

            IsManualPlacement = true;
            UpdateStatus();
            UpdateBoards();
            UpdateShipCounts();
            StartButtonText = "Начать игру";
            StatusText = "Расставьте корабли и начните игру";
        }

        private void ComputerCellClick(Cell cell)
        {
            if (_gameManager.State == GameState.Playing &&
                _gameManager.IsPlayerTurn &&
                cell.State != CellState.Hit &&
                cell.State != CellState.Miss)
            {
                _gameManager.PlayerShoot(cell.Row, cell.Column);
                UpdateStatus();
                UpdateBoards();
            }
        }

        private void PlayerCellClick(Cell cell)
        {
            // Режим ручной расстановки
            if (IsManualPlacement && _shipPlacer.SelectedShip != null)
            {
                if (_shipPlacer.PlaceShip(cell.Row, cell.Column))
                {
                    UpdateBoards();
                    if (AllShipsPlaced())
                        StatusText = "Все корабли расставлены! Нажмите 'Начать игру'";
                }
                else
                {
                    StatusText = "Нельзя разместить корабль здесь!";
                }
            }
            // Режим выбора клетки после попадания в мину
            else if (_gameManager.State == GameState.Playing &&
                     _gameManager.IsSelectingCellForMine &&
                     cell.IsPlayerBoard &&
                     cell.State == CellState.Ship) // Проверяем, что это неповрежденная клетка корабля
            {
                _gameManager.PlayerSelectCellForMine(cell.Row, cell.Column);
                UpdateBoards();
                UpdateStatus();
            }
        }

        private void SelectShip(ShipTemplate ship) => _shipPlacer.SelectShip(ship);

        private void RotateShip() => _shipPlacer.RotateShip();

        private void UpdateBoards()
        {
            PlayerCells.Clear();
            ComputerCells.Clear();

            for (int i = 0; i < _sizeBoard; i++)
            {
                for (int j = 0; j < _sizeBoard; j++)
                {
                    PlayerCells.Add(_gameManager.PlayerBoard.Cells[i, j]);
                    ComputerCells.Add(_gameManager.ComputerBoard.Cells[i, j]);
                }
            }
        }

        private void UpdateShipCounts() => OnPropertyChanged(nameof(ShipPlacer));

        private bool AllShipsPlaced() => _shipPlacer.AvailableShips.All(ship => ship.Count == 0);

        private void UpdateStatus()
        {
            // Если игрок выбирает клетку для мины, показываем специальное сообщение
            if (_gameManager.IsSelectingCellForMine)
            {
                StatusText = "Вы попали на мину! Выберите одну клетку своего корабля для показа компьютеру.";
                return;
            }

            StatusText = _gameManager.State switch
            {
                GameState.Setup => "Расставьте корабли и начните игру",
                GameState.Playing => _gameManager.IsPlayerTurn ? "Ваш ход" : "Ход компьютера",
                GameState.PlayerWon => "Вы победили!",
                GameState.ComputerWon => "Компьютер победил!",
                _ => "Неизвестное состояние"
            };
        }

        private void OnGameStateChanged()
        {
            UpdateStatus();
            UpdateBoards();
            // Уведомляем об изменении свойства IsPlayerBoardInteractive
            OnPropertyChanged(nameof(IsPlayerBoardInteractive));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}