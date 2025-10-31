using UnityEngine;
using UnityEngine.UI;

public class FakeProgressBar : MonoBehaviour
{
    [Header("UI")]
    public Image progressFill;      // 指向 BarFill（Image Type=Filled）

    [Header("Timing")]
    [Tooltip("由 0→1 的視覺時間（秒），越小越快")]
    public float duration = 3.0f;   // 假進度總時長
    [Tooltip("進度曲線（可用EaseInOut更自然）")]
    public AnimationCurve curve = AnimationCurve.EaseInOut(0,0, 1,1);

    float t;    // 0..1
    bool done;

    void OnEnable()
    {
        t = 0f;
        done = false;
        if (progressFill) progressFill.fillAmount = 0f;
    }

    void Update()
    {
        if (done || progressFill == null) return;

        t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, duration);
        float shown = Mathf.Clamp01(curve.Evaluate(Mathf.Clamp01(t)));
        progressFill.fillAmount = shown;

        if (shown >= 1f) done = true; // 到頂就停住（不切場景）
    }
}
