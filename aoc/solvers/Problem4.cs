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
    public class Problem4 : ProblemBase
    {
        public class BingoBoard
        {
            public readonly int [,] Board;
            private readonly bool[,] _called = new bool[5, 5];

            private BingoBoard(int[,] board)
            {
                Board = board;
            }

            private int Hits()
            {
                return _called.Cast<bool>().Count(c => c);
            }

            public bool TryCall(int target, out int score)
            {
                int before = Hits();
                for(int r=0;r<5;r++)
                    for(int c=0;c<5;c++)
                        if (Board[r, c] == target)
                            _called[r, c] = true;
                int after = Hits();
                if (after > before + 1)
                {
                }

                IEnumerable<bool> Row(int i)
                {
                    for (int c = 0; c < 5; c++)
                        yield return _called[i, c];
                }
                IEnumerable<bool> Col(int i)
                {
                    for (int r = 0; r < 5; r++)
                        yield return _called[r, i];
                }

                score = 0;
                for (int i0 = 0; i0 < Board.GetLength(0); i0++)
                for (int i1 = 0; i1 < Board.GetLength(1); i1++)
                {
                    if (!_called[i0, i1])
                        score += Board[i0, i1];
                }

                return //Diag().All(x => x) ||
                    //OffDiag().All(x => x) ||
                    Enumerable.Range(0, 5).Any(i => Row(i).All(x => x) || Col(i).All(x => x));

            }

            public static async Task<BingoBoard> ReadAsync(IAsyncEnumerator<string> data)
            {
                int[,] b = new int[5, 5];
                for (int i = 0; i < 5; i++)
                {
                    int[] cs = data.Current.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
                    for (int c = 0; c < cs.Length; c++)
                    {
                        b[i, c] = cs[c];
                    }

                    await data.MoveNextAsync();
                }

                return new BingoBoard(b);
            }
        }

        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            var e = data.GetAsyncEnumerator();
            await e.MoveNextAsync();
            var calls = e.Current.Split(',').Select(int.Parse).ToArray();
            await e.MoveNextAsync();
            List<BingoBoard> boards = new List<BingoBoard>();
            while (await e.MoveNextAsync())
            {
                boards.Add(await BingoBoard.ReadAsync(e));
            }

            bool won = false;

            foreach (var c in calls)
            {
                foreach (BingoBoard b in boards.ToList())
                {
                    if (b.TryCall(c, out var score))
                    {
                        if (!won)
                        {
                            Console.WriteLine($"First: {c} x {score} = {c * score}  (Sig {b.Board[0,0]})");
                            won = true;
                        }

                        if (boards.Count == 1)
                        {
                            Console.WriteLine($"Last: {c} x {score} = {c * score}  (Sig {b.Board[0,0]})");
                        }

                        boards.Remove(b);
                    }
                }
            }
        }
    }
}