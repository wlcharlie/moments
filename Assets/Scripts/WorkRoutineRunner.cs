using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections;

public class WorkRoutineRunner : MonoBehaviour
{
    public PlayableDirector director;
    public CanvasGroup fade;
    public string nextScene = "WorkScene";
    public float fadeTime = 0.3f;
    public KeyCode skipKey = KeyCode.Space;

    IEnumerator Start()
    {
        yield return Fade(1, 0);
        director.Play();

        while (director.state == PlayState.Playing)
        {
            if (Input.GetKeyDown(skipKey))
            {
                director.time = director.duration - 0.05f;
            }
            yield return null;
        }

        yield return Fade(0, 1);
        SceneManager.LoadScene(nextScene);
    }

    IEnumerator Fade(float from, float to)
    {
        if (fade == null) yield break;
        float t = 0;
        fade.alpha = from;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            fade.alpha = Mathf.Lerp(from, to, t / fadeTime);
            yield return null;
        }
        fade.alpha = to;
    }
}
