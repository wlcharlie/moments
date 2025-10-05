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
            Sprite sprite = EditorGUILayout.ObjectField(AssetDatabase.LoadAssetAtPath<Sprite>(currentValue), typeof(Sprite), false) as Sprite;
            return sprite != null ? AssetDatabase.GetAssetPath(sprite) : string.Empty;
        }

        // STEP 4: Replace the code in this Draw method with your own 
        // editor GUI code. If you leave it as-is, it will just draw
        // it as a plain text field. This is the GUI version, which
        // uses an absolute Rect position instead of auto-layout.
        public override string Draw(Rect rect, string currentValue, DialogueDatabase database)
        {
            Sprite sprite = EditorGUI.ObjectField(rect, AssetDatabase.LoadAssetAtPath<Sprite>(currentValue), typeof(Sprite), false) as Sprite;
            return sprite != null ? AssetDatabase.GetAssetPath(sprite) : string.Empty;
        }
    }
}



/**/
