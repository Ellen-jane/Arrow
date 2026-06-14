using UnityEngine;
using System;
using System.IO;

[Serializable]
public class GameSaveData
{
    public int currentLevel = 1;
    public int highScore = 0;
    public int totalArrowsCleared = 0;
    public int totalGamesPlayed = 0;

    public int eraserCount = 3;
    public int magicWandCount = 2;
    public int timeFreezeCount = 1;

    public bool soundEnabled = true;
    public bool musicEnabled = true;
    public bool vibrationEnabled = true;
    public float moveSpeed = 5f;
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private string savePath;
    private GameSaveData currentSaveData;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "game_save.json");
        LoadGame();
    }

    public void LoadGame()
    {
        if (File.Exists(savePath))
        {
            try
            {
                string json = File.ReadAllText(savePath);
                currentSaveData = JsonUtility.FromJson<GameSaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load save data: {e.Message}");
                currentSaveData = new GameSaveData();
            }
        }
        else
        {
            currentSaveData = new GameSaveData();
        }

        ApplySettings();
    }

    public void SaveGame()
    {
        UpdateSaveDataFromManagers();

        try
        {
            string json = JsonUtility.ToJson(currentSaveData, true);
            File.WriteAllText(savePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save game data: {e.Message}");
        }
    }

    private void UpdateSaveDataFromManagers()
    {
        if (ItemManager.Instance != null)
        {
            currentSaveData.eraserCount = ItemManager.Instance.eraserCount;
            currentSaveData.magicWandCount = ItemManager.Instance.magicWandCount;
            currentSaveData.timeFreezeCount = ItemManager.Instance.timeFreezeCount;
        }

        currentSaveData.soundEnabled = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        currentSaveData.musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        currentSaveData.vibrationEnabled = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1;
        currentSaveData.moveSpeed = PlayerPrefs.GetFloat("MoveSpeed", 5f);
    }

    private void ApplySettings()
    {
        PlayerPrefs.SetInt("SoundEnabled", currentSaveData.soundEnabled ? 1 : 0);
        PlayerPrefs.SetInt("MusicEnabled", currentSaveData.musicEnabled ? 1 : 0);
        PlayerPrefs.SetInt("VibrationEnabled", currentSaveData.vibrationEnabled ? 1 : 0);
        PlayerPrefs.SetFloat("MoveSpeed", currentSaveData.moveSpeed);
        PlayerPrefs.Save();
    }

    public void UpdateHighScore(int score)
    {
        if (score > currentSaveData.highScore)
        {
            currentSaveData.highScore = score;
            SaveGame();
        }
    }

    public void IncrementArrowsCleared(int count)
    {
        currentSaveData.totalArrowsCleared += count;
    }

    public void IncrementGamesPlayed()
    {
        currentSaveData.totalGamesPlayed++;
    }

    public void SetCurrentLevel(int level)
    {
        currentSaveData.currentLevel = level;
        SaveGame();
    }

    public GameSaveData GetSaveData()
    {
        return currentSaveData;
    }

    public void ResetSaveData()
    {
        currentSaveData = new GameSaveData();
        SaveGame();
        ApplySettings();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
}
