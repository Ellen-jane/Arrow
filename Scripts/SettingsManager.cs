using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public bool soundEnabled = true;
    public bool musicEnabled = true;
    public bool vibrationEnabled = true;
    public float dragSpeed = 1f;

    private void Awake()
    {
        soundEnabled = PlayerPrefs.GetInt("SoundEnabled", soundEnabled ? 1 : 0) == 1;
        musicEnabled = PlayerPrefs.GetInt("MusicEnabled", musicEnabled ? 1 : 0) == 1;
        vibrationEnabled = PlayerPrefs.GetInt("VibrationEnabled", vibrationEnabled ? 1 : 0) == 1;
        dragSpeed = PlayerPrefs.GetFloat("MoveSpeed", dragSpeed);
    }
}
