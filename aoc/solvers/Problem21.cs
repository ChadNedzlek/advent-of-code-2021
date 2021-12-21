// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aoc.solvers
{
    public class Problem21 : ProblemBase
    {
        protected async Task Part1(IAsyncEnumerable<string> data)
        {
            List<int> playerPosition = new List<int>();
            List<int> scores = new List<int>();
            await foreach (var startingPosition in Data.As<int>(data, @".*?(\d+)$"))
            {
                playerPosition.Add(startingPosition);
                scores.Add(0);
            }

            int die = 1;
            int rolled = 0;

            int Roll()
            {
                int r = 0;
                for (int i = 0; i < 3; i++)
                {
                    rolled++;
                    r += die;
                    die = (die % 100) + 1;
                }

                return r;
            }

            for (int i = 0;scores.Max() < 1000; i = (i + 1) % playerPosition.Count)
            {
                int roll = Roll();
                int pos = playerPosition[i];
                playerPosition[i] = (pos + roll - 1) % 10 + 1;
                scores[i] += playerPosition[i];
                Console.WriteLine($"Player {i + 1} rolled {roll}, moving from {pos} to {playerPosition[i]}, for a total score of {scores[i]}");
            }

            int losingScore = scores.Min();
            Console.WriteLine($"Loser {losingScore} x {rolled} = {losingScore * rolled}");
        }

        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            List<int> playerPosition = new List<int>();
            await foreach (var startingPosition in Data.As<int>(data, @".*?(\d+)$"))
            {
                playerPosition.Add(startingPosition);
            }
            var initState = new GameState((byte)playerPosition[0], (byte)playerPosition[1], 0, 0, false);
            
            Dictionary<GameState, Wins> universe = new Dictionary<GameState, Wins>();
            Stack<GameState> unresolved = new Stack<GameState>();
            unresolved.Push(initState);

            const int winGoal = 21;
            Wins RunGame(GameState state)
            {
                Wins Calc(GameState inner)
                {
                    if (state.aScore >= winGoal)
                        return new Wins(1, 0);
                
                    if (state.bScore >= winGoal)
                        return new Wins(0, 1);

                    (byte move, int mult)[] moves = {
                        (3, 1),
                        (4, 3),
                        (5, 6),
                        (6, 7),
                        (7, 6),
                        (8, 3),
                        (9, 1)
                    };

                    Wins RunTurn(Func<byte, GameState> makeMove)
                    {
                        var wins = new Wins(0, 0);
                        List<GameState> need = new List<GameState>();
                        foreach (var move in moves)
                        {
                            GameState nextState = makeMove(move.move);
                            if (universe.TryGetValue(nextState, out var nextWins))
                            {
                                wins += nextWins * move.mult;
                            }
                            else
                            {
                                need.Add(nextState);
                            }
                        }

                        if (need.Count == 0)
                        {
                            return wins;
                        }

                        unresolved.Push(inner);
                        foreach (var n in need)
                        {
                            unresolved.Push(n);
                        }

                        return null;
                    }

                    if (inner.bTurn)
                    {
                        return RunTurn(inner.MoveB);
                    }
                    
                    return RunTurn(inner.MoveA);
                }

                if (!universe.TryGetValue(state, out var result))
                {
                    Wins value = result = Calc(state);
                    if (result != null)
                    {
                        universe.Add(state, value);
                    }
                }
                return result;
            }

            while (unresolved.Count != 0)
            {
                RunGame(unresolved.Pop());
            }
            
            Wins result = RunGame(initState);

            long best = Math.Max(result.a, result.b);
            long worst = Math.Min(result.a, result.b);
            Console.WriteLine($"Winner wins {best:N0} universes, loser in {worst:N0}, with {universe.Count:N0} states");
        }

        public record GameState(byte aPosition, byte bPosition, byte aScore, byte bScore, bool bTurn)
        {
            public GameState MoveB(byte roll)
            {
                byte b = (byte)((byte)(bPosition + roll - 1) % 10 + 1);
                return new GameState(aPosition, b, aScore, (byte)(bScore + b), false);
            }
            
            public GameState MoveA(byte roll)
            {
                byte a = (byte)((byte)(aPosition + roll - 1) % 10 + 1);
                return new GameState(a, bPosition, (byte)(aScore + a), bScore, true);
            }
        }

        public record Wins(long a, long b)
        {
            public static Wins operator *(Wins w, int count)
            {
                return new Wins(w.a * count, w.b * count);
            }
            public static Wins operator +(Wins x, Wins y)
            {
                return new Wins(x.a + y.a, x.b + y.b);
            }
        }
    }
}
