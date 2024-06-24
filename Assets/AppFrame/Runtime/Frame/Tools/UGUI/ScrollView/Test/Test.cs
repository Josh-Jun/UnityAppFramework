using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    
    public ScrollView scrollView;
    List<ChatDialogData> testData = new List<ChatDialogData>();
    // Start is called before the first frame update
    void Start()
    {
        // 构造测试数据
         InitData();
        
         scrollView.SetInitFunc((index, rectTransform) =>
         {
             // 更新item的UI元素
             rectTransform.gameObject.SetActive(true);
             ChatDialogData data = testData[index];
             ChatDialogItem item = rectTransform.gameObject.GetComponent<ChatDialogItem>();
             item.AddChatDialog(data.type, data.msg);
             testData[index].size = rectTransform.sizeDelta;
         });
        scrollView.SetUpdateFunc((index, rectTransform) =>
        {
            // 更新item的UI元素
            rectTransform.gameObject.SetActive(true);
            ChatDialogData data = testData[index];
            ChatDialogItem item = rectTransform.gameObject.GetComponent<ChatDialogItem>();
            item.AddChatDialog(data.type, data.msg);
        });
        scrollView.SetItemSizeFunc((index) =>
        {
            // 返回item的尺寸
            return testData[index].size;
        });
        scrollView.SetItemCountFunc(() =>
        {
            // 返回数据列表item的总数
            return testData.Count;
        });

        scrollView.UpdateData();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ChatDialogData data = new ChatDialogData();
            data.type = ChatDialogType.AI;
            data.msg = "你好";
            testData.Add(data);
            
            
            
            scrollView.UpdateData();
            scrollView.ScrollToLast();
            //Debug.Log("Name_" + testData.Count);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            ChatDialogData data = new ChatDialogData();
            data.type = ChatDialogType.AI;
            data.msg = "习近平总书记指出，中国将持续推进产业结构和能源结构调整，大力发展可再生能源。在不久前召开的全国生态环境保护大会上习近平总书记指出，中国将持续推进产业结构和能源结构调整，大力发展可再生能源。在不久前召开的全国生态环境保护大会上";
            testData.Add(data);
            
            
            
            scrollView.UpdateData();
            scrollView.ScrollToLast();
            //Debug.Log("Name_" + testData.Count);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            ChatDialogData data = new ChatDialogData();
            data.type = ChatDialogType.Person;
            data.msg = "你能做些什么";
            testData.Add(data);
            
            
            
            scrollView.UpdateData();
            scrollView.ScrollToLast();
            //Debug.Log("Name_" + testData.Count);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            ChatDialogData data = new ChatDialogData();
            data.type = ChatDialogType.Person;
            data.msg = "今年以来，从蔚蓝大海到雪域高原再到乡村田园，一台台大型风机拔地而起，迎风旋转，实现一个个新的突破。";
            testData.Add(data);
            
            
            
            scrollView.UpdateData();
            scrollView.ScrollToLast();
            //Debug.Log("Name_" + testData.Count);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            ChatDialogData data = new ChatDialogData();
            data.type = ChatDialogType.Time;
            data.msg = "16:24";
            testData.Add(data);
            
            
            
            scrollView.UpdateData();
            scrollView.ScrollToLast();
            //Debug.Log("Name_" + testData.Count);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            testData.Clear();
            scrollView.ClearData();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            InitData();
            scrollView.UpdateData();
            scrollView.ScrollToLast();
        }
    }
    
    private void InitData()
    {
        // 构建50000个排名数据
        for (int i = 1; i <= 10; ++i)
        {
            ChatDialogData data = new ChatDialogData();
            data.type = ChatDialogType.Person;
            data.msg = "Name_" + i;
            testData.Add(data);
            //Debug.Log("Name_" + i);
        }
    }
}
