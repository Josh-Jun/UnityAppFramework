using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.Ask
{
    public class AskData
    {
        public string content;
        public string confirm;
        public string cancel;
        public Action confirm_callback;
        public Action cancel_callback;
    }
}