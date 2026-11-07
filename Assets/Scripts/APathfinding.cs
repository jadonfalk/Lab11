using System.Collections.Generic;
using UnityEngine;

public class APathfinding : MonoBehaviour
{
    private List<Vector2Int> path = new List<Vector2Int>();
    [SerializeField] private Vector2Int start = new Vector2Int(0, 1);
    [SerializeField] private Vector2Int goal = new Vector2Int(4, 4);

    public int Width => grid.GetLength(1);
    public int Height => grid.GetLength(0);
    public Vector2Int StartPosition => start;
    public Vector2Int GoalPosition => goal;
    public string Status { get; private set; } = "Ready";
    public bool IsOnPath(Vector2Int position) => path.Contains(position);
    public bool IsObstacle(Vector2Int position) => IsInBounds(position) && grid[position.y, position.x] == 1;

    private Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1)
    };

    private int[,] grid = new int[,]
    {
        { 0, 1, 0, 0, 0 },
        { 0, 1, 0, 1, 0 },
        { 0, 0, 0, 1, 0 },
        { 0, 1, 1, 1, 0 },
        { 0, 0, 0, 0, 0 }
    };

    private void Start()
    {
        // Keep Inspector endpoints valid on the original 5 x 5 grid.
        start = ClampToGrid(start);
        goal = ClampToGrid(goal);
        grid[start.y, start.x] = 0;
        grid[goal.y, goal.x] = 0;
        RecalculatePath();
    }

    private Vector2Int ClampToGrid(Vector2Int position)
    {
        return new Vector2Int(Mathf.Clamp(position.x, 0, Width - 1),
                              Mathf.Clamp(position.y, 0, Height - 1));
    }

    // Grid is indexed [y, x]; positions are expressed as (x, y).
    public void GenerateRandomGrid(int width, int height, float obstacleProbability)
    {
        if (width < 1 || height < 1 || float.IsNaN(obstacleProbability))
        {
            Debug.LogWarning("Use positive dimensions and a valid probability.");
            return;
        }
        float probability = Mathf.Clamp01(obstacleProbability);
        grid = new int[height, width];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                grid[y, x] = probability >= 1f || Random.value < probability ? 1 : 0;

        start = ClampToGrid(start);
        goal = ClampToGrid(goal);
        // Endpoints stay walkable. Random layouts are not guaranteed solvable.
        grid[start.y, start.x] = 0;
        grid[goal.y, goal.x] = 0;
        RecalculatePath();
    }

    public void SetStart(Vector2Int position)
    {
        if (!IsInBounds(position) || IsObstacle(position)) return;
        start = position;
        RecalculatePath();
    }

    public void SetGoal(Vector2Int position)
    {
        if (!IsInBounds(position) || IsObstacle(position)) return;
        goal = position;
        RecalculatePath();
    }

    public void AddObstacle(Vector2Int position)
    {
        if (!IsInBounds(position) || position == start || position == goal) return;
        grid[position.y, position.x] = 1;
        RecalculatePath();
    }

    public void RemoveObstacle(Vector2Int position)
    {
        if (!IsInBounds(position)) return;
        grid[position.y, position.x] = 0;
        RecalculatePath();
    }

    public void RecalculatePath()
    {
        FindPath(start, goal);
    }

    private void OnDrawGizmos()
    {
        float cellSize = 1f;

        for (int y = 0; y < grid.GetLength(0); y++)
        {
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                Vector3 cellPosition = new Vector3(x * cellSize, 0, y * cellSize);
                Gizmos.color = grid[y, x] == 1 ? Color.black : Color.white;
                Gizmos.DrawCube(cellPosition, new Vector3(cellSize, 0.1f, cellSize));
            }
        }

        foreach (var step in path)
        {
            Vector3 cellPosition = new Vector3(step.x * cellSize, 0.02f, step.y * cellSize);
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(cellPosition, new Vector3(cellSize, 0.1f, cellSize));
        }

        Gizmos.color = Color.green;
        Gizmos.DrawCube(new Vector3(start.x * cellSize, 0.04f, start.y * cellSize), new Vector3(cellSize, 0.1f, cellSize));

        Gizmos.color = Color.red;
        Gizmos.DrawCube(new Vector3(goal.x * cellSize, 0.06f, goal.y * cellSize), new Vector3(cellSize, 0.1f, cellSize));
    }

    private bool IsInBounds(Vector2Int point)
    {
        return point.x >= 0 && point.x < grid.GetLength(1) && point.y >= 0 && point.y < grid.GetLength(0);
    }

    private int Heuristic(Vector2Int a, Vector2Int b)
    {
        // Manhattan distance
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private void FindPath(Vector2Int start, Vector2Int goal)
    {
        // Clear first so an unsuccessful search cannot display a stale route.
        path.Clear();
        if (!IsInBounds(start) || !IsInBounds(goal) || IsObstacle(start) || IsObstacle(goal))
        {
            Status = "Invalid start or goal.";
            return;
        }
        var frontier = new PriorityQueue<Vector2Int, int>();
        frontier.Enqueue(start, 0);

        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var costSoFar = new Dictionary<Vector2Int, int>();

        cameFrom[start] = start;
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            if (current == goal)
                break;

            foreach (Vector2Int direction in directions)
            {
                Vector2Int next = current + direction;

                if (!IsInBounds(next) || grid[next.y, next.x] == 1)
                    continue;

                int newCost = costSoFar[current] + 1; // all moves cost 1

                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    int priority = newCost + Heuristic(next, goal);
                    frontier.Enqueue(next, priority);
                    cameFrom[next] = current;
                }
            }
        }

        if (!cameFrom.ContainsKey(goal))
        {
            Status = "No path found. Remove obstacles or choose new endpoints.";
            Debug.Log(Status);
            return;
        }

        // Reconstruct path
        path.Clear();
        Vector2Int step = goal;
        while (step != start)
        {
            path.Add(step);
            step = cameFrom[step];
        }
        path.Add(start);
        path.Reverse();
        Status = "Path found: " + (path.Count - 1) + " moves.";
    }
}