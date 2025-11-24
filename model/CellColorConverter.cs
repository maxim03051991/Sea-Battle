using Sea_Battle.model;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Sea_Battle.model
{
    public class CellColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CellState state)
            {
                return state switch
                {
                    CellState.Empty => Brushes.LightBlue,
                    CellState.Ship => Brushes.Gray,
                    CellState.Hit => Brushes.Red,
                    CellState.Miss => Brushes.White,
                    _ => Brushes.LightBlue
                };
            }
            return Brushes.LightBlue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}