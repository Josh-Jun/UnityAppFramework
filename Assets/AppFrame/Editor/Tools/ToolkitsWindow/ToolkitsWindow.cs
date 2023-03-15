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

    [MenuItem("Tools/ToolkitsWindow", false, 1)]
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
        var textField = root.Q<TextField>("text_prefab_path");
        textField.value = "";
        var fontObjectField = root.Q<ObjectField>("object_font");
        fontObjectField.objectType = typeof(Font);
        
        root.Q<Button>("prefab_path_browse").clicked += () =>
        {
            textField.value = ChangePrefabsFont.Browse();
        };
        
        root.Q<Button>("change_font_apply").clicked += () =>
        {
            ChangePrefabsFont.ChangeFont((Font)fontObjectField.value, textField.value);
        };
        #endregion
        
        infos = root.Q<ScrollView>("infos");
    }

    private void OnItemsChosen(IEnumerable<int> objs)
    {
        foreach (var index in objs)
        {
            infos.Clear();
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
    
    private void ShowHelpBox(string msg, HelpBoxMessageType type = HelpBoxMessageType.Info)
    {
        var helpBox = new HelpBox(msg, HelpBoxMessageType.Info);
        infos.Add(helpBox);
    }
}