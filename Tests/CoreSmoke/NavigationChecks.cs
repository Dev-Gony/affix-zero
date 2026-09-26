using System;
using System.Collections.Generic;
using AffixZero.Core;

internal static class NavigationChecks
{
    public static void Run(Action<bool, string> check, Action<Action, string> throws)
    {
        var wall = new List<GridCell> { new GridCell(2, 0), new GridCell(2, 1), new GridCell(2, 2), new GridCell(2, 2) };
        var dungeon = new DungeonNavigation(5, 4, wall);
        var start = new GridCell(0, 1);
        var goal = new GridCell(4, 1);
        IReadOnlyList<GridCell> path = dungeon.FindPath(start, goal);
        check(path.Count == 8 && path[0] != start && path[path.Count - 1] == goal,
            "navigation takes shortest eight-step detour with start excluded and goal included");
        bool legal = true;
        GridCell previous = start;
        foreach (GridCell cell in path)
        {
            legal &= dungeon.IsWalkable(cell) && Math.Abs(cell.X - previous.X) + Math.Abs(cell.Y - previous.Y) == 1;
            previous = cell;
        }
        check(legal, "every path segment uses one cardinal step without crossing a wall");
        wall.Clear();
        wall.Add(new GridCell(0, 1));
        check(!dungeon.IsWalkable(new GridCell(2, 2)) && dungeon.IsWalkable(start) &&
            dungeon.FindPath(start, goal).Count == 8, "duplicate obstacles are harmless and caller mutations cannot change topology");
        bool protectedPath = false;
        try { ((IList<GridCell>)path).Clear(); } catch (NotSupportedException) { protectedPath = true; }
        check(protectedPath && dungeon.FindPath(start, goal).Count == 8, "returned route cannot mutate stored or subsequent routes");

        var split = new DungeonNavigation(5, 3, new[] { new GridCell(2, 0), new GridCell(2, 1), new GridCell(2, 2) });
        check(split.FindPath(start, goal).Count == 0, "sealed wall reports unreachable goal without partial route");
        check(split.TryNearestReachable(start, new[] { goal, new GridCell(2, 1), new GridCell(1, 1) },
            out int selected, out IReadOnlyList<GridCell> selectedPath) && selected == 2 && selectedPath.Count == 1,
            "target selection skips unreachable and blocked enemies");
        check(!split.TryNearestReachable(start, new[] { goal, new GridCell(-1, 1), new GridCell(2, 1) },
            out selected, out selectedPath) && selected == -1 && selectedPath.Count == 0,
            "all invalid targets return explicit failed selection and empty route");
        var open = new DungeonNavigation(4, 4, null);
        check(open.TryNearestReachable(new GridCell(1, 1), new[] { new GridCell(0, 1), new GridCell(2, 1) },
            out selected, out selectedPath) && selected == 0 && selectedPath[0] == new GridCell(0, 1),
            "equidistant targets preserve input order independent of BFS direction order");
        check(dungeon.TryNearestReachable(new GridCell(1, 1), new[] { new GridCell(3, 1), new GridCell(0, 3) },
            out selected, out selectedPath) && selected == 1 && selectedPath.Count == 3,
            "selection favors shorter walk over geometrically closer target behind wall");
        check(open.FindPath(start, start).Count == 0 && open.TryNearestReachable(start,
            new[] { new GridCell(1, 1), start, start }, out selected, out selectedPath) && selected == 1 && selectedPath.Count == 0,
            "same-cell candidate is successful zero-step route and first duplicate wins");
        check(!dungeon.IsWalkable(new GridCell(-1, 0)) && !dungeon.IsWalkable(new GridCell(5, 0)) &&
            !dungeon.IsWalkable(new GridCell(0, 4)) && !dungeon.IsWalkable(new GridCell(int.MaxValue, int.MinValue)) &&
            dungeon.FindPath(new GridCell(-1, 0), goal).Count == 0 && dungeon.FindPath(start, new GridCell(5, 0)).Count == 0,
            "out-of-bounds coordinates never index grid memory or produce paths");
        check(dungeon.FindPath(new GridCell(2, 1), new GridCell(2, 1)).Count == 0 &&
            !dungeon.TryNearestReachable(new GridCell(2, 1), new[] { start }, out selected, out selectedPath) &&
            selected == -1 && selectedPath.Count == 0, "blocked start cannot select targets including its own cell");
        check(!open.TryNearestReachable(start, null, out selected, out selectedPath) && selected == -1 &&
            !open.TryNearestReachable(start, Array.Empty<GridCell>(), out selected, out selectedPath) && selectedPath.Count == 0,
            "null and empty target lists report no target");
        var same = new GridCell(7, -2);
        var values = new HashSet<GridCell> { same, new GridCell(7, -2) };
        check(same.Equals((object)new GridCell(7, -2)) && same == new GridCell(7, -2) && same != new GridCell(-2, 7) &&
            !same.Equals(null) && !same.Equals("7,-2") && values.Count == 1,
            "grid cells have consistent value equality and hash behavior");
        throws(() => new DungeonNavigation(0, 1, null), "zero grid width rejected");
        throws(() => new DungeonNavigation(1, -1, null), "negative grid height rejected");
        throws(() => new DungeonNavigation(int.MaxValue, 1, null), "oversized grid width rejected before allocation");
        throws(() => new DungeonNavigation(1, 257, null), "grid height above limit rejected");
        throws(() => new DungeonNavigation(2, 2, new[] { new GridCell(2, 1) }), "out-of-bounds obstacle rejected at construction");
        var maximum = new DungeonNavigation(256, 256, null);
        check(maximum.FindPath(new GridCell(0, 0), new GridCell(255, 255)).Count == 510,
            "maximum supported grid resolves cardinal shortest route without integer overflow");
        var single = new DungeonNavigation(1, 1, null);
        check(single.IsWalkable(default) && single.TryNearestReachable(default, new[] { default(GridCell) },
            out selected, out selectedPath) && selected == 0 && selectedPath.Count == 0,
            "single-cell dungeon supports already-reached target");
    }
}
