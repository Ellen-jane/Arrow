using UnityEngine;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public Image[] healthHearts;
    public Text timeText;
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
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        settingsPanel.SetActive(false);
        gameOverPanel.SetActive(false);
        gameWinPanel.SetActive(false);

        GameManager.Instance.OnHealthChanged += UpdateHealthDisplay;
        GameManager.Instance.OnTimeChanged += UpdateTimeDisplay;
        GameManager.Instance.OnGameOver += ShowGameOver;
        GameManager.Instance.OnGameWin += ShowGameWin;

        settingsButton.onClick.AddListener(ToggleSettings);

        eraserButton.onClick.AddListener(() => ItemManager.Instance.SelectItem(ItemType.Eraser));
        magicWandButton.onClick.AddListener(() => ItemManager.Instance.SelectItem(ItemType.MagicWand));
        timeFreezeButton.onClick.AddListener(() => ItemManager.Instance.SelectItem(ItemType.TimeFreeze));

        UpdateHealthDisplay();
        UpdateTimeDisplay();
        UpdateItemCounts();
    }

    public void UpdateHealthDisplay()
    {
        for (int i = 0; i < healthHearts.Length; i++)
        {
            healthHearts[i].sprite = i < GameManager.Instance.currentHealth ? fullHeartSprite : emptyHeartSprite;
            healthHearts[i].enabled = i < GameManager.Instance.maxHealth;
        }
    }

    public void UpdateTimeDisplay()
    {
        float time = GameManager.Instance.remainingTime;
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    public void UpdateItemCounts()
    {
        eraserCountText.text = ItemManager.Instance.eraserCount.ToString();
        magicWandCountText.text = ItemManager.Instance.magicWandCount.ToString();
        timeFreezeCountText.text = ItemManager.Instance.timeFreezeCount.ToString();
    }

    public void ToggleSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    public void ShowGameWin()
    {
        gameWinPanel.SetActive(true);
    }

    public void RestartGame()
    {
        gameOverPanel.SetActive(false);
        gameWinPanel.SetActive(false);
        settingsPanel.SetActive(false);
        GameManager.Instance.InitGame();
        ArrowGenerator.Instance.ClearAllPositions();
        ArrowGenerator.Instance.GenerateArrows();
    }

    public void HighlightSelectedItem(ItemType itemType)
    {
        eraserButton.GetComponent<Image>().color = itemType == ItemType.Eraser ? Color.yellow : Color.white;
        magicWandButton.GetComponent<Image>().color = itemType == ItemType.MagicWand ? Color.yellow : Color.white;
        timeFreezeButton.GetComponent<Image>().color = itemType == ItemType.TimeFreeze ? Color.yellow : Color.white;
    }
}