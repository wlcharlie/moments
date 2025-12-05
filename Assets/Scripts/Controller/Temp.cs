using System.Collections;
using PixelCrushers.DialogueSystem;
using UnityEngine;

public class Temp : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(DelayedTransition());
    }

    private IEnumerator DelayedTransition()
    {
        // 等待三秒
        yield return new WaitForSeconds(3f);

        // 切換到 MainStoryScene
        TransitionManager.Instance.LoadSceneWithTransition("MainStoryScene", TransitionType.Cover, onLoadDone: () =>
        {
            Debug.Log("已切換到 MainStoryScene");
            DialogueManager.SetDialoguePanel(true, immediate: true);
            DialogueManager.StopAllConversations();
            DialogueManager.StartConversation("CH01_SC04_SE04");
        });
    }
}
