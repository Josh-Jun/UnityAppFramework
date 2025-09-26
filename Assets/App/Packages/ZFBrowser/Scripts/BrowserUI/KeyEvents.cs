#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
#define ZF_OSX
#endif
using System.Collections.Generic;
using UnityEngine;

namespace ZenFulcrum.EmbeddedBrowser {
/// <summary>
/// Helper class for IBrowserUI implementations for getting/generating keyboard events for sending to an IBrowserUI.
/// </summary>
public class KeyEvents {


	/** Fills up with key events as they happen. */
	protected List<Event> keyEvents = new List<Event>();

	/** Swaps with keyEvents on InputUpdate and is returned in the main getter. */
	protected List<Event> keyEventsLast = new List<Event>();

	/// <summary>
	/// After calling InputUpdate, contains the key events to send to the browser. 
	/// </summary>
	public List<Event> Events {
		get { return keyEventsLast; }
	}

	/** List of keys Unity won't give us events for. So we have to poll. */
	static readonly KeyCode[] keysToCheck = {
#if ZF_OSX
		//On windows you get GUI events for ctrl, super, alt. On mac...you don't!
		KeyCode.LeftCommand,
		KeyCode.RightCommand,
		KeyCode.LeftControl,
		KeyCode.RightControl,
		KeyCode.LeftAlt,
		KeyCode.RightAlt,
		//KeyCode.CapsLock, unity refuses to inform us of this, so there's not much we can do
#endif
		//Unity consistently doesn't send events for shift across all platforms.
		KeyCode.LeftShift,
		KeyCode.RightShift,
	};

	/// <summary>
	/// Call once per frame before grabbing 
	/// </summary>
	public void InputUpdate() {
		//Note: keyEvents gets filled in OnGUI as things happen. InputUpdate get called just before it's read.
		//To get the right events to the right place at the right time, swap the "double buffer" of key events.
		var tmp = keyEvents;
		keyEvents = keyEventsLast;
		keyEventsLast = tmp;
		keyEvents.Clear();

		//Unity doesn't include events for some keys, so fake it by checking each frame.
		for (int i = 0; i < keysToCheck.Length; i++) {
			if (Input.GetKeyDown(keysToCheck[i])) {
				//Prepend down, postpend up. We don't know which happened first, but pressing
				//modifiers usually precedes other key presses and releasing tends to follow.
				keyEventsLast.Insert(0, new Event() { type = EventType.KeyDown, keyCode = keysToCheck[i] });
			} else if (Input.GetKeyUp(keysToCheck[i])) {
				keyEventsLast.Add(new Event() { type = EventType.KeyUp, keyCode = keysToCheck[i] });
			}
		}
	}

	/// <summary>
	/// Call this with any GUI events you get from Unity that you want to have passed to the browser.
	/// </summary>
	/// <param name="ev"></param>
	public void Feed(Event ev) {
		if (ev.type != EventType.KeyDown && ev.type != EventType.KeyUp) return;

		//		if (ev.character != 0) Debug.Log("ev >>> " + ev.character);
		//		else if (ev.type == EventType.KeyUp) Debug.Log("ev ^^^ " + ev.keyCode);
		//		else if (ev.type == EventType.KeyDown) Debug.Log("ev vvv " + ev.keyCode);

		keyEvents.Add(new Event(ev));
	}

	/// <summary>
	/// Injects a key press. Call Release laster to let go.
	/// If the key you are pressing represents a character this may not type that character. Use Type() instead.
	/// </summary>
	/// <param name="key"></param>
	public void Press(KeyCode key) {
		keyEvents.Add(new Event {
			type = EventType.KeyDown, keyCode = key
		});
	}

	public void Release(KeyCode key) {
		keyEvents.Add(new Event {
			type = EventType.KeyUp, keyCode = key
		});
	}

	/// <summary>
	/// Types the given text in. THis does not simulate pressing each key and releasing it.
	/// </summary>
	/// <param name="text"></param>
	public void Type(string text) {
		for (int i = 0; i < text.Length; i++) {
			//fixme: multibyte chars >16 bits
			keyEvents.Add(new Event {
				type = EventType.KeyDown, character = text[i],
			});
		}
	}
}

}
