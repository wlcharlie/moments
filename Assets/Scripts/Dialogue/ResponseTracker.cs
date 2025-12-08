using UnityEngine;
using PixelCrushers.DialogueSystem;

public class ResponseTracker : MonoBehaviour
{
    private void OnEnable()
    {
        TrackedResponseButton.OnResponseClicked += HandleResponseClicked;
    }

    private void OnDisable()
    {
        TrackedResponseButton.OnResponseClicked -= HandleResponseClicked;
    }

    private void HandleResponseClicked(Response response)
    {
        if (response == null)
        {
            Debug.LogWarning("ResponseTracker: response 是 null");
            return;
        }

        // 取得對話資訊
        string conversationTitle = DialogueManager.lastConversationStarted;
        string question = GetQuestion(response);
        string answer = response.formattedText.text;

        // 取得玩家 ID
        string playerId = GetPlayerId();

        Debug.Log($"ResponseTracker: 記錄回應 - PlayerId: {playerId}, Conversation: {conversationTitle}, Question: {question}, Answer: {answer}");

        // 提交到資料服務
        if (DataServiceManager.Instance != null)
        {
            // Debug.Log($"ResponseTracker: 提交回應到 DataServiceManager - PlayerId: {playerId}, Conversation: {conversationTitle}, Question: {question}, Answer: {answer}");
            DataServiceManager.Instance.SubmitResponse(playerId, conversationTitle, question, answer);
        }
        else
        {
            Debug.LogWarning("ResponseTracker: DataServiceManager.Instance 是 null");
        }
    }

    private string GetQuestion(Response response)
    {
        // 嘗試從 destinationEntry 取得 Title 作為問題
        if (response.destinationEntry != null && !string.IsNullOrEmpty(response.destinationEntry.Title))
        {
            return response.destinationEntry.Title;
        }

        // 備用方案：使用 conversationID 和 entryID
        return $"Entry_{response.destinationEntry?.conversationID}_{response.destinationEntry?.id}";
    }

    private string GetPlayerId()
    {
        if (PlayerDataManager.Instance != null)
        {
            return PlayerDataManager.Instance.PlayerId;
        }

        // 備用方案：使用裝置識別碼
        Debug.LogWarning("ResponseTracker: PlayerDataManager.Instance 是 null，使用裝置識別碼");
        return SystemInfo.deviceUniqueIdentifier;
    }
}
