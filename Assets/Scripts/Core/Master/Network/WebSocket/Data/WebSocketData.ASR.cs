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
    #region 阿里语音识别

    #region Token请求结果

    [Serializable]
    public class TokenResponse
    {
        public TokenInfo Token;
        public string NlsRequestId;
    }

    [Serializable]
    public class TokenInfo
    {
        public string Id; // Token值
        public long ExpireTime; // 过期时间戳（秒）
    }

    #endregion

    [Serializable]
    public class TTSResponseData
    {
        public int error_code;
        public int status;
        public string error_message;
        public string request_id;
        public string url;
        public TTSTaskData data;
    }

    [Serializable]
    public class TTSTaskData
    {
        public string task_id;
        public string audio_address;
    }

    [Serializable]
    public class TTSData
    {
        public TTSPayloadData payload;
        public TTSHeaderData header;
        public TTSContentData context;
    }

    [Serializable]
    public class TTSPayloadData
    {
        public TTSRequestData tts_request;
        public bool enable_notify;
    }

    [Serializable]
    public class TTSContentData
    {
        public string device_id;
    }

    [Serializable]
    public class TTSHeaderData
    {
        public string appkey;
        public string token;
    }

    [Serializable]
    public class TTSRequestData
    {
        public int sample_rate;
        public string text;
        public string voice;
        public string format;
        public bool enable_subtitle;
    }

    [Serializable]
    public class TranscriptionData
    {
        public Header header;
    }

    [Serializable]
    public class TranscriptionData<T>
    {
        public Header header;
        public T payload;
    }

    [Serializable]
    public class Header
    {
        public string appkey;
        public string message_id;
        public string task_id;
        public string @namespace = "SpeechTranscriber";
        public string name;
    }

    [Serializable]
    public class StartTranscriptionData
    {
        public string format;
        public int sample_rate;
        public bool enable_intermediate_result;
        public bool enable_punctuation_prediction;
        public bool enable_inverse_text_normalization;
        public string customization_id;
        public string vocabulary_id;
        public int max_sentence_silence;
        public bool enable_words;
        public bool disfluency;
        public float speech_noise_threshold;
        public bool enable_semantic_sentence_detection;
    }

    [Serializable]
    public class TranscriptionResult
    {
        public int index;
        public int time;
        public Word[] words;
        public string result;
        public int status;
    }

    [Serializable]
    public class Word
    {
        public int startTime;
        public int endTime;
        public string text;
    }

    [Serializable]
    public class SentenceBeginData
    {
        public int index;
        public int time;
    }

    [Serializable]
    public class SentenceEndData
    {
        public int index;
        public int time;
        public string result;
        public int status;
        public int begin_time;
        public double confidence;
        public Word[] words;
        public StashResult stash_result;
        public string gender;
        public string fixed_result;
        public string unfixed_result;
        public string audio_extra_info;
        public int gender_score;
        public string sentence_id;
    }

    [Serializable]
    public class StashResult
    {
        public string fixedText;
        public string unfixedText;
        public string text;
        public int sentenceId;
        public int beginTime;
        public int currentTime;

        public Word[] words;
    }

    #endregion

    #region 讯飞语音识别

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
        public List<World> cw = new();

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
        public List<WorldListData> rt = new();

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
    
    #endregion
}