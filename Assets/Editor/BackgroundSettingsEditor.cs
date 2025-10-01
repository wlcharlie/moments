using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(BackgroundSettings))]
public class BackgroundSettingsEditor : Editor
{
    private SerializedProperty backgroundSprites;
    private bool showPreview = true;
    private Vector2 scrollPosition;
    private string searchFilter = "";

    private void OnEnable()
    {
        backgroundSprites = serializedObject.FindProperty("backgroundSprites");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        BackgroundSettings settings = (BackgroundSettings)target;

        // Header
        EditorGUILayout.LabelField("Background Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Statistics
        EditorGUILayout.BeginVertical(GUI.skin.box);
        EditorGUILayout.LabelField($"Total Sprites: {backgroundSprites.arraySize}");
        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();

        // Search filter
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
        searchFilter = EditorGUILayout.TextField(searchFilter);
        if (GUILayout.Button("Clear", GUILayout.Width(50)))
        {
            searchFilter = "";
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        // Toggle preview
        showPreview = EditorGUILayout.Toggle("Show Previews", showPreview);
        EditorGUILayout.Space();

        // Sprite list with scroll view
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(400));

        // Draw each sprite entry
        List<int> indicesToRemove = new List<int>();

        for (int i = 0; i < backgroundSprites.arraySize; i++)
        {
            SerializedProperty element = backgroundSprites.GetArrayElementAtIndex(i);
            SerializedProperty nameProperty = element.FindPropertyRelative("spriteName");
            SerializedProperty spriteProperty = element.FindPropertyRelative("sprite");

            string spriteName = nameProperty.stringValue;

            // Apply search filter
            if (!string.IsNullOrEmpty(searchFilter) &&
                !string.IsNullOrEmpty(spriteName) &&
                !spriteName.ToLower().Contains(searchFilter.ToLower()))
            {
                continue;
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();

            // Preview thumbnail
            if (showPreview && spriteProperty.objectReferenceValue != null)
            {
                Sprite sprite = (Sprite)spriteProperty.objectReferenceValue;
                if (sprite != null && sprite.texture != null)
                {
                    GUILayout.Label(sprite.texture, GUILayout.Width(48), GUILayout.Height(48));
                }
            }

            EditorGUILayout.BeginVertical();

            // Store old sprite reference
            Sprite oldSprite = (Sprite)spriteProperty.objectReferenceValue;

            // Sprite field
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(spriteProperty, GUIContent.none);

            // Remove button
            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                indicesToRemove.Add(i);
            }
            EditorGUILayout.EndHorizontal();

            // Auto-update name when sprite changes
            Sprite newSprite = (Sprite)spriteProperty.objectReferenceValue;
            if (newSprite != oldSprite && newSprite != null)
            {
                if (newSprite.texture != null)
                {
                    nameProperty.stringValue = newSprite.texture.name;
                }
                else
                {
                    nameProperty.stringValue = newSprite.name;
                }
            }
            else if (newSprite == null)
            {
                nameProperty.stringValue = "";
            }

            // Name field
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Name:", GUILayout.Width(40));
            nameProperty.stringValue = EditorGUILayout.TextField(nameProperty.stringValue);
            EditorGUILayout.EndHorizontal();

            // Duplicate name warning
            if (HasDuplicateName(nameProperty.stringValue, i))
            {
                EditorGUILayout.HelpBox("Duplicate name detected!", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        // Remove marked indices
        for (int i = indicesToRemove.Count - 1; i >= 0; i--)
        {
            backgroundSprites.DeleteArrayElementAtIndex(indicesToRemove[i]);
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();

        // Add new sprite button
        if (GUILayout.Button("Add New Sprite", GUILayout.Height(30)))
        {
            backgroundSprites.arraySize++;
        }

        // Clear all button
        if (GUILayout.Button("Clear All", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Clear All Sprites",
                "Are you sure you want to remove all sprites?", "Yes", "No"))
            {
                backgroundSprites.ClearArray();
            }
        }

        EditorGUILayout.EndHorizontal();

        // Batch operations
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Batch Operations", EditorStyles.boldLabel);

        if (GUILayout.Button("Sort by Name"))
        {
            SortSpritesByName();
        }

        if (GUILayout.Button("Remove Empty Entries"))
        {
            RemoveEmptyEntries();
        }

        // Auto-populate from folder
        EditorGUILayout.Space();
        if (GUILayout.Button("Auto-populate from Folder"))
        {
            string folderPath = EditorUtility.OpenFolderPanel("Select Sprite Folder", "Assets", "");
            if (!string.IsNullOrEmpty(folderPath))
            {
                PopulateFromFolder(folderPath);
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

    private bool HasDuplicateName(string name, int currentIndex)
    {
        if (string.IsNullOrEmpty(name)) return false;

        for (int i = 0; i < backgroundSprites.arraySize; i++)
        {
            if (i == currentIndex) continue;

            SerializedProperty element = backgroundSprites.GetArrayElementAtIndex(i);
            SerializedProperty nameProperty = element.FindPropertyRelative("spriteName");

            if (nameProperty.stringValue == name)
            {
                return true;
            }
        }
        return false;
    }

    private void SortSpritesByName()
    {
        List<(string name, Sprite sprite)> spriteList = new List<(string, Sprite)>();

        for (int i = 0; i < backgroundSprites.arraySize; i++)
        {
            SerializedProperty element = backgroundSprites.GetArrayElementAtIndex(i);
            SerializedProperty nameProperty = element.FindPropertyRelative("spriteName");
            SerializedProperty spriteProperty = element.FindPropertyRelative("sprite");

            spriteList.Add((nameProperty.stringValue, (Sprite)spriteProperty.objectReferenceValue));
        }

        spriteList.Sort((a, b) => string.Compare(a.name, b.name));

        backgroundSprites.ClearArray();

        foreach (var item in spriteList)
        {
            backgroundSprites.arraySize++;
            SerializedProperty element = backgroundSprites.GetArrayElementAtIndex(backgroundSprites.arraySize - 1);
            element.FindPropertyRelative("spriteName").stringValue = item.name;
            element.FindPropertyRelative("sprite").objectReferenceValue = item.sprite;
        }
    }

    private void RemoveEmptyEntries()
    {
        for (int i = backgroundSprites.arraySize - 1; i >= 0; i--)
        {
            SerializedProperty element = backgroundSprites.GetArrayElementAtIndex(i);
            SerializedProperty spriteProperty = element.FindPropertyRelative("sprite");

            if (spriteProperty.objectReferenceValue == null)
            {
                backgroundSprites.DeleteArrayElementAtIndex(i);
            }
        }
    }

    private void PopulateFromFolder(string folderPath)
    {
        if (!folderPath.StartsWith(Application.dataPath))
        {
            Debug.LogError("Please select a folder within the Assets directory");
            return;
        }

        string relativePath = "Assets" + folderPath.Substring(Application.dataPath.Length);
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { relativePath });

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);

            if (sprite != null)
            {
                backgroundSprites.arraySize++;
                SerializedProperty element = backgroundSprites.GetArrayElementAtIndex(backgroundSprites.arraySize - 1);
                element.FindPropertyRelative("spriteName").stringValue = sprite.texture.name;
                element.FindPropertyRelative("sprite").objectReferenceValue = sprite;
            }
        }
    }
}