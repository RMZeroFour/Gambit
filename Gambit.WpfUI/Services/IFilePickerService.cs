using Microsoft.Win32;

namespace Gambit.UI.Services;

public interface IFilePickerService
{
	IFilePickerService SetInitialDir (string dir);
	IFilePickerService SetFilter (string filter);
	IFilePickerService SetExtension (string ext);
	string OpenFile ();
}

public class FilePickerService : IFilePickerService
{
	private string initialDir;
	private string fileFilter;
	private string fileExtension;

	public IFilePickerService SetInitialDir (string dir)
	{
		initialDir = dir;
		return this;
	}

	public IFilePickerService SetFilter (string filter)
	{
		fileFilter = filter;
		return this;
	}

	public IFilePickerService SetExtension (string ext)
	{
		fileExtension = ext;
		return this;
	}

	public string OpenFile ()
	{
		OpenFileDialog ofd = new()
		{
			InitialDirectory = initialDir,
			Filter = fileFilter,
			DefaultExt = fileExtension,
		};

		return ofd.ShowDialog().GetValueOrDefault() ? ofd.FileName : null;
	}

	public string SaveFile ()
	{
		SaveFileDialog sfd = new()
		{
			DefaultExt = fileExtension,
			AddExtension = true,
			Filter = fileFilter,
			InitialDirectory = initialDir,
		};

		return sfd.ShowDialog().GetValueOrDefault() ? sfd.FileName : null;
	}
}