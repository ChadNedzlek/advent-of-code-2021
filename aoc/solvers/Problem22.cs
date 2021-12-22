// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aoc.solvers
{
    public class Problem22 : ProblemBase
    {
        protected async Task Part1(IAsyncEnumerable<string> data)
        {
            bool[,,] cubes = new bool[101, 101, 101];
            await foreach (var (state, x1, x2, y1, y2, z1, z2) in Data.As<string, int, int, int, int, int, int>(data, @"^(\w+) x=(-?\d+)..(-?\d+),y=(-?\d+)..(-?\d+),z=(-?\d+)..(-?\d+)$"))
            {
                for (int x = Math.Max(-50, x1); x <= Math.Min(50, x2); x++)
                {
                    for (int y = Math.Max(-50, y1); y <= Math.Min(50, y2); y++)
                    {
                        for (int z = Math.Max(-50, z1); z <= Math.Min(50, z2); z++)
                        {
                            cubes[x + 50, y + 50, z + 50] = state == "on";
                        }
                    }
                }
            }
            
            Console.WriteLine($"Count on = {cubes.Cast<bool>().Count(x => x)}");
        }

        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            List<Descriptor> descriptors = new();
            await foreach (var (state, x1, x2, y1, y2, z1, z2) in Data.As<string, int, int, int, int, int, int>(data, @"^(\w+) x=(-?\d+)..(-?\d+),y=(-?\d+)..(-?\d+),z=(-?\d+)..(-?\d+)$"))
            {
                var newDescriptor = new Descriptor(state == "on", new R(x1, x2), new R(y1, y2), new R(z1, z2));
                descriptors = descriptors.SelectMany(d => d.Remove(newDescriptor)).ToList();
                if (newDescriptor.on)
                {
                    descriptors.Add(newDescriptor);
                }
            }

            long sum = 0;
            foreach (Descriptor d in descriptors)
            {
                long dSize = d.Size;
                if (dSize < 0)
                {
                }

                sum += dSize;
            }

            Console.WriteLine($"Total sum = {sum}");
            
        }

        public class R
        {
            public R(int start, int end)
            {
                if (end < start)
                    throw new ArgumentException();
                this.start = start;
                this.end = end;
            }

            public int start { get; init; }
            public int end { get; init; }

            public void Deconstruct(out int start, out int end)
            {
                start = this.start;
                end = this.end;
            }
        }

        public record Descriptor(bool on, R x, R y, R z)
        {
            public IEnumerable<Descriptor> Remove(Descriptor other)
            {
                if (other.x.end < x.start || other.x.start > x.end ||
                    other.y.end < y.start || other.y.start > y.end ||
                    other.z.end < z.start || other.z.start > z.end)
                {
                    yield return this;
                    yield break;
                }

                R nx = x;
                if (other.x.end < x.end)
                {
                    nx = new R(nx.start, other.x.end);
                    yield return new Descriptor(@on, new R(other.x.end + 1, x.end), y, z);
                }

                if (other.x.start > x.start)
                {
                    nx = new R(other.x.start, nx.end);
                    yield return new Descriptor(@on, new R(x.start, other.x.start - 1), y, z);
                }

                R ny = y;
                if (other.y.end < y.end)
                {
                    ny = new R(ny.start, other.y.end);
                    yield return new Descriptor(@on, nx, new R(other.y.end + 1, y.end), z);
                }

                if (other.y.start > y.start)
                {
                    ny = new R(other.y.start, ny.end);
                    yield return new Descriptor(@on, nx, new R(y.start, other.y.start - 1), z);
                }

                if (other.z.end < z.end)
                {
                    yield return new Descriptor(@on, nx, ny, new R(other.z.end + 1, z.end));
                }

                if (other.z.start > z.start)
                {
                    yield return new Descriptor(@on, nx, ny, new R(z.start, other.z.start - 1));
                }
            }

            public long Size => (x.end - x.start + 1L) * (y.end - y.start + 1L) * (z.end - z.start + 1L);

            public override string ToString()
            {
                var o = on ? "on" : "off";
                return $"{o} (size={Size}) x={x.start}..{x.end},y={y.start}..{y.end},z={z.start}..{z.end}";
            }
        }
    }
}
