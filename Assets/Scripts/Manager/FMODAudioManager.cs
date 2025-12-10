using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

/// <summary>
/// Manages FMOD EventInstances for playback control (play, stop, etc.)
/// Use this instead of PlayOneShot when you need to stop or control events later.
/// </summary>
public static class FMODAudioManager
{
    private static Dictionary<string, EventInstance> activeEvents = new Dictionary<string, EventInstance>();

    /// <summary>
    /// Play an FMOD event and track it for later control.
    /// If the same event path is already playing, it will be stopped first.
    /// </summary>
    /// <param name="eventPath">Full event path (e.g., "event:/Music/BGM")</param>
    public static void PlayEvent(string eventPath)
    {
        // Stop existing instance if playing
        if (activeEvents.TryGetValue(eventPath, out EventInstance existingInstance))
        {
            existingInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            existingInstance.release();
            activeEvents.Remove(eventPath);
        }

        // Create and start new instance
        EventInstance instance = RuntimeManager.CreateInstance(eventPath);
        if (instance.isValid())
        {
            instance.start();
            activeEvents[eventPath] = instance;
        }
        else
        {
            Debug.LogWarning($"FMODAudioManager: Failed to create instance for {eventPath}");
        }
    }

    /// <summary>
    /// Stop an FMOD event.
    /// </summary>
    /// <param name="eventPath">Full event path (e.g., "event:/Music/BGM")</param>
    /// <param name="stopMode">ALLOWFADEOUT for fade, IMMEDIATE for instant stop</param>
    public static void StopEvent(string eventPath, FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
    {
        if (activeEvents.TryGetValue(eventPath, out EventInstance instance))
        {
            instance.stop(stopMode);
            instance.release();
            activeEvents.Remove(eventPath);
        }
        else
        {
            Debug.LogWarning($"FMODAudioManager: No active event found for {eventPath}");
        }
    }

    /// <summary>
    /// Stop all active FMOD events.
    /// </summary>
    /// <param name="stopMode">ALLOWFADEOUT for fade, IMMEDIATE for instant stop</param>
    public static void StopAllEvents(FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
    {
        foreach (var kvp in activeEvents)
        {
            kvp.Value.stop(stopMode);
            kvp.Value.release();
        }
        activeEvents.Clear();
    }

    /// <summary>
    /// Check if an event is currently playing.
    /// </summary>
    public static bool IsEventPlaying(string eventPath)
    {
        if (activeEvents.TryGetValue(eventPath, out EventInstance instance))
        {
            instance.getPlaybackState(out PLAYBACK_STATE state);
            return state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING;
        }
        return false;
    }
}
