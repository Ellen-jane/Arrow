using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    private Vector2 directionVector;

    public Vector2 HeadPosition => bodyPositions.Count > 0 ? bodyPositions[0] : Vector2.zero;

    private void Awake()
    {
        directionVector = GetDirectionVector(direction);
    }

    public void Initialize(Direction dir, List<Vector2> positions, GameObject headPrefab, GameObject bodyPrefab)
    {
        direction = dir;
        bodyPositions = new List<Vector2>(positions);
        directionVector = GetDirectionVector(direction);

        CreateVisuals(headPrefab, bodyPrefab);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddArrow(this);
        }
    }

    public void HandleClick()
    {
        if (GameManager.Instance == null || !GameManager.Instance.CanAcceptBoardInput)
        {
            return;
        }

        if (isMoving || isErasing)
        {
            return;
        }

        if (ItemManager.Instance != null && ItemManager.Instance.IsEraserArmed)
        {
            ItemManager.Instance.UseEraser(this);
            return;
        }

        StartMove();
    }

    private void OnMouseDown()
    {
        HandleClick();
    }

    public void StartMove()
    {
        if (isMoving || bodyPositions.Count == 0 || GameManager.Instance == null)
        {
            return;
        }

        StartCoroutine(MoveCoroutine());
    }

    private IEnumerator MoveCoroutine()
    {
        isMoving = true;
        GameManager.Instance.RegisterArrowMovement();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMoveSound();
        }

        List<Vector2> startPositions = new List<Vector2>(bodyPositions);
        int exitSteps;
        int blockedStep;
        bool canExit = TryGetExitPath(out exitSteps, out blockedStep);

        if (canExit)
        {
            Vector2 exitOffset = directionVector * GameManager.Instance.arrowSize * exitSteps;
            yield return AnimateOffset(startPositions, exitOffset);
            ApplyLogicalOffset(exitOffset);
            FinishMovement();
            GameManager.Instance.RemoveArrow(this);
            yield break;
        }

        int clearSteps = Mathf.Max(0, blockedStep - 1);
        Vector2 bumpOffset = directionVector * GameManager.Instance.arrowSize * clearSteps;
        if (clearSteps == 0)
        {
            bumpOffset = directionVector * GameManager.Instance.arrowSize * 0.35f;
        }

        yield return AnimateOffset(startPositions, bumpOffset);
        yield return AnimateOffset(startPositions, Vector2.zero);

        FinishMovement();
        GameManager.Instance.DamagePlayer();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBounceSound();
        }
    }

    private void FinishMovement()
    {
        isMoving = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnregisterArrowMovement();
        }
    }

    private bool TryGetExitPath(out int exitSteps, out int blockedStep)
    {
        exitSteps = CalculateStepsToLeavePlayArea();
        blockedStep = 0;

        float size = GameManager.Instance.arrowSize;
        for (int step = 1; step <= exitSteps; step++)
        {
            Vector2 offset = directionVector * size * step;

            for (int i = 0; i < bodyPositions.Count; i++)
            {
                Vector2 futurePosition = bodyPositions[i] + offset;

                if (!GameManager.Instance.IsInsidePlayArea(futurePosition))
                {
                    continue;
                }

                if (GameManager.Instance.IsPositionOccupied(futurePosition, this))
                {
                    blockedStep = step;
                    return false;
                }
            }
        }

        return true;
    }

    private int CalculateStepsToLeavePlayArea()
    {
        int steps = 0;
        float size = GameManager.Instance.arrowSize;

        while (steps < 1000)
        {
            steps++;
            Vector2 offset = directionVector * size * steps;

            bool anySegmentStillInside = false;
            for (int i = 0; i < bodyPositions.Count; i++)
            {
                if (GameManager.Instance.IsInsidePlayArea(bodyPositions[i] + offset))
                {
                    anySegmentStillInside = true;
                    break;
                }
            }

            if (!anySegmentStillInside)
            {
                return steps;
            }
        }

        Debug.LogWarning($"Arrow {name} could not find an exit distance. Removing it defensively.");
        return steps;
    }

    private IEnumerator AnimateOffset(List<Vector2> startPositions, Vector2 targetOffset)
    {
        Vector2 currentOffset = GetCurrentVisualOffset(startPositions);
        float distance = Vector2.Distance(currentOffset, targetOffset);
        float speed = Mathf.Max(0.01f, GameManager.Instance.moveSpeed);
        float duration = Mathf.Max(0.05f, distance / speed);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);
            Vector2 offset = Vector2.Lerp(currentOffset, targetOffset, t);
            ApplyVisualOffset(startPositions, offset);
            yield return null;
        }

        ApplyVisualOffset(startPositions, targetOffset);
    }

    private Vector2 GetCurrentVisualOffset(List<Vector2> startPositions)
    {
        if (bodySegments.Count == 0 || startPositions.Count == 0 || bodySegments[0] == null)
        {
            return Vector2.zero;
        }

        return (Vector2)bodySegments[0].transform.position - startPositions[0];
    }

    private void ApplyVisualOffset(List<Vector2> startPositions, Vector2 offset)
    {
        int count = Mathf.Min(startPositions.Count, bodySegments.Count);
        for (int i = 0; i < count; i++)
        {
            if (bodySegments[i] != null)
            {
                bodySegments[i].transform.position = startPositions[i] + offset;
            }
        }
    }

    private void ApplyLogicalOffset(Vector2 offset)
    {
        for (int i = 0; i < bodyPositions.Count; i++)
        {
            bodyPositions[i] += offset;
        }
    }

    private void CreateVisuals(GameObject headPrefab, GameObject bodyPrefab)
    {
        for (int i = bodySegments.Count - 1; i >= 0; i--)
        {
            if (bodySegments[i] != null)
            {
                Destroy(bodySegments[i]);
            }
        }

        bodySegments.Clear();

        for (int i = 0; i < bodyPositions.Count; i++)
        {
            GameObject prefab = i == 0 ? headPrefab : bodyPrefab;
            if (prefab == null)
            {
                continue;
            }

            GameObject segment = Instantiate(prefab, bodyPositions[i], Quaternion.identity, transform);
            float visualScale = GameManager.Instance != null ? GameManager.Instance.arrowSize : 1f;
            segment.transform.localScale *= visualScale;
            segment.name = i == 0 ? "Head" : $"Body_{i}";
            bodySegments.Add(segment);
            AttachClickTargets(segment);
        }
    }

    private void AttachClickTargets(GameObject segment)
    {
        Collider2D[] colliders = segment.GetComponentsInChildren<Collider2D>();

        if (colliders.Length == 0)
        {
            float colliderSize = GameManager.Instance != null ? GameManager.Instance.arrowSize : 1f;
            BoxCollider2D collider = segment.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one * colliderSize * 0.9f;
            colliders = new Collider2D[] { collider };
        }

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].isTrigger = true;

            ArrowSegmentClickArea clickArea = colliders[i].GetComponent<ArrowSegmentClickArea>();
            if (clickArea == null)
            {
                clickArea = colliders[i].gameObject.AddComponent<ArrowSegmentClickArea>();
            }

            clickArea.Owner = this;
        }
    }

    public bool IsPositionOccupied(Vector2 position)
    {
        float arrowSize = GameManager.Instance != null ? GameManager.Instance.arrowSize : 1f;
        float epsilon = Mathf.Max(0.01f, arrowSize * 0.25f);

        for (int i = 0; i < bodyPositions.Count; i++)
        {
            Vector2 current = bodyPositions[i];
            if (Mathf.Abs(current.x - position.x) <= epsilon && Mathf.Abs(current.y - position.y) <= epsilon)
            {
                return true;
            }
        }

        return false;
    }

    public void Erase()
    {
        if (isErasing)
        {
            return;
        }

        isErasing = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RemoveArrow(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static Vector2 GetDirectionVector(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:
                return Vector2.up;
            case Direction.Down:
                return Vector2.down;
            case Direction.Left:
                return Vector2.left;
            case Direction.Right:
                return Vector2.right;
            default:
                return Vector2.right;
        }
    }
}
