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

        if (soundToggle != null)
            soundToggle.onValueChanged.AddListener(OnSoundChanged);
        if (musicToggle != null)
            musicToggle.onValueChanged.AddListener(OnMusicChanged);
        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.AddListener(OnVibrationChanged);
        if (speedSlider != null)
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    private void LoadSettings()
    {
        if (soundToggle != null)
            soundToggle.isOn = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        if (musicToggle != null)
            musicToggle.isOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        if (vibrationToggle != null)
            vibrationToggle.isOn = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
        if (speedSlider != null)
            speedSlider.value = PlayerPrefs.GetFloat("MoveSpeed", 5f);

        UpdateSpeedText(speedSlider != null ? speedSlider.value : PlayerPrefs.GetFloat("MoveSpeed", 5f));
    }

    private void OnSoundChanged(bool enabled)
    {
        PlayerPrefs.SetInt("SoundEnabled", enabled ? 1 : 0);
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSoundEnabled(enabled);
    }

    private void OnMusicChanged(bool enabled)
    {
        PlayerPrefs.SetInt("MusicEnabled", enabled ? 1 : 0);
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicEnabled(enabled);
    }

    private void OnVibrationChanged(bool enabled)
    {
        PlayerPrefs.SetInt("VibrationEnabled", enabled ? 1 : 0);
    }

    private void OnSpeedChanged(float value)
    {
        PlayerPrefs.SetFloat("MoveSpeed", value);
        if (GameManager.Instance != null)
            GameManager.Instance.moveSpeed = value;
        UpdateSpeedText(value);
    }

    private void UpdateSpeedText(float value)
    {
        if (speedText != null)
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
