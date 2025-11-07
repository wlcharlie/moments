using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class StartGameButtonPulse : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, Range(0f, 1f)] private float minimumAlpha = 0.4f;
    [SerializeField, Min(0.1f)] private float periodSeconds = 1.5f;
    [SerializeField, Range(0f, 0.9f)] private float maxAlphaHoldFraction = 0.2f;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool pauseOnHover = true;

    private CanvasGroup canvasGroup;
    private bool isHovered;
    private float timer;

    private void Awake()
    {
        EnsureCanvasGroup();
    }

    private void OnEnable()
    {
        timer = 0f;
        EnsureCanvasGroup();
        canvasGroup.alpha = 1f;
    }

    private void Reset()
    {
        EnsureCanvasGroup();
    }

    private void Update()
    {
        if (pauseOnHover && isHovered)
        {
            canvasGroup.alpha = 1f;
            return;
        }

        float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        timer += deltaTime;

        if (periodSeconds <= Mathf.Epsilon)
        {
            canvasGroup.alpha = 1f;
            return;
        }

        float cycle = Mathf.Repeat(timer / periodSeconds, 1f);
        float holdFraction = Mathf.Clamp01(maxAlphaHoldFraction);

        float remainingFraction = Mathf.Max(0.0001f, 1f - holdFraction);
        float halfFade = remainingFraction * 0.5f;
        float alpha;

        if (cycle < holdFraction)
        {
            alpha = 1f;
        }
        else if (cycle < holdFraction + halfFade)
        {
            float t = (cycle - holdFraction) / halfFade;
            alpha = Mathf.Lerp(1f, minimumAlpha, t);
        }
        else
        {
            float t = (cycle - holdFraction - halfFade) / halfFade;
            alpha = Mathf.Lerp(minimumAlpha, 1f, t);
        }

        canvasGroup.alpha = Mathf.Clamp01(alpha);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        if (pauseOnHover)
        {
            canvasGroup.alpha = 1f;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }

    private void EnsureCanvasGroup()
    {
        if (canvasGroup != null)
        {
            return;
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        minimumAlpha = Mathf.Clamp01(minimumAlpha);
        periodSeconds = Mathf.Max(0.1f, periodSeconds);
        maxAlphaHoldFraction = Mathf.Clamp(maxAlphaHoldFraction, 0f, 0.9f);
        if (!Application.isPlaying)
        {
            EnsureCanvasGroup();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }
    }
#endif
}

