// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace aoc.solvers
{
    public class Problem5 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            int[,] board = new int[1000, 1000];
            await foreach (var line in Data.As<int, int, int, int>(data, @"^(\d+),(\d+) -> (\d+),(\d+)$"))
            {
                var (x1, y1, x2, y2) = line;
                if (x1 > x2)
                {
                    (x1, x2) = (x2, x1);
                    (y1, y2) = (y2, y1);
                }
                else if (x1 == x2 && y1 > y2)
                {
                    (x1, x2) = (x2, x1);
                    (y1, y2) = (y2, y1);
                }

                var (x, y) = (x1, y1);

                void DX(int i)
                {
                    x = x1 + i;
                }
                void DY(int i)
                {
                    y = y1 + i;
                }
                void DDown(int i)
                {
                    DX(i);
                    DY(i);
                }
                void DUp(int i)
                {
                    DX(i);
                    DY(-i);
                }

                Action<int> go;
                if (x1 == x2)
                {
                    go = DY;
                }
                else if (y1 == y2)
                {
                    go = DX;
                }
                else if(y2 > y1)
                {
                    go = DDown;
                }
                else
                {
                    go = DUp;
                }

                for(int i=0; x != x2 || y != y2; go(++i))
                {
                    board[x, y]++;
                }
                board[x, y]++;
            }

            var intersection = board.Cast<int>().Count(c => c > 1);
            
            Console.WriteLine($"{intersection} overlaps");
        }
    }
}