using UnityEngine;
using System.Collections.Generic;
using System;

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public class Arrow : MonoBehaviour
{
    public Direction direction;
    public List<Vector2> bodyPositions = new List<Vector2>();
    public List<GameObject> bodySegments = new List<GameObject>();

    public bool isMoving;
    public bool isErasing;

    private Vector2 headStartPos;
    private Vector2 directionVector;
    private float moveProgress;
    private float segmentMoveTime;
    private bool isMoveComplete;

    public Vector2 HeadPosition => bodyPositions.Count > 0 ? bodyPositions[0] : Vector2.zero;

    private void Awake()
    {
        directionVector = GetDirectionVector(direction);
    }

    public void Initialize(Direction dir, List<Vector2> positions, GameObject headPrefab, GameObject bodyPrefab)
    {
        direction = dir;
        bodyPositions = new List<Vector2>(positions);
        directionVector = GetDirectionVector(dir);
        CreateVisuals(headPrefab, bodyPrefab);
        GameManager.Instance.AddArrow(this);
    }

    private Vector2 GetDirectionVector(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up: return Vector2.up;
            case Direction.Down: return Vector2.down;
            case Direction.Left: return Vector2.left;
            case Direction.Right: return Vector2.right;
            default: return Vector2.right;
        }
    }

    private void CreateVisuals(GameObject headPrefab, GameObject bodyPrefab)
    {
        foreach (var segment in bodySegments)
        {
            DestroyImmediate(segment);
        }
        bodySegments.Clear();

        for (int i = 0; i < bodyPositions.Count; i++)
        {
            Vector2 pos = bodyPositions[i];
            GameObject prefab = i == 0 ? headPrefab : bodyPrefab;

            GameObject segment = Instantiate(prefab, pos, Quaternion.identity, transform);
            bodySegments.Add(segment);
        }
    }

    private void OnMouseDown()
    {
        if (!GameManager.Instance.isGameRunning || GameManager.Instance.isPaused) return;
        if (isMoving || isErasing) return;

        if (ItemManager.Instance.currentItem == ItemType.Eraser && !ItemManager.Instance.isUsingEraser)
        {
            ItemManager.Instance.UseEraser(this);
            return;
        }

        StartMove();
    }

    public void StartMove()
    {
        if (isMoving) return;

        isMoving = true;
        headStartPos = HeadPosition;
        moveProgress = 0f;
        segmentMoveTime = 1f / GameManager.Instance.moveSpeed;
        isMoveComplete = false;

        StartCoroutine(MoveCoroutine());
    }

    private System.Collections.IEnumerator MoveCoroutine()
    {
        float size = GameManager.Instance.arrowSize;
        int segmentsToMove = CalculateMoveSegments();

        if (segmentsToMove <= 0)
        {
            isMoving = false;
            yield break;
        }

        for (int i = 0; i < segmentsToMove; i++)
        {
            Vector2 nextHeadPos = bodyPositions[0] + directionVector * size;

            foreach (var arrow in GameManager.Instance.arrows)
            {
                if (arrow != this && arrow.IsPositionOccupied(nextHeadPos))
                {
                    isMoving = false;
                    GameManager.Instance.DamagePlayer();
                    yield break;
                }
            }

            MoveOneSegment();
            yield return new WaitForSeconds(segmentMoveTime);
        }

        isMoving = false;
        isMoveComplete = true;

        if (bodyPositions.Count == 0)
        {
            GameManager.Instance.RemoveArrow(this);
        }
    }

    private int CalculateMoveSegments()
    {
        int count = 0;
        float size = GameManager.Instance.arrowSize;
        Vector2 currentPos = HeadPosition;
        Vector2 nextPos = currentPos + directionVector * size;

        while (IsValidMovePosition(nextPos))
        {
            count++;
            currentPos = nextPos;
            nextPos = currentPos + directionVector * size;
        }

        return count;
    }

    private bool IsValidMovePosition(Vector2 position)
    {
        foreach (var arrow in GameManager.Instance.arrows)
        {
            if (arrow != this && arrow.IsPositionOccupied(position))
                return false;
        }
        return true;
    }

    private void MoveOneSegment()
    {
        float size = GameManager.Instance.arrowSize;

        for (int i = bodyPositions.Count - 1; i > 0; i--)
        {
            bodyPositions[i] = bodyPositions[i - 1];
        }

        bodyPositions[0] += directionVector * size;

        for (int i = 0; i < bodyPositions.Count && i < bodySegments.Count; i++)
        {
            bodySegments[i].transform.position = bodyPositions[i];
        }

        if (bodyPositions.Count > 1)
        {
            Vector2 tailPos = bodyPositions[bodyPositions.Count - 1];
            bool isOutside = !IsPositionVisible(tailPos);

            if (isOutside)
            {
                bodyPositions.RemoveAt(bodyPositions.Count - 1);

                if (bodySegments.Count > bodyPositions.Count)
                {
                    Destroy(bodySegments[bodySegments.Count - 1]);
                    bodySegments.RemoveAt(bodySegments.Count - 1);
                }
            }
        }
    }

    private bool IsPositionVisible(Vector2 position)
    {
        Camera cam = Camera.main;
        Vector3 screenPos = cam.WorldToScreenPoint(position);
        return screenPos.x >= -50 && screenPos.x <= Screen.width + 50 &&
               screenPos.y >= -50 && screenPos.y <= Screen.height + 50;
    }

    public bool IsPositionOccupied(Vector2 position)
    {
        float size = GameManager.Instance.arrowSize * 0.9f;
        foreach (var pos in bodyPositions)
        {
            if (Vector2.Distance(pos, position) < size)
                return true;
        }
        return false;
    }

    public void Erase()
    {
        isErasing = true;
        GameManager.Instance.RemoveArrow(this);
    }
}