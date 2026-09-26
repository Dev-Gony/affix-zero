using System;
using System.Collections.Generic;

namespace AffixZero.Core
{
    public readonly struct GridCell : IEquatable<GridCell>
    {
        public int X { get; }
        public int Y { get; }

        public GridCell(int x, int y) { X = x; Y = y; }
        public bool Equals(GridCell other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is GridCell other && Equals(other);
        public override int GetHashCode() => unchecked((X * 397) ^ Y);
        public static bool operator ==(GridCell left, GridCell right) => left.Equals(right);
        public static bool operator !=(GridCell left, GridCell right) => !left.Equals(right);
    }

    // Immutable topology. Presentation owns world coordinates and target lifetimes.
    public sealed class DungeonNavigation
    {
        private readonly bool[] blocked;
        public int Width { get; }
        public int Height { get; }

        public DungeonNavigation(int width, int height, IEnumerable<GridCell> blocked)
        {
            if (width < 1 || width > 256) throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 1 || height > 256) throw new ArgumentOutOfRangeException(nameof(height));
            Width = width;
            Height = height;
            this.blocked = new bool[width * height];
            if (blocked == null) return;
            foreach (GridCell cell in blocked)
            {
                if (!InBounds(cell)) throw new ArgumentOutOfRangeException(nameof(blocked));
                this.blocked[Index(cell)] = true;
            }
        }

        public bool IsWalkable(GridCell cell) => InBounds(cell) && !blocked[Index(cell)];

        public IReadOnlyList<GridCell> FindPath(GridCell start, GridCell goal)
        {
            if (!IsWalkable(start) || !IsWalkable(goal) || start == goal) return Array.Empty<GridCell>();
            int[] parents = Search(start, out _);
            return BuildPath(Index(start), Index(goal), parents);
        }

        public bool TryNearestReachable(GridCell start, IReadOnlyList<GridCell> candidates,
            out int index, out IReadOnlyList<GridCell> path)
        {
            index = -1;
            path = Array.Empty<GridCell>();
            if (!IsWalkable(start) || candidates == null || candidates.Count == 0) return false;
            int[] parents = Search(start, out int[] distance);
            int shortest = int.MaxValue;
            for (int i = 0; i < candidates.Count; i++)
            {
                GridCell candidate = candidates[i];
                if (!IsWalkable(candidate)) continue;
                int steps = distance[Index(candidate)];
                if (steps < 0 || steps >= shortest) continue;
                shortest = steps;
                index = i;
            }
            if (index < 0) return false;
            path = BuildPath(Index(start), Index(candidates[index]), parents);
            return true;
        }

        private int[] Search(GridCell start, out int[] distance)
        {
            var parents = new int[blocked.Length];
            distance = new int[blocked.Length];
            for (int i = 0; i < parents.Length; i++) { parents[i] = -1; distance[i] = -1; }
            int source = Index(start);
            parents[source] = source;
            distance[source] = 0;
            var queue = new Queue<int>();
            queue.Enqueue(source);
            while (queue.Count > 0)
            {
                int current = queue.Dequeue();
                GridCell cell = Cell(current);
                Visit(new GridCell(cell.X + 1, cell.Y), current, parents, distance, queue);
                Visit(new GridCell(cell.X, cell.Y + 1), current, parents, distance, queue);
                Visit(new GridCell(cell.X - 1, cell.Y), current, parents, distance, queue);
                Visit(new GridCell(cell.X, cell.Y - 1), current, parents, distance, queue);
            }
            return parents;
        }

        private void Visit(GridCell cell, int from, int[] parents, int[] distance, Queue<int> queue)
        {
            if (!IsWalkable(cell)) return;
            int next = Index(cell);
            if (parents[next] >= 0) return;
            parents[next] = from;
            distance[next] = distance[from] + 1;
            queue.Enqueue(next);
        }

        private IReadOnlyList<GridCell> BuildPath(int source, int destination, int[] parents)
        {
            if (parents[destination] < 0 || source == destination) return Array.Empty<GridCell>();
            var result = new List<GridCell>();
            for (int current = destination; current != source; current = parents[current]) result.Add(Cell(current));
            result.Reverse();
            return result.AsReadOnly();
        }

        private bool InBounds(GridCell cell) => cell.X >= 0 && cell.X < Width && cell.Y >= 0 && cell.Y < Height;
        private int Index(GridCell cell) => cell.Y * Width + cell.X;
        private GridCell Cell(int index) => new GridCell(index % Width, index / Width);
    }
}
