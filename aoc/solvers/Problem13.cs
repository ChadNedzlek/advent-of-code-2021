// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace aoc.solvers
{
    public class Problem13 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            int mx = 0, my = 0;
            List<(int x, int y)> points = new List<(int x, int y)>();
            var e = data.GetAsyncEnumerator();
            while (await e.MoveNextAsync())
            {
                if (string.IsNullOrWhiteSpace(e.Current))
                {
                    break;
                }

                (int x, int y) = Data.Parse<int, int>(e.Current, @"^(\d+),(\d+)$");
                mx = Math.Max(x + 1, mx);
                my = Math.Max(y + 1, my);
                points.Add((x, y));
            }

            bool[,] paper = new bool[my, mx];
            foreach ((int x, int y) in points)
            {
                paper[y, x] = true;
            }

            //DumpPaper(paper, mx, my);

            bool first = true;

            while (await e.MoveNextAsync())
            {
                (char xy, int coord) = Data.Parse<char, int>(e.Current, @"fold along (.)=(\d+)");
                switch (xy)
                {
                    case'y':
                        for (int i = 1; coord - i >= 0 && coord + i < my; i++)
                        {
                            for (int x = 0; x < mx; x++)
                            {
                                paper[coord - i, x] |= paper[coord + i, x];
                            }
                        }

                        my = coord;
                        break;
                    case'x':
                        for (int i = 1; coord - i >= 0 && coord + i < mx; i++)
                        {
                            for (int y = 0; y < my; y++)
                            {
                                paper[y, coord - i] |= paper[y, coord + i];
                            }
                        }

                        mx = coord;
                        break;
                }

                int count = 0;
                for (int y = 0; y < my; y++)
                for (int x = 0; x < mx; x++)
                {
                    if (paper[y, x])
                        count++;
                }
                
                Console.WriteLine();
                //DumpPaper(paper, mx, my);

                if (first)
                {
                    Console.WriteLine($"After fold, {count} points");
                    first = false;
                }
            }

            DumpPaper(paper, mx, my);
        }

        private void DumpPaper(bool[,] paper, int maxX, int maxY)
        {
            for (int y = 0; y < maxY; y++)
            {
                for (int x = 0; x < maxX; x++)
                {
                    Console.Write(paper[y, x] ? "#" : " ");
                }

                Console.WriteLine();
            }
        }
    }
}
