using UnityEngine;
using PixelCrushers.DialogueSystem;

public class ResponseMenuTitle : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("ResponseMenuTitle Start");
        var dialogueSystemEvents = DialogueManager.instance.GetComponent<DialogueSystemEvents>();
        if (dialogueSystemEvents != null)
        {
            dialogueSystemEvents.conversationEvents.onConversationResponseMenu.AddListener(SetResponseMenuTitle);
            Debug.Log("Event listener added successfully");
        }
        else
        {
            Debug.LogError("DialogueSystemEvents component not found on DialogueManager!");
        }
    }

    void SetResponseMenuTitle(Response[] responses)
    {
        Debug.Log("SetResponseMenuTitle called");
        if (responses != null && responses.Length > 0)
        {
            string entryTitle = responses[0].destinationEntry.Title;
            Debug.Log($"OnConversationResponseMenu: {entryTitle}");
            // get text mesh pro
            TMPro.TextMeshProUGUI textMeshPro = GetComponent<TMPro.TextMeshProUGUI>();
            if (textMeshPro != null)
            {
                textMeshPro.text = entryTitle;
                Debug.Log("Response Menu Title updated successfully");
            }
            else
            {
                Debug.LogError("TextMeshPro component not found on ResponseMenuTitle GameObject!");
            }

            // list all component name
            // var components = GetComponents<Component>();
            // foreach (var component in components)
            // {
            //     Debug.Log($"Component: {component.GetType().Name}");
            // }
        }
    }
}
