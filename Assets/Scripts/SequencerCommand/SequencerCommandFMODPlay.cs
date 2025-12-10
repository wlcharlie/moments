using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// Sequencer command to play an FMOD event.
    /// Usage: FMODPlay(eventName, [mode])
    /// - eventName: The event name (without "event:/" prefix)
    /// - mode (optional): "oneshot" for fire-and-forget, "tracked" for controllable playback. Default is "oneshot".
    /// Use "tracked" mode if you need to stop the event later with FMODStop.
    /// Example: FMODPlay(UI/Click)
    /// Example: FMODPlay(Music/BGM, tracked)
    /// </summary>
    public class SequencerCommandFMODPlay : SequencerCommand
    {
        public void Awake()
        {
            string eventName = GetParameter(0);
            string mode = GetParameter(1, "oneshot").ToLower();
            string path = "event:/" + eventName;

            if (mode == "tracked")
            {
                // Use FMODAudioManager for tracked playback (can be stopped later)
                FMODAudioManager.PlayEvent(path);
            }
            else
            {
                // Fire-and-forget playback
                FMODUnity.RuntimeManager.PlayOneShot(path);
            }

            Stop();
        }
    }
}
