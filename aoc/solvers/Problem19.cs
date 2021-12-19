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
    public class Problem19 : ProblemBase
    {
        public record Coord(int X, int Y, int Z)
        {
            public static Coord operator +(Coord a, Coord b)
            {
                return new Coord(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
            }
            
            public static Coord operator -(Coord a, Coord b)
            {
                return new Coord(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
            }

            public Coord Abs()
            {
                return new Coord(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));
            }

            public long Distance(Coord b)
            {
                var diff = (this - b).Abs();
                return diff.X + diff.Y + diff.Z;
            }
        }

        public class Beacon
        {
            public string Name { get; }

            public Beacon(string name, IEnumerable<Coord> beaconOffsets)
                : this(name, beaconOffsets, ImmutableList.Create(new Coord(0, 0, 0)))
            {
            }

            private Beacon(string name, IEnumerable<Coord> beaconOffsets, ImmutableList<Coord> probes)
            {
                Name = name;
                Probes = probes;
                BeaconOffsets = beaconOffsets.ToImmutableHashSet();
                _xSig = Project(p => p.X);
                _ySig = Project(p => p.Y);
                _zSig = Project(p => p.Z);
            }

            public ImmutableList<Coord> Probes { get; }
            public ImmutableHashSet<Coord> BeaconOffsets { get; }
            private Dictionary<int, int> _xSig;
            private Dictionary<int, int> _ySig;
            private Dictionary<int, int> _zSig;

            private ImmutableList<Beacon> _rotated = null;
            public Beacon Rotate(int iRotation)
            {
                if (_rotated == null)
                {
                    _rotated = Rotations.Select(
                        rotation => new Beacon(Name, BeaconOffsets.Select(o => Multiply(o, rotation)))
                    ).ToImmutableList();
                }

                return _rotated[iRotation];
            }
            public bool TryMerge(Beacon b, out Beacon mergedBeacon)
            {
                if (!TryGetOptions(_xSig, b._xSig, out var xMatches) ||
                    !TryGetOptions(_ySig, b._ySig, out var yMatches) ||
                    !TryGetOptions(_zSig, b._zSig, out var zMatches))
                {
                    mergedBeacon = null;
                    return false;
                }

                var pairs = from x in xMatches
                    from y in yMatches
                    from z in zMatches
                    orderby x.strength * y.strength * z.strength
                    select (x.position, y.position, z.position);

                foreach (var (x,y,z) in pairs)
                {
                    var bTranslate = b.BeaconOffsets.Select(b => b + new Coord(-x, -y, -z)).ToImmutableHashSet();
                    var mergedProbes = bTranslate
                        .Concat(BeaconOffsets)
                        .ToImmutableHashSet();
                    if (mergedProbes.Count + 12 > BeaconOffsets.Count + b.BeaconOffsets.Count)
                    {
                        // Signature lead to false positive
                        continue;
                    }

                    if (HasMissingBeacon(bTranslate))
                    {
                        // The overlap causes a beacon to be missed, so this is an invalid overlap
                        continue;
                    }

                    if (b.HasMissingBeacon(BeaconOffsets.Select(b => b + new Coord(x, y, z))))
                    {
                        // The overlap causes a beacon to be missed, so this is an invalid overlap
                        continue;
                    }

                    mergedBeacon = new Beacon(
                        Name + "|" + b.Name,
                        mergedProbes,
                        Probes.Concat(b.Probes.Select(p => p + new Coord(-x, -y, -z))).ToImmutableList()
                    );
                    return true;
                }

                mergedBeacon = null;
                return false;
            }

            private bool HasMissingBeacon(IEnumerable<Coord> otherBeacons)
            {
                foreach (Coord otherBeacon in otherBeacons)
                {
                    foreach (Coord probe in Probes)
                    {
                        if (CanDetect(probe, otherBeacon))
                        {
                            if (!BeaconOffsets.Contains(otherBeacon))
                            {
                                // Something in "this" should have been able to see a node in B, and couldn't
                                // SO this is invalid
                                return true;
                            }
                        }
                    }
                }

                return false;
            }

            private bool CanDetect(Coord probe, Coord beacon)
            {
                var dist = (probe - beacon).Abs();
                if (dist.X > 1000 || dist.Y > 1000 || dist.Z > 1000)
                    return false;
                return true;
            }

            private bool TryGetOptions(
                Dictionary<int, int> aSig,
                Dictionary<int, int> bSig,
                out List<(int strength, int position)> options)
            {
                options = new List<(int strength, int position)>();
                var aMin = aSig.Keys.Min();
                var aMax = aSig.Keys.Max();
                var bMin = bSig.Keys.Min();
                var bMax = bSig.Keys.Max();
                var max = bMax - aMin + 2000;
                var min = bMin - aMax - 2000;
                for (int offset = min; offset <= max; offset++)
                {
                    int hits = CountOverlap(aSig, bSig, offset);
                    if (hits >= 12)
                    {
                        options.Add((hits, offset));
                    }
                }

                if (options.Count == 0)
                {
                    return false;
                }

                return true;
            }

            private static int CountOverlap(Dictionary<int, int> a, Dictionary<int, int> b, int offset)
            {
                int hits = 0;
                foreach ((int index, int bCount) in b)
                {
                    if (a.TryGetValue(index - offset, out var aCount))
                    {
                        hits += Math.Min(aCount, bCount);
                    }
                }

                return hits;
            }

            public Dictionary<int, int> Project(Func<Coord, int> proj)
            {
                Dictionary<int, int> projection = new Dictionary<int, int>();
                foreach (var point in BeaconOffsets)
                {
                    projection.Increment(proj(point));
                }

                return projection;
            }

            public override string ToString()
            {
                return $"Scanner {Name} ({BeaconOffsets.Count} beacons)";
            }
        }

        public static ImmutableList<int[,]> Rotations = CreateRotations().ToImmutableList();

        private static IEnumerable<int[,]> CreateRotations()
        {
            List<int[,]> go = new List<int[,]>();
            var identity = new Coord(1, 2, 3);
            for(int x=0;x<4;x++)
                for(int y=0;y<4;y++)
            for (int z = 0; z < 4; z++)
            {
                var composite = MatrixMultiply(MatrixMultiply(RotX(x), RotY(y)), RotZ(z));
                var sig = Multiply(identity, composite);
                if (go.All(m => Multiply(identity, m) != sig))
                {
                    go.Add(composite);
                }
            }

            return go;
        }

        public static Coord RotatePoint(Coord c, int iRotation)
        {
            int[,] matrix = Rotations[iRotation];
            return Multiply(c, matrix);
        }

        private static Coord Multiply(Coord c, int[,] matrix)
        {
            var res = MatrixMultiply(new int[,] { { c.X, c.Y, c.Z, 1 } }, matrix);
            return new Coord(res[0, 0], res[0, 1], res[0, 2]);
        }

        public static int[,] RotX(int turns)
        { 
            return new int[,]
            {
                { 1, 0, 0, 0 },
                { 0, Cos(turns), -Sin(turns), 0 },
                { 0, Sin(turns), Cos(turns), 0 },
                { 0, 0, 0, 1 },
            };
        }

        public static int[,] RotY(int turns)
        {
            return new int[,]
            {
                { Cos(turns), 0, Sin(turns), 0 },
                { 0, 1, 0, 0 },
                { -Sin(turns), 0, Cos(turns), 0 },
                { 0, 0, 0, 1 },
            };
        }
        public static int[,] RotZ(int turns)
        {
            return new int[,]
            {
                { 1, 0, 0, 0 },
                { 0, Cos(turns), -Sin(turns), 0 },
                { 0, Sin(turns), Cos(turns), 0 },
                { 0, 0, 0, 1 },
            };
        }

        public static int Cos(int turns)
        {
            return (int)Math.Round(Math.Cos(Math.PI / 2 * turns));
        }
        
        public static int Sin(int turns)
        {
            return (int)Math.Round(Math.Sin(Math.PI / 2 * turns));
        }

        public static int[,] MatrixMultiply(int[,] a, int[,] b)
        {
            int a0 = a.GetLength(0);
            int a1 = a.GetLength(1);
            int b0 = b.GetLength(0);
            int b1 = b.GetLength(1);
            
            if (a1 != b0)
            {
                throw new ArgumentException();
            }

            int[,] c = new int[a0, b1];
            for (int i0 = 0; i0 < c.GetLength(0); i0++)
            for (int i1 = 0; i1 < c.GetLength(1); i1++)
            {
                for (int x = 0; x < a1; x++)
                {
                    c[i0, i1] += a[i0, x] * b[x, i1];
                }
            }

            return c;
        }

        protected async override Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            List<Beacon> beacons = new List<Beacon>();
            List<Coord> coords = new List<Coord>();
            string id = "NONE";
            await foreach (var line in data)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (coords.Count != 0)
                    {
                        beacons.Add(new Beacon(id, coords));
                        coords.Clear();
                    }

                    continue;
                }

                var m = Regex.Match(line, "--- scanner (.*) ---");
                if (m.Success)
                {
                    id = m.Groups[1].Value;
                    continue;
                }

                var (x,y,z) = Data.Parse<int, int, int>(line, @"^(-?\d+),(-?\d+),(-?\d+)$");
                coords.Add(new Coord(x, y, z));
            }
            if (coords.Count != 0)
            {
                beacons.Add(new Beacon(id, coords));
                coords.Clear();
            }

            var universe = beacons[0];
            beacons.RemoveAt(0);
            while (beacons.Count > 0)
            {
                Console.WriteLine($"Current beacon count: {beacons.Count}");
                universe = MergeOneBeacon(universe, beacons);
            }
            
            Console.WriteLine($"Merged all beacons, found {universe.BeaconOffsets.Count} probes");

            var distances = from a in universe.Probes
                from b in universe.Probes
                where a != b
                select a.Distance(b);
            
            Console.WriteLine($"Furthest distance: {distances.Max()}");
        }

        private static Beacon MergeOneBeacon(Beacon universe, List<Beacon> beacons)
        {
            foreach (var b in beacons)
            {
                for (int iRotation = 0; iRotation < Rotations.Count; iRotation++)
                {
                    if (universe.TryMerge(b.Rotate(iRotation), out Beacon merged))
                    {
                        beacons.Remove(b);

                        Console.WriteLine($"Merging {universe.Name} into {b.Name} ");

                        return merged;
                    }
                }
            }

            throw new ArgumentException("FAILED TO MERGE!");
        }
    }
}
