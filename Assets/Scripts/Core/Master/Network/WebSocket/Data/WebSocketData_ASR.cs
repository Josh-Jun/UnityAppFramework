/* *
 * ===============================================
 * author      : Josh@win
 * e-mail      : shijun_z@163.com
 * create time : 2026年3月18 11:8
 * function    : 
 * ===============================================
 * */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.Core.Master
{
    [Serializable]
    public class ASRResponseData
    {
        public string action;
        public string data;
        public string code;
        public string desc;
        public string sid;
    }
    
    [Serializable]
    public class World
    {
        public int sc;

        public string w;

        public string wp;

        public string rl;

        public int wb;

        public int wc;

        public int we;

    }
    [Serializable]
    public class WorldData
    {
        public List<World> cw = new ();

        public int wb;

        public int we;

    }
    [Serializable]
    public class WorldListData
    {
        public List<WorldData> ws;

    }
    [Serializable]
    public class SentenceData
    {
        public List<WorldListData> rt = new ();

        public string bg;

        public string type; // 0-最终结果；1-中间结果

        public string ed;

    }
    [Serializable]
    public class CN
    {
        public SentenceData st;

    }
    [Serializable]
    public class ASRResultData
    {
        public int seg_id;

        public CN cn;

        public bool ls;

    }
}