using UnityEngine;

public class TiledBackground : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // Set draw mode to tiled
        sr.drawMode = SpriteDrawMode.Tiled;

        // Calculate how big the sprite needs to be to cover the camera
        Camera cam = Camera.main;
        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;

        // Set the size to cover the screen
        sr.size = new Vector2(width, height);

        // Center it on camera
        transform.position = cam.transform.position;
        //123
    }
}