using System.Windows;
using Autofac;
using Gambit.UI.Services;
using Gambit.UI.ViewModels;

namespace Gambit.UI;

public partial class App : Application
{
	private IContainer container;

	protected override void OnStartup (StartupEventArgs e)
	{
		base.OnStartup(e);

		var builder = new ContainerBuilder();

		//builder.RegisterType<TimerService>().As<ITimerService>();
		builder.RegisterType<DispatcherTimerService>().As<ITimerService>();
		builder.RegisterType<FilePickerService>().As<IFilePickerService>();
		builder.RegisterType<KeyboardGamepadService>().As<IGamepadService>();
		builder.RegisterType<NotifyUserService>().As<INotifyUserService>();

		builder.RegisterType<MainWindowVM>();
		builder.RegisterType<GraphicsVM>();
		builder.RegisterType<InputVM>();

		builder.RegisterType<DebugHostVM>();
		builder.RegisterType<SerialDebugVM>().As<IDebugToolVM>();
		builder.RegisterType<RegisterDebugVM>().As<IDebugToolVM>();
		builder.RegisterType<CartridgeDebugVM>().As<IDebugToolVM>();
		builder.RegisterType<BitmapDebugVM>().As<IDebugToolVM>();
		builder.RegisterType<DisassemblyDebugVM>().As<IDebugToolVM>();

		container = builder.Build();

		MainWindow = new MainWindow() { DataContext = container.Resolve<MainWindowVM>() };
		MainWindow.Show();
	}

	protected override void OnExit (ExitEventArgs e)
	{
		base.OnExit(e);

		container.Dispose();
	}
}