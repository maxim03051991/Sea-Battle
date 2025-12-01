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
        private GameManager _gameManager; //управляет игровой логикой
        private ManualShipPlacer _shipPlacer; //отвечает за ручную расстановку кораблей
        private string _statusText; //текст статуса игры
        private string _startButtonText; //текст кнопки начала игры
        private bool _isManualPlacement; //флаг режима расстановки кораблей
        private bool _isNewRulesMode;
        private int _sizeBoard;

        public GameViewModel(bool isNewRulesMode)
        {
            _isNewRulesMode = isNewRulesMode;
            _sizeBoard = _isNewRulesMode ? 15 : 10;
            _gameManager = new GameManager(_isNewRulesMode, _sizeBoard);

            // Убрана автоматическая расстановка кораблей для игрока в новом режиме
            // Игрок теперь будет расставлять корабли вручную в обоих режимах

            _shipPlacer = new ManualShipPlacer(_isNewRulesMode, _gameManager.PlayerBoard);
            _gameManager.GameStateChanged += OnGameStateChanged;

            StatusText = "Расставьте корабли и начните игру";
            StartButtonText = "Начать игру";
            IsManualPlacement = true; // Всегда включаем ручную расстановку

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
                    _sizeBoard = 10; // В классическом режиме всегда 10
                else
                    _sizeBoard = value; // В новом режиме - выбранный размер

                OnPropertyChanged();
                OnPropertyChanged(nameof(AvailableBoardSizes)); // Уведомляем об изменении доступных размеров
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
            StartGameCommand = new RelayCommand(StartGame); //начало/сброс игры
            ComputerCellClickCommand = new RelayCommand<Cell>(ComputerCellClick); //клик по ячейке компьютера
            PlayerCellClickCommand = new RelayCommand<Cell>(PlayerCellClick);//клик по ячейке игрока
            SelectShipCommand = new RelayCommand<ShipTemplate>(SelectShip); //выбор корабля для расстановки
            RotateShipCommand = new RelayCommand(RotateShip); //поворот корабля
            ChangeBoardSizeCommand = new RelayCommand<object>(ChangeBoardSize); // выбор размера доски
        }
        private void ChangeBoardSize(object size)
        {
            int newSize;

            // Пытаемся преобразовать параметр в int разными способами
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


        private void StartGame() // начало игры
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

        // сброс игры
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

        // обработка ходов 
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

        //Расстановка кораблей
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

        // выбор корабля для расстановки
        private void SelectShip(ShipTemplate ship) => _shipPlacer.SelectShip(ship);

        //поворот корабля
        private void RotateShip() => _shipPlacer.RotateShip();

        private void ChangeBoardSize(int size)
        {
            SizeBoard = size; // ИСПРАВЛЕНО: используем свойство вместо поля
            ResetGame();
        }

        //Обновление досок
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