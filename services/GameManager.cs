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

        public GameManager() //конструктор
        {
            PlayerBoard = new GameBoard(true);  // Доска игрока
            ComputerBoard = new GameBoard(false); // Доска компьютера
            State = GameState.Setup;
            GameStateChanged?.Invoke();
        }
        // начало игры
        public void StartGame()
        {
            AutoPlaceShips(ComputerBoard);
            IsPlayerTurn = true;
            State = GameState.Playing;
            GameStateChanged?.Invoke(); // Уведомление: игра началась!
        }
        //перезапустить игру
        public void ResetGame()
        {
            PlayerBoard = new GameBoard(true);
            ComputerBoard = new GameBoard(false);
            State = GameState.Setup;
            IsPlayerTurn = false;
            GameStateChanged?.Invoke();
        }

        // авто расстановка кораблей для компьютера
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
                    int row = rand.Next(0, 10);
                    int col = rand.Next(0, 10);
                    bool horizontal = rand.Next(0, 2) == 0;

                    placed = board.PlaceShip(row, col, size, horizontal);
                    attempts++;
                }
            }
        }
        //очистка доски
        private void ClearBoard(GameBoard board)
        {
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    if (board.Cells[i, j].State == CellState.Ship)
                    {
                        board.Cells[i, j].State = CellState.Empty;
                        board.Cells[i, j].Ship = null;
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

            if (result == CellState.Hit && ComputerBoard.AllShipsSunk())
            {
                State = GameState.PlayerWon;
            }
            else if (result == CellState.Miss)
            {
                IsPlayerTurn = false;
                Task.Delay(1000).ContinueWith(_ => Application.Current.Dispatcher.Invoke(ComputerShoot));
            }

            GameStateChanged?.Invoke();
            return true;
        }
        //выстрел компьютера
        private void ComputerShoot()
        {
            if (State != GameState.Playing || IsPlayerTurn)
                return;

            Random rand = new Random();
            int row, col;
            CellState result;

            do
            {
                row = rand.Next(0, 10);
                col = rand.Next(0, 10);
            } while (PlayerBoard.Cells[row, col].State == CellState.Miss ||
                     PlayerBoard.Cells[row, col].State == CellState.Hit);

            result = PlayerBoard.Shoot(row, col);

            if (result == CellState.Hit && PlayerBoard.AllShipsSunk())
            {
                State = GameState.ComputerWon;
            }
            else if (result == CellState.Miss)
            {
                IsPlayerTurn = true;
            }
            else if (result == CellState.Hit)
            {
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