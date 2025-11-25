using Sea_Battle.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sea_Battle.services
{
    public class GameManager
    {
        public GameBoard PlayerBoard { get; private set; }
        public GameBoard ComputerBoard { get; private set; }
        public bool IsPlayerTurn { get; private set; }
        public GameState State { get; private set; }

        public GameManager()
        {
            PlayerBoard = new GameBoard(true);  // Поле игрока
            ComputerBoard = new GameBoard(false); // Поле компьютера
            State = GameState.Setup;
            GameStateChanged?.Invoke(); // Уведомить о начальном состоянии
        }

        public void StartGame()
        {
            // Автоматическая расстановка кораблей компьютера
            AutoPlaceShips(ComputerBoard);

            IsPlayerTurn = true;
            State = GameState.Playing;
            GameStateChanged?.Invoke();
        }

        public void AutoPlacePlayerShips()
        {
            AutoPlaceShips(PlayerBoard);
        }

        private void AutoPlaceShips(GameBoard board)
        {
            // Очищаем существующие корабли
            board.Ships.Clear();
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

        public bool PlayerShoot(int row, int col)
        {
            if (!IsPlayerTurn || State != GameState.Playing)
                return false;

            var result = ComputerBoard.Shoot(row, col);

            if (result == CellState.Hit && ComputerBoard.AllShipsSunk())
            {
                State = GameState.PlayerWon;
                GameStateChanged?.Invoke();
            }
            else if (result == CellState.Miss) // Только при промахе передаем ход
            {
                IsPlayerTurn = false;
                GameStateChanged?.Invoke(); // Уведомляем об изменении хода

                // Компьютер ходит с задержкой
                Task.Delay(1000).ContinueWith(_ =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        ComputerShoot();
                    });
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else if (result == CellState.Hit)
            {
                // Игрок попал, но игра продолжается - ход остается у игрока
                GameStateChanged?.Invoke();
            }

            return true;
        }

        public void ComputerShoot()
        {
            if (!IsPlayerTurn && State == GameState.Playing)
            {
                ComputerShootInternal();
            }
        }

        private void ComputerShootInternal()
        {
            if (State != GameState.Playing || IsPlayerTurn)
                return;

            Random rand = new Random();
            int row, col;
            CellState result;

            // Ищем случайную свободную клетку
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
                IsPlayerTurn = true; // Передаем ход игроку
            }
            else if (result == CellState.Hit)
            {
                // Компьютер попал - ходит снова
                // Небольшая пауза перед следующим выстрелом
                Task.Delay(800).ContinueWith(_ =>
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        ComputerShoot();
                    });
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }

            // Всегда уведомляем об изменениях
            GameStateChanged?.Invoke();
        }

        //  уведомления об изменениях
        public event Action? GameStateChanged;
    }

    public enum GameState
    {
        Setup,
        Playing,
        PlayerWon,
        ComputerWon
    }

}
    
        
