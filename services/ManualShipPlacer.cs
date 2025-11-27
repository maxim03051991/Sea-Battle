using Sea_Battle.model;
using System.Collections.Generic;
using System.Linq;

namespace Sea_Battle.services
{
    //ручная расстановка кораблей
    public class ManualShipPlacer
    {
        private GameBoard _board; //игровое поле
        private List<ShipTemplate> _availableShips; //список доступных для размещения кораблей
        private ShipTemplate _selectedShip;//выбранный в данный момент корабль
        private bool _isHorizontal = true; //ориентация корабля

        //контруктор принимает игровое поле и инициализирует список доступных кораблей
        public ManualShipPlacer(GameBoard board)
        {
            _board = board;
            InitializeAvailableShips();
        }

        public List<ShipTemplate> AvailableShips => _availableShips;
        public ShipTemplate SelectedShip => _selectedShip;
        public bool IsHorizontal
        {
            get => _isHorizontal;
            set => _isHorizontal = value;
        }
        //Создает начальный набор кораблей согласно правилам "Морского боя"
        private void InitializeAvailableShips()
        {
            _availableShips = new List<ShipTemplate>
            {
                new ShipTemplate(4, 1, "4-палубный"),
                new ShipTemplate(3, 2, "3-палубный"),
                new ShipTemplate(2, 3, "2-палубный"),
                new ShipTemplate(1, 4, "1-палубный")
            };
        }
        //Выбирает корабль для размещения
        public void SelectShip(ShipTemplate ship)
        {
            if (ship.Count > 0)
                _selectedShip = ship;
        }
        //Основной метод размещения корабля
        public bool PlaceShip(int row, int col)
        {
            if (_selectedShip?.Count == 0)
                return false;

            if (_board.PlaceShip(row, col, _selectedShip.SizeShip, _isHorizontal))
            {
                _selectedShip.Count--;
                if (_selectedShip.Count == 0)
                    _selectedShip = null;
                return true;
            }

            return false;
        }
        //ориентацию корабля между горизонтальной и вертикальной
        public void RotateShip() => _isHorizontal = !_isHorizontal;
        //сбрасывает состояние таблицы
        public void Reset()
        {
            ClearBoard(); 
            InitializeAvailableShips();
            _selectedShip = null;
            _isHorizontal = true;
        }
        //Очищает доску
        private void ClearBoard()
        {
            foreach (var ship in _board.Ships.ToList())
            {
                foreach (var cell in ship.Cells)
                {
                    cell.State = CellState.Empty;
                    cell.Ship = null;
                }
            }
            _board.Ships.Clear();
        }
    }
}