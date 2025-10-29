using UnityEngine;

[System.Serializable]
public class AnimalData
{
    [Header("基本資訊")]
    [Tooltip("動物的序列號碼")]
    public string no;

    [Tooltip("動物名稱")]
    public string animalName;

    [Header("視覺素材")]
    [Tooltip("動物的主要頭像/圖示")]
    public Sprite mainSprite;

    [Tooltip("捕獲時的影像（可包含多個幀或狀態）")]
    public Sprite[] captureSprites;

    [Header("收集狀態")]
    [Tooltip("是否已收集此動物")]
    public bool collected;

    [Tooltip("是否已遇見此動物")]
    public bool seen;

    [Header("額外資訊")]
    [TextArea(3, 5)]
    [Tooltip("動物的描述文字")]
    public string description;

    // 建構函式
    public AnimalData(string no, string animalName)
    {
        this.no = no;
        this.animalName = animalName;
        this.collected = false;
        this.seen = false;
    }
}
