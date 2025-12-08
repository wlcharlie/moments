using UnityEngine;
using PixelCrushers.DialogueSystem;

public class TrackedResponseButton : StandardUIResponseButton
{
    // 靜態 event，所有按鈕共用
    public static event System.Action<Response> OnResponseClicked;

    public override void OnClick()
    {
        Debug.Log($"TrackedResponseButton OnClick triggered. Response: {response?.formattedText.text}");

        // 觸發追蹤 event
        OnResponseClicked?.Invoke(response);

        // 執行原本的邏輯
        base.OnClick();
    }
}