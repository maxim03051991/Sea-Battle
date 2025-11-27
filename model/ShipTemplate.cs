using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sea_Battle.model
{
    public class ShipTemplate : INotifyPropertyChanged
    {
        private int _count;

        public int Size { get; set; }
        public int Count
        {
            get => _count;
            set
            {
                _count = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));
            }
        }
        public string Name { get; set; }
        public string DisplayName => $"{Name} ({Count} осталось)";

        public ShipTemplate(int size, int count, string name)
        {
            Size = size;
            _count = count;
            Name = name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
