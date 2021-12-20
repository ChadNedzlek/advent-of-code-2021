// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace aoc.solvers
{
    public class Problem20 : ProblemBase
    {
        public void Grow(List<List<byte>> input, byte pad)
        {
            foreach (List<byte> bools in input)
            {
                bools.Insert(0, pad);
                bools.Insert(0, pad);
                bools.Add(pad);
                bools.Add(pad);
            }

            int newWidth = input[0].Count;
            input.Insert(0, Enumerable.Repeat(pad, newWidth).ToList());
            input.Insert(0, Enumerable.Repeat(pad, newWidth).ToList());
            input.Add(Enumerable.Repeat(pad, newWidth).ToList());
            input.Add(Enumerable.Repeat(pad, newWidth).ToList());
        }

        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            var e = data.GetAsyncEnumerator();
            await e.MoveNextAsync();
            byte[] algorithm = e.Current.Select(c => c == '#' ? (byte)1 : (byte)0).ToArray();
            await e.MoveNextAsync();
            Debug.Assert(string.IsNullOrEmpty(e.Current));
            List<List<byte>> board = new List<List<byte>>();
            while (await e.MoveNextAsync())
            {
                board.Add(e.Current.Select(c => c == '#' ? (byte)1 : (byte)0).ToList());
            }

            const int iterations = 50;
            byte pad = 0;
            for (int i = 0; i < iterations; i++)
            {
                //DumpMap(board);
                //Console.WriteLine();
                Grow(board, pad);
                pad ^= algorithm[0];
                List<List<byte>> next = board.Select(b => Enumerable.Repeat(pad, b.Count).ToList()).ToList();
                for (int r = 1; r < board.Count-1; r++)
                {
                    List<byte> row = board[r];
                    for (int c = 1; c < row.Count - 1; c++)
                    {
                        int index = 0;
                        for (int br = r - 1; br <= r + 1; br++)
                        {
                            for (int bc = c - 1; bc <= c + 1; bc++)
                            {
                                index = (index << 1) + board[br][bc];
                            }
                        }

                        next[r][c] = algorithm[index];
                    }
                }

                board = next;
            }
            DumpMap(board);

            long lit = board.Sum(b => b.Sum(c => (long)c));
            Console.WriteLine($"After 2 iterations, {lit} lit cells");
            
        }

        private void DumpMap(List<List<byte>> board)
        {
            foreach (List<byte> row in board)
            {
                foreach (byte b in row)
                {
                    Console.Write(b == 1 ? '#' : '.');
                }

                Console.WriteLine();
            }
        }
    }
}
