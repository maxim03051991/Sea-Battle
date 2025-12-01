using Sea_Battle.model;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Sea_Battle.Converters
{
    public class MineUsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CellState state)
            {
                // Отображаем точку только для использованных мин
                return state == CellState.MineUsed ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}