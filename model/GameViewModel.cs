using Sea_Battle.services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Sea_Battle.model
{
    public class GameViewModel : INotifyPropertyChanged
    {
        private GameManager _gameManager;
        private string _statusText;
        private string _startButtonText;

        public GameViewModel()
        {
            _gameManager = new GameManager();
            _gameManager.GameStateChanged += OnGameStateChanged;
            StatusText = "Расставьте корабли и начните игру";
            StartButtonText = "Начать игру"; // начальный текст
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

        private void OnGameStateChanged()
        {
            UpdateStatus();
            UpdateBoards();

        }

        public ObservableCollection<Cell> PlayerCells { get; set; } = new ObservableCollection<Cell>();
        public ObservableCollection<Cell> ComputerCells { get; set; } = new ObservableCollection<Cell>();

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

        private void InitializeCommands()
        {
            StartGameCommand = new RelayCommand(StartGame);
            ComputerCellClickCommand = new RelayCommand<Cell>(ComputerCellClick);
        }

        private void StartGame()
        {
            // Если игра уже шла, то пересоздаем GameManager для сброса
            if (_gameManager.State != GameState.Setup)
            {
                // Отписываемся от старого менеджера
                _gameManager.GameStateChanged -= OnGameStateChanged;
                // Создаем новый менеджер
                _gameManager = new GameManager();
                _gameManager.GameStateChanged += OnGameStateChanged;
            }

            _gameManager.StartGame();
            UpdateStatus();
            UpdateBoards();
            StartButtonText = "Начать новую игру"; // после начала игры меняем текст
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

        private void UpdateBoards()
        {
            // Очищаем и перезаполняем коллекции
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
