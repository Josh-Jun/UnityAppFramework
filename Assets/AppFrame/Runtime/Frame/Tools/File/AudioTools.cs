using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
/// <summary>
/// 功能:音频工具 
/// </summary>
namespace AppFrame.Tools
{
    public class AudioTools
    {
        /// <summary>
        /// AudioClip转byte[]
        /// </summary>
        /// <param name="audioClip"></param>
        /// <returns></returns>
        public static byte[] ClipToBytes(AudioClip audioClip)
        {
            float[] samples = new float[audioClip.samples];

            audioClip.GetData(samples, 0);

            short[] intData = new short[samples.Length];

            byte[] bytesData = new byte[samples.Length * 2];

            int rescaleFactor = 32767;

            for (int i = 0; i < samples.Length; i++)
            {
                intData[i] = (short)(samples[i] * rescaleFactor);
                byte[] byteArr = new byte[2];
                byteArr = BitConverter.GetBytes(intData[i]);
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
        public static AudioClip BytesToClip(byte[] rawData, int frequency)
        {
            float[] samples = new float[rawData.Length / 2];
            float rescaleFactor = 32767;
            short st = 0;
            float ft = 0;

            for (int i = 0; i < rawData.Length; i += 2)
            {
                st = BitConverter.ToInt16(rawData, i);
                ft = st / rescaleFactor;
                samples[i / 2] = ft;
            }

            AudioClip audioClip = AudioClip.Create("mySound", samples.Length, 1, frequency, false, false);
            audioClip.SetData(samples, 0);

            return audioClip;
        }
    }
}