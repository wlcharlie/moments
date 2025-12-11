using UnityEngine;

public class SceneLoadController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FMODAudioManager.StopEvent("event:/music/music_piece_main");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
