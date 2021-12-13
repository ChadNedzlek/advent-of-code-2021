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
    public class Problem8 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            int count = 0;
            int sum = 0;
            await foreach (var line in data)
            {
                var (input, output) = line.Split('|').Select(x => x.Trim()).ToArray();
                var oParts = output.Split(' ').ToArray();
                var iParts = input.Split(' ').ToArray();
                var all = iParts.Concat(oParts).ToArray();
                count += oParts.Count(o => o.Length is 2 or 3 or 4 or 7);

                Dictionary<char, char> segments = new Dictionary<char, char>();
                Dictionary<int, string> map = new Dictionary<int, string>();
                map.Add(1, iParts.Single(x => x.Length == 2));
                map.Add(4, iParts.Single(x => x.Length == 4));
                map.Add(7, iParts.Single(x => x.Length == 3));
                map.Add(8, iParts.Single(x => x.Length == 7));
                map.Add(6, iParts.Where(x => x.Length == 6).Single(x => x.Intersect(map[1]).Count() == 1));

                var fourCross = map[4].Except(map[1]).ToArray();
                map.Add(0, iParts.Where(x => x.Length == 6).Single(x => x.Except(fourCross).Count() == 5));
                map.Add(9, iParts.Single(x => x.Length == 6 && x != map[6] && x != map[0]));
                map.Add(5, iParts.Where(x => x.Length == 5).Single(x => map[6].Except(x).Count() == 1));
                map.Add(3, iParts.Where(x => x.Length == 5 && x != map[5]).Single(x => x.Intersect(map[1]).Count() == 2));
                map.Add(2, iParts.Where(x => x.Length == 5 && x != map[5]).Single(x => x.Intersect(map[1]).Count() == 1));
                
                /*  aaa
                 * b   c
                 * b   c
                 *  ddd
                 * e   f
                 * e   f
                 *  ggg
                 */

                int ToValue(string s)
                {
                    return map.Single(m => m.Value.Length == s.Length && s.Intersect(m.Value).Count() == s.Length).Key;
                }

                int value = ToValue(oParts[0]) * 1000 +
                    ToValue(oParts[1]) * 100 +
                    ToValue(oParts[2]) * 10 +
                    ToValue(oParts[3]);
                sum += value;
            }
            
            Console.WriteLine($"1, 4, 7, or 8 appear {count} times");
            Console.WriteLine($"sum = {sum}");
        }
    }
}