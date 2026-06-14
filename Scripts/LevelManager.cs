using UnityEngine;
using System;

[Serializable]
public class LevelConfig
{
    public int level = 3;
    public int health = 3;
    public float timeLimitSeconds = 480f;
    public float arrowMoveSpeed = 10f;
    public float gridSize = 0.5f;
    public Rect playArea = new Rect(-6.5f, -5f, 13f, 10f);

    public int arrowCount = 72;
    public int minArrowLength = 2;
    public int maxArrowLength = 16;

    public int eraserCount = 3;
    public int magicWandCount = 2;
    public int timeFreezeCount = 1;
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public int defaultLevel = 3;
    public bool autoStartOnStart = true;
    public LevelConfig currentConfig = new LevelConfig();

    private bool hasStartedLevel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (autoStartOnStart && !hasStartedLevel)
        {
            StartLevel(defaultLevel);
        }
    }

    public void StartLevel(int level)
    {
        EnsureRuntimeManagers();

        currentConfig = CreateConfig(level);

        GameManager.Instance.ConfigureLevel(currentConfig);

        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.ResetForLevel(currentConfig);
        }

        if (ArrowGenerator.Instance != null)
        {
            ArrowGenerator.Instance.Configure(currentConfig);
        }

        GameManager.Instance.InitGame();

        if (ArrowGenerator.Instance != null)
        {
            ArrowGenerator.Instance.GenerateArrows();
        }

        hasStartedLevel = true;
    }

    public void RestartLevel()
    {
        StartLevel(currentConfig != null ? currentConfig.level : defaultLevel);
    }

    public LevelConfig CreateConfig(int level)
    {
        int safeLevel = Mathf.Max(1, level);
        LevelConfig config = new LevelConfig();
        config.level = safeLevel;
        config.health = 3;
        config.timeLimitSeconds = 480f;
        config.arrowMoveSpeed = 10f;
        config.gridSize = 0.5f;
        config.playArea = new Rect(-6.5f, -5f, 13f, 10f);

        config.arrowCount = Mathf.Clamp(36 + safeLevel * 12, 42, 96);
        config.minArrowLength = 2;
        config.maxArrowLength = Mathf.Clamp(10 + safeLevel * 2, 12, 20);

        config.eraserCount = 3;
        config.magicWandCount = 2;
        config.timeFreezeCount = 1;

        return config;
    }

    private void EnsureRuntimeManagers()
    {
        if (GameManager.Instance == null)
        {
            new GameObject("GameManager").AddComponent<GameManager>();
        }

        if (ItemManager.Instance == null)
        {
            new GameObject("ItemManager").AddComponent<ItemManager>();
        }

        if (ArrowGenerator.Instance == null)
        {
            new GameObject("ArrowGenerator").AddComponent<ArrowGenerator>();
        }
    }
}
