using UnityEngine;

public class DataServiceManager : MonoBehaviour
{
  public static DataServiceManager Instance { get; private set; }

  private IDataService dataService;

  private void Awake()
  {
    if (Instance == null)
    {
      Instance = this;
      DontDestroyOnLoad(gameObject);

      // 在這裡切換服務，未來只需改這行
      dataService = GetComponent<SupabaseService>();

      if (dataService == null)
      {
        Debug.LogError("DataServiceManager: 找不到 SupabaseService 元件，請確認已加入到同一個 GameObject");
      }
      // dataService = GetComponent<FirebaseService>();
      // dataService = GetComponent<GoogleSheetService>();
    }
    else
    {
      Destroy(gameObject);
    }
  }

  public void SubmitResponse(string playerId, string conversationTitle, string question, string answer)
  {
    dataService.SubmitResponse(playerId, conversationTitle, question, answer);
  }
}