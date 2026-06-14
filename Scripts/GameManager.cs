using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Level")]
    public int currentLevel = 3;
    public int maxHealth = 3;
    public float totalTime = 480f;
    public float moveSpeed = 10f;
    public float arrowSize = 0.5f;
    public Rect playArea = new Rect(-6.5f, -5f, 13f, 10f);
    public bool autoStartWithoutLevelManager = true;

    public int currentHealth;
    public float remainingTime;
    public bool isGameRunning;
    public bool isPaused;

    public readonly List<Arrow> arrows = new List<Arrow>();

    public Action OnHealthChanged;
    public Action OnTimeChanged;
    public Action OnLevelChanged;
    public Action OnGameOver;
    public Action OnGameWin;
    public Action OnArrowRemoved;

    private int movingArrowCount;

    public bool IsAnyArrowMoving => movingArrowCount > 0;
    public bool CanAcceptBoardInput => isGameRunning && !isPaused && !IsAnyArrowMoving;

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
        if (!isGameRunning && autoStartWithoutLevelManager && LevelManager.Instance == null)
        {
            InitGame();
        }
    }

    private void Update()
    {
        if (!isGameRunning || isPaused)
        {
            return;
        }

        remainingTime -= Time.deltaTime;
        if (remainingTime < 0f)
        {
            remainingTime = 0f;
        }

        OnTimeChanged?.Invoke();

        if (remainingTime <= 0f)
        {
            GameOver();
        }
    }

    public void ConfigureLevel(LevelConfig config)
    {
        if (config == null)
        {
            return;
        }

        currentLevel = config.level;
        maxHealth = config.health;
        totalTime = config.timeLimitSeconds;
        moveSpeed = config.arrowMoveSpeed;
        arrowSize = config.gridSize;
        playArea = config.playArea;
        OnLevelChanged?.Invoke();
    }

    public void InitGame()
    {
        ClearArrows();

        currentHealth = maxHealth;
        remainingTime = totalTime;
        movingArrowCount = 0;
        isGameRunning = true;
        isPaused = false;

        OnHealthChanged?.Invoke();
        OnTimeChanged?.Invoke();
        OnLevelChanged?.Invoke();
    }

    public void ClearArrows()
    {
        for (int i = arrows.Count - 1; i >= 0; i--)
        {
            if (arrows[i] != null)
            {
                Destroy(arrows[i].gameObject);
            }
        }

        arrows.Clear();
    }

    public void AddArrow(Arrow arrow)
    {
        if (arrow != null && !arrows.Contains(arrow))
        {
            arrows.Add(arrow);
        }
    }

    public void RemoveArrow(Arrow arrow)
    {
        if (arrow == null)
        {
            return;
        }

        arrows.Remove(arrow);

        if (arrow.gameObject != null)
        {
            Destroy(arrow.gameObject);
        }

        OnArrowRemoved?.Invoke();

        if (isGameRunning && arrows.Count == 0)
        {
            GameWin();
        }
    }

    public void RegisterArrowMovement()
    {
        movingArrowCount++;
    }

    public void UnregisterArrowMovement()
    {
        movingArrowCount = Mathf.Max(0, movingArrowCount - 1);
    }

    public void DamagePlayer()
    {
        if (!isGameRunning)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - 1);
        OnHealthChanged?.Invoke();

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    public void AddTime(float seconds)
    {
        remainingTime = Mathf.Max(0f, remainingTime + seconds);
        OnTimeChanged?.Invoke();
    }

    public void GameOver()
    {
        if (!isGameRunning)
        {
            return;
        }

        isGameRunning = false;
        OnGameOver?.Invoke();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOverSound();
        }
    }

    public void GameWin()
    {
        if (!isGameRunning)
        {
            return;
        }

        isGameRunning = false;
        OnGameWin?.Invoke();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameWinSound();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
    }

    public bool IsPositionOccupied(Vector2 position, Arrow ignoredArrow = null)
    {
        foreach (var arrow in arrows)
        {
            if (arrow != null && arrow != ignoredArrow && arrow.IsPositionOccupied(position))
            {
                return true;
            }
        }

        return false;
    }

    public Vector2 SnapToGrid(Vector2 worldPosition)
    {
        return new Vector2(
            Mathf.Round(worldPosition.x / arrowSize) * arrowSize,
            Mathf.Round(worldPosition.y / arrowSize) * arrowSize
        );
    }

    public bool IsInsidePlayArea(Vector2 position)
    {
        return position.x >= playArea.xMin &&
               position.x <= playArea.xMax &&
               position.y >= playArea.yMin &&
               position.y <= playArea.yMax;
    }
}
