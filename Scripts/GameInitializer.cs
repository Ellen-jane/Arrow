using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    public int initialLevel = 3;
    public bool autoStart = true;

    private void Awake()
    {
        EnsureGameManager();
        EnsureItemManager();
        EnsureAudioManager();
        EnsureSaveManager();
        EnsureArrowGenerator();
        EnsureCanvasAndUIManager();
        EnsureLevelManager();
    }

    private void Start()
    {
        if (autoStart && LevelManager.Instance != null)
        {
            LevelManager.Instance.autoStartOnStart = false;
            LevelManager.Instance.StartLevel(initialLevel);
        }
    }

    private void EnsureGameManager()
    {
        if (GameManager.Instance == null)
        {
            InstantiateOrCreate<GameManager>("Prefabs/GameBootstrapper/GameManager", "GameManager");
        }
    }

    private void EnsureItemManager()
    {
        if (ItemManager.Instance == null)
        {
            InstantiateOrCreate<ItemManager>("Prefabs/GameBootstrapper/ItemManager", "ItemManager");
        }
    }

    private void EnsureAudioManager()
    {
        if (AudioManager.Instance == null)
        {
            InstantiateOrCreate<AudioManager>("Prefabs/GameBootstrapper/AudioManager", "AudioManager");
        }
    }

    private void EnsureSaveManager()
    {
        if (SaveManager.Instance == null)
        {
            InstantiateOrCreate<SaveManager>("Prefabs/GameBootstrapper/SaveManager", "SaveManager");
        }
    }

    private void EnsureLevelManager()
    {
        if (LevelManager.Instance == null)
        {
            LevelManager levelManager = InstantiateOrCreate<LevelManager>("Prefabs/GameBootstrapper/LevelManager", "LevelManager");
            levelManager.autoStartOnStart = false;
            levelManager.defaultLevel = initialLevel;
        }
        else
        {
            LevelManager.Instance.autoStartOnStart = false;
        }
    }

    private void EnsureArrowGenerator()
    {
        if (ArrowGenerator.Instance == null)
        {
            new GameObject("ArrowGenerator").AddComponent<ArrowGenerator>();
        }
    }

    private void EnsureCanvasAndUIManager()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        GameObject rootObject = canvas != null ? canvas.gameObject : null;

        if (rootObject == null)
        {
            GameObject rootPrefab = Resources.Load<GameObject>("Prefabs/Root");
            rootObject = rootPrefab != null ? Instantiate(rootPrefab) : new GameObject("Root");
            rootObject.name = "Root";
        }

        if (rootObject.GetComponent<Canvas>() == null)
        {
            rootObject.AddComponent<Canvas>();
        }

        if (UIManager.Instance == null)
        {
            rootObject.AddComponent<UIManager>();
        }
    }

    private T InstantiateOrCreate<T>(string resourcePath, string objectName) where T : Component
    {
        T existing = FindObjectOfType<T>();
        if (existing != null)
        {
            return existing;
        }

        GameObject prefab = Resources.Load<GameObject>(resourcePath);
        GameObject instance = prefab != null ? Instantiate(prefab) : new GameObject(objectName);
        instance.name = objectName;

        T component = instance.GetComponent<T>();
        if (component == null)
        {
            component = instance.AddComponent<T>();
        }

        return component;
    }
}
