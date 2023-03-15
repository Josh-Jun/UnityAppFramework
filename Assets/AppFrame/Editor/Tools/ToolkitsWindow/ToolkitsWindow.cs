using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.UI;
using Button = UnityEngine.UIElements.Button;


public class ToolkitsWindow : EditorWindow
{
    private VisualElement root;
    private ListView leftListView;
    private Label view_title;
    private string[] itemsName =
    {
        "BuildApp", 
        "BuildAssetBundle", 
        "SetAppScriptConfig", 
        "SetAppTableConfig", 
        "Table2CSharp", 
        "ChangePrefabsFont", 
        "FindSameFileName",
    };

    private List<VisualElement> viewElements = new List<VisualElement>();
    private int stamp = 0;

    private ScrollView infos;

    #region ChangePrefabsFont
    private ObjectField fontObjectField;
    private TextField textField;
    #endregion

    [MenuItem("Tools/ToolkitsWindow")]
    public static void ShowExample()
    {
        var icon = EditorGUIUtility.IconContent("Assembly Icon").image;
        ToolkitsWindow wnd = GetWindow<ToolkitsWindow>();
        wnd.titleContent = new GUIContent("Toolkits", icon);
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/AppFrame/Editor/Tools/ToolkitsWindow/ToolkitsWindow.uxml");
        visualTree.CloneTree(root);

        leftListView = root.Q<ListView>("left");
        // leftListView.
        leftListView.itemsSource = itemsName;
        leftListView.makeItem = MakeListItem;
        leftListView.bindItem = BindListItem;
        leftListView.onSelectedIndicesChange += OnItemsChosen;
        
        view_title = root.Q<Label>("view_title");
        view_title.text = itemsName[0];

        for (int i = 0; i < itemsName.Length; i++)
        {
            var view = root.Q<VisualElement>(itemsName[i]);
            view.style.display = DisplayStyle.None;
            viewElements.Add(view);
        }
        viewElements[stamp].style.display = DisplayStyle.Flex;

        #region ChangePrefabsFont
        textField = root.Q<TextField>("text_prefab_path");
        textField.value = "";
        fontObjectField = root.Q<ObjectField>("object_font");
        fontObjectField.objectType = typeof(Font);
        
        root.Q<Button>("prefab_path_browse").clicked += Browse;
        
        root.Q<Button>("change_font_apply").clicked += ChangePrefabsFont;
        #endregion
        
        infos = root.Q<ScrollView>("infos");
    }

    private void OnItemsChosen(IEnumerable<int> objs)
    {
        foreach (var index in objs)
        {
            view_title.text = itemsName[index];
            viewElements[stamp].style.display = DisplayStyle.None;
            viewElements[index].style.display = DisplayStyle.Flex;
            stamp = index;
        }
    }

    private void BindListItem(VisualElement ve, int index)
    {
        var label = ve as Label;
        label.text = itemsName[index];
        label.name = index.ToString();
    }

    private VisualElement MakeListItem()
    {
        var label = new Label();
        label.style.unityTextAlign = TextAnchor.MiddleLeft;
        label.style.paddingLeft = 16;
        return label;
    }
    
    #region ChangePrefabsFont
    private void Browse()
    {
        var newPath = EditorUtility.OpenFolderPanel("Prefabs Folder", textField.value, string.Empty);
        if (!string.IsNullOrEmpty(newPath))
        {
            var gamePath = System.IO.Path.GetFullPath(".");
            gamePath = gamePath.Replace("\\", "/");
            if (newPath.StartsWith(gamePath) && newPath.Length > gamePath.Length)
                newPath = newPath.Remove(0, gamePath.Length + 1);
            textField.value = newPath;
        }
    }
    private void ChangePrefabsFont()
    {
        var changeFont = (Font)fontObjectField.value;
        if (changeFont == null)
        {
            ShowHelpBox("请选择要更换的字体");
            return;
        }

        string[] allPath = AssetDatabase.FindAssets("t:Prefab", new string[] { textField.value });
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
                t.font = (Font)fontObjectField.value;
                EditorUtility.SetDirty(t);
                AssetDatabase.SaveAssets();
            }
            
            if (item.childCount > 0)
            {
                SetAllTextFont(item);
            }
        }
    }
    #endregion
    
    private void ShowHelpBox(string msg, HelpBoxMessageType type = HelpBoxMessageType.Info)
    {
        var helpBox = new HelpBox(msg, HelpBoxMessageType.Info);
        infos.Add(helpBox);
    }
}