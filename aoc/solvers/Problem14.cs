// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aoc.solvers
{
    public class Problem14 : ProblemBase
    {
        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            var e = data.GetAsyncEnumerator();
            await e.MoveNextAsync();
            string polymer = e.Current;
            await e.MoveNextAsync();
            Dictionary<(char a, char b), char> insertions = new Dictionary<(char a, char b), char>(); 
            while (await e.MoveNextAsync())
            {
                var (a, b, ins) = Data.Parse<char, char, char>(e.Current, @"^(.)(.) -> (.)$");
                insertions.Add((a,b),ins);
            }

            Dictionary<(char a, char b, int stepCount), ImmutableDictionary<char, long>> stepCounts =
                new Dictionary<(char a, char b, int stepCount), ImmutableDictionary<char, long>>();

            ImmutableDictionary<char, long> CalculateStep(char a, char b, int count)
            {
                if (count == 0)
                    return ImmutableDictionary<char, long>.Empty;
                
                if (!insertions.TryGetValue((a, b), out var ins))
                {
                    return CalculateStep(a, b, count - 1);
                }

                if (stepCounts.TryGetValue((a, b, count), out var cached))
                {
                    return cached;
                }

                var sum = ImmutableDictionary.CreateBuilder<char,long>();
                foreach (KeyValuePair<char, long> p in CalculateStep(a, ins, count - 1))
                {
                    sum.AddOrUpdate(p.Key, p.Value, v => v + p.Value);
                }
                foreach (KeyValuePair<char, long> p in CalculateStep(ins, b, count - 1))
                {
                    sum.AddOrUpdate(p.Key, p.Value, v => v + p.Value);
                }
                sum.AddOrUpdate(ins, 1, v => v + 1);
                ImmutableDictionary<char,long> ret = sum.ToImmutable();
                stepCounts.Add((a,b,count), ret);
                return ret;
            }

            ImmutableDictionary<char, long> CalculateString(string s, int count)
            {
                var sum = ImmutableDictionary.CreateBuilder<char,long>();
                for (int i = 0; i < s.Length - 1; i++)
                {
                    foreach (var p in CalculateStep(s[i], s[i + 1], count))
                    {
                        sum.AddOrUpdate(p.Key, p.Value, v => v + p.Value);
                    }

                    sum.AddOrUpdate(s[i], 1, v => v + 1);
                }
                sum.AddOrUpdate(s[^1], 1, v => v + 1);
                return sum.ToImmutable();
            }

            {
                var tenSteps = CalculateString(polymer, 10);
                var most = tenSteps.Max(p => p.Value);
                var least = tenSteps.Min(p => p.Value);
                Console.WriteLine($"After 10 steps, {most} - {least} = {most - least}");
            }
            {
                var fourtySteps = CalculateString(polymer, 40);
                var most = fourtySteps.Max(p => p.Value);
                var least = fourtySteps.Min(p => p.Value);
                Console.WriteLine($"After 10 steps, {most} - {least} = {most - least}");
            }
        }

        private void DumpPaper(bool[,] paper, int maxX, int maxY)
        {
        }
    }
}
