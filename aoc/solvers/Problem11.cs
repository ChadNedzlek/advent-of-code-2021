// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace aoc.solvers
{
    public class Problem11 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            int[,] sea = new int[10, 10];
            int row = 0;
            await foreach (var line in data)
            {
                for (int c = 0; c < 10; c++)
                {
                    sea[row, c] = line[c] - '0';
                }

                row++;
            }


            long flashCount = 0;
            for(int i=0;;i++)
            {
                // for (int p0 = 0; p0 < sea.GetLength(0); p0++)
                // {
                //     for (int p1 = 0; p1 < sea.GetLength(1); p1++)
                //     {
                //         Console.Write(sea[p0, p1]);
                //     }
                //     Console.WriteLine();
                // }
                // Console.WriteLine();

                int[,] copy = (int[,])sea.Clone();
                bool[,] flashed = new bool[10,10];

                void Inc(int r, int c)
                {
                    if (r is < 0 or >= 10)
                        return;
                    if (c is < 0 or >= 10)
                        return;
                    copy[r, c]++;
                }
                
                for (int f0 = 0; f0 < copy.GetLength(0); f0++)
                for (int f1 = 0; f1 < copy.GetLength(1); f1++)
                {
                    copy[f0, f1]++;
                }
                bool addedFlash;
                do
                {
                    addedFlash = false;
                    for (int f0 = 0; f0 < copy.GetLength(0); f0++)
                    for (int f1 = 0; f1 < copy.GetLength(1); f1++)
                    {
                        if (copy[f0, f1] > 9 && !flashed[f0, f1])
                        {
                            flashCount++;
                            flashed[f0, f1] = true;
                            addedFlash = true;
                            Inc(f0 - 1, f1 - 1);
                            Inc(f0 - 1, f1);
                            Inc(f0 - 1, f1 + 1);
                            Inc(f0, f1 - 1);
                            Inc(f0, f1 + 1);
                            Inc(f0 + 1, f1 - 1);
                            Inc(f0 + 1, f1);
                            Inc(f0 + 1, f1 + 1);
                        }
                    }
                } while (addedFlash);

                if (flashed.Cast<bool>().All(x => x))
                {
                    Console.WriteLine($"First flash at step {i + 1}");
                    return;
                }

                for (int f0 = 0; f0 < copy.GetLength(0); f0++)
                for (int f1 = 0; f1 < copy.GetLength(1); f1++)
                {
                    if (flashed[f0, f1])
                    {
                        copy[f0, f1] = 0;
                    }
                }

                sea = copy;
            }
            
            Console.WriteLine($"{flashCount} flashes");
        }
    }
}