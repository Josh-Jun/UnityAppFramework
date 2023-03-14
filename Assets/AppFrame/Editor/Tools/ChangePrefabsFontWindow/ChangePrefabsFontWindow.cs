using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.UI;
using Button = UnityEngine.UIElements.Button;


public class ChangePrefabsFontWindow : EditorWindow
{
    [MenuItem("Tools/New ToolsWindow/ChangePrefabsFont")]
    public static void ShowExample()
    {
        ChangePrefabsFontWindow wnd = GetWindow<ChangePrefabsFontWindow>();
        wnd.titleContent = new GUIContent("ChangePrefabsFont");
    }
    private Font changeFont;
    private string prefabsPath;

    private ObjectField fontObjectField;
    private TextField textField;
    private ScrollView infos;
    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/AppFrame/Editor/Tools/ChangePrefabsFontWindow/ChangePrefabsFontWindow.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);
        
        textField = root.Q<TextField>("prefab_path");
        
        fontObjectField = root.Q<ObjectField>("font");
        fontObjectField.objectType = typeof(Font);
        
        root.Q<Button>("browse").clicked += Browse;
        
        root.Q<Button>("apply").clicked += ChangePrefabsFont;

        infos = new ScrollView();
        root.Add(infos);
    }

    private void Browse()
    {
        var newPath = EditorUtility.OpenFolderPanel("Prefabs Folder", prefabsPath, string.Empty);
        if (!string.IsNullOrEmpty(newPath))
        {
            var gamePath = System.IO.Path.GetFullPath(".");
            gamePath = gamePath.Replace("\\", "/");
            if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length)
                newPath = newPath.Remove(0, gamePath.Length + 1);
            prefabsPath = newPath;
        }
        textField.value = prefabsPath;
    }
    private void ChangePrefabsFont()
    {
        changeFont = (Font)fontObjectField.value;
        if (changeFont == null)
        {
            ShowHelpBox("请选择要更换的字体");
            return;
        }

        string[] allPath = AssetDatabase.FindAssets("t:Prefab", new string[] { prefabsPath });
        for (int i = 0; i < allPath.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allPath[i]);
            var obj = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            if (obj != null)
            {
                SetAllTextFont(obj.transform);
            }
        }

        AssetDatabase.Refresh();
    }

    private void SetAllTextFont(Transform go)
    {
        foreach (Transform item in go)
        {
            Text t = item.GetComponent<Text>();
            if (t != null)
            {
                t.font = changeFont;
                EditorUtility.SetDirty(t);
                AssetDatabase.SaveAssets();
            }
            
            if (item.childCount > 0)
            {
                SetAllTextFont(item);
            }
        }
    }

    private void ShowHelpBox(string msg)
    {
        var helpBox = new HelpBox(msg, HelpBoxMessageType.Info);
        infos.Add(helpBox);
    }
}