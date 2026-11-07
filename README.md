# Unity A\* Pathfinding Lab

## 

## Reflection

### 1\. How does each pathfinding algorithm calculate and prioritize paths?

BFS explores cells in order of their distance in number of moves from the start. A first-in, first-out queue ensures that all cells at one distance are considered before cells farther away. It finds a shortest path when every move has the same cost, but it does not account for differing terrain costs.

Dijkstra tracks the cheapest known accumulated cost, g(n), to reach each cell. Its priority queue selects the cell with the lowest cost next, and it updates a neighbor whenever it finds a cheaper route. It finds least-cost paths with nonnegative movement costs. With unit costs, its exploration is similar to BFS, but priority-queue operations add overhead.

A\* uses f(n) = g(n) + h(n), combining the accumulated cost with an estimate of the remaining cost. This lab uses Manhattan distance: abs(x - goal.x) + abs(y - goal.y). It is appropriate for four-directional movement costing 1 per move, because it never overestimates the remaining distance and is consistent. A\* finds a shortest route here and can focus exploration toward the goal. It is not universally faster than every other algorithm; performance depends on the map, heuristic, and queue implementation.

### 2\. What challenges arise when dynamically updating obstacles in real time?

A new obstacle can invalidate a path that was previously usable, and some edits can make the goal unreachable. The displayed path must be cleared before recalculating so the user never sees an old route presented as valid. Endpoints and positions also need validation to avoid blocked endpoints or out-of-bounds array access.

This lab recalculates immediately after each accepted change. That is simple for small grids, but frequent updates or many agents could cause frame delays. A larger implementation could batch changes, limit search work per frame, or use incremental replanning. If an agent were following the path, it should stop or obtain an updated route when the path becomes blocked. An asynchronous search would also need a grid version or snapshot so results from an older grid are not applied after a new edit.

### 3\. Which algorithm should you choose, and how should you adapt it for larger grids or open-world settings?

For a known start and goal, A\* is a useful choice because its heuristic directs the search toward the target. BFS remains a simple choice for small grids with equal costs, while Dijkstra is useful when there is no informative heuristic or when computing costs outward from a source to many destinations.

For larger grids, I would replace the current linear-scan priority queue with a binary heap, reduce allocations, and spread expensive searches over multiple frames or run them on an appropriate background data representation. For an open world, I would divide the map into regions and use hierarchical routing, finding a coarse route between regions and refining nearby sections. A navigation mesh may represent walkable space more efficiently than a dense grid. Frequent local changes could justify incremental algorithms such as D\* Lite. The heuristic must remain appropriate for the actual movement rules and costs.

### 4\. What would your approach be for weighted cells, such as difficult terrain?

I would keep obstacle occupancy separate from a movement-cost array, with values such as 1 for normal ground and 3 for difficult terrain. In A\* or Dijkstra, the neighbor calculation would become `newCost = costSoFar\[current] + movementCost\[next.y, next.x]`, charging the cost of entering the next cell. BFS would no longer guarantee the least-cost route because fewer moves could be more expensive.

For A\*, I would multiply Manhattan distance by the smallest possible traversable step cost to preserve a safe lower bound. If zero-cost movement were allowed, that multiplier would be zero, reducing this heuristic to Dijkstra's behavior. Dijkstra requires nonnegative costs. For fractional costs, I would change the cost dictionary and queue priorities to floats. The UI could display terrain using distinct colors, and changing a terrain cost would trigger replanning just like an obstacle change.

## 

