using UnityEngine;
using FMODUnity;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// Sequencer command to pause or resume a StudioEventEmitter on a GameObject.
    /// Usage: EmitterPause(GameObjectName, [paused])
    /// - GameObjectName: The name of the GameObject with StudioEventEmitter component
    /// - paused (optional): "true" to pause, "false" to resume. Default is "true".
    /// Example: EmitterPause(BGMPlayer)         // Pauses the emitter
    /// Example: EmitterPause(BGMPlayer, true)   // Pauses the emitter
    /// Example: EmitterPause(BGMPlayer, false)  // Resumes the emitter
    /// </summary>
    public class SequencerCommandEmitterPause : SequencerCommand
    {
        public void Awake()
        {
            string goName = GetParameter(0);
            bool paused = GetParameterAsBool(1, true);

            if (string.IsNullOrEmpty(goName))
            {
                if (DialogueDebug.logWarnings)
                    Debug.LogWarning("Dialogue System: EmitterPause() requires a GameObject name.");
            }
            else
            {
                GameObject go = GameObject.Find(goName);
                if (go != null)
                {
                    var emitter = go.GetComponent<StudioEventEmitter>();
                    if (emitter != null)
                    {
                        emitter.EventInstance.setPaused(paused);
                    }
                    else
                    {
                        if (DialogueDebug.logWarnings)
                            Debug.LogWarning($"Dialogue System: EmitterPause() - No StudioEventEmitter found on '{goName}'.");
                    }
                }
                else
                {
                    if (DialogueDebug.logWarnings)
                        Debug.LogWarning($"Dialogue System: EmitterPause() - GameObject '{goName}' not found.");
                }
            }

            Stop();
        }
    }
}
