using System.Text;
using AppFrame.Tools;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        // TimeUpdateManager.Instance.StartTimer((time) =>
        // {
        //     Log.I(time);
        // }, UpdateMold.FixedUpdate);
    }

    private StringBuilder _stringBuilder = new StringBuilder();
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var data = ExcelTools.ReadExcel("C:/Users/Meta_Unity/Desktop/p.xlsx");

            for (int i = 0; i < data.Count; i++)
            {
                for (int m = 1; m < data[i].datas.GetLength(0); m+=2)
                {
                    for (int n = 1; n < data[i].datas.GetLength(1); n++)
                    {
                        _stringBuilder.AppendLine($"// {data[i].datas[m+1,n]}");
                        _stringBuilder.AppendLine($"public const string {data[i].datas[m,n]} = \"android.permission.{data[i].datas[m,n]}\";");
                    }
                }
            }
            Debug.Log(_stringBuilder.ToString());
            FileTools.CreateFile("C:/Users/Meta_Unity/Desktop/p.txt", _stringBuilder.ToString());
        }
    }
}
