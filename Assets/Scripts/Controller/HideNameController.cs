using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HideNameController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] private Image image;
    [SerializeField] private string visibleColor = "#FFFFFFFF";
    [SerializeField] private string hiddenColor = "#FFFFFF00";

    void OnEnable()
    {
        if (textMeshPro != null)
        {
            textMeshPro.OnPreRenderText += OnTextChanged;
            UpdateImageVisibility();
        }
    }

    void OnDisable()
    {
        if (textMeshPro != null)
        {
            textMeshPro.OnPreRenderText -= OnTextChanged;
        }
    }

    private void OnTextChanged(TMP_TextInfo textInfo)
    {
        StartCoroutine(UpdateImageVisibilityNextFrame());
    }

    private IEnumerator UpdateImageVisibilityNextFrame()
    {
        yield return null;
        UpdateImageVisibility();
    }

    private void UpdateImageVisibility()
    {
        if (image == null) return;

        // 1. 過濾文字：如果是「旁白」則改成空字串
        if (textMeshPro.text == "旁白")
        {
            textMeshPro.text = "";
        }

        // 2. Trim
        string trimmedText = textMeshPro.text.Trim();

        // 3. 判斷顯示/隱藏
        bool shouldHide = string.IsNullOrEmpty(trimmedText);
        string hexColor = shouldHide ? hiddenColor : visibleColor;
        if (ColorUtility.TryParseHtmlString(hexColor, out Color color))
        {
            image.color = color;
        }
    }
}
