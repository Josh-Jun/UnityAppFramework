using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using AppFrame.Info;
using AppFrame.Tools;
using UnityEngine.Networking;

namespace AppFrame.Manager
{
    public partial class NetcomManager : SingletonMonoEvent<NetcomManager>
    {
        public const string HttpAppID = "";
        public const string HttpAppSecret = "";

        /// <summary>
        /// Token验证
        /// </summary>
        public static string Token { get; set; }

        /// <summary>
        /// HTTP服务器地址
        /// </summary>
        public static string HttpAddress
        {
            private set { }
            get
            {
                return AppInfo.AppConfig.IsTestServer ? "" : ""; //测试服务器:生产服务器
            }
        }

        /// <summary>
        /// Socket服务器地址
        /// </summary>
        public static string SocketAddress
        {
            private set { }
            get
            {
                return AppInfo.AppConfig.IsTestServer ? "" : ""; //测试服务器:生产服务器
            }
        }

        /// <summary>
        /// Socket服务器端口
        /// </summary>
        public static int SocketPort
        {
            private set { }
            get
            {
                return AppInfo.AppConfig.IsTestServer ? 17888 : 8080; //测试服务器:生产服务器
            }
        }

        public static string ABUrl
        {
            private set { }
            get
            {
                string test_url = Path.Combine(Application.dataPath.Replace("Assets", ""), "AssetBundle/"); //本地AB包地址
                string pro_url = "https://meta-oss.genimous.com/vr-ota/dev_test/AssetBundle/"; //服务器AB包地址
                return AppInfo.AppConfig.IsTestServer ? test_url : pro_url;
            }
        }

        public static string AppUrl
        {
            private set { }
            get
            {
                string test_url = Path.Combine(Application.dataPath.Replace("Assets", ""), "App/"); //本地AB包地址
                string pro_url = "https://meta-oss.genimous.com/vr-ota/App/"; //服务器AB包地址
                return AppInfo.AppConfig.IsTestServer ? test_url : pro_url;
            }
        }

        protected override void OnSingletonMonoInit()
        {
            base.OnSingletonMonoInit();
            unityWebRequesters = new UnityWebRequester[maxUnityWebRequesterNumber];
            TimeTaskManager.Instance.AddTimeTask(CreateUnityWebRequester, 1, TimeUnit.Second, -1);
        }

        private static UnityWebRequester[] unityWebRequesters;
        private byte maxUnityWebRequesterNumber = 10;
        private static int pointer = -1;
        private void CreateUnityWebRequester()
        {
            if (pointer + 1 < maxUnityWebRequesterNumber)
            {
                pointer++;
                UnityWebRequester uwr = new UnityWebRequester();
                unityWebRequesters[pointer] = uwr;
            }
        }
        
        public static UnityWebRequester Uwr
        {
            private set { }
            get
            {
                UnityWebRequester unityWebRequester;
                if (pointer > -1)
                {
                    unityWebRequester = unityWebRequesters[pointer];
                    unityWebRequesters[pointer] = null;
                    pointer--;
                }
                else
                {
                    unityWebRequester = new UnityWebRequester();
                }

                return unityWebRequester;
            }
        }
    }
}