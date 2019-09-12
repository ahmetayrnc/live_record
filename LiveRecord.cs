#if PEAK_DEV
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using PeakGames.Amy.CoreUI.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LiveRecord : MonoBehaviour
{
    //config
    private const int ImageCount = 600;
    private const float ScaleRatio = 0.35f;
    private const int JpegQuality = 80;

    //images
    private Queue<byte[]> _images;
    private Queue<byte[]> _tempImages;

    //cams
    private Camera _cam;
    private Camera _uiCam;

    //texture
    private int _width;
    private int _height;
    private RenderTexture _renderTexture;
    private Texture2D _texture;
    private Rect _rect;

    //control variables
    private bool _recording;
    private int _progress;

    //level
    private LevelManager _levelManager;

    //record state
    private RecordState _recordState;

    private enum RecordState
    {
        CamToRenderTexture,
        TextureToTexture,
        TextureToBytes,
        Break
    }

    private void Awake()
    {
        _levelManager = GameObject.Find("LevelManagerGO").GetComponent<LevelManager>();
        _cam = GameObject.Find("GameCamera").GetComponent<Camera>();
        _uiCam = GameObject.Find("UiCamera").GetComponent<Camera>();
        _images = new Queue<byte[]>(ImageCount);
        _tempImages = new Queue<byte[]>(ImageCount);

        _width = (int) (Screen.width * ScaleRatio);
        _height = (int) (Screen.height * ScaleRatio);

        _renderTexture = new RenderTexture(_width, _height, 24);

        _texture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);

        _rect = new Rect(0, 0, _width, _height);

        StartCoroutine(StartRecording());
    }

    private IEnumerator StartRecording()
    {
        yield return new WaitForSeconds(1f);
        _recording = true;
    }

    private void LateUpdate()
    {
        if (!_recording) return;

        switch (_recordState)
        {
            case RecordState.CamToRenderTexture:
                CamToRenderTexture();
                break;
            case RecordState.TextureToTexture:
                RenderTextureToTexture();
                break;
            case RecordState.TextureToBytes:
                TextureToBytes();
                break;
            case RecordState.Break:
                _recordState = RecordState.CamToRenderTexture;
                break;
        }
    }

    private void CamToRenderTexture()
    {
        _cam.targetTexture = _renderTexture;
        _cam.Render();
        _cam.targetTexture = null;

        _uiCam.targetTexture = _renderTexture;
        _uiCam.Render();
        _uiCam.targetTexture = null;

        _recordState = RecordState.TextureToTexture;
    }

    private void RenderTextureToTexture()
    {
        RenderTexture.active = _renderTexture;
        _texture.ReadPixels(_rect, 0, 0);
        RenderTexture.active = null;

        _recordState = RecordState.TextureToBytes;
    }

    private void TextureToBytes()
    {
        if (_images.Count >= ImageCount)
        {
            _images.Dequeue();
        }

        _images.Enqueue(_texture.EncodeToJPG(JpegQuality));

        _recordState = RecordState.Break;
    }

    public void RecordLast30S(Button button, TextMeshProUGUI text)
    {
        StartCoroutine(RecordMovie(button, text));
    }

    private IEnumerator RecordMovie(Button button, TextMeshProUGUI text)
    {
        button.enabled = false;
        var initialButtonText = text.text;
        _tempImages = new Queue<byte[]>(_images);

        var capturePath = string.Format("{0}/gameplay_capture/{1:yyyy-MM-dd_HH-mm-ss}_level_{2}/",
            Application.persistentDataPath,
            DateTime.Now, _levelManager.CurrentLevel.LevelNo);

        if (!Directory.Exists(capturePath))
        {
            Directory.CreateDirectory(capturePath);
        }

        _progress = 0;

        var frameCount = 0;
        var totalFrameCount = _tempImages.Count;
        while (_tempImages.Count > 0)
        {
            var bytes = _tempImages.Dequeue();
            var framename = frameCount.ToString().PadLeft(4, '0');
            var filename = "/f_" + framename + ".jpeg";
            var fullFilePath = capturePath + filename;
            File.WriteAllBytes(fullFilePath, bytes);
            frameCount++;
            _progress++;
            text.text = (_progress * 100 / Mathf.Max(totalFrameCount, 1)) + "%";

            if (frameCount % 3 == 0)
            {
                yield return null;
            }
        }

        var capturesDir = string.Format("{0}/gameplay_capture/", Application.persistentDataPath);
        var captureDirs = Directory.GetDirectories(capturesDir);
        if (captureDirs.Length > 10)
        {
            Array.Sort(captureDirs, StringComparer.Ordinal);
            var willBeDeletedCapture = captureDirs[0];
            if (!Directory.Exists(willBeDeletedCapture))
            {
                Directory.CreateDirectory(willBeDeletedCapture);
            }
            else
            {
                var hi = Directory.GetFiles(willBeDeletedCapture);

                for (var i = 0; i < hi.Length; i++)
                {
                    File.Delete(hi[i]);
                }

                Directory.Delete(willBeDeletedCapture);
            }
        }

        _progress = 0;
        button.enabled = true;
        text.text = initialButtonText;
    }
}
#endif