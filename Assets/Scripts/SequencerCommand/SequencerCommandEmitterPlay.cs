using UnityEngine;
using FMODUnity;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{
    /// <summary>
    /// Sequencer command to play/start a StudioEventEmitter on a GameObject.
    /// Usage: EmitterPlay(GameObjectName)
    /// - GameObjectName: The name of the GameObject with StudioEventEmitter component
    /// Example: EmitterPlay(BGMPlayer)
    /// </summary>
    public class SequencerCommandEmitterPlay : SequencerCommand
    {
        public void Awake()
        {
            string goName = GetParameter(0);

            if (string.IsNullOrEmpty(goName))
            {
                if (DialogueDebug.logWarnings)
                    Debug.LogWarning("Dialogue System: EmitterPlay() requires a GameObject name.");
            }
            else
            {
                GameObject go = GameObject.Find(goName);
                if (go != null)
                {
                    var emitter = go.GetComponent<StudioEventEmitter>();
                    if (emitter != null)
                    {
                        emitter.Play();
                    }
                    else
                    {
                        if (DialogueDebug.logWarnings)
                            Debug.LogWarning($"Dialogue System: EmitterPlay() - No StudioEventEmitter found on '{goName}'.");
                    }
                }
                else
                {
                    if (DialogueDebug.logWarnings)
                        Debug.LogWarning($"Dialogue System: EmitterPlay() - GameObject '{goName}' not found.");
                }
            }

            Stop();
        }
    }
}
