using System;
using System.Collections;
using UnityEngine;

public class DiceController : MonoBehaviour
{
    [SerializeField] private GameObject diceVisual;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Sprites")]
    [SerializeField] private Sprite[] faceSprites;  // 6 張正面點數 (index 0 = 1點)

    [Header("Settings")]
    [SerializeField] private float rollDuration = 1.5f;  // 滾動時間

    public event Action<int> OnRollComplete;

    public void Roll()
    {
        int result = UnityEngine.Random.Range(1, 7);
        StartCoroutine(RollCoroutine(result));
    }

    /// <summary>
    /// 擲出指定結果的骰子（用於強制移動）
    /// </summary>
    public void RollWithResult(int forcedResult)
    {
        int result = Mathf.Clamp(forcedResult, 1, 6);
        StartCoroutine(RollCoroutine(result));
    }

    private IEnumerator RollCoroutine(int result)
    {
        diceVisual.SetActive(true);
        StartRoll();
        yield return new WaitForSeconds(rollDuration);
        StopRoll(result);
        yield return new WaitForSeconds(2f);
        diceVisual.SetActive(false);
        OnRollComplete?.Invoke(result);
    }

    private void StartRoll()
    {
        // 播放FMOD obj_dice_drop
        FMODUnity.RuntimeManager.PlayOneShot("event:/object/obj_dice_drop");
        animator.enabled = true;
        animator.Play("roll");
    }

    private void StopRoll(int result)
    {
        // result: 1~6
        animator.enabled = false;
        spriteRenderer.sprite = faceSprites[result - 1];
    }
}