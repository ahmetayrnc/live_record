using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SaveRecordingButton : MonoBehaviour
{
    [Tooltip("Progress Text that will show the save progress")]
    public TextMeshProUGUI progressText;

    [Tooltip("Live record object in the scene that captures the frames.")]
    public LiveRecord liveRecord;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        liveRecord.SaveCapture(_button, progressText);
    }
}