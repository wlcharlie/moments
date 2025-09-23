using UnityEngine;
using UnityEditor;
using System.IO;

public static class UpdateSpriteNameEditor
{
    [MenuItem("CONTEXT/TextureImporter/Update Sprite Name", priority = 999)]
    private static void UpdateSpriteName(MenuCommand command)
    {
        TextureImporter textureImporter = (TextureImporter)command.context;
        string path = AssetDatabase.GetAssetPath(textureImporter);
        string fileName = Path.GetFileNameWithoutExtension(path);

        SpriteMetaData[] spritesheet = textureImporter.spritesheet;
        for (int i = 0; i < spritesheet.Length; i++)
        {
            spritesheet[i].name = $"{fileName}_{i}";
        }
        textureImporter.spritesheet = spritesheet;

        EditorUtility.SetDirty(textureImporter);
        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
        textureImporter.SaveAndReimport();
    }
}
