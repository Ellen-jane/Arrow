using UnityEngine;
using System.Collections.Generic;
using System;

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
    public int timeFreezeCount = 1;

    public ItemType currentItem;
    public bool isUsingEraser;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SelectItem(ItemType itemType)
    {
        if (currentItem == itemType)
        {
            currentItem = ItemType.None;
            isUsingEraser = false;
        }
        else
        {
            currentItem = itemType;

            if (itemType == ItemType.MagicWand)
            {
                UseMagicWand();
            }
            else if (itemType == ItemType.TimeFreeze)
            {
                UseTimeFreeze();
            }
            else if (itemType == ItemType.Eraser)
            {
                isUsingEraser = true;
            }
        }

        UIManager.Instance.HighlightSelectedItem(currentItem);
        UIManager.Instance.UpdateItemCounts();
    }

    public void UseEraser(Arrow targetArrow)
    {
        if (eraserCount <= 0) return;
        if (targetArrow == null) return;

        eraserCount--;
        isUsingEraser = false;
        currentItem = ItemType.None;

        targetArrow.Erase();

        UIManager.Instance.HighlightSelectedItem(currentItem);
        UIManager.Instance.UpdateItemCounts();

        AudioManager.Instance.PlayEraseSound();
    }

    public void UseMagicWand()
    {
        if (magicWandCount <= 0) return;

        magicWandCount--;
        currentItem = ItemType.None;

        List<Arrow> arrows = new List<Arrow>(GameManager.Instance.arrows);
        if (arrows.Count == 0) return;

        int countToRemove = Mathf.Min(3, arrows.Count);
        System.Random random = new System.Random();

        for (int i = 0; i < countToRemove; i++)
        {
            if (arrows.Count == 0) break;

            int index = random.Next(arrows.Count);
            Arrow arrowToRemove = arrows[index];
            arrows.RemoveAt(index);

            GameManager.Instance.RemoveArrow(arrowToRemove);
        }

        UIManager.Instance.HighlightSelectedItem(currentItem);
        UIManager.Instance.UpdateItemCounts();

        AudioManager.Instance.PlayMagicSound();
    }

    public void UseTimeFreeze()
    {
        if (timeFreezeCount <= 0) return;

        timeFreezeCount--;
        currentItem = ItemType.None;

        GameManager.Instance.AddTime(60f);
        GameManager.Instance.FreezeTime(5f);

        UIManager.Instance.HighlightSelectedItem(currentItem);
        UIManager.Instance.UpdateItemCounts();

        AudioManager.Instance.PlayTimeFreezeSound();
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

        UIManager.Instance.UpdateItemCounts();
    }
}