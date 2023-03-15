using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Godot;
using OsuSkinMixer.Statics;

namespace OsuSkinMixer.Models;

/// <summary>
/// A class that represents an operation to be peformed to a single skin.
/// </summary>
public class Operation
{
    private const int MAX_OPERATION_COUNT = 100;

    [JsonPropertyName("type")]
    public OperationType Type { get; set; }

    [JsonPropertyName("target_skin_name")]
    public string TargetSkinName { get; set; }

    [JsonPropertyName("time_started")]
    public DateTime? TimeStarted { get; set; }

    [JsonIgnore]
    public OsuSkin TargetSkin { get; }

    [JsonIgnore]
    public string Description => $"{Type} {TargetSkinName}";

    [JsonIgnore]
    public bool Started => _task != null;

    [JsonIgnore]
    public bool CanUndo => UndoAction != null && !_isUndone;

    [JsonIgnore]
    private Action UndoAction { get; }

    [JsonIgnore]
    private Action Action { get; }

    [JsonIgnore]
    private Task _task;

    [JsonIgnore]
    private bool _isUndone;

    public Operation()
    {
    }

    public Operation(OperationType type, OsuSkin targetSkin, Action action, Action undoAction = null)
    {
        Type = type;
        TargetSkin = targetSkin;
        TargetSkinName = targetSkin.Name;
        Action = action;
        UndoAction = undoAction;
    }

    public Task RunOperation()
    {
        if (_task != null)
            return _task;

        OsuData.SweepPaused = true;
        TimeStarted = DateTime.Now;

        Settings.Log($"Running operation: {Description}");
        Settings.Content.Operations.Add(this);

        if (Settings.Content.Operations.Count > MAX_OPERATION_COUNT)
            Settings.Content.Operations.RemoveRange(0, Settings.Content.Operations.Count - MAX_OPERATION_COUNT);

        _task = Task.Run(() => Action())
            .ContinueWith(t =>
            {
                OsuData.SweepPaused = false;

                if (t.IsFaulted)
                {
                    if (t.Exception.InnerException is OperationCanceledException)
                    {
                        Settings.Log($"Operation canceled: {Description}");
                        return;
                    }

                    Settings.PushException(t.Exception);
                    return;
                }

                Settings.Log($"Operation completed: {Description}");
            });

        return _task;
    }

    public void UndoOperation()
    {
        if (_task?.IsCompleted != true)
            return;

        Settings.Log($"Undoing operation: {Description}");

        try
        {
            UndoAction();
            _isUndone = true;
            Settings.Content.Operations.Remove(this);
        }
        catch (Exception e)
        {
            Settings.PushException(e);
        }
    }
}