using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 控制骰子轉動動畫和結果
/// </summary>
public class DiceRoller : MonoBehaviour
{
    [Header("骰子圖片")]
    [SerializeField] private Sprite[] diceAnimationFrames; // dice_1 到 dice_9 的動畫幀
    [SerializeField] private Sprite diceResultSprite; // dice.png - 結果顯示的底圖
    [SerializeField] private Image diceImage; // 顯示骰子的 UI Image

    [Header("結果顯示方式")]
    [SerializeField] private bool useDotsDisplay = true; // 使用點點顯示（true）或數字顯示（false）
    [SerializeField] private DiceDots diceDots; // 點點顯示組件
    [SerializeField] private TMPro.TextMeshProUGUI resultNumberText; // 數字顯示組件（備用）

    [Header("動畫設定")]
    [SerializeField] private float animationDuration = 1.8f; // 動畫總時長（秒）
    [SerializeField] private float frameInterval = 0.08f; // 每幀間隔時間（秒）
    [SerializeField] private float slowDownDuration = 0.6f; // 最後減速時長（秒）

    [Header("結果顯示")]
    [SerializeField] private float resultDisplayDuration = 1.0f; // 結果顯示時長

    private bool isRolling = false;
    private int finalResult = 0; // 1-6 的最終結果

    // 事件：骰子動畫完成時觸發
    public System.Action<int> OnDiceRollComplete;

    /// <summary>
    /// 開始擲骰子
    /// </summary>
    public void RollDice()
    {
        if (isRolling) return;

        // 生成 1-6 的隨機結果
        finalResult = Random.Range(1, 7);
        StartCoroutine(RollAnimation());
    }

    /// <summary>
    /// 骰子轉動動畫協程
    /// </summary>
    private IEnumerator RollAnimation()
    {
        isRolling = true;
        float elapsed = 0f;

        // 隱藏結果顯示
        if (useDotsDisplay && diceDots != null)
        {
            diceDots.gameObject.SetActive(false);
        }
        else if (resultNumberText != null)
        {
            resultNumberText.gameObject.SetActive(false);
        }

        // 第一階段：快速轉動
        float fastDuration = animationDuration - slowDownDuration;
        while (elapsed < fastDuration)
        {
            // 隨機顯示動畫幀
            int randomFrame = Random.Range(0, diceAnimationFrames.Length);
            diceImage.sprite = diceAnimationFrames[randomFrame];

            yield return new WaitForSeconds(frameInterval);
            elapsed += frameInterval;
        }

        // 第二階段：減速並停在結果
        float slowDownElapsed = 0f;
        while (slowDownElapsed < slowDownDuration)
        {
            // 隨機顯示動畫幀，但間隔逐漸變長
            int randomFrame = Random.Range(0, diceAnimationFrames.Length);
            diceImage.sprite = diceAnimationFrames[randomFrame];

            float dynamicInterval = Mathf.Lerp(frameInterval, frameInterval * 3, slowDownElapsed / slowDownDuration);
            yield return new WaitForSeconds(dynamicInterval);
            slowDownElapsed += dynamicInterval;
        }

        // 顯示最終結果
        ShowResult(finalResult);

        // 等待一段時間後觸發完成事件
        yield return new WaitForSeconds(resultDisplayDuration);

        isRolling = false;
        OnDiceRollComplete?.Invoke(finalResult);
    }

    /// <summary>
    /// 顯示骰子結果
    /// 使用 dice.png 作為底圖，並在上面顯示點數（點點或數字）
    /// </summary>
    private void ShowResult(int result)
    {
        // 使用 dice.png 作為結果底圖
        if (diceResultSprite != null)
        {
            diceImage.sprite = diceResultSprite;
        }

        // 顯示結果
        if (useDotsDisplay && diceDots != null)
        {
            // 使用點點顯示
            diceDots.gameObject.SetActive(true);
            diceDots.SetDotNumber(result);
        }
        else if (resultNumberText != null)
        {
            // 使用數字顯示
            resultNumberText.text = result.ToString();
            resultNumberText.gameObject.SetActive(true);
        }

        Debug.Log($"骰子結果: {result}");
    }

    /// <summary>
    /// 獲取當前是否正在轉動
    /// </summary>
    public bool IsRolling()
    {
        return isRolling;
    }

    /// <summary>
    /// 獲取最後的骰子結果
    /// </summary>
    public int GetLastResult()
    {
        return finalResult;
    }
}
