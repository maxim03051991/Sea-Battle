using Sea_Battle.model;
using System.Collections.Generic;
using System.Linq;

namespace Sea_Battle.services
{
    //ручная расстановка кораблей
    public class ManualShipPlacer
    {
        private bool _isNewRulesMode;
        private GameBoard _board; //игровое поле
        private List<ShipTemplate> _availableShips; //список доступных для размещения кораблей
        private ShipTemplate _selectedShip;//выбранный в данный момент корабль
        private bool _isHorizontal = true; //ориентация корабля


        //контруктор принимает игровое поле и инициализирует список доступных кораблей
        public ManualShipPlacer(bool isNewRulesMode, GameBoard board)
        {
            _isNewRulesMode = isNewRulesMode;
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
            if (!_isNewRulesMode) // ИСПРАВЛЕНО: было = вместо ==
            {
                _availableShips = new List<ShipTemplate>
                {
                    new ShipTemplate(4, 1, "4-палубный"),
                    new ShipTemplate(3, 2, "3-палубный"),
                    new ShipTemplate(2, 3, "2-палубный"),
                    new ShipTemplate(1, 4, "1-палубный")
                };
            }
            else
            {
                _availableShips = new List<ShipTemplate>
                {
                    new ShipTemplate(4, 1, "Квадрат 2x2"), // Размер 4 клетки
                    new ShipTemplate(4, 1, "Г-образная"),   // Размер 4 клетки
                    new ShipTemplate(4, 1, "Т-образная"),   // Размер 5 клеток
                    new ShipTemplate(4, 1, "Z-образная")    // Размер 5 клеток
                };
            }
        }
        //Выбирает корабль для размещения
        public void SelectShip(ShipTemplate ship)
        {
            if (ship.Count > 0)
            {
                _selectedShip = ship;
                // Сбрасываем ориентацию при выборе нового корабля
                _isHorizontal = true;
            }
        }

        //Основной метод размещения корабля
        public bool PlaceShip(int row, int col)
        {
            if (_selectedShip?.Count == 0)
                return false;

            if (_isNewRulesMode)
            {
                // Ручная расстановка изогнутых кораблей для нового режима
                return PlaceCurvedShip(row, col);
            }
            else
            {
                // Классическая расстановка прямых кораблей
                if (_board.PlaceShip(row, col, _selectedShip.SizeShip, _isHorizontal))
                {
                    _selectedShip.Count--;
                    if (_selectedShip.Count == 0)
                        _selectedShip = null;
                    return true;
                }
                return false;
            }
        }

        // Метод для размещения изогнутых кораблей
        private bool PlaceCurvedShip(int row, int col)
        {
            List<Cell> shipCells = null;

            // Определяем тип корабля по имени и получаем соответствующие клетки
            switch (_selectedShip.Name)
            {
                case "Квадрат 2x2":
                    shipCells = _board.GetSquareCells(row, col);
                    break;
                case "Г-образная":
                    shipCells = _board.GetLShapeCells(row, col);
                    break;
                case "Т-образная":
                    shipCells = _board.GetTShapeCells(row, col);
                    break;
                case "Z-образная":
                    shipCells = _board.GetZShapeCells(row, col);
                    break;
            }

            if (shipCells != null && _board.CanPlaceCurvedShip(shipCells))
            {
                if (_board.PlaceCurvedShip(shipCells))
                {
                    _selectedShip.Count--;
                    if (_selectedShip.Count == 0)
                        _selectedShip = null;
                    return true;
                }
            }

            return false;
        }


        //ориентацию корабля между горизонтальной и вертикальной
        public void RotateShip() => _isHorizontal = !_isHorizontal;
        //сбрасывает состояние таблицы
        // ManualShipPlacer.cs - изменим метод Reset для нового режима
        public void Reset()
        {
            ClearBoard();
            InitializeAvailableShips();
            _selectedShip = null;
            _isHorizontal = true;

            // В новом режиме автоматически расставляем изогнутые корабли
            if (_isNewRulesMode)
            {
                // Здесь можно добавить вызов автоматической расстановки
                // или оставить пустым, если расстановка делается в GameViewModel
            }
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