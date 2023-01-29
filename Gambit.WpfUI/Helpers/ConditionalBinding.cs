using System;
using System.Globalization;
using System.Windows.Data;

namespace Gambit.UI.Helpers;

public class TernaryConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		string[] parts = parameter.ToString().Split('|');
		return (bool)value ? parts[0] : parts[1];
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}