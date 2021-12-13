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
    public class Problem10 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            long points = 0;
            var completions = new List<long>();
            await foreach (var line in data)
            {
                string reduced = line;
                int len;
                do
                {
                    len = reduced.Length;
                    reduced = reduced.Replace("()", "").Replace("[]", "").Replace("{}", "").Replace("<>", "");
                } while (len != reduced.Length);

                bool corrupted = false;
                foreach (var c in reduced)
                {
                    int cPoint = c switch
                    {
                        ')' => 3,
                        ']' => 57,
                        '}' => 1197,
                        '>' => 25137,
                        _ => 0
                    };
                    if (cPoint != 0)
                    {
                        corrupted = true;
                        points += cPoint;
                        break;
                    }
                }

                if (!corrupted)
                {
                    long completionPoints = 0;
                    foreach (var c in reduced.Reverse())
                    {
                        completionPoints *= 5;
                        completionPoints += c switch
                        {
                            '(' => 1,
                            '[' => 2,
                            '{' => 3,
                            '<' => 4,
                            _ => throw new ArgumentOutOfRangeException()
                        };
                    }

                    completions.Add(completionPoints);
                }
            }

            completions.Sort();
            var middle = completions[completions.Count / 2];
            
            Console.WriteLine($"Mismatched points = {points}");
            Console.WriteLine($"Completion points = {middle}");
        }
    }
}