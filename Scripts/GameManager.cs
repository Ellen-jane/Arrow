using UnityEngine;
using System.Collections.Generic;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int maxHealth = 3;
    public float totalTime = 480f;
    public float moveSpeed = 5f;
    public float arrowSize = 1f;

    public int currentHealth;
    public float remainingTime;
    public bool isGameRunning;
    public bool isPaused;
    public bool isTimeFrozen;

    public List<Arrow> arrows = new List<Arrow>();
    public Action OnHealthChanged;
    public Action OnTimeChanged;
    public Action OnGameOver;
    public Action OnGameWin;
    public Action OnArrowRemoved;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        InitGame();
    }

    private void Update()
    {
        if (!isGameRunning || isPaused) return;

        if (!isTimeFrozen)
        {
            remainingTime -= Time.deltaTime;
            OnTimeChanged?.Invoke();

            if (remainingTime <= 0)
            {
                GameOver();
            }
        }
    }

    public void InitGame()
    {
        currentHealth = maxHealth;
        remainingTime = totalTime;
        isGameRunning = true;
        isPaused = false;
        isTimeFrozen = false;

        foreach (var arrow in arrows)
        {
            Destroy(arrow.gameObject);
        }
        arrows.Clear();

        OnHealthChanged?.Invoke();
        OnTimeChanged?.Invoke();
    }

    public void AddArrow(Arrow arrow)
    {
        if (!arrows.Contains(arrow))
            arrows.Add(arrow);
    }

    public void RemoveArrow(Arrow arrow)
    {
        arrows.Remove(arrow);
        Destroy(arrow.gameObject);
        OnArrowRemoved?.Invoke();

        if (arrows.Count == 0)
        {
            GameWin();
        }
    }

    public void DamagePlayer()
    {
        currentHealth--;
        OnHealthChanged?.Invoke();

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    public void AddTime(float seconds)
    {
        remainingTime += seconds;
        OnTimeChanged?.Invoke();
    }

    public void FreezeTime(float seconds)
    {
        isTimeFrozen = true;
        Invoke("UnfreezeTime", seconds);
    }

    private void UnfreezeTime()
    {
        isTimeFrozen = false;
    }

    public void GameOver()
    {
        isGameRunning = false;
        OnGameOver?.Invoke();
    }

    public void GameWin()
    {
        isGameRunning = false;
        OnGameWin?.Invoke();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
    }

    public bool IsPositionOccupied(Vector2 position)
    {
        foreach (var arrow in arrows)
        {
            if (arrow.IsPositionOccupied(position))
                return true;
        }
        return false;
    }

    public Vector2 GetGridPosition(Vector2 worldPosition)
    {
        return new Vector2(Mathf.Round(worldPosition.x / arrowSize) * arrowSize,
                          Mathf.Round(worldPosition.y / arrowSize) * arrowSize);
    }
}