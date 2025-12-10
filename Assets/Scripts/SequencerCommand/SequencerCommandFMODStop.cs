using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// Sequencer command to stop an FMOD event.
    /// Usage: FMODStop(eventName, [stopMode])
    /// - eventName: The event name (without "event:/" prefix)
    /// - stopMode (optional): "fadeout" or "immediate". Default is "fadeout".
    /// Example: FMODStop(Music/BGM)
    /// Example: FMODStop(Music/BGM, immediate)
    /// </summary>
    public class SequencerCommandFMODStop : SequencerCommand
    {
        public void Awake()
        {
            string eventName = GetParameter(0);
            string stopModeParam = GetParameter(1, "fadeout").ToLower();

            if (string.IsNullOrEmpty(eventName))
            {
                if (DialogueDebug.logWarnings)
                    Debug.LogWarning("Dialogue System: FMODStop() requires an event name.");
            }
            else
            {
                string path = "event:/" + eventName;
                var stopMode = stopModeParam == "immediate"
                    ? FMOD.Studio.STOP_MODE.IMMEDIATE
                    : FMOD.Studio.STOP_MODE.ALLOWFADEOUT;

                FMODAudioManager.StopEvent(path, stopMode);
            }

            Stop();
        }
    }
}
