using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Gambit.UI.Helpers;

[System.Windows.Markup.ContentProperty("Converters")]
public class ChainedConverter : IValueConverter
{
	public ObservableCollection<IValueConverter> Converters { get; } = new();

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		=> Converters.Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotImplementedException();
}