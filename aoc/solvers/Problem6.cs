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
    public class Problem6 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            var line = await data.FirstAsync();
            var fish = line.Split(',').Select(int.Parse).ToArray();

            Dictionary<int, long> cache = new Dictionary<int, long>();
            long FishCount(int fish, int at, int target)
            {
                if (at >= target)
                {
                    return 1;
                }

                if (fish == 0)
                {
                    if (!cache.TryGetValue(target - at, out var cached))
                    {
                        cached = FishCount(8, at + 1, target) + FishCount(6, at + 1, target);
                        cache.Add(target - at, cached);
                    }

                    return cached;
                }

                return FishCount(0, at + fish, target);
            }

            var day18 = fish.Select(f => FishCount(f, 0, 18)).ToList();
            var day80 = fish.Select(f => FishCount(f, 0, 80)).ToList();
            var day256 = fish.Select(f => FishCount(f, 0, 256)).ToList();
            Console.WriteLine($"18 day Sum({string.Join(", ", day18)}) = {day18.Sum()}");
            Console.WriteLine($"80 day Sum({string.Join(", ", day80)}) = {day80.Sum()}");
            Console.WriteLine($"256 day Sum({string.Join(", ", day256)}) = {day256.Sum()}");
        }
    }
}