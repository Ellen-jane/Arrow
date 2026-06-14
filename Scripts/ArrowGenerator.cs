using UnityEngine;
using System.Collections.Generic;

public class ArrowGenerator : MonoBehaviour
{
    public static ArrowGenerator Instance;

    public GameObject arrowHeadPrefab;
    public GameObject arrowBodyPrefab;

    public int minArrows = 10;
    public int maxArrows = 20;
    public int minLength = 3;
    public int maxLength = 15;
    public float gridSize = 1f;

    public Rect playArea;

    private HashSet<Vector2> occupiedPositions = new HashSet<Vector2>();
    private System.Random random;
    private Camera mainCamera;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        random = new System.Random();
        mainCamera = Camera.main;

        if (playArea == new Rect(0, 0, 0, 0))
        {
            SetPlayAreaFromCamera();
        }
    }

    private void Start()
    {
        Invoke("DelayedGenerate", 0.1f);
    }

    private void DelayedGenerate()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null!");
            return;
        }
        GenerateArrows();
    }

    private void SetPlayAreaFromCamera()
    {
        if (mainCamera != null)
        {
            float cameraDistance = Mathf.Abs(mainCamera.transform.position.z);

            Vector2 bottomLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, cameraDistance));
            Vector2 topRight = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, cameraDistance));

            float padding = gridSize * 2f;
            playArea = new Rect(
                bottomLeft.x + padding,
                bottomLeft.y + padding,
                topRight.x - bottomLeft.x - padding * 2,
                topRight.y - bottomLeft.y - padding * 2
            );
        }
        else
        {
            playArea = new Rect(-8, -4, 16, 8);
        }
    }

    public void GenerateArrows()
    {
        if (GameManager.Instance == null) return;

        occupiedPositions.Clear();
        int arrowCount = random.Next(minArrows, maxArrows + 1);

        for (int i = 0; i < arrowCount; i++)
        {
            GenerateSingleArrow();
        }
    }

    private bool GenerateSingleArrow()
    {
        int attempts = 0;
        const int maxAttempts = 100;

        while (attempts < maxAttempts)
        {
            attempts++;

            Direction direction = (Direction)random.Next(4);
            int length = random.Next(minLength, maxLength + 1);

            Vector2 startPos = GetRandomStartPosition(direction);

            if (!playArea.Contains(startPos)) continue;
            if (!IsValidPosition(startPos)) continue;

            List<Vector2> positions = GenerateArrowPath(startPos, direction, length);
            if (positions != null && positions.Count >= minLength)
            {
                CreateArrow(direction, positions);
                MarkPositionsOccupied(positions);
                return true;
            }
        }

        return false;
    }

    private Vector2 GetRandomStartPosition(Direction direction)
    {
        float x = 0, y = 0;

        switch (direction)
        {
            case Direction.Up:
                x = UnityEngine.Random.Range(playArea.xMin, playArea.xMax);
                y = playArea.yMin;
                break;
            case Direction.Down:
                x = UnityEngine.Random.Range(playArea.xMin, playArea.xMax);
                y = playArea.yMax;
                break;
            case Direction.Left:
                x = playArea.xMax;
                y = UnityEngine.Random.Range(playArea.yMin, playArea.yMax);
                break;
            case Direction.Right:
                x = playArea.xMin;
                y = UnityEngine.Random.Range(playArea.yMin, playArea.yMax);
                break;
        }

        return SnapToGrid(new Vector2(x, y));
    }

    private Vector2 SnapToGrid(Vector2 position)
    {
        return new Vector2(
            Mathf.Round(position.x / gridSize) * gridSize,
            Mathf.Round(position.y / gridSize) * gridSize
        );
    }

    private bool IsValidPosition(Vector2 position)
    {
        if (!playArea.Contains(position)) return false;
        if (occupiedPositions.Contains(position)) return false;
        if (GameManager.Instance != null && GameManager.Instance.IsPositionOccupied(position)) return false;
        return true;
    }

    private List<Vector2> GenerateArrowPath(Vector2 startPos, Direction direction, int length)
    {
        List<Vector2> positions = new List<Vector2>();
        Vector2 currentPos = startPos;
        Vector2 dirVector = GetDirectionVector(direction);

        if (!IsValidPosition(currentPos)) return null;

        positions.Add(currentPos);

        for (int i = 1; i < length; i++)
        {
            Vector2 nextPos = currentPos + dirVector * gridSize;

            if (!IsValidPosition(nextPos))
            {
                Direction? newDir = TryChangeDirection(currentPos, direction);
                if (newDir.HasValue)
                {
                    dirVector = GetDirectionVector(newDir.Value);
                    direction = newDir.Value;
                    nextPos = currentPos + dirVector * gridSize;
                }
                else
                {
                    break;
                }
            }

            if (!IsValidPosition(nextPos)) break;

            positions.Add(nextPos);
            currentPos = nextPos;
        }

        return positions;
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

    private Direction? TryChangeDirection(Vector2 currentPos, Direction currentDir)
    {
        List<Direction> possibleDirs = new List<Direction>();

        foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
        {
            if (dir == currentDir) continue;
            if (IsOppositeDirection(dir, currentDir)) continue;

            Vector2 nextPos = currentPos + GetDirectionVector(dir) * gridSize;
            if (IsValidPosition(nextPos))
            {
                possibleDirs.Add(dir);
            }
        }

        if (possibleDirs.Count > 0)
        {
            return possibleDirs[random.Next(possibleDirs.Count)];
        }

        return null;
    }

    private bool IsOppositeDirection(Direction dir1, Direction dir2)
    {
        return (dir1 == Direction.Up && dir2 == Direction.Down) ||
               (dir1 == Direction.Down && dir2 == Direction.Up) ||
               (dir1 == Direction.Left && dir2 == Direction.Right) ||
               (dir1 == Direction.Right && dir2 == Direction.Left);
    }

    private void CreateArrow(Direction direction, List<Vector2> positions)
    {
        GameObject arrowObj = new GameObject($"Arrow_{direction}_{positions.Count}");
        Arrow arrow = arrowObj.AddComponent<Arrow>();

        string headPrefabName = $"ArrowHead{direction}";
        string bodyPrefabName = $"ArrowBody{direction}";

        GameObject headPrefab = Resources.Load<GameObject>($"Prefabs/{headPrefabName}");
        GameObject bodyPrefab = Resources.Load<GameObject>($"Prefabs/{bodyPrefabName}");

        if (headPrefab == null) headPrefab = arrowHeadPrefab;
        if (bodyPrefab == null) bodyPrefab = arrowBodyPrefab;

        arrow.Initialize(direction, positions, headPrefab, bodyPrefab);

        BoxCollider2D collider = arrowObj.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
    }

    private void MarkPositionsOccupied(List<Vector2> positions)
    {
        foreach (var pos in positions)
        {
            occupiedPositions.Add(pos);
        }
    }

    public void ClearOccupiedPosition(Vector2 position)
    {
        occupiedPositions.Remove(position);
    }

    public void ClearAllPositions()
    {
        occupiedPositions.Clear();
    }

    private void OnDrawGizmos()
    {
        if (playArea != new Rect(0, 0, 0, 0))
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(playArea.center, playArea.size);

            Gizmos.color = Color.gray;
            for (float x = playArea.xMin; x <= playArea.xMax; x += gridSize)
            {
                for (float y = playArea.yMin; y <= playArea.yMax; y += gridSize)
                {
                    Gizmos.DrawWireSphere(new Vector3(x, y, 0), 0.05f);
                }
            }
        }
    }
}