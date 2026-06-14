using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    public Toggle soundToggle;
    public Toggle musicToggle;
    public Toggle vibrationToggle;
    public Slider speedSlider;
    public Text speedText;
    public Button closeButton;

    private void Start()
    {
        LoadSettings();

        soundToggle.onValueChanged.AddListener(OnSoundChanged);
        musicToggle.onValueChanged.AddListener(OnMusicChanged);
        vibrationToggle.onValueChanged.AddListener(OnVibrationChanged);
        speedSlider.onValueChanged.AddListener(OnSpeedChanged);
        closeButton.onClick.AddListener(ClosePanel);
    }

    private void LoadSettings()
    {
        soundToggle.isOn = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        musicToggle.isOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        vibrationToggle.isOn = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
        speedSlider.value = PlayerPrefs.GetFloat("MoveSpeed", 5f);
        UpdateSpeedText(speedSlider.value);
    }

    private void OnSoundChanged(bool enabled)
    {
        PlayerPrefs.SetInt("SoundEnabled", enabled ? 1 : 0);
        AudioManager.Instance.SetSoundEnabled(enabled);
    }

    private void OnMusicChanged(bool enabled)
    {
        PlayerPrefs.SetInt("MusicEnabled", enabled ? 1 : 0);
        AudioManager.Instance.SetMusicEnabled(enabled);
    }

    private void OnVibrationChanged(bool enabled)
    {
        PlayerPrefs.SetInt("VibrationEnabled", enabled ? 1 : 0);
    }

    private void OnSpeedChanged(float value)
    {
        PlayerPrefs.SetFloat("MoveSpeed", value);
        GameManager.Instance.moveSpeed = value;
        UpdateSpeedText(value);
    }

    private void UpdateSpeedText(float value)
    {
        speedText.text = $"{value:F1}x";
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }

    public bool IsSoundEnabled()
    {
        return PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
    }

    public bool IsMusicEnabled()
    {
        return PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
    }

    public bool IsVibrationEnabled()
    {
        return PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
    }

    public float GetMoveSpeed()
    {
        return PlayerPrefs.GetFloat("MoveSpeed", 5f);
    }
}