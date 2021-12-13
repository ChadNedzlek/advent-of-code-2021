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
    public class Problem3 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            // await Part1(data);
            await Part2(data);
        }

        private async Task Part2(IAsyncEnumerable<string> data)
        {
            var list = await data.ToListAsync();

            var oTarget = ParseBinary(Reduce(list, true));
            var co2Target = ParseBinary(Reduce(list, false));
            
            
            Console.WriteLine($"{oTarget} x {co2Target} = {oTarget * co2Target}");
        }

        private string Reduce(List<string> list, bool high)
        {
            list = list.ToList();
            for (int i = 0; list.Count > 1; i++)
            {
                var c = list.Count(l => l[i] == '1');
                char target = c > list.Count / 2 ? '1' : '0';
                if (c * 2 == list.Count)
                {
                    target = '1';
                }

                if (high)
                {
                    list.RemoveAll(l => l[i] != target);
                }
                else
                {
                    list.RemoveAll(l => l[i] == target);
                }
            }

            return list[0];
        }

        private static async Task Part1(IAsyncEnumerable<string> data)
        {
            int nLines = 0;
            Dictionary<long, int> counts = new Dictionary<long, int>();
            await foreach (var line in data)
            {
                nLines++;
                long val = ParseBinary(line);

                for (int i = 0; i < line.Length; i++)
                {
                    int bit = 1 << i;
                    if ((val & bit) != 0)
                    {
                        counts[bit] = counts.GetValueOrDefault(bit) + 1;
                    }
                }
            }

            long small = 0, big = 0;
            foreach (var p in counts)
            {
                if (p.Value < nLines / 2)
                {
                    small |= p.Key;
                }
                else
                {
                    big |= p.Key;
                }
            }

            Console.WriteLine($"{small} x {big} = {small * big}");
        }

        private static long ParseBinary(string line)
        {
            long val = 0;
            foreach (var c in line)
            {
                val = val << 1 | (uint)(c == '1' ? 1 : 0);
            }

            return val;
        }
    }
}