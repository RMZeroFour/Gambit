using System.Windows;

namespace Gambit.UI.Services;

public interface INotifyUserService
{
	void Notify(string message);
}

public class NotifyUserService : INotifyUserService
{
	public void Notify(string message) => MessageBox.Show(message);
}