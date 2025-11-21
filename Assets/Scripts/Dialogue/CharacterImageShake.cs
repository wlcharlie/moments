using UnityEngine;
using System.Collections;

/// <summary>
/// 角色圖片左右晃動效果
/// 可以設定為持續晃動或單次晃動
/// </summary>
public class CharacterImageShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [Tooltip("晃動強度（左右移動的距離）")]
    [SerializeField] private float shakeIntensity = 0.1f;
    
    [Tooltip("晃動頻率（每秒晃動次數）")]
    [SerializeField] private float shakeFrequency = 2f;

    // 公開屬性，供 Sequencer Command 使用
    public float ShakeIntensity 
    { 
        get => shakeIntensity; 
        set => shakeIntensity = Mathf.Max(0f, value); 
    }
    
    public float ShakeFrequency 
    { 
        get => shakeFrequency; 
        set => shakeFrequency = Mathf.Max(0.1f, value); 
    }
    
    [Tooltip("是否持續晃動（開啟後會一直晃動）")]
    [SerializeField] private bool continuousShake = false;
    
    [Tooltip("單次晃動持續時間（當 continuousShake 為 false 時使用）")]
    [SerializeField] private float shakeDuration = 0.5f;
    
    [Tooltip("使用未受時間縮放影響的時間")]
    [SerializeField] private bool useUnscaledTime = true;

    private Transform targetTransform;
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;
    private bool isShaking = false;

    void Awake()
    {
        targetTransform = transform;
        originalPosition = targetTransform.localPosition;
    }

    void OnEnable()
    {
        if (continuousShake)
        {
            StartContinuousShake();
        }
    }

    void OnDisable()
    {
        StopShake();
    }

    /// <summary>
    /// 開始持續晃動
    /// </summary>
    public void StartContinuousShake()
    {
        if (isShaking) return;
        
        continuousShake = true;
        isShaking = true;
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(ContinuousShakeCoroutine());
    }

    /// <summary>
    /// 開始單次晃動
    /// </summary>
    public void ShakeOnce(float? customDuration = null)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        
        float duration = customDuration ?? shakeDuration;
        shakeCoroutine = StartCoroutine(SingleShakeCoroutine(duration));
    }

    /// <summary>
    /// 停止晃動
    /// </summary>
    public void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
        
        isShaking = false;
        continuousShake = false;
        targetTransform.localPosition = originalPosition;
    }

    /// <summary>
    /// 更新原始位置（當物件位置改變時使用）
    /// </summary>
    public void UpdateOriginalPosition()
    {
        if (!isShaking)
        {
            originalPosition = targetTransform.localPosition;
        }
    }

    private IEnumerator ContinuousShakeCoroutine()
    {
        float timer = 0f;
        
        while (continuousShake)
        {
            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            timer += deltaTime;
            
            // 使用正弦波產生平滑的左右晃動
            float offset = Mathf.Sin(timer * shakeFrequency * Mathf.PI * 2f) * shakeIntensity;
            targetTransform.localPosition = originalPosition + new Vector3(offset, 0f, 0f);
            
            yield return null;
        }
        
        // 恢復原始位置
        targetTransform.localPosition = originalPosition;
        isShaking = false;
    }

    private IEnumerator SingleShakeCoroutine(float duration)
    {
        float elapsed = 0f;
        Vector3 startPosition = targetTransform.localPosition;
        
        while (elapsed < duration)
        {
            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += deltaTime;
            
            // 計算衰減（隨時間減弱晃動）
            float progress = elapsed / duration;
            float damper = 1f - Mathf.Pow(progress, 2); // 平方衰減
            
            // 使用正弦波產生平滑的左右晃動
            float offset = Mathf.Sin(elapsed * shakeFrequency * Mathf.PI * 2f) * shakeIntensity * damper;
            targetTransform.localPosition = startPosition + new Vector3(offset, 0f, 0f);
            
            yield return null;
        }
        
        // 恢復原始位置
        targetTransform.localPosition = originalPosition;
        shakeCoroutine = null;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        shakeIntensity = Mathf.Max(0f, shakeIntensity);
        shakeFrequency = Mathf.Max(0.1f, shakeFrequency);
        shakeDuration = Mathf.Max(0.1f, shakeDuration);
    }
#endif
}

