// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace aoc.solvers
{
    public class Problem12 : ProblemBase
    {
        public class Node
        {
            public List<Node> Connections { get; } = new List<Node>();

            public Node(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            var nodes = new Dictionary<string, Node>();
            await foreach (var line in data)
            {
                var nodeNames = line.Split('-');
                if (!nodes.TryGetValue(nodeNames[0], out var a))
                {
                    nodes.Add(nodeNames[0], a = new Node(nodeNames[0]));
                }

                if (!nodes.TryGetValue(nodeNames[1], out var b))
                {
                    nodes.Add(nodeNames[1], b = new Node(nodeNames[1]));
                }

                a.Connections.Add(b);
                b.Connections.Add(a);
            }

            List<ImmutableList<Node>> validRoutes = new List<ImmutableList<Node>>();
            Queue<ImmutableList<Node>> pendingRoutes = new Queue<ImmutableList<Node>>();
            pendingRoutes.Enqueue(ImmutableList.Create(nodes["start"]));
            while (pendingRoutes.TryDequeue(out var path))
            {
                Node node = path[^1];
                foreach (var n in node.Connections)
                {
                    if (n.Name == "end")
                    {
                        validRoutes.Add(path.Add(n));
                        continue;
                    }
                    if (n.Name == "start")
                    {
                        continue;
                    }

                    var candidate = path.Add(n);

                    if (IsValid(candidate))
                    {
                        pendingRoutes.Enqueue(path.Add(n));
                    }
                }
            }

            Console.WriteLine($"{validRoutes.Count} valid paths");
        }
        
        private bool IsValid(ImmutableList<Node> path)
        {
            string str = string.Join(",", path.Select(n => n.Name));
            var smallCaveRevisits = path.Where(n => Char.IsLower(n.Name[0])).GroupBy(n => n.Name).Select(g => g.Count());
            if (smallCaveRevisits.Any(r => r > 2))
                return false;
            if (smallCaveRevisits.Count(r => r == 2) > 1)
                return false;
            return true;
        }
    }
}
