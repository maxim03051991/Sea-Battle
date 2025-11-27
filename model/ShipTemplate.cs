using System.ComponentModel;
using System.Runtime.CompilerServices;

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