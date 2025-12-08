using UnityEngine;
using UnityEditor;
using System;
using System.Collections;

namespace PixelCrushers.DialogueSystem
{

    // STEP 1: Name your type below, replacing "My Type":
    [CustomFieldTypeService.Name("Sprite")]

    // STEP 2: Rename the class by changing TemplateType to your type name:
    public class CustomFieldType_Sprite : CustomFieldType
    {

        // STEP 3: Replace the code in this Draw method with your own 
        // editor GUI code. If you leave it as-is, it will just draw
        // it as a plain text field. This is the GUILayout version.
        public override string Draw(string currentValue, DialogueDatabase database)
        {
            Sprite currentSprite = LoadSpriteFromValue(currentValue);
            Sprite sprite = EditorGUILayout.ObjectField(currentSprite, typeof(Sprite), false) as Sprite;
            return sprite != null ? GetSpriteValue(sprite) : string.Empty;
        }

        // STEP 4: Replace the code in this Draw method with your own 
        // editor GUI code. If you leave it as-is, it will just draw
        // it as a plain text field. This is the GUI version, which
        // uses an absolute Rect position instead of auto-layout.
        public override string Draw(Rect rect, string currentValue, DialogueDatabase database)
        {
            Sprite currentSprite = LoadSpriteFromValue(currentValue);
            Sprite sprite = EditorGUI.ObjectField(rect, currentSprite, typeof(Sprite), false) as Sprite;
            return sprite != null ? GetSpriteValue(sprite) : string.Empty;
        }

        // Store as "path|spriteName" to handle sprite sheets
        private string GetSpriteValue(Sprite sprite)
        {
            string path = AssetDatabase.GetAssetPath(sprite);
            // Check if this is a sub-sprite (from a sprite sheet)
            if (AssetDatabase.LoadAssetAtPath<Sprite>(path) != sprite)
            {
                // It's a sub-sprite, store path and name
                return $"{path}|{sprite.name}";
            }
            // Single sprite, just store path
            return path;
        }

        private Sprite LoadSpriteFromValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;

            // Check if value contains sprite name separator
            if (value.Contains("|"))
            {
                string[] parts = value.Split('|');
                string path = parts[0];
                string spriteName = parts[1];

                // Load all sprites at path and find the one with matching name
                UnityEngine.Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var obj in sprites)
                {
                    if (obj is Sprite s && s.name == spriteName)
                    {
                        return s;
                    }
                }
                return null;
            }

            // Legacy: just a path, load directly
            return AssetDatabase.LoadAssetAtPath<Sprite>(value);
        }
    }
}



/**/
