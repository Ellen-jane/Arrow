using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

public enum ItemType
{
    None,
    Eraser,
    MagicWand,
    TimeFreeze
}

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    public int eraserCount = 3;
    public int magicWandCount = 2;

    [FormerlySerializedAs("timeFreezerCount")]
    public int timeFreezeCount = 1;

    public ItemType currentItem = ItemType.None;

    public bool IsEraserArmed => currentItem == ItemType.Eraser && eraserCount > 0;

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

    public void ResetForLevel(LevelConfig config)
    {
        if (config == null)
        {
            return;
        }

        eraserCount = config.eraserCount;
        magicWandCount = config.magicWandCount;
        timeFreezeCount = config.timeFreezeCount;
        currentItem = ItemType.None;
        RefreshUI();
    }

    public void SelectItem(ItemType itemType)
    {
        if (itemType == currentItem)
        {
            CancelSelection();
            return;
        }

        switch (itemType)
        {
            case ItemType.Eraser:
                currentItem = eraserCount > 0 ? ItemType.Eraser : ItemType.None;
                RefreshUI();
                break;
            case ItemType.MagicWand:
                UseMagicWand();
                break;
            case ItemType.TimeFreeze:
                UseTimeFreeze();
                break;
            default:
                CancelSelection();
                break;
        }
    }

    public void CancelSelection()
    {
        currentItem = ItemType.None;
        RefreshUI();
    }

    public void UseEraser(Arrow targetArrow)
    {
        if (eraserCount <= 0 || targetArrow == null)
        {
            CancelSelection();
            return;
        }

        eraserCount--;
        currentItem = ItemType.None;
        targetArrow.Erase();
        RefreshUI();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEraseSound();
        }
    }

    public void UseMagicWand()
    {
        if (magicWandCount <= 0 || GameManager.Instance == null || GameManager.Instance.arrows.Count == 0)
        {
            CancelSelection();
            return;
        }

        magicWandCount--;
        currentItem = ItemType.None;

        List<Arrow> candidates = new List<Arrow>();
        for (int i = 0; i < GameManager.Instance.arrows.Count; i++)
        {
            Arrow arrow = GameManager.Instance.arrows[i];
            if (arrow != null && !arrow.isMoving && !arrow.isErasing)
            {
                candidates.Add(arrow);
            }
        }

        int countToRemove = Mathf.Min(3, candidates.Count);
        for (int i = 0; i < countToRemove; i++)
        {
            int index = Random.Range(0, candidates.Count);
            Arrow arrowToRemove = candidates[index];
            candidates.RemoveAt(index);
            GameManager.Instance.RemoveArrow(arrowToRemove);
        }

        RefreshUI();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMagicSound();
        }
    }

    public void UseTimeFreeze()
    {
        if (timeFreezeCount <= 0 || GameManager.Instance == null)
        {
            CancelSelection();
            return;
        }

        timeFreezeCount--;
        currentItem = ItemType.None;
        GameManager.Instance.AddTime(60f);
        RefreshUI();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayTimeFreezeSound();
        }
    }

    public void AddItem(ItemType itemType, int count = 1)
    {
        switch (itemType)
        {
            case ItemType.Eraser:
                eraserCount += count;
                break;
            case ItemType.MagicWand:
                magicWandCount += count;
                break;
            case ItemType.TimeFreeze:
                timeFreezeCount += count;
                break;
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HighlightSelectedItem(currentItem);
            UIManager.Instance.UpdateItemCounts();
        }
    }
}
