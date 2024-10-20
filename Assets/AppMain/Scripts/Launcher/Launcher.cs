using System.Collections;
using AppFrame.Config;
using HybridCLR;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Launcher
{
    public class Launcher : MonoBehaviour
    {
        private Slider slider;
        private Text text;
        private void Awake()
        {
            slider = transform.Find("Canvas/Slider").GetComponent<Slider>();
            text = transform.Find("Canvas/Slider/Text").GetComponent<Text>();
            slider.gameObject.SetActive(false);
            
            Global.AppConfig = Resources.Load<AppConfig>("AppConfig");

            HybridABManager.Instance.InitManager();
            
            HotFix.Init((isHotfix) =>
            {
                StartCoroutine(DownLoading(isHotfix));
            });
        }
        
        private IEnumerator DownLoading(bool isHotfix)
        {
            if (isHotfix)
            {
                slider.gameObject.SetActive(true);
                float time = 0;
                float previousSize = 0;
                float speed = 0;
                while (HotFix.GetProgress < 1)
                {
                    yield return new WaitForEndOfFrame();
                    time += Time.deltaTime;
                    if (time >= 1f)
                    {
                        speed = (HotFix.GetLoadedSize - previousSize);
                        previousSize = HotFix.GetLoadedSize;
                        time = 0;
                    }

                    speed = speed > 0 ? speed : 0;
                    //set progress 0-1
                    slider.value = HotFix.GetProgress;
                    text.text = $"{HotFix.GetLoadedSize.ToString("F2")}M/{HotFix.TotalSize.ToString("F2")}M  {speed.ToString("F2")}M/S";
                }
                yield return new WaitForEndOfFrame();
                //set progress 1
                slider.value = 1;
                text.text = $"{HotFix.TotalSize.ToString("F2")}M/{HotFix.TotalSize.ToString("F2")}M  {0}M/S";
                yield return new WaitForSeconds(0.2f);
            }
            //更新下载完成，开始运行App
            
            Load(isHotfix);
        }

        private void Load(bool isHotfix)
        {
            if (isHotfix)
            {
                LoadMetadataForAOTAssemblies();
                LoadHotfixAssemblies();
                var ab = HybridABManager.Instance.LoadAssetBundle("Scenes", "AppScene");
                Global.UpdateView = HybridABManager.Instance.LoadAsset<GameObject>("App", "Update", "UpdateView");
            }
            
            SceneManager.LoadScene("AppScene");
        }

        private void LoadHotfixAssemblies()
        {
            foreach (var assemblyName in Global.HotfixAssemblyNames)
            {
                var ta = HybridABManager.Instance.LoadAsset<TextAsset>("App", "Dll", assemblyName);
                System.Reflection.Assembly.Load(ta.bytes);
            }
        }
        private void LoadMetadataForAOTAssemblies()
        {
            HomologousImageMode mode = HomologousImageMode.SuperSet;
            foreach (var aotDllName in Global.AOTMetaAssemblyNames)
            {
                TextAsset ta =  HybridABManager.Instance.LoadAsset<TextAsset>("App", "Dll", aotDllName);
                LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(ta.bytes, mode);
            }
        }
    }
}