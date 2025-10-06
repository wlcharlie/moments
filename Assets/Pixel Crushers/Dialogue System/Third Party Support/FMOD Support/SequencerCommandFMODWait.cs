using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using FMOD.Studio;
using System;

namespace PixelCrushers.DialogueSystem.SequencerCommands
{

    /// <summary>
    /// Syntax: FMODWait(event, [eventPath], [parameter, value])
    /// 
    /// Plays an FMOD event.
    /// Based on contribution from Studio Sai (Eternights).
    /// 
    /// - event: Event to play.
    /// - eventPath: Event path. Defaults to: "event:/Dialogue/Dialogue"
    /// - parameter: Optional parameter to set. For none, omit or leave blank.
    /// - value: Value to set parameter to.
    /// </summary>
    public class SequencerCommandFMODWait : SequencerCommand
    {
        PLAYBACK_STATE state;
        EVENT_CALLBACK dialogueCallback;
        protected EventInstance eventInstance;

        public class VoiceData
        {
            public string key;
            public EVENT_CALLBACK_TYPE eventCallbackType;
        }

        protected VoiceData voiceData;
        protected GCHandle voiceDataHandle;
        protected string eventPath;

        public virtual IEnumerator Start()
        {
            dialogueCallback = new FMOD.Studio.EVENT_CALLBACK(DialogueEventCallback);

            voiceData = new VoiceData();
            voiceData.key = GetParameter(0);

            eventPath = GetParameter(1, "event:/Dialogue/Dialogue");

            string paramName = GetParameter(2, "");
            float paramValue = GetParameterAs<float>(3, 0);

            PlayDialogue(voiceData, paramName, paramValue);

            if (voiceData.eventCallbackType != EVENT_CALLBACK_TYPE.SOUND_STOPPED)
            {
                while (IsVoicePlaying())
                {
                    yield return null;
                }
            }

            Stop();
        }

        protected virtual bool IsVoicePlaying()
        {
            if (voiceData.eventCallbackType == EVENT_CALLBACK_TYPE.SOUND_STOPPED || voiceData.eventCallbackType == EVENT_CALLBACK_TYPE.STOPPED || voiceData.eventCallbackType == EVENT_CALLBACK_TYPE.DESTROYED) return false;
            eventInstance.getPlaybackState(out state);
            return voiceData.eventCallbackType != EVENT_CALLBACK_TYPE.SOUND_STOPPED;
        }

        protected virtual void OnDestroy()
        {
            if (state != PLAYBACK_STATE.STOPPED)
            {
                eventInstance.release();
                if (IsVoicePlaying())
                {
                    eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                }
            }
        }

        protected virtual void PlayDialogue(VoiceData inVoiceData, string paramName, float paramValue)
        {
            eventInstance = FMODUnity.RuntimeManager.CreateInstance(eventPath);

            // Pin the key string in memory and pass a pointer through the user data
            voiceDataHandle = GCHandle.Alloc(inVoiceData);
            eventInstance.setUserData(GCHandle.ToIntPtr(voiceDataHandle));

            if (!string.IsNullOrEmpty(paramName)) eventInstance.setParameterByName(paramName, paramValue);

            eventInstance.setCallback(dialogueCallback);
            eventInstance.start();
            eventInstance.release();
        }

        protected virtual void PlayDialogue(VoiceData inVoiceData)
        {
            PlayDialogue(inVoiceData, string.Empty, 0);
        }

        [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
        protected static FMOD.RESULT DialogueEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
        {
            FMOD.Studio.EventInstance instance = new FMOD.Studio.EventInstance(instancePtr);

            // Retrieve the user data
            IntPtr voiceDataInstancePtr;
            FMOD.RESULT result = instance.getUserData(out voiceDataInstancePtr);
            if (result != FMOD.RESULT.OK)
            {
                Debug.LogError($"Dialogue System: Sequencer: FMODWait(): Timeline Callback error: {result}");
            }
            else if (voiceDataInstancePtr != IntPtr.Zero)
            {
                // Get the object to store beat and marker details
                GCHandle voiceDataHandle = GCHandle.FromIntPtr(voiceDataInstancePtr);
                VoiceData voiceData = (VoiceData)voiceDataHandle.Target;

                voiceData.eventCallbackType = type;
                switch (type)
                {
                    case FMOD.Studio.EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
                        {
                            FMOD.MODE soundMode = FMOD.MODE.LOOP_NORMAL | FMOD.MODE.CREATECOMPRESSEDSAMPLE |
                                                  FMOD.MODE.NONBLOCKING;
                            var parameter = (FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr,
                                typeof(FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES));

                            if (voiceData.key.Contains("."))
                            {
                                FMOD.Sound dialogueSound;
                                var soundResult = FMODUnity.RuntimeManager.CoreSystem.createSound(
                                    Application.streamingAssetsPath + "/" + voiceData.key, soundMode, out dialogueSound);
                                if (soundResult == FMOD.RESULT.OK)
                                {
                                    parameter.sound = dialogueSound.handle;
                                    parameter.subsoundIndex = -1;
                                    Marshal.StructureToPtr(parameter, parameterPtr, false);
                                }
                            }
                            else
                            {
                                FMOD.Studio.SOUND_INFO dialogueSoundInfo;
                                var keyResult =
                                    FMODUnity.RuntimeManager.StudioSystem.getSoundInfo(voiceData.key, out dialogueSoundInfo);
                                if (keyResult != FMOD.RESULT.OK)
                                {
                                    break;
                                }

                                FMOD.Sound dialogueSound;
                                var soundResult = FMODUnity.RuntimeManager.CoreSystem.createSound(
                                    dialogueSoundInfo.name_or_data, soundMode | dialogueSoundInfo.mode,
                                    ref dialogueSoundInfo.exinfo, out dialogueSound);
                                if (soundResult == FMOD.RESULT.OK)
                                {
                                    parameter.sound = dialogueSound.handle;
                                    parameter.subsoundIndex = dialogueSoundInfo.subsoundindex;
                                    Marshal.StructureToPtr(parameter, parameterPtr, false);
                                }
                            }

                            break;
                        }
                    case FMOD.Studio.EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND:
                        {
                            var parameter = (FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(parameterPtr,
                                typeof(FMOD.Studio.PROGRAMMER_SOUND_PROPERTIES));
                            var sound = new FMOD.Sound(parameter.sound);
                            sound.release();

                            break;
                        }
                    case FMOD.Studio.EVENT_CALLBACK_TYPE.DESTROYED:
                        {
                            // Now the event has been destroyed, unpin the string memory so it can be garbage collected
                            voiceDataHandle.Free();

                            break;
                        }
                }
            }

            return FMOD.RESULT.OK;
        }

    }
}
