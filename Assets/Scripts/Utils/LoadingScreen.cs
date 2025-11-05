using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image progressBar;      // 指向 BarFill（Image Type=Filled）

    [Header("Smooth Settings")]
    [SerializeField] private float smoothSpeed = 5f;  // 平滑速度

    private float targetProgress = 0f;
    private float currentProgress = 0f;

    void Update()
    {
        // 平滑插值到目標進度
        if (currentProgress != targetProgress)
        {
            currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * smoothSpeed);

            // 當非常接近目標值時直接設為目標值
            if (Mathf.Abs(targetProgress - currentProgress) < 0.001f)
            {
                currentProgress = targetProgress;
            }

            if (progressBar != null)
            {
                progressBar.fillAmount = currentProgress;
            }
        }
    }

    /// <summary>
    /// 更新進度條的填充量
    /// </summary>
    /// <param name="progress">進度值 (0-1)</param>
    public void UpdateProgress(float progress)
    {
        targetProgress = Mathf.Clamp01(progress);
    }

    /// <summary>
    /// 重置進度條到 0
    /// </summary>
    public void ResetProgress()
    {
        targetProgress = 0f;
        currentProgress = 0f;
        if (progressBar != null)
        {
            progressBar.fillAmount = 0f;
        }
    }
}
