using UnityEngine;
using System.Collections.Generic;

public class ArrowGenerator : MonoBehaviour
{
    public static ArrowGenerator Instance;

    public GameObject arrowHeadPrefab;
    public GameObject arrowBodyPrefab;

    public int targetArrowCount = 64;
    public int minLength = 2;
    public int maxLength = 14;
    public float gridSize = 0.5f;
    public Rect playArea = new Rect(-6.5f, -5f, 13f, 10f);
    public int maxAttemptsPerArrow = 300;

    private readonly HashSet<Vector2> occupiedPositions = new HashSet<Vector2>();
    private readonly List<Vector2> gridCells = new List<Vector2>();
    private System.Random random;

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

        random = new System.Random();
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.arrows.Count == 0)
        {
            GenerateArrows();
        }
    }

    public void Configure(LevelConfig config)
    {
        if (config == null)
        {
            return;
        }

        targetArrowCount = config.arrowCount;
        minLength = config.minArrowLength;
        maxLength = config.maxArrowLength;
        gridSize = config.gridSize;
        playArea = config.playArea;
    }

    public void GenerateArrows()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null. Cannot generate arrows.");
            return;
        }

        occupiedPositions.Clear();
        BuildGridCells();

        int created = 0;
        int attempts = 0;
        int maxAttempts = Mathf.Max(targetArrowCount * maxAttemptsPerArrow, maxAttemptsPerArrow);

        while (created < targetArrowCount && attempts < maxAttempts)
        {
            attempts++;
            if (GenerateSingleArrow())
            {
                created++;
            }
        }

        if (created < targetArrowCount)
        {
            Debug.LogWarning($"Generated {created}/{targetArrowCount} arrows. Try lowering density or arrow length if the board looks too sparse.");
        }
    }

    private void BuildGridCells()
    {
        gridCells.Clear();

        float xMin = Mathf.Ceil(playArea.xMin / gridSize) * gridSize;
        float xMax = Mathf.Floor(playArea.xMax / gridSize) * gridSize;
        float yMin = Mathf.Ceil(playArea.yMin / gridSize) * gridSize;
        float yMax = Mathf.Floor(playArea.yMax / gridSize) * gridSize;

        for (float x = xMin; x <= xMax; x += gridSize)
        {
            for (float y = yMin; y <= yMax; y += gridSize)
            {
                Vector2 cell = new Vector2(RoundToGrid(x), RoundToGrid(y));
                if (IsInsidePlayArea(cell))
                {
                    gridCells.Add(cell);
                }
            }
        }
    }

    private bool GenerateSingleArrow()
    {
        for (int attempt = 0; attempt < maxAttemptsPerArrow; attempt++)
        {
            if (gridCells.Count == 0)
            {
                return false;
            }

            Direction direction = (Direction)random.Next(4);
            int length = PickArrowLength();
            Vector2 headPosition = gridCells[random.Next(gridCells.Count)];

            if (!IsCellAvailable(headPosition))
            {
                continue;
            }

            List<Vector2> positions = GenerateTrailingPath(headPosition, direction, length);
            if (positions.Count < minLength)
            {
                continue;
            }

            CreateArrow(direction, positions);
            MarkPositionsOccupied(positions);
            return true;
        }

        return false;
    }

    private List<Vector2> GenerateTrailingPath(Vector2 headPosition, Direction headDirection, int targetLength)
    {
        List<Vector2> positions = new List<Vector2>();
        HashSet<Vector2> localPositions = new HashSet<Vector2>();

        positions.Add(headPosition);
        localPositions.Add(headPosition);

        Vector2 current = headPosition;
        Direction travelDirection = Opposite(headDirection);

        for (int i = 1; i < targetLength; i++)
        {
            List<Direction> candidates = GetCandidateBodyDirections(travelDirection, i == 1);
            bool added = false;

            for (int c = 0; c < candidates.Count; c++)
            {
                Direction candidate = candidates[c];
                Vector2 next = SnapToGrid(current + Arrow.GetDirectionVector(candidate) * gridSize);

                if (!IsCellAvailable(next) || localPositions.Contains(next))
                {
                    continue;
                }

                positions.Add(next);
                localPositions.Add(next);
                current = next;
                travelDirection = candidate;
                added = true;
                break;
            }

            if (!added)
            {
                break;
            }
        }

        return positions;
    }

    private int PickArrowLength()
    {
        if (maxLength <= minLength)
        {
            return minLength;
        }

        bool makeLongArrow = random.NextDouble() < 0.25d;
        if (makeLongArrow)
        {
            int longMin = Mathf.Max(minLength, Mathf.FloorToInt(maxLength * 0.55f));
            return random.Next(longMin, maxLength + 1);
        }

        int shortMax = Mathf.Min(maxLength, minLength + 5);
        return random.Next(minLength, shortMax + 1);
    }

    private List<Direction> GetCandidateBodyDirections(Direction travelDirection, bool firstBodySegment)
    {
        List<Direction> candidates = new List<Direction>();

        if (firstBodySegment)
        {
            candidates.Add(travelDirection);
            return candidates;
        }

        candidates.Add(Direction.Up);
        candidates.Add(Direction.Down);
        candidates.Add(Direction.Left);
        candidates.Add(Direction.Right);

        Direction blockedBacktrack = Opposite(travelDirection);
        candidates.Remove(blockedBacktrack);
        Shuffle(candidates);

        if (candidates.Remove(travelDirection))
        {
            candidates.Insert(0, travelDirection);
        }

        return candidates;
    }

    private void Shuffle(List<Direction> directions)
    {
        for (int i = directions.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            Direction temp = directions[i];
            directions[i] = directions[j];
            directions[j] = temp;
        }
    }

    private Direction Opposite(Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                return Direction.Down;
            case Direction.Down:
                return Direction.Up;
            case Direction.Left:
                return Direction.Right;
            case Direction.Right:
                return Direction.Left;
            default:
                return Direction.Left;
        }
    }

    private bool IsCellAvailable(Vector2 position)
    {
        return IsInsidePlayArea(position) && !occupiedPositions.Contains(position);
    }

    private bool IsInsidePlayArea(Vector2 position)
    {
        return position.x >= playArea.xMin &&
               position.x <= playArea.xMax &&
               position.y >= playArea.yMin &&
               position.y <= playArea.yMax;
    }

    private Vector2 SnapToGrid(Vector2 position)
    {
        return new Vector2(RoundToGrid(position.x), RoundToGrid(position.y));
    }

    private float RoundToGrid(float value)
    {
        return Mathf.Round(value / gridSize) * gridSize;
    }

    private void CreateArrow(Direction direction, List<Vector2> positions)
    {
        GameObject arrowObj = new GameObject($"Arrow_{direction}_{positions.Count}");
        Arrow arrow = arrowObj.AddComponent<Arrow>();

        GameObject headPrefab = Resources.Load<GameObject>($"Prefabs/ArrowHead{direction}");
        GameObject bodyPrefab = Resources.Load<GameObject>($"Prefabs/ArrowBody{direction}");

        if (headPrefab == null)
        {
            headPrefab = arrowHeadPrefab;
        }

        if (bodyPrefab == null)
        {
            bodyPrefab = arrowBodyPrefab;
        }

        arrow.Initialize(direction, positions, headPrefab, bodyPrefab);
    }

    private void MarkPositionsOccupied(List<Vector2> positions)
    {
        for (int i = 0; i < positions.Count; i++)
        {
            occupiedPositions.Add(positions[i]);
        }
    }

    public void ClearAllPositions()
    {
        occupiedPositions.Clear();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(playArea.center, playArea.size);

        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.35f);
        for (float x = playArea.xMin; x <= playArea.xMax; x += gridSize)
        {
            for (float y = playArea.yMin; y <= playArea.yMax; y += gridSize)
            {
                Gizmos.DrawWireSphere(new Vector3(x, y, 0), 0.035f);
            }
        }
    }
}
