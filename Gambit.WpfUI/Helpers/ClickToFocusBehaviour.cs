using System.Windows;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace Gambit.WpfUI.Helpers;

public class ClickToFocusBehaviour : Behavior<UIElement>
{
	protected override void OnAttached () => AssociatedObject.MouseDown += OnMouseDown;

	private void OnMouseDown (object sender, MouseButtonEventArgs e) => AssociatedObject.Focus();

	protected override void OnDetaching () => AssociatedObject.MouseDown -= OnMouseDown;
}
