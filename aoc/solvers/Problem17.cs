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
    public class Problem17 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            (int x1, int x2, int y1, int y2) = await Data.As<int, int, int, int>(data, @"target area: x=(-?\d+)..(-?\d+), y=(-?\d+)..(-?\d+)").FirstAsync();
            int xt = (int)Math.Sqrt(x1 * 2);
            int yt = -y1 - 1;
            int height = yt * (yt + 1) / 2;
            Console.WriteLine($"Guess: {xt},{yt}, height: {height}");
            Console.WriteLine();

            int found = 0;
            for (int cx = 1; cx <= x2; cx++)
            {
                for (int cy = y1; cy <= yt; cy++)
                {
                    int dx = cx;
                    int dy = cy;
                    int x = 0;
                    int y = 0;
                    while (x <= x2 && y >= y1)
                    {
                        x += dx;
                        y += dy;
                        dx = Math.Max(0, dx - 1);
                        dy--;
                        
                        if (x >= x1 && x <= x2 && y >= y1 && y <= y2)
                        {
                            Console.WriteLine($"Found: {cx},{cy}");
                            found++;
                            break;
                        }
                    }
                }
            }
            Console.WriteLine();
            Console.WriteLine($"Found {found} options");
        }
    }
}
