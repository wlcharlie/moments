using UnityEngine;
using FMODUnity;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// Sequencer command to stop a StudioEventEmitter on a GameObject.
    /// Usage: EmitterStop(GameObjectName, [stopMode])
    /// - GameObjectName: The name of the GameObject with StudioEventEmitter component
    /// - stopMode (optional): "fadeout" or "immediate". Default is "fadeout".
    /// Example: EmitterStop(BGMPlayer)
    /// Example: EmitterStop(BGMPlayer, immediate)
    /// </summary>
    public class SequencerCommandEmitterStop : SequencerCommand
    {
        public void Awake()
        {
            string goName = GetParameter(0);
            string stopModeParam = GetParameter(1, "fadeout").ToLower();

            if (string.IsNullOrEmpty(goName))
            {
                if (DialogueDebug.logWarnings)
                    Debug.LogWarning("Dialogue System: EmitterStop() requires a GameObject name.");
            }
            else
            {
                GameObject go = GameObject.Find(goName);
                if (go != null)
                {
                    var emitter = go.GetComponent<StudioEventEmitter>();
                    if (emitter != null)
                    {
                        var stopMode = stopModeParam == "immediate"
                            ? FMOD.Studio.STOP_MODE.IMMEDIATE
                            : FMOD.Studio.STOP_MODE.ALLOWFADEOUT;

                        emitter.EventInstance.stop(stopMode);
                    }
                    else
                    {
                        if (DialogueDebug.logWarnings)
                            Debug.LogWarning($"Dialogue System: EmitterStop() - No StudioEventEmitter found on '{goName}'.");
                    }
                }
                else
                {
                    if (DialogueDebug.logWarnings)
                        Debug.LogWarning($"Dialogue System: EmitterStop() - GameObject '{goName}' not found.");
                }
            }

            Stop();
        }
    }
}
