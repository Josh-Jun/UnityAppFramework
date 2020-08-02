
using System;

namespace EventController {
    public class EventListenerData {
        public object EventListener { get; set; }
        public string EventName { get; set; }
        public Delegate EventDelegate { get; set; }
        public EventDispatcherMode EventListeningMode { get; set; }

        public EventListenerData(object aEventListener,string aEventName_string,Delegate aEventDelegate,EventDispatcherMode aEventListeningMode) {
            EventListener = aEventListener;
            EventName = aEventName_string;
            EventDelegate = aEventDelegate;
            EventListeningMode = aEventListeningMode;
        }
    }
}