using UnityEngine;

// Small Game-view interface for recording the lab, separate from A* logic.
[RequireComponent(typeof(APathfinding))]
public class PathfindingDemo : MonoBehaviour
{
    [SerializeField, Range(1, 30)] private int randomWidth = 10;
    [SerializeField, Range(1, 30)] private int randomHeight = 10;
    [SerializeField, Range(0f, 1f)] private float obstacleProbability = 0.25f;

    private APathfinding pathfinding;
    private int mode;
    private Vector2 scroll;
    private readonly string[] modes = { "Set Start", "Set Goal", "Add Wall", "Erase Wall" };

    private void Awake()
    {
        pathfinding = GetComponent<APathfinding>();
    }

    private void OnGUI()
    {
        if (pathfinding == null) return;
        GUI.Box(new Rect(10, 10, 510, 160), "A* Pathfinding Lab");
        mode = GUI.Toolbar(new Rect(20, 40, 490, 28), mode, modes);
        GUI.Label(new Rect(20, 72, 490, 24), "Choose a mode, then click a cell. S = start, G = goal.");
        if (GUI.Button(new Rect(20, 100, 155, 28), "Random Grid"))
            pathfinding.GenerateRandomGrid(randomWidth, randomHeight, obstacleProbability);
        if (GUI.Button(new Rect(185, 100, 155, 28), "Empty Grid"))
            pathfinding.GenerateRandomGrid(randomWidth, randomHeight, 0f);
        GUI.Label(new Rect(20, 135, 490, 25), pathfinding.Status);

        const float cell = 34f;
        Rect viewport = new Rect(10, 180, Mathf.Max(100f, Screen.width - 20f),
                                 Mathf.Max(100f, Screen.height - 190f));
        Rect content = new Rect(0, 0, pathfinding.Width * cell, pathfinding.Height * cell);
        scroll = GUI.BeginScrollView(viewport, scroll, content);
        for (int y = 0; y < pathfinding.Height; y++)
        {
            for (int x = 0; x < pathfinding.Width; x++)
            {
                Vector2Int position = new Vector2Int(x, y);
                bool isStart = position == pathfinding.StartPosition;
                bool isGoal = position == pathfinding.GoalPosition;
                bool wall = pathfinding.IsObstacle(position);
                Color color = wall ? Color.black : Color.white;
                if (pathfinding.IsOnPath(position)) color = new Color(0.2f, 0.55f, 1f);
                if (isStart) color = Color.green;
                if (isGoal) color = Color.red;
                if (isStart && isGoal) color = Color.yellow;
                string label = isStart && isGoal ? "S/G" : isStart ? "S" : isGoal ? "G" : wall ? "#" : "";
                // Higher y appears above lower y, matching the XZ gizmo grid.
                Rect rect = new Rect(x * cell, (pathfinding.Height - 1 - y) * cell, cell - 2, cell - 2);
                Color previous = GUI.backgroundColor;
                GUI.backgroundColor = color;
                if (GUI.Button(rect, label))
                {
                    if (mode == 0) pathfinding.SetStart(position);
                    else if (mode == 1) pathfinding.SetGoal(position);
                    else if (mode == 2) pathfinding.AddObstacle(position);
                    else pathfinding.RemoveObstacle(position);
                }
                GUI.backgroundColor = previous;
            }
        }
        GUI.EndScrollView();
    }
}
