// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aoc.solvers
{
    public class Problem15 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            List<string> listify = await data.ToListAsync();
            int[,] cost = new int[listify.Count, listify[0].Length];
            int oRows = cost.GetLength(0);
            int oCols = cost.GetLength(1);
            for (int r = 0; r < listify.Count; r++)
            {
                string s = listify[r];
                for (int c = 0; c < s.Length; c++)
                {
                    cost[r, c] = s[c] - '0';
                }
            }

            int[,] expanded = new int[cost.GetLength(0) * 5, cost.GetLength(1) * 5];
            int rows = cost.GetLength(0);
            int cols = cost.GetLength(1);
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    for (int ro = 0; ro < 5; ro++)
                    {
                        for (int co = 0; co < 5; co++)
                        {
                            int kr = r + ro * rows;
                            int kc = c + co * cols;
                            expanded[kr, kc] = cost[r, c] + ro + co;
                            while (expanded[kr, kc] > 9)
                                expanded[kr, kc] -= 9;
                        }
                    }
                }
            }
            cost = expanded;
            rows = cost.GetLength(0);
            cols = cost.GetLength(1);
            for (int r = 0; r < 20; r += 1)
            {
                for (int c = 0; c < 20; c += 1)
                {
                    Console.Write(cost[r, c]);
                }

                Console.WriteLine();
            }

            Path[,] lengths = new Path[rows, cols];
            PriorityQueue<Path, long> queue = new PriorityQueue<Path, long>();
            Enqueue(queue, new Path(cost));
            while (queue.TryDequeue(out var path, out _))
            {
                (int row, int col) at = path.At;
                if (lengths[at.row, at.col] != null)
                    continue;
                lengths[at.row, at.col] = path;
                
                if (at.row > 0)
                    Enqueue(queue, path.Append(at.row - 1, at.col));
                if (at.col > 0)
                    Enqueue(queue, path.Append(at.row, at.col - 1));
                if (at.row < rows - 1)
                    Enqueue(queue, path.Append(at.row + 1, at.col));
                if (at.col < cols - 1)
                    Enqueue(queue, path.Append(at.row, at.col + 1));
            }

            Console.WriteLine($"Best cost path {lengths[rows-1, cols-1].Length}");
        }

        public static void Enqueue(PriorityQueue<Path, long> q, Path p)
        {
            q.Enqueue(p, p.Length);
        }

        public class Path
        {
            public  ImmutableList<(int row, int col)> Points { get; }
            public (int row, int col) At => Points[^1];
            public long Length { get; }
            public long Remaining { get; }
            public long Estimate  => Length + Remaining;
            private readonly int[,] _costs;

            public Path(int[,] costs)
            {
                _costs = costs;
                Points = ImmutableList.Create((0, 0));
            }

            private Path(int[,] costs, ImmutableList<(int row, int col)> points)
            {
                _costs = costs;
                Points = points;
                Length = CalculateLength();
                Remaining = CalculateRemaining();
            }

            private long CalculateLength() => Points.Skip(1).Sum(x => (long)_costs[x.row, x.col]);
            private long CalculateRemaining() => _costs.GetUpperBound(0) - Points[^1].row + _costs.GetUpperBound(1) - Points[^1].col;

            public Path Append(int row, int col)
            {
                return new Path(_costs, Points.Add((row, col)));
            }
            
            public bool TryAppend(int row, int col, out Path p)
            {
                if (Points.Contains((row, col)))
                {
                    p = null;
                    return false;
                }

                p = new Path(_costs, Points.Add((row, col)));
                return true;
            }
        }
    }
}
