using System;
using UnityEngine;

namespace App.Core.Tools
{
    /// <summary>
    /// 功能:音频工具 
    /// </summary>
    public class AudioTools
    {
        private const float ReScaleFactor = 32768f;
        /// <summary>
        /// AudioClip转byte[]
        /// </summary>
        /// <param name="audioClip"></param>
        /// <returns></returns>
        public static byte[] ClipToBytes(AudioClip audioClip)
        {
            var samples = new float[audioClip.samples];

            audioClip.GetData(samples, 0);

            var intData = new short[samples.Length];

            var bytesData = new byte[samples.Length * 2];

            for (var i = 0; i < samples.Length; i++)
            {
                intData[i] = (short)(samples[i] * ReScaleFactor);
                var byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }

            return bytesData;
        }

        /// <summary>
        /// byte[]转AudioClip
        /// </summary>
        /// <param name="rawData"></param>
        /// <param name="frequency">32000,24000</param>
        /// <returns></returns>
        public static AudioClip BytesToClip(byte[] rawData, int frequency = 16000)
        {
            var samples = new float[rawData.Length / 2];

            for (var i = 0; i < rawData.Length; i += 2)
            {
                var st = BitConverter.ToInt16(rawData, i);
                var ft = st / ReScaleFactor;
                samples[i / 2] = ft;
            }

            var audioClip = AudioClip.Create("clip", samples.Length, 1, frequency, false);
            audioClip.SetData(samples, 0);

            return audioClip;
        }
    }
}