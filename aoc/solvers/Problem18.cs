// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace aoc.solvers
{
    public class Problem18 : ProblemBase
    {
        public abstract class Node
        {
            public PairNode Parent { get; private set; }

            public void SetParent(PairNode n)
            {
                Debug.Assert(n != null);
                Parent = n;
            }

            public static Node Parse(string description)
            {
                ReadOnlySpan<char> span = description.AsSpan();
                return Parse(ref span);
            }

            private static Node Parse(ref ReadOnlySpan<char> span)
            {
                if (span[0] == '[')
                {
                    span = span[1..];
                    var left = Parse(ref span);
                    Debug.Assert(span[0] == ',');
                    span = span[1..];
                    var right = Parse(ref span);
                    Debug.Assert(span[0] == ']');
                    span = span[1..];
                    return new PairNode(left, right);
                }

                var value = span[0] - '0';
                span = span[1..];
                return new LiteralNode(value);
            }

            public Node Add(Node n)
            {
                return new PairNode(this.Clone(), n.Clone()).Reduced();
            }

            public abstract Node Clone();

            public abstract long Magnitude();

            public static Node operator +(Node a, Node b)
            {
                if (a == null)
                    return b;
                return a.Add(b);
            }

            public Node Reduced()
            {
                Node n = this;
                while (true)
                {
                    if (n.TryExplode(out var exploded))
                    {
                        //Console.WriteLine("  -X-  " + n);
                        n = exploded;
                        n.Prep();
                        continue;
                    }
                    
                    if (n.TrySplit(out var split))
                    {
                        //Console.WriteLine("  -S-  " + n);
                        n = split;
                        continue;
                    }

                    break;
                }

                return n;
            }

            public abstract bool TrySplit(out Node node);
            public abstract bool TryExplode(out Node node);

            public abstract void Prep();
        }

        public class LiteralNode : Node
        {
            public LiteralNode(int value)
            {
                OldValue = Value = value;
            }

            public int Value { get; private set; }
            public int OldValue { get; private set; }

            public void AddValue(int v)
            {
                OldValue = Value;
                Value += v;
            }

            public override void Prep()
            {
                OldValue = Value;
            }

            public override Node Clone() => new LiteralNode(Value);

            public override long Magnitude() => Value;

            public override bool TrySplit(out Node node)
            {
                if (Value < 10)
                {
                    node = this;
                    return false;
                }

                var l = Value / 2;
                var r = Value - l;
                node = new PairNode(new LiteralNode(l), new LiteralNode(r));
                return true;
            }

            public override bool TryExplode(out Node node)
            {
                node = this;
                return false;
            }

            public override string ToString()
            {
                return OldValue.ToString();
            }
        }

        public class PairNode : Node
        {
            public Node Left { get; }
            public Node Right { get; }

            public PairNode(Node left, Node right)
            {
                Left = left;
                Right = right;
                Left.SetParent(this);
                Right.SetParent(this);
            }

            public override Node Clone() => new PairNode(Left.Clone(), Right.Clone());

            public override long Magnitude() => 3 * Left.Magnitude() + 2 * Right.Magnitude();

            public override bool TrySplit(out Node node)
            {
                if (Left.TrySplit(out var l))
                {
                    node = new PairNode(l, Right);
                    return true;
                }

                if (Right.TrySplit(out var r))
                {
                    node = new PairNode(Left, r);
                    return true;
                }

                node = this;
                return false;
            }

            public override bool TryExplode(out Node node)
            {
                int depth = 0;
                Node n = this;
                while (n != null)
                {
                    n = n.Parent;
                    depth++;
                }

                if (depth <= 4 || Left is PairNode || Right is PairNode)
                {
                    if (Left.TryExplode(out var l))
                    {
                        node = new PairNode(l, Right);
                        return true;
                    }
                    
                    if (Right.TryExplode(out var r))
                    {
                        node = new PairNode(Left, r);
                        return true;
                    }

                    node = this;
                    return false;
                }

                WalkLeft()?.AddValue(((LiteralNode)Left).Value);
                WalkRight()?.AddValue(((LiteralNode)Right).Value);
                node = new LiteralNode(0);
                return true;
            }

            public override void Prep()
            {
                Left.Prep();
                Right.Prep();
            }

            private LiteralNode WalkRight()
            {
                if (Parent == null)
                    return null;
                
                if (Parent.Right == this)
                {
                    return Parent.WalkRight();
                }

                Node p = Parent.Right;
                while (p is PairNode pair)
                {
                    p = pair.Left;
                }

                return (LiteralNode)p;
            }

            private LiteralNode WalkLeft()
            {
                if (Parent == null)
                    return null;
                
                if (Parent.Left == this)
                {
                    return Parent.WalkLeft();
                }

                Node p = Parent.Left;
                while (p is PairNode pair)
                {
                    p = pair.Right;
                }

                return (LiteralNode)p;
            }

            public override string ToString()
            {
                return $"[{Left},{Right}]";
            }
        }

        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            List<Node> current = new List<Node>();
            await foreach (string line in data)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    TestSet(current);

                    current.Clear();
                    continue;
                }

                Node n = Node.Parse(line);
                current.Add(n);
            }

            if (current != null)
            {
                TestSet(current);
            }
        }

        private static void TestSet(List<Node> current)
        {
            var s = current.Aggregate((a, b) => a + b);
            Console.WriteLine($"M: {s.Magnitude()} <<< {s}");
            long bestMag = 0;
            Node bestMagNode = null;
            foreach (var a in current)
            {
                foreach (var b in current)
                {
                    if (a == b)
                    {
                        continue;
                    }

                    Node sum = (a + b);
                    long m = sum.Magnitude();
                    if (m > bestMag)
                    {
                        bestMag = m;
                        bestMagNode = sum;
                    }
                }
            }
            Console.WriteLine($"Best: {bestMag} <<< {bestMagNode}");

            var sorted = from a in current
                from b in current
                where a != b
                let x = a + b
                orderby x.Magnitude() descending
                select x;
            var best = sorted.First();
            
            Console.WriteLine($"L: {best.Magnitude()} <<< {best}");
        }
    }
}
