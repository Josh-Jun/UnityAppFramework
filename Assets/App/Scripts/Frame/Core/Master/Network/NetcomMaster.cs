using System.IO;
using App.Core.Tools;
using App.Runtime.Helper;
using UnityEngine;

namespace App.Core.Master
{
    public partial class NetcomMaster : SingletonMonoEvent<NetcomMaster>
    {
        protected override void OnSingletonMonoInit()
        {
            base.OnSingletonMonoInit();
            unityWebRequesters = new UnityWebRequester[maxUnityWebRequesterNumber];
            TimeTaskMaster.Instance.AddTimeTask(CreateUnityWebRequester, 1, TimeUnit.Second, -1);
        }

        private static UnityWebRequester[] unityWebRequesters;
        private const byte maxUnityWebRequesterNumber = 10;
        private static int pointer = -1;
        private static void CreateUnityWebRequester()
        {
            if (pointer + 1 >= maxUnityWebRequesterNumber) return;
            pointer++;
            var uwr = new UnityWebRequester();
            unityWebRequesters[pointer] = uwr;
        }
        
        public static UnityWebRequester Uwr
        {
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