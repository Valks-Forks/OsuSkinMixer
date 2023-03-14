using Godot;
using OsuSkinMixer.Components;
using OsuSkinMixer.Statics;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace OsuSkinMixer;

public partial class Splash : Control
{
	private Label UpdatingLabel;
	private SetupPopup SetupPopup;
	private QuestionPopup ExceptionPopup;
	private TextEdit ExceptionTextEdit;
	private OkPopup UpdateCanceledPopup;
	private AnimationPlayer AnimationPlayer;

	public override void _Ready()
	{
		Settings.Log($"osu! skin mixer {Settings.VERSION} at {DateTime.Now}");

		DisplayServer.WindowSetTitle("osu! skin mixer by rednir");
		DisplayServer.WindowSetMinSize(new Vector2I(650, 300));

		UpdatingLabel = GetNode<Label>("%UpdatingLabel");
		SetupPopup = GetNode<SetupPopup>("%SetupPopup");
		ExceptionPopup = GetNode<QuestionPopup>("%ExceptionPopup");
		ExceptionTextEdit = GetNode<TextEdit>("%ExceptionTextEdit");
		UpdateCanceledPopup = GetNode<OkPopup>("%UpdateCanceledPopup");
		AnimationPlayer = GetNode<AnimationPlayer>("%AnimationPlayer");
		AnimationPlayer.AnimationFinished += OnAnimationFinished;
		UpdateCanceledPopup.PopupOut += LoadSkins;

		AnimationPlayer.Play("loading");

		Task.Run(Initialise)
			.ContinueWith(t =>
			{
				if (t.IsFaulted)
				{
					GD.PushError(t.Exception);
					ExceptionTextEdit.Text = t.Exception.ToString();
					ExceptionPopup.In();
				}
			});
	}

	private void Initialise()
	{
		Settings.InitialiseSettingsFile();

		if (File.Exists(Settings.AutoUpdateInstallerPath))
		{
			if (Settings.Content.LastVersion == Settings.VERSION)
			{
				// Try run installer, if it succeeds execution should stop here.
				UpdatingLabel.Visible = true;
				TryRunInstaller();
				UpdatingLabel.Visible = false;
				return;
			}

			// Update finished, clean up installer.
			File.Delete(Settings.AutoUpdateInstallerPath);
		}

		LoadSkins();
	}

	private void TryRunInstaller()
	{
		Settings.Log($"Running installer from {Settings.AutoUpdateInstallerPath}");

		try
		{
			Process installer = Process.Start(Settings.AutoUpdateInstallerPath, "/silent");
			installer.WaitForExit();
		}
		catch (Exception e)
		{
			GD.PrintErr(e);
		}

		UpdateCanceledPopup.In();
	}

	private void LoadSkins()
	{
		if (!OsuData.TryLoadSkins())
		{
			SetupPopup.In();
			SetupPopup.PopupOut += () => AnimationPlayer.Play("out");
			return;
		}

		AnimationPlayer.Play("out");
	}

	private void OnAnimationFinished(StringName animationName)
	{
        if (animationName == "out" && GetTree().ChangeSceneToFile("res://src/Main.tscn") != Error.Ok)
		{
			ExceptionTextEdit.Text = "Failed to load scene.";
			ExceptionPopup.In();
		}
    }
}
