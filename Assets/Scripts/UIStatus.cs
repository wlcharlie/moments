using System;
using UnityEngine;

public enum StatusType
{
    Heart,
    Money,
    Energy
}

[System.Serializable]
public class ComponentReferences
{
    public Sprite heartSprite;
    public Sprite moneySprite;
    public Sprite energySprite;
    public GameObject meterObject;
    public GameObject imageObject;
}

public class UIStatus : MonoBehaviour
{

    private readonly String heartBgColor = "#FFAC9C";
    private readonly String moneyBgColor = "#93D9BF";
    private readonly String energyBgColor = "#FFE77A";
    [SerializeField] private StatusType statusType = StatusType.Heart;
    [Range(0, 1)]
    [SerializeField] private float value = 1f;

    [SerializeField] private ComponentReferences references;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        references.meterObject.GetComponent<UnityEngine.UI.Image>().color = GetStatusColor();
        references.imageObject.GetComponent<UnityEngine.UI.Image>().sprite = GetStatusSprite();
        UpdateMeterHeight();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnValidate()
    {
        if (references.meterObject != null)
        {
            references.meterObject.GetComponent<UnityEngine.UI.Image>().color = GetStatusColor();
            UpdateMeterHeight();
        }
        if (references.imageObject != null)
        {
            references.imageObject.GetComponent<UnityEngine.UI.Image>().sprite = GetStatusSprite();
        }
    }

    private Color GetStatusColor()
    {
        return statusType switch
        {
            StatusType.Heart => ColorUtility.TryParseHtmlString(heartBgColor, out var heartColor) ? heartColor : Color.white,
            StatusType.Money => ColorUtility.TryParseHtmlString(moneyBgColor, out var moneyColor) ? moneyColor : Color.white,
            StatusType.Energy => ColorUtility.TryParseHtmlString(energyBgColor, out var energyColor) ? energyColor : Color.white,
            _ => Color.white,
        };
    }

    private Sprite GetStatusSprite()
    {
        return statusType switch
        {
            StatusType.Heart => references.heartSprite,
            StatusType.Money => references.moneySprite,
            StatusType.Energy => references.energySprite,
            _ => null,
        };
    }

    private void UpdateMeterHeight()
    {
        var rectTransform = references.meterObject.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            var size = rectTransform.sizeDelta;
            size.y = value * 80f;
            rectTransform.sizeDelta = size;
        }
    }
}
