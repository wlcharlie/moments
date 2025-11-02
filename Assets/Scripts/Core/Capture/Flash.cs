using System.Collections;
using UnityEngine;

public class Flash : MonoBehaviour
{
    [SerializeField] private float fadeInDuration = 0.1f;  // 快速淡入時間
    [SerializeField] private float fadeOutDuration = 0.5f; // 慢慢淡出時間

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetAlpha(0f);
    }

    [ContextMenu("觸發閃光")]
    public void TriggerFlash()
    {
        StopAllCoroutines();
        StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        // 快速淡入到1
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(elapsed / fadeInDuration);
            yield return null;
        }
        SetAlpha(1f);

        // 慢慢淡出到0
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(1f - elapsed / fadeOutDuration);
            yield return null;
        }
        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}
