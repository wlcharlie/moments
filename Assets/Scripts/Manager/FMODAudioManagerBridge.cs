using UnityEngine;

/// <summary>
/// 轉發呼叫給 FMODAudioManager 靜態類
/// 用於 UI 事件綁定，避免直接引用靜態類
/// 使用方式：在 Unity Editor 建立 ScriptableObject asset，然後在 UI Button 的 onClick 事件中引用靜態方法
/// </summary>
[CreateAssetMenu(fileName = "FMODAudioManagerBridge", menuName = "Bridges/FMOD Audio Manager Bridge")]
public class FMODAudioManagerBridge : ScriptableObject
{
    // 靜態方法可以直接被 Unity UI 事件系統引用

    /// <summary>
    /// 播放 FMOD 事件
    /// </summary>
    /// <param name="eventPath">完整事件路徑 (e.g., "event:/Music/BGM")</param>
    public static void PlayEvent(string eventPath)
    {
        FMODAudioManager.PlayEvent(eventPath);
    }

    /// <summary>
    /// 停止 FMOD 事件（允許淡出）
    /// </summary>
    /// <param name="eventPath">完整事件路徑 (e.g., "event:/Music/BGM")</param>
    public static void StopEvent(string eventPath)
    {
        FMODAudioManager.StopEvent(eventPath, FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    /// <summary>
    /// 立即停止 FMOD 事件
    /// </summary>
    /// <param name="eventPath">完整事件路徑 (e.g., "event:/Music/BGM")</param>
    public static void StopEventImmediate(string eventPath)
    {
        FMODAudioManager.StopEvent(eventPath, FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    /// <summary>
    /// 停止所有 FMOD 事件（允許淡出）
    /// </summary>
    public static void StopAllEvents()
    {
        FMODAudioManager.StopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    /// <summary>
    /// 立即停止所有 FMOD 事件
    /// </summary>
    public static void StopAllEventsImmediate()
    {
        FMODAudioManager.StopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }
}
