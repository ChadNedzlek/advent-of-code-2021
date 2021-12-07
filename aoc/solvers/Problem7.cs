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
    public class Problem7 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            var line = await data.FirstAsync();
            var crabs = line.Split(',').Select(int.Parse).ToArray();

            var max = crabs.Max();

            int lFuel = -1;
            int lPoint = -1;
            int mFuel = -1;
            int mPoint = -1;
            for (int i = 0; i < max; i++)
            {
                int f = crabs.Sum(x => Math.Abs(x - i));
                int m = crabs.Sum(
                    x =>
                    {
                        var n = Math.Abs(x - i);
                        return n * (n + 1) / 2;
                    }
                );
                if (lFuel == -1 || f < lFuel)
                {
                    lFuel = f;
                    lPoint = i;
                }

                if (mFuel == -1 || m < mFuel)
                {
                    mFuel = m;
                    mPoint = i;
                }
            }
            Console.WriteLine($"At point {lPoint} linear fuel cost is {lFuel}");
            Console.WriteLine($"At point {mPoint} multiply fuel cost is {mFuel}");
        }
    }
}