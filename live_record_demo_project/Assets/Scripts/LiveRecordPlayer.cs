using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LiveRecordPlayer : MonoBehaviour
{
    //ui
    public RawImage display;
    public TextMeshProUGUI playButtonText;
    public Slider playSlider;
    public GameObject playbackObjects;
    public TextMeshProUGUI captureOptionsText;
    public TMP_InputField captureNoInputField;
    public TextMeshProUGUI folderName;

    private const string PlayingStateText = "Pause";
    private const string PausedStateText = "Play";
    private const string EndStateText = "Restart";

    //rendering
    private byte[][] _images;
    private Texture2D _texture;

    //control variables
    private int _currentFrame;
    private PlaybackState _playbackState;

    //playback update
    private CustomFixedUpdate _playbackUpdate;

    //capture dirs
    private string[] _captureDirs;

    private enum PlaybackState
    {
        Playing,
        Paused,
        End,
    }

    private void Awake()
    {
        _texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        SetPlaybackState(PlaybackState.Paused);
        _currentFrame = 0;
        playbackObjects.SetActive(false);
        _playbackUpdate = new CustomFixedUpdate(OnPlaybackUpdate, 15);

        var capturesDir = string.Format("{0}/gameplay_capture/", Application.persistentDataPath);

        if (!Directory.Exists(capturesDir))
        {
            captureOptionsText.text = "No captures";
            return;
        }

        var captureDirs = Directory.GetDirectories(capturesDir);

        if (captureDirs.Length == 0)
        {
            captureOptionsText.text = "No captures";
            return;
        }

        _captureDirs = captureDirs;
        captureOptionsText.text = "0-" + (captureDirs.Length - 1);
        captureNoInputField.text = (_captureDirs.Length - 1).ToString();

        LoadCapture(captureDirs.Length - 1);
    }

    public void GetImagesFromDisk()
    {
        if (_captureDirs == null || _captureDirs.Length == 0)
        {
            captureOptionsText.text = "No captures";
            return;
        }

        if (!int.TryParse(captureNoInputField.text, out var captureNo))
        {
            captureNo = _captureDirs.Length - 1;
        }

        captureNo = Mathf.Clamp(captureNo, 0, _captureDirs.Length - 1);
        captureNoInputField.text = captureNo.ToString();

        LoadCapture(captureNo);
    }

    private void LoadCapture(int captureNo)
    {
        Array.Sort(_captureDirs, StringComparer.Ordinal);
        var captureDir = _captureDirs[captureNo];
        folderName.text = Path.GetFileName(captureDir);

        var imageFiles = Directory.GetFiles(captureDir);

        if (imageFiles.Length == 0)
        {
            //no images
            return;
        }

        Array.Sort(imageFiles, StringComparer.Ordinal);

        _images = new byte[imageFiles.Length][];

        for (var i = 0; i < _images.Length; i++)
        {
            _images[i] = File.ReadAllBytes(imageFiles[i]);
        }

        SetupSlider();
        SetFrameInside(0, true);
        playbackObjects.SetActive(true);
    }

    public void OnSliderMoved()
    {
        SetFrameOutside((int) playSlider.value);
    }

    public void TogglePlay()
    {
        switch (_playbackState)
        {
            case PlaybackState.Playing:
                SetPlaybackState(PlaybackState.Paused);
                break;
            case PlaybackState.Paused:
                SetPlaybackState(PlaybackState.Playing);
                break;
            case PlaybackState.End:
                SetFrameInside(0, true);
                SetPlaybackState(PlaybackState.Paused);
                break;
        }
    }

    public void NextFrame()
    {
        var toFrame = Mathf.Clamp(_currentFrame + 1, 0, GetDuration());
        SetFrameInside(toFrame, true);
    }

    public void PrevFrame()
    {
        var toFrame = Mathf.Clamp(_currentFrame - 1, 0, _currentFrame);
        SetFrameInside(toFrame, true);
    }

    public void ClosePlayer()
    {
        gameObject.SetActive(false);
    }

    private int GetDuration()
    {
        return !HasImages() ? 0 : _images.Length;
    }

    private void Update()
    {
        _playbackUpdate.Update();
    }

    private void OnPlaybackUpdate(float dt)
    {
        if (!HasImages()) return;

        if (_playbackState != PlaybackState.Playing) return;

        SetFrameInside(_currentFrame, false);
        _currentFrame = Mathf.Clamp(_currentFrame + 1, 0, GetDuration());
    }

    private void SetupSlider()
    {
        playSlider.value = 0;
        playSlider.minValue = 0;
        playSlider.maxValue = GetDuration();
    }

    private bool HasImages()
    {
        return _images != null;
    }

    private void DisplayCurrentFrame()
    {
        _texture.LoadImage(_images[_currentFrame]);
        display.texture = _texture;
    }

    private void SetPlaybackState(PlaybackState newState)
    {
        _playbackState = newState;
        switch (_playbackState)
        {
            case PlaybackState.Playing:
                playButtonText.text = PlayingStateText;
                break;
            case PlaybackState.Paused:
                playButtonText.text = PausedStateText;
                break;
            case PlaybackState.End:
                playButtonText.text = EndStateText;
                break;
        }
    }

    private void SetFrameInside(int frameNo, bool shouldPause)
    {
        if (frameNo >= GetDuration())
        {
            SetPlaybackState(PlaybackState.End);
            return;
        }

        if (frameNo < 0)
        {
            return;
        }

        _currentFrame = frameNo;
        DisplayCurrentFrame();
        playSlider.value = _currentFrame;

        if (shouldPause)
        {
            SetPlaybackState(PlaybackState.Paused);
        }
    }

    private void SetFrameOutside(int frameNo)
    {
        if (frameNo >= GetDuration())
        {
            SetPlaybackState(PlaybackState.End);
            return;
        }

        if (frameNo < 0)
        {
            return;
        }

        _currentFrame = frameNo;
        DisplayCurrentFrame();

        SetPlaybackState(PlaybackState.Paused);
    }
}

public class CustomFixedUpdate
{
    public delegate void OnFixedUpdateCallback(float aDeltaTime);

    private float m_FixedTimeStep;
    private float m_Timer = 0;

    private OnFixedUpdateCallback m_Callback;

    private float m_MaxAllowedTimeStep = 0f;

    public float MaxAllowedTimeStep
    {
        get { return m_MaxAllowedTimeStep; }
        set { m_MaxAllowedTimeStep = value; }
    }

    private float deltaTime
    {
        get { return m_FixedTimeStep; }
        set { m_FixedTimeStep = Mathf.Max(value, 0.000001f); } // max rate: 1000000
    }

    public float updateRate
    {
        get { return 1.0f / deltaTime; }
        set { deltaTime = 1.0f / value; }
    }

    private CustomFixedUpdate(float aTimeStep, OnFixedUpdateCallback aCallback, float aMaxAllowedTimestep)
    {
        if (aCallback == null)
            throw new ArgumentException("CustomFixedUpdate needs a valid callback");
        if (aTimeStep <= 0f)
            throw new System.ArgumentException("TimeStep needs to be greater than 0");
        deltaTime = aTimeStep;
        m_Callback = aCallback;
        m_MaxAllowedTimeStep = aMaxAllowedTimestep;
    }

    public CustomFixedUpdate(float aTimeStep, OnFixedUpdateCallback aCallback) : this(aTimeStep, aCallback, 0f)
    {
    }

    public CustomFixedUpdate(OnFixedUpdateCallback aCallback) : this(0.01f, aCallback, 0f)
    {
    }

    private CustomFixedUpdate(OnFixedUpdateCallback aCallback, float aFPS, float aMaxAllowedTimestep) : this(1f / aFPS,
        aCallback, aMaxAllowedTimestep)
    {
    }

    public CustomFixedUpdate(OnFixedUpdateCallback aCallback, float aFPS) : this(aCallback, aFPS, 0f)
    {
    }

    private void Update(float aDeltaTime)
    {
        m_Timer -= aDeltaTime;
        if (m_MaxAllowedTimeStep > 0)
        {
            var timeout = Time.realtimeSinceStartup + m_MaxAllowedTimeStep;
            while (m_Timer < 0f && Time.realtimeSinceStartup < timeout)
            {
                m_Callback(m_FixedTimeStep);
                m_Timer += m_FixedTimeStep;
            }
        }
        else
        {
            while (m_Timer < 0f)
            {
                m_Callback(m_FixedTimeStep);
                m_Timer += m_FixedTimeStep;
            }
        }
    }

    public void Update()
    {
        Update(Time.deltaTime);
    }
}