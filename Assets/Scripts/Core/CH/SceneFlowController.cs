using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlowController : MonoBehaviour
{
    [Header("Scene & Dialogue")]
    [SerializeField] private string targetSceneName = "MainStoryScene";
    [SerializeField] private string conversationId = "CH01_SC02_SE01";

    [Header("Transition")]
    [SerializeField] private TransitionType transitionType = TransitionType.LoadingScreen;

    [Header("Visual Cleanup")]
    [Tooltip("在切換前要停掉動畫的 Animator 清單。")]
    [SerializeField] private Animator[] animatorsToDisable;
    [Tooltip("在切換前要關閉的物件 (通常是書本/特效，以避免殘影。")] 
    [SerializeField] private GameObject[] objectsToHide;
    [Tooltip("在切換前要淡出的 CanvasGroup。")] 
    [SerializeField] private CanvasGroup[] canvasGroupsToZero;
    [Tooltip("在切換前要設為透明的 Image。")] 
    [SerializeField] private UnityEngine.UI.Image[] imagesToTransparent;

    public void OnBookGlowEnd()
    {
        DisableVisuals();

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.LoadSceneWithTransition(
                targetSceneName,
                transitionType,
                onLoadDone: () => DialogueManager.StartConversation(conversationId));
        }
        else
        {
            SceneManager.LoadScene(targetSceneName);
            DialogueManager.StartConversation(conversationId);
        }
    }

    private void DisableVisuals()
    {
        if (animatorsToDisable != null)
        {
            foreach (var animator in animatorsToDisable)
            {
                if (animator != null)
                {
                    animator.enabled = false;
                }
            }
        }

        if (canvasGroupsToZero != null)
        {
            foreach (var group in canvasGroupsToZero)
            {
                if (group != null)
                {
                    group.alpha = 0f;
                }
            }
        }

        if (objectsToHide != null)
        {
            foreach (var go in objectsToHide)
            {
                if (go != null)
                {
                    go.SetActive(false);
                }
            }
        }

        if (imagesToTransparent != null)
        {
            foreach (var image in imagesToTransparent)
            {
                if (image != null)
                {
                    Color c = image.color;
                    c.a = 0f;
                    image.color = c;
                }
            }
        }
    }
}
