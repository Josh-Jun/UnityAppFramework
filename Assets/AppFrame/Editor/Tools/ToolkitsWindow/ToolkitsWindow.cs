using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;


public class ToolkitsWindow : EditorWindow
{
    private VisualElement root;
    private ListView leftListView;
    private string[] itemsName =
    {
        "item1", 
        "item2", 
        "item3", 
        "item4", 
        "item5", 
        "item6", 
        "item7", 
        "item8", 
        "item9", 
        "item10", 
        "item11", 
        "item12", 
        "item13", 
        "item14"
    };


    [MenuItem("Tools/ToolkitsWindow")]
    public static void ShowExample()
    {
        ToolkitsWindow wnd = GetWindow<ToolkitsWindow>();
        wnd.titleContent = new GUIContent("ToolkitsWindow");
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

        _button = root.Q<Button>();
    }

    private Button _button;
    private void OnItemsChosen(IEnumerable<int> objs)
    {
        foreach (var index in objs)
        {
            Debug.Log(index);
            DisplayStyle displayStyle = index == 1 ? DisplayStyle.Flex : DisplayStyle.None;
            _button.style.display = displayStyle;
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
        label.style.paddingLeft = 5;
        return label;
    }
}