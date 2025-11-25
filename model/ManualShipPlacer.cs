using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sea_Battle.model
{
    public class ManualShipPlacer
    {
        private GameBoard _board;
        private List<ShipTemplate> _availableShips;
        private ShipTemplate? _selectedShip;
        private bool _isHorizontal = true;

        public ManualShipPlacer(GameBoard board)
        {
            _board = board;
            InitializeAvailableShips();
        }

        public List<ShipTemplate> AvailableShips => _availableShips;
        public ShipTemplate? SelectedShip => _selectedShip;
        public bool IsHorizontal
        {
            get => _isHorizontal;
            set => _isHorizontal = value;
        }

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

        public void SelectShip(ShipTemplate ship)
        {
            if (ship.Count > 0)
            {
                _selectedShip = ship;
            }
        }

        public bool PlaceShip(int row, int col)
        {
            if (_selectedShip == null || _selectedShip.Count == 0)
                return false;

            if (_board.PlaceShip(row, col, _selectedShip.Size, _isHorizontal))
            {
                _selectedShip.Count--;

                // Если корабли этого типа закончились, сбрасываем выбор
                if (_selectedShip.Count == 0)
                {
                    _selectedShip = null;
                }

                return true;
            }

            return false;
        }

        public void RotateShip()
        {
            _isHorizontal = !_isHorizontal;
        }

        public void Reset()
        {
            InitializeAvailableShips();
            _selectedShip = null;
            _isHorizontal = true;
        }
    }

    public class ShipTemplate
    {
        public int Size { get; set; }
        public int Count { get; set; }
        public string Name { get; set; }
        public string DisplayName => $"{Name} ({Count} осталось)";

        public ShipTemplate(int size, int count, string name)
        {
            Size = size;
            Count = count;
            Name = name;
        }
    }
}

