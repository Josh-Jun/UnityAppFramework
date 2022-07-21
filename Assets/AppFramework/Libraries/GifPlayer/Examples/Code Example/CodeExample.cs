using UnityEngine;
using UnityEngine.UI;

public class CodeExample : MonoBehaviour {
    private GifPlayer _gifPlayer;

    public Button PlayButton;
    public Button PauseButton;

    public void Awake() {
        // Get the GIF player component
        _gifPlayer = GetComponent<GifPlayer>();

        // Set the file to use. File has to be in StreamingAssets folder or a remote url (For example: http://www.example.com/example.gif).
        _gifPlayer.FileName = "AnimatedGIFPlayerExampe 3.gif";

        // Disable autoplay
        _gifPlayer.AutoPlay = false;

        // Add ready event to start play when GIF is ready to play
        _gifPlayer.OnReady += OnGifLoaded;
        
        // Add ready event for when loading has failed
        _gifPlayer.OnLoadError += OnGifLoadError;

        // Init the GIF player
        _gifPlayer.Init();
    
    }

    private void OnGifLoaded() {
        PlayButton.interactable = true;

        Debug.Log("GIF size: width: " + _gifPlayer.Width + "px, height: " + _gifPlayer.Height + " px");
    }

    private void OnGifLoadError() {
        Debug.Log("Error Loading GIF");
    }

    public void Play() {
        // Start playing the GIF
        _gifPlayer.Play();

        // Disable the play button
        PlayButton.interactable = false;

        // Enable the pause button
        PauseButton.interactable = true;
    }

    public void Pause() {
        // Stop playing the GIF
        _gifPlayer.Pause();

        // Enable the play button
        PlayButton.interactable = true;

        // Disable the pause button
        PauseButton.interactable = false;
    }

    public void OnDisable() {
        _gifPlayer.OnReady -= Play;
    }
}
