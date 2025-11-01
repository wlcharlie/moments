using UnityEngine;
using System.Collections;

public class ComicImageFader : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float fadeDuration = 0.5f;

    private Sprite previousSprite;
    private Coroutine currentFadeCoroutine;

    void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        previousSprite = spriteRenderer.sprite;

        // If starting with a sprite, start with alpha 0 and fade in
        if (previousSprite != null)
        {
            SetAlpha(0f);
            StartFadeIn();
        }
    }

    void Update()
    {
        // Monitor sprite changes
        if (spriteRenderer.sprite != previousSprite)
        {
            HandleSpriteChange(previousSprite, spriteRenderer.sprite);
            previousSprite = spriteRenderer.sprite;
        }
    }

    private void HandleSpriteChange(Sprite oldSprite, Sprite newSprite)
    {
        // Stop any ongoing fade
        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);

        if (oldSprite == null && newSprite != null)
        {
            // Fade in: null -> image
            SetAlpha(0f);
            currentFadeCoroutine = StartCoroutine(FadeIn());
        }
        else if (oldSprite != null && newSprite == null)
        {
            // Fade out: image -> null
            currentFadeCoroutine = StartCoroutine(FadeOut());
        }
        else if (oldSprite != null && newSprite != null)
        {
            // Cross fade: image -> different image
            currentFadeCoroutine = StartCoroutine(CrossFade());
        }
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(1f);
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        float startAlpha = GetAlpha();

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(0f);
    }

    private IEnumerator CrossFade()
    {
        yield return StartCoroutine(FadeOut());
        yield return StartCoroutine(FadeIn());
    }

    private void SetAlpha(float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }

    private float GetAlpha()
    {
        return spriteRenderer.color.a;
    }

    // Public methods to manually trigger fades
    public void StartFadeIn()
    {
        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);
        currentFadeCoroutine = StartCoroutine(FadeIn());
    }

    public void StartFadeOut()
    {
        if (currentFadeCoroutine != null)
            StopCoroutine(currentFadeCoroutine);
        currentFadeCoroutine = StartCoroutine(FadeOut());
    }
}
