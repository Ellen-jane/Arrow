using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public Image[] healthHearts;
    public Text timeText;
    public Text levelStatusText;
    public Button settingsButton;
    public GameObject settingsPanel;

    public Button eraserButton;
    public Button magicWandButton;
    public Button timeFreezeButton;

    public Text eraserCountText;
    public Text magicWandCountText;
    public Text timeFreezeCountText;

    public GameObject gameOverPanel;
    public GameObject gameWinPanel;

    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

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
        AutoWire();
        BindEvents();
        HidePanels();
        UpdateLevelDisplay();
        UpdateHealthDisplay();
        UpdateTimeDisplay();
        UpdateItemCounts();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.OnHealthChanged -= UpdateHealthDisplay;
        GameManager.Instance.OnTimeChanged -= UpdateTimeDisplay;
        GameManager.Instance.OnLevelChanged -= UpdateLevelDisplay;
        GameManager.Instance.OnGameOver -= ShowGameOver;
        GameManager.Instance.OnGameWin -= ShowGameWin;
    }

    private void AutoWire()
    {
        if (timeText == null)
        {
            timeText = FindTextByName("Time");
        }

        if (settingsButton == null)
        {
            settingsButton = FindButtonByName("Settings");
        }

        if (settingsPanel == null)
        {
            settingsPanel = FindPanelByName("Settings");
        }

        if (eraserButton == null)
        {
            eraserButton = FindButtonByName("Eraser");
        }

        if (magicWandButton == null)
        {
            magicWandButton = FindButtonByName("MagicWand");
            if (magicWandButton == null)
            {
                magicWandButton = FindButtonByName("MaginWand");
            }
        }

        if (timeFreezeButton == null)
        {
            timeFreezeButton = FindButtonByName("TimeFreezer");
        }

        if (eraserCountText == null && eraserButton != null)
        {
            eraserCountText = FindTextInChildren(eraserButton.transform);
        }

        if (magicWandCountText == null && magicWandButton != null)
        {
            magicWandCountText = FindTextInChildren(magicWandButton.transform);
        }

        if (timeFreezeCountText == null && timeFreezeButton != null)
        {
            timeFreezeCountText = FindTextInChildren(timeFreezeButton.transform);
        }

        if (gameOverPanel == null)
        {
            gameOverPanel = FindPanelByName("GameOver");
        }

        if (gameWinPanel == null)
        {
            gameWinPanel = FindPanelByName("GameWin");
        }

        if (levelStatusText == null)
        {
            levelStatusText = CreateLevelStatusText();
        }
    }

    private void BindEvents()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnHealthChanged += UpdateHealthDisplay;
            GameManager.Instance.OnTimeChanged += UpdateTimeDisplay;
            GameManager.Instance.OnLevelChanged += UpdateLevelDisplay;
            GameManager.Instance.OnGameOver += ShowGameOver;
            GameManager.Instance.OnGameWin += ShowGameWin;
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveListener(ToggleSettings);
            settingsButton.onClick.AddListener(ToggleSettings);
        }

        if (eraserButton != null)
        {
            eraserButton.onClick.RemoveAllListeners();
            eraserButton.onClick.AddListener(() => SelectItem(ItemType.Eraser));
        }

        if (magicWandButton != null)
        {
            magicWandButton.onClick.RemoveAllListeners();
            magicWandButton.onClick.AddListener(() => SelectItem(ItemType.MagicWand));
        }

        if (timeFreezeButton != null)
        {
            timeFreezeButton.onClick.RemoveAllListeners();
            timeFreezeButton.onClick.AddListener(() => SelectItem(ItemType.TimeFreeze));
        }
    }

    private void SelectItem(ItemType itemType)
    {
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.SelectItem(itemType);
        }
    }

    private void HidePanels()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (gameWinPanel != null)
        {
            gameWinPanel.SetActive(false);
        }
    }

    public void UpdateHealthDisplay()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (healthHearts != null && healthHearts.Length > 0)
        {
            for (int i = 0; i < healthHearts.Length; i++)
            {
                if (healthHearts[i] == null)
                {
                    continue;
                }

                healthHearts[i].sprite = i < GameManager.Instance.currentHealth ? fullHeartSprite : emptyHeartSprite;
                healthHearts[i].enabled = i < GameManager.Instance.maxHealth;
            }
        }

        UpdateLevelDisplay();
    }

    public void UpdateLevelDisplay()
    {
        if (GameManager.Instance == null || levelStatusText == null)
        {
            return;
        }

        levelStatusText.text = $"Level {GameManager.Instance.currentLevel}\nHP {GameManager.Instance.currentHealth}/{GameManager.Instance.maxHealth}";
    }

    public void UpdateTimeDisplay()
    {
        if (GameManager.Instance == null || timeText == null)
        {
            return;
        }

        float time = Mathf.Max(0f, GameManager.Instance.remainingTime);
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    public void UpdateItemCounts()
    {
        if (ItemManager.Instance == null)
        {
            return;
        }

        if (eraserCountText != null)
        {
            eraserCountText.text = ItemManager.Instance.eraserCount.ToString();
        }

        if (magicWandCountText != null)
        {
            magicWandCountText.text = ItemManager.Instance.magicWandCount.ToString();
        }

        if (timeFreezeCountText != null)
        {
            timeFreezeCountText.text = ItemManager.Instance.timeFreezeCount.ToString();
        }
    }

    public void ToggleSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void ShowGameWin()
    {
        if (gameWinPanel != null)
        {
            gameWinPanel.SetActive(true);
        }
    }

    public void RestartGame()
    {
        HidePanels();

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RestartLevel();
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.InitGame();
        }

        if (ArrowGenerator.Instance != null)
        {
            ArrowGenerator.Instance.ClearAllPositions();
            ArrowGenerator.Instance.GenerateArrows();
        }
    }

    public void HighlightSelectedItem(ItemType itemType)
    {
        SetButtonTint(eraserButton, itemType == ItemType.Eraser);
        SetButtonTint(magicWandButton, itemType == ItemType.MagicWand);
        SetButtonTint(timeFreezeButton, itemType == ItemType.TimeFreeze);
    }

    private void SetButtonTint(Button button, bool selected)
    {
        if (button == null || button.image == null)
        {
            return;
        }

        button.image.color = selected ? new Color(1f, 0.92f, 0.35f, 1f) : Color.white;
    }

    private Text FindTextByName(string objectName)
    {
        Text[] texts = FindObjectsOfType<Text>();
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].name == objectName)
            {
                return texts[i];
            }
        }

        return null;
    }

    private Button FindButtonByName(string objectName)
    {
        Button[] buttons = FindObjectsOfType<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].name == objectName)
            {
                return buttons[i];
            }
        }

        return null;
    }

    private GameObject FindPanelByName(string objectName)
    {
        Transform[] transforms = FindObjectsOfType<Transform>();
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == objectName && transforms[i].GetComponent<Button>() == null)
            {
                return transforms[i].gameObject;
            }
        }

        return null;
    }

    private Text FindTextInChildren(Transform parent)
    {
        Text[] texts = parent.GetComponentsInChildren<Text>(true);
        return texts.Length > 0 ? texts[0] : null;
    }

    private Text CreateLevelStatusText()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }

        if (canvas == null)
        {
            return null;
        }

        GameObject textObject = new GameObject("LevelStatus");
        textObject.transform.SetParent(canvas.transform, false);

        RectTransform rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -70f);
        rect.sizeDelta = new Vector2(260f, 72f);

        Text text = textObject.AddComponent<Text>();
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 28;
        text.color = new Color(0.12f, 0.32f, 0.65f, 1f);
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.raycastTarget = false;
        return text;
    }
}
