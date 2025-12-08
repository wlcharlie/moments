using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;

public class SupabaseService : MonoBehaviour, IDataService
{
  [Header("Supabase 設定")]
  [SerializeField] private string supabaseUrl = "https://你的專案.supabase.co";
  [SerializeField] private string supabaseKey = "你的 anon key";
  [SerializeField] private string tableName = "event_responses";

  public void SubmitResponse(string playerId, string conversationTitle, string question, string answer)
  {
    StartCoroutine(PostResponse(playerId, conversationTitle, question, answer));
  }

  private IEnumerator PostResponse(string playerId, string conversationTitle, string question, string answer)
  {
    string url = $"{supabaseUrl}/rest/v1/{tableName}";

    string jsonBody = JsonUtility.ToJson(new ResponseData
    {
      player_id = playerId,
      conversation_title = conversationTitle,
      question = question,
      answer = answer
    });

    using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
    {
      byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
      request.uploadHandler = new UploadHandlerRaw(bodyRaw);
      request.downloadHandler = new DownloadHandlerBuffer();

      request.SetRequestHeader("Content-Type", "application/json");
      request.SetRequestHeader("apikey", supabaseKey);
      request.SetRequestHeader("Authorization", $"Bearer {supabaseKey}");
      request.SetRequestHeader("Prefer", "return=minimal");

      yield return request.SendWebRequest();

      if (request.result == UnityWebRequest.Result.Success)
      {
        Debug.Log("[DataService] 資料送出成功！");
      }
      else
      {
        Debug.LogError($"[DataService] 錯誤：{request.error}");
      }
    }
  }

  [System.Serializable]
  private class ResponseData
  {
    public string player_id;
    public string conversation_title;
    public string question;
    public string answer;
  }
}