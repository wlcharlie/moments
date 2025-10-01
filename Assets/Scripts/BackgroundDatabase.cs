using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BackgroundDatabase", menuName = "Game/Background Database")]
public class BackgroundDatabase : ScriptableObject
{
    [SerializeField] private List<BackgroundSprite> backgroundSprites;

    public Sprite GetBackgroundSprite(string spriteName)
    {
        BackgroundSprite foundSprite = backgroundSprites.Find(bs => bs.spriteName == spriteName);
        return foundSprite?.sprite;
    }

    public bool HasSprite(string spriteName)
    {
        return backgroundSprites.Exists(bs => bs.spriteName == spriteName);
    }
}