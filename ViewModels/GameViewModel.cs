using Sea_Battle.model;
using Sea_Battle.services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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

        public GameViewModel()
        {
            _gameManager = new GameManager();
            _shipPlacer = new ManualShipPlacer(_gameManager.PlayerBoard);
            _gameManager.GameStateChanged += OnGameStateChanged;

            StatusText = "Расставьте корабли и начните игру";
            StartButtonText = "Начать игру";
            IsManualPlacement = true;

            InitializeCommands();
            UpdateBoards();
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

        private void InitializeCommands()
        {
            StartGameCommand = new RelayCommand(StartGame);
            ComputerCellClickCommand = new RelayCommand<Cell>(ComputerCellClick);
            PlayerCellClickCommand = new RelayCommand<Cell>(PlayerCellClick);
            SelectShipCommand = new RelayCommand<ShipTemplate>(SelectShip);
            RotateShipCommand = new RelayCommand(RotateShip);
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
            _gameManager.ResetGame();
            _shipPlacer = new ManualShipPlacer(_gameManager.PlayerBoard);
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
        }

        private void SelectShip(ShipTemplate ship) => _shipPlacer.SelectShip(ship);

        private void RotateShip() => _shipPlacer.RotateShip();

        private void UpdateBoards()
        {
            PlayerCells.Clear();
            ComputerCells.Clear();

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
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
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}