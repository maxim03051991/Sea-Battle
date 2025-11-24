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
        }

        public void StartGame()
        {
            // Автоматическая расстановка кораблей
            AutoPlaceShips(PlayerBoard);
            AutoPlaceShips(ComputerBoard);

            IsPlayerTurn = true;
            State = GameState.Playing;
        }

        private void AutoPlaceShips(GameBoard board)
        {
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
            }
            else if (result != CellState.Hit)
            {
                IsPlayerTurn = false;
                // Даем компьютеру сделать ход с задержкой
                Task.Delay(1000).ContinueWith(_ => ComputerShoot(), TaskScheduler.FromCurrentSynchronizationContext());
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
                result = PlayerBoard.Shoot(row, col);
            } while (result == CellState.Miss || result == CellState.Hit);

            if (result == CellState.Hit && PlayerBoard.AllShipsSunk())
            {
                State = GameState.ComputerWon;
                // Уведомляем об изменении состояния
                GameStateChanged?.Invoke();
            }
            else if (result != CellState.Hit)
            {
                IsPlayerTurn = true;
            }
            else
            {
                // Компьютер попал - ходит снова через Dispatcher
                var dispatcher = System.Windows.Application.Current.Dispatcher;
                dispatcher.BeginInvoke(new Action(() =>
                {
                    ComputerShoot();
                }), System.Windows.Threading.DispatcherPriority.Background);
            }

            // Уведомляем об изменениях
            GameStateChanged?.Invoke();
        }

        //  уведомления об изменениях
        public event Action GameStateChanged;
    }

    public enum GameState
        {
            Setup,
            Playing,
            PlayerWon,
            ComputerWon
        }
    
}
