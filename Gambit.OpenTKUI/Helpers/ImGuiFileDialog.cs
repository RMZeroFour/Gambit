using System;
using System.IO;
using ImGuiNET;

namespace Gambit.OpenTKUI;

public class ImGuiFileDialog
{
	private readonly string name;
	private Action<string> callback;

	private string directoryInput;
	private string currentFolder;
	private string selectedPath;

	private bool shouldOpenPopup;

	public string LastOpenedFile { get; private set; }

	public ImGuiFileDialog (string name) => this.name = name;

	public void SetPopupCallback (Action<string> callback)
	{
		this.callback = callback;
	}

	public void InitPopup (string initialFolder)
	{
		shouldOpenPopup = true;
		currentFolder = initialFolder;
		directoryInput = currentFolder;
	}

	public void RenderPopup ()
	{
		if (shouldOpenPopup)
		{
			ImGui.OpenPopup(name);
			shouldOpenPopup = false;
		}

		ImGui.SetNextWindowPos(ImGui.GetWindowViewport().GetCenter(), ImGuiCond.Always, new(0.5f, 0.5f));
		ImGui.SetNextWindowSize(ImGui.GetWindowViewport().Size * 0.75f, ImGuiCond.Always);

		bool open = true;
		if (ImGui.BeginPopupModal(name, ref open, ImGuiWindowFlags.NoDecoration))
		{
			RenderPopupHeader();
			ImGui.Dummy(new(0, 4));
			RenderPopupEntries();

			ImGui.EndPopup();
		}
	}

	private void RenderPopupHeader ()
	{
		if (ImGui.InputText("##directory_input", ref directoryInput, 256))
		{
			if (Directory.Exists(directoryInput))
				currentFolder = directoryInput;
		}

		ImGui.SameLine();

		if (ImGui.ArrowButton("Up", ImGuiDir.Up))
		{
			currentFolder = Directory.GetParent(currentFolder)?.FullName ?? currentFolder;
			directoryInput = currentFolder;
		}

		ImGui.SameLine();

		if (ImGui.Button("Open"))
		{
			if (IsDirectory(selectedPath))
			{
				currentFolder = selectedPath;
			}
			else
			{
				LastOpenedFile = selectedPath;
				callback(selectedPath);
				ImGui.CloseCurrentPopup();
			}
		}

		ImGui.SameLine();

		if (ImGui.Button("Cancel"))
			ImGui.CloseCurrentPopup();
	}

	private void RenderPopupEntries ()
	{
		if (ImGui.BeginTable("##filesystem", 2, ImGuiTableFlags.ScrollY))
		{
			ImGui.TableSetupColumn("##entry_name", ImGuiTableColumnFlags.None, 0.8f);
			ImGui.TableSetupColumn("##entry_type", ImGuiTableColumnFlags.None, 0.2f);

			if (Directory.Exists(currentFolder))
			{
				foreach (string path in Directory.EnumerateFileSystemEntries(currentFolder))
				{
					bool isDir = IsDirectory(path);

					string shortName = isDir
						? new DirectoryInfo(path).Name
						: new FileInfo(path).Name;

					ImGui.TableNextRow();

					ImGui.TableSetColumnIndex(1);
					ImGui.Text(isDir ? "Directory" : "File");

					ImGui.TableSetColumnIndex(0);
					bool sel = selectedPath == path;
					if (ImGui.Selectable(shortName, ref sel, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowDoubleClick))
					{
						if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
						{
							if (isDir)
							{
								currentFolder = path;
							}
							else
							{
								LastOpenedFile = path;
								callback(path);
								ImGui.CloseCurrentPopup();
							}
						}
						else
						{
							selectedPath = path;
						}
					}
				}
			}

			ImGui.EndTable();
		}
	}

	private static bool IsDirectory (string file) => File.GetAttributes(file).HasFlag(FileAttributes.Directory);
}