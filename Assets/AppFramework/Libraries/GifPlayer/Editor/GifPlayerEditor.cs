using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
[CustomEditor(typeof(GifPlayer))]
public class GifPlayerEditor : Editor
{
    private Rect _rt;
    private GifPlayer _component;
    private static bool _advancedOptionsFoldout;

    public void OnEnable()
    {
        _component = (GifPlayer)target;
        if (!Application.isPlaying) _component.Init();
    }

    public override void OnInspectorGUI()
    {
        var inspectorWidth =
            EditorGUIUtility.currentViewWidth -
            50; // EditorGUIUtility.currentViewWidth doesn't take scroll bar into account.
        //target graphic
        var content = new GUIContent("Target Component", "Drag the target component for the GIF here");

        EditorGUI.BeginChangeCheck();

        if (_component.TargetComponent != null && _component.TargetComponent is Renderer)
        {
            var r = (Renderer)_component.TargetComponent;
            if (r.sharedMaterials.Length > 1)
            {
                var options = new string[r.sharedMaterials.Length];
                for (var i = 0; i < r.sharedMaterials.Length; i++)
                {
                    options[i] = i + " " + r.sharedMaterials[i].name;
                }

                _component.TargetMaterialNumber = EditorGUILayout.Popup("Target Material",
                    _component.TargetMaterialNumber, options);
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            _component.Init();
        }

        if (!(_component.TargetComponent is MeshRenderer) && !(_component.TargetComponent is RawImage) &&
            !(_component.TargetComponent is SpriteRenderer) && !(_component.TargetComponent is Image))
        {
            EditorGUILayout.HelpBox("Set a Mesh Renderer, Sprite Renderer or Rawimage as the target of the GIF",
                MessageType.Warning);
        }

        if (_component.TargetComponent is MeshRenderer)
        {
            var targetg = (MeshRenderer)_component.TargetComponent;
            if (targetg.sharedMaterial == null)
            {
                EditorGUILayout.HelpBox("No material set on Renderer", MessageType.Warning);
            }
        }

        if (_component.TargetComponent is SpriteRenderer)
        {
            var targetg = (SpriteRenderer)_component.TargetComponent;
            if (targetg.sharedMaterial == null)
            {
                EditorGUILayout.HelpBox("No material set on Sprite Renderer", MessageType.Warning);
            }
        }


        //empty line
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        //loop
        content = new GUIContent("Loop", "Restart playback after all frames are shown");
        _component.Loop = EditorGUILayout.Toggle(content, _component.Loop);

        //autoplay
        content = new GUIContent("Autoplay", "Start playback automatically after gif has been loaded");
        _component.AutoPlay = EditorGUILayout.Toggle(content, _component.AutoPlay);

        //empty line
        EditorGUILayout.Space();

        //load gif buttons
        GUILayout.BeginHorizontal(GUILayout.Width(inspectorWidth));
        EditorGUILayout.LabelField("File Name", _component.FileName);

        if (GUILayout.Button("Select GIF"))
        {
            var path = EditorUtility.OpenFilePanel(
                "Select GIF file to display",
                "Assets/StreamingAssets",
                "gif");

            var trimmedPath = Regex.Replace(path, @"^.*?StreamingAssets\s*(.*?)", "$1").TrimStart('/', '\'');
            _component.FileName = trimmedPath;
            // trimmedPath.Substring(0, trimmedPath.LastIndexOf(".", StringComparison.Ordinal));

            _component.Init();
            GUIUtility.ExitGUI();
        }

        GUILayout.EndHorizontal();


        //preview image
        if (_component.GifTexture != null && _component.FileName.Length != 0)
        {
            _rt = GUILayoutUtility.GetRect(new GUIContent("Gif Image", _component.GifTexture, "Gif Image"),
                new GUIStyle
                {
                    fixedHeight = inspectorWidth / _component.GifTexture.width * _component.GifTexture.height,
                    fixedWidth = inspectorWidth
                });
            GUI.DrawTexture(_rt, _component.GifTexture);

            //play / pause controls
            GUILayout.BeginHorizontal(GUILayout.Width(inspectorWidth));


            //play button
            EditorGUI.BeginDisabledGroup(_component.State == GifPlayerState.Playing ||
                                         _component.State == GifPlayerState.PreProcessing);
            if (GUILayout.Button(EditorGUIUtility.FindTexture("PlayButton")))
            {
                _component.Play();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(_component.State != GifPlayerState.Playing);
            if (GUILayout.Button(EditorGUIUtility.FindTexture("PauseButton")))
            {
                _component.Pause();
            }

            EditorGUI.EndDisabledGroup();


            GUILayout.EndHorizontal();

            //gif details
            EditorGUILayout.LabelField(" Size: " + _component.GifTexture.width + "x" + _component.GifTexture.height +
                                       " px" +
                                       (Application.isPlaying ? "" : " Frames: " + _component.GetNumberOfFrames()));
        }

        EditorGUILayout.Space();

        //advanced options
        EditorGUILayout.BeginVertical();

        _advancedOptionsFoldout = GUILayout.Toggle(_advancedOptionsFoldout, "Advanced Options", "Foldout",
            GUILayout.ExpandWidth(false));

        if (_advancedOptionsFoldout)
        {
            //threaded decoder
            content = new GUIContent("Threaded Decoder", "Decoder runs in a seperate thread.");
            _component.UseThreadedDecoder = EditorGUILayout.Toggle(content, _component.UseThreadedDecoder);

            //cache frames
            content = new GUIContent("Cache frames", "Cache frames that are decoded. Uses less CPU but more memory.");
            _component.CacheFrames = EditorGUILayout.Toggle(content, _component.CacheFrames);

            //buffer all frames
            EditorGUI.BeginDisabledGroup(!_component.CacheFrames);
            content = new GUIContent("Buffer all Frames", "Load all frames at load instead of each frame seperate.");
            _component.BufferAllFrames = EditorGUILayout.Toggle(content, _component.BufferAllFrames);
            EditorGUI.EndDisabledGroup();

            //playback speed
            content = new GUIContent("Override Time.timeScale",
                "If disabled playback speed scales with Time.timeScale.");
            _component.OverrideTimeScale = EditorGUILayout.Toggle(content, _component.OverrideTimeScale);

            //buffer all frames
            EditorGUI.BeginDisabledGroup(!_component.OverrideTimeScale);
            content = new GUIContent("Playback Speed",
                "Playback speed of the Gif. (1 = normal speed as set by the Gif)");
            _component.TimeScale = EditorGUILayout.FloatField(content, _component.TimeScale);
            EditorGUI.EndDisabledGroup();
        }

        EditorGUILayout.EndVertical();

        //keep updating
        if (!Application.isPlaying) _component.Update();
        if (_component.State == GifPlayerState.Playing)
        {
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }


        if (GUI.changed && !Application.isPlaying)
        {
            EditorUtility.SetDirty(_component);
            EditorSceneManager.MarkSceneDirty(_component.gameObject.scene);
        }
    }
}