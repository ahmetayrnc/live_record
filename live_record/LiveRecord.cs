using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LiveRecord : MonoBehaviour
{
    //config
    [Tooltip("Number of images/frames stored in the memory.")] [Range(0, 600)]
    public int imageCount = 600;

    [Tooltip("The downscale ratio of the recorded frames.")] [Range(0, 1f)]
    public float scaleRatio = 0.35f;

    [Tooltip("Quality of the JPEGs stored in the persistent storage.")] [Range(0, 100)]
    public int jpegQuality = 80;

    [Tooltip("Whether the recording should start automatically inside the awake method or should start manually.")]
    public bool autoStart = true;

    [Tooltip("How much the tool will wait before starting the recording.")] [Range(0, 10f)]
    public float startDelay = 1f;

    [Tooltip("Number of captures that will be kept on the persistent storage, the oldest capture will be deleted.")]
    [Range(0, 100)]
    public int captureCountToKeep = 10;

    //cams
    [Tooltip("Cameras you want to be rendered in the footage")]
    public Camera[] cameras;

    //images
    private Queue<byte[]> _images;
    private Queue<byte[]> _tempImages;

    //texture
    private int _width;
    private int _height;
    private RenderTexture _renderTexture;
    private Texture2D _texture;
    private Rect _rect;

    //control variables
    private bool _recording;
    private int _progress;

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
        _images = new Queue<byte[]>(imageCount);
        _tempImages = new Queue<byte[]>(imageCount);

        _width = (int) (Screen.width * scaleRatio);
        _height = (int) (Screen.height * scaleRatio);

        _renderTexture = new RenderTexture(_width, _height, 24);

        _texture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);

        _rect = new Rect(0, 0, _width, _height);

        if (autoStart)
        {
            StartRecording();
        }
    }

    public void StartRecording()
    {
        StartCoroutine(StartRecordingCo());
    }

    public void StopRecording()
    {
        _recording = false;
    }

    public void SaveCapture(Button button, TextMeshProUGUI text)
    {
        StartCoroutine(SaveCaptureCo(button, text));
    }

    private IEnumerator StartRecordingCo()
    {
        yield return new WaitForSeconds(startDelay);
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
        foreach (var cam in cameras)
        {
            cam.targetTexture = _renderTexture;
            cam.Render();
            cam.targetTexture = null;
        }

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
        if (_images.Count >= imageCount)
        {
            _images.Dequeue();
        }

        _images.Enqueue(_texture.EncodeToJPG(jpegQuality));

        _recordState = RecordState.Break;
    }

    private IEnumerator SaveCaptureCo(Button button, TextMeshProUGUI text)
    {
        button.enabled = false;
        var initialButtonText = text.text;
        _tempImages = new Queue<byte[]>(_images);

        var capturePath = $"{Application.persistentDataPath}/gameplay_capture/{DateTime.Now:yyyy-MM-dd_HH-mm-ss}/";

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
            var frameName = frameCount.ToString().PadLeft(4, '0');
            var fileName = "/f_" + frameName + ".jpeg";
            var fullFilePath = capturePath + fileName;
            File.WriteAllBytes(fullFilePath, bytes);
            frameCount++;
            _progress++;
            text.text = (_progress * 100 / Mathf.Max(totalFrameCount, 1)) + "%";

            if (frameCount % 3 == 0)
            {
                yield return null;
            }
        }

        var capturesDir = $"{Application.persistentDataPath}/gameplay_capture/";
        var captureDirs = Directory.GetDirectories(capturesDir);
        if (captureDirs.Length > captureCountToKeep)
        {
            Array.Sort(captureDirs, StringComparer.Ordinal);
            var willBeDeletedCapture = captureDirs[0];
            if (!Directory.Exists(willBeDeletedCapture))
            {
                Directory.CreateDirectory(willBeDeletedCapture);
            }
            else
            {
                var deletedCaptureFolder = Directory.GetFiles(willBeDeletedCapture);

                foreach (var file in deletedCaptureFolder)
                {
                    File.Delete(file);
                }

                Directory.Delete(willBeDeletedCapture);
            }
        }

        _progress = 0;
        button.enabled = true;
        text.text = initialButtonText;
    }
}