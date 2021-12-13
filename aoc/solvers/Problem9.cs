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
    public class Problem9 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            List<List<byte>> heights = new List<List<byte>>();
            await foreach (var line in data)
            {
                heights.Add(line.Select(c => byte.Parse(c.ToString())).ToList());
            }

            byte GetHeight(int r, int c)
            {
                if (r < 0 || c < 0)
                    return 255;
                if (r >= heights.Count)
                    return 255;
                List<byte> row = heights[r];
                if (c >= row.Count)
                    return 255;
                return row[c];
            }

            long BasinSize(List<List<byte>> sizes, int r, int c)
            {
                if (r < 0 || c < 0)
                    return 0;
                if (r >= heights.Count)
                    return 0;
                List<byte> row = heights[r];
                if (c >= row.Count)
                    return 0;
                if (row[c] == 9)
                    return 0;
                row[c] = 9;
                return 1 +
                    BasinSize(sizes, r - 1, c) +
                    BasinSize(sizes, r + 1, c) +
                    BasinSize(sizes, r, c - 1) +
                    BasinSize(sizes, r, c + 1);
            }

            int lowSum = 0;
            List<long> basinSize = new List<long>();
            for (int r = 0; r < heights.Count; r++)
            {
                for (int c = 0; c < heights[r].Count; c++)
                {
                    byte h = heights[r][c];
                    if (h < GetHeight(r - 1, c) &&
                        h < GetHeight(r + 1, c) &&
                        h < GetHeight(r, c - 1) &&
                        h < GetHeight(r, c + 1))
                    {
                        lowSum += h + 1;
                        var copy = heights.Select(h => h.ToList()).ToList();
                        basinSize.Add(BasinSize(copy, r, c));
                    }
                }
            }

            var biggest = basinSize.OrderByDescending(b => b).Take(3).ToList();
            Console.WriteLine($"Risk level: {lowSum}");
            Console.WriteLine($"Biggest 3 basins: {string.Join(", ", biggest)} = {biggest.Aggregate(1L, (x, y) => x * y)}");
        }
    }
}