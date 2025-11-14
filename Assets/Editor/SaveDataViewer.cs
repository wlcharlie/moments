using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class SaveDataViewer : EditorWindow
{
    private string saveFolderPath;
    private List<SaveFileInfo> saveFiles = new List<SaveFileInfo>();
    private Vector2 scrollPosition;
    private SaveFileInfo selectedFile;
    private Vector2 jsonScrollPosition;
    private bool showDeleteAllConfirmation = false;

    private class SaveFileInfo
    {
        public string fileName;
        public string filePath;
        public string fileSize;
        public string lastModified;
        public string jsonContent;
    }

    [MenuItem("Window/存檔資料檢視器")]
    public static void ShowWindow()
    {
        var window = GetWindow<SaveDataViewer>("存檔資料檢視器");
        window.minSize = new Vector2(500, 400);
    }

    void OnEnable()
    {
        // 初始化時取得存檔路徑
        saveFolderPath = Application.persistentDataPath;
        RefreshSaveFileList();
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        // 標題區域
        DrawHeader();

        EditorGUILayout.Space(10);

        // 主要內容區域 - 分割為左右兩欄
        EditorGUILayout.BeginHorizontal();

        // 左側：檔案列表
        DrawFileList();

        EditorGUILayout.Space(5);

        // 右側：JSON 內容預覽
        DrawJsonPreview();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    void DrawHeader()
    {
        EditorGUILayout.LabelField("存檔資料夾", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.SelectableLabel(saveFolderPath, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));

        if (GUILayout.Button("打開資料夾", GUILayout.Width(100)))
        {
            EditorUtility.RevealInFinder(saveFolderPath);
        }

        if (GUILayout.Button("重新整理", GUILayout.Width(80)))
        {
            RefreshSaveFileList();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // 統計資訊
        EditorGUILayout.LabelField($"找到 {saveFiles.Count} 個存檔檔案", EditorStyles.miniLabel);
    }

    void DrawFileList()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(250));
        EditorGUILayout.LabelField("存檔檔案列表", EditorStyles.boldLabel);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

        if (saveFiles.Count == 0)
        {
            EditorGUILayout.HelpBox("沒有找到任何存檔檔案", MessageType.Info);
        }
        else
        {
            foreach (var fileInfo in saveFiles)
            {
                DrawFileItem(fileInfo);
            }
        }

        EditorGUILayout.EndScrollView();

        // 底部按鈕
        EditorGUILayout.Space(5);

        if (saveFiles.Count > 0)
        {
            if (!showDeleteAllConfirmation)
            {
                if (GUILayout.Button("刪除所有存檔", GUILayout.Height(30)))
                {
                    showDeleteAllConfirmation = true;
                }
            }
            else
            {
                EditorGUILayout.HelpBox("確定要刪除所有存檔嗎？此操作無法復原！", MessageType.Warning);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("確認刪除", GUILayout.Height(25)))
                {
                    DeleteAllSaveFiles();
                    showDeleteAllConfirmation = false;
                }
                if (GUILayout.Button("取消", GUILayout.Height(25)))
                {
                    showDeleteAllConfirmation = false;
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndVertical();
    }

    void DrawFileItem(SaveFileInfo fileInfo)
    {
        bool isSelected = selectedFile == fileInfo;

        // 使用不同的背景顏色表示選中狀態
        Color originalColor = GUI.backgroundColor;
        if (isSelected)
        {
            GUI.backgroundColor = new Color(0.3f, 0.5f, 0.8f, 0.5f);
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUI.backgroundColor = originalColor;

        EditorGUILayout.BeginHorizontal();

        // 檔案名稱（可點擊選中）
        if (GUILayout.Button(fileInfo.fileName, EditorStyles.label))
        {
            selectedFile = fileInfo;
        }

        EditorGUILayout.EndHorizontal();

        // 檔案資訊
        EditorGUILayout.LabelField($"大小: {fileInfo.fileSize}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"修改: {fileInfo.lastModified}", EditorStyles.miniLabel);

        // 操作按鈕
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("檢視", EditorStyles.miniButton, GUILayout.Width(50)))
        {
            selectedFile = fileInfo;
        }

        if (GUILayout.Button("刪除", EditorStyles.miniButton, GUILayout.Width(50)))
        {
            if (EditorUtility.DisplayDialog("確認刪除", $"確定要刪除 {fileInfo.fileName} 嗎？", "刪除", "取消"))
            {
                DeleteSaveFile(fileInfo);
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(3);
    }

    void DrawJsonPreview()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

        EditorGUILayout.LabelField("JSON 內容預覽", EditorStyles.boldLabel);

        if (selectedFile == null)
        {
            EditorGUILayout.HelpBox("請從左側選擇一個檔案來預覽內容", MessageType.Info);
        }
        else
        {
            // 檔案資訊
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("檔案名稱:", EditorStyles.miniBoldLabel);
            EditorGUILayout.SelectableLabel(selectedFile.fileName, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.LabelField("完整路徑:", EditorStyles.miniBoldLabel);
            EditorGUILayout.SelectableLabel(selectedFile.filePath, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // JSON 內容
            EditorGUILayout.LabelField("內容:", EditorStyles.miniBoldLabel);
            jsonScrollPosition = EditorGUILayout.BeginScrollView(jsonScrollPosition, GUILayout.ExpandHeight(true));

            EditorGUILayout.TextArea(selectedFile.jsonContent, EditorStyles.textArea, GUILayout.ExpandHeight(true));

            EditorGUILayout.EndScrollView();

            // 操作按鈕
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("複製 JSON 到剪貼簿", GUILayout.Height(25)))
            {
                EditorGUIUtility.systemCopyBuffer = selectedFile.jsonContent;
                Debug.Log("JSON 內容已複製到剪貼簿");
            }
            if (GUILayout.Button("在系統中顯示檔案", GUILayout.Height(25)))
            {
                EditorUtility.RevealInFinder(selectedFile.filePath);
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    void RefreshSaveFileList()
    {
        saveFiles.Clear();
        selectedFile = null;

        if (!Directory.Exists(saveFolderPath))
        {
            Debug.LogWarning($"存檔資料夾不存在: {saveFolderPath}");
            return;
        }

        string[] jsonFiles = Directory.GetFiles(saveFolderPath, "*.json");

        foreach (string filePath in jsonFiles)
        {
            FileInfo fileInfo = new FileInfo(filePath);

            string jsonContent = "";
            try
            {
                jsonContent = File.ReadAllText(filePath);
            }
            catch (System.Exception e)
            {
                jsonContent = $"無法讀取檔案: {e.Message}";
            }

            saveFiles.Add(new SaveFileInfo
            {
                fileName = Path.GetFileName(filePath),
                filePath = filePath,
                fileSize = FormatFileSize(fileInfo.Length),
                lastModified = fileInfo.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss"),
                jsonContent = jsonContent
            });
        }

        // 按修改時間排序（最新的在前）
        saveFiles = saveFiles.OrderByDescending(f => File.GetLastWriteTime(f.filePath)).ToList();

        Debug.Log($"已重新整理存檔列表，找到 {saveFiles.Count} 個檔案");
    }

    void DeleteSaveFile(SaveFileInfo fileInfo)
    {
        try
        {
            File.Delete(fileInfo.filePath);
            Debug.Log($"已刪除存檔: {fileInfo.fileName}");
            RefreshSaveFileList();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"刪除存檔失敗: {e.Message}");
        }
    }

    void DeleteAllSaveFiles()
    {
        try
        {
            int count = 0;
            foreach (var fileInfo in saveFiles)
            {
                File.Delete(fileInfo.filePath);
                count++;
            }
            Debug.Log($"已刪除 {count} 個存檔檔案");
            RefreshSaveFileList();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"刪除所有存檔失敗: {e.Message}");
        }
    }

    string FormatFileSize(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";
        else if (bytes < 1024 * 1024)
            return $"{bytes / 1024.0:F2} KB";
        else
            return $"{bytes / (1024.0 * 1024.0):F2} MB";
    }
}
