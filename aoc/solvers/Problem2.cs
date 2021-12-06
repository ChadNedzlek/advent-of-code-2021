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
    public class Problem2 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            int x = 0;
            int y = 0;
            int aimedY = 0;
            await foreach (var line in data)
            {
                var m = Regex.Match(line, @"^(\w+) (\d+)$");
                var d = int.Parse(m.Groups[2].Value);
                switch (m.Groups[1].Value)
                {
                    case "forward":
                        x += d;
                        aimedY += y * d;
                        break;
                        case "down":
                            y += d;
                            break;
                        case "up":
                            y -= d;
                            break;
                }
            }
            
            Console.WriteLine($"{x} x {y} = {x * y}");
            Console.WriteLine($"Aimed: {x} x {aimedY} = {x * aimedY}");
        }
    }
}