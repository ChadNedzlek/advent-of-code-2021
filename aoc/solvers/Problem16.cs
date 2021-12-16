// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aoc.solvers
{
    public class Problem16 : ProblemBase
    {
        public class BitStream
        {
            public long Position { get; private set; }
            private string _buffer;
            private int _offset;
            private int _bitOffset;

            public BitStream(string buffer)
            {
                _buffer = buffer;
            }

            public int? Read()
            {
                if (_offset >= _buffer.Length)
                {
                    return null;
                }

                var c = _buffer[_offset];
                int v;
                if (c > '9')
                    v = c - 'A' + 10;
                else
                    v = c - '0';
                int bit = ((v >> (3 - _bitOffset)) & 1);
                _bitOffset++;
                Position++;
                if (_bitOffset == 4)
                {
                    _bitOffset = 0;
                    _offset++;
                }

                return bit;
            }

            public int ReadInt(int count)
            {
                int v = 0;
                for (int i = 0; i < count; i++)
                {
                    v = v * 2 + Read().Value;
                }
                return v;
            }
        }

        public abstract class Packet
        {
            public int Version { get; }
            public int TypeId { get; }

            public Packet(int version, int typeId)
            {
                Version = version;
                TypeId = typeId;
            }

            public abstract long CalculateValue();
        }

        public class LiteralPacket : Packet
        {
            public long Value { get; }

            public LiteralPacket(int version, int typeId, long value)
                : base(version, typeId)
            {
                Value = value;
            }

            public override long CalculateValue() => Value;
        }

        public class ListPacket : Packet
        {
            public ImmutableList<Packet> SubPackets { get; }

            public ListPacket(int version, int typeId, IEnumerable<Packet> subPackets)
                : base(version, typeId)
            {
                SubPackets = subPackets.ToImmutableList();
            }

            public override long CalculateValue()
            {
                switch (TypeId)
                {
                    case 0:
                        return SubPackets.Sum(p => p.CalculateValue());
                    case 1:
                        return SubPackets.Aggregate(1L, (v, p) => v * p.CalculateValue());
                    case 2:
                        return SubPackets.Min(p => p.CalculateValue());
                    case 3:
                        return SubPackets.Max(p => p.CalculateValue());
                    case 5:
                        return SubPackets[0].CalculateValue() > SubPackets[1].CalculateValue() ? 1 : 0;
                    case 6:
                        return SubPackets[0].CalculateValue() < SubPackets[1].CalculateValue() ? 1 : 0;
                    case 7:
                        return SubPackets[0].CalculateValue() == SubPackets[1].CalculateValue() ? 1 : 0;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        protected override async Task ExecuteCoreAsync(IAsyncEnumerable<string> data)
        {
            await foreach (var line in data)
            {
                ExecuteSync(line);
            }
        }

        private void ExecuteSync(string line)
        {
            Packet ReadPacket(BitStream s)
            {
                var version = s.ReadInt(3);
                var typeId = s.ReadInt(3);
                switch (typeId)
                {
                    case 4:
                        int nibble;
                        long number = 0;
                        do
                        {
                            nibble = s.ReadInt(5);
                            number = number * 16 + (nibble & 0xF);
                        } while ((nibble & 16) != 0);

                        return new LiteralPacket(version, typeId, number);
                    default:
                        int? lengthMode = s.Read();
                        if (lengthMode == 0)
                        {
                            var length = s.ReadInt(15);
                            var start = s.Position;
                            List<Packet> subPackets = new List<Packet>();
                            while (s.Position - start < length)
                            {
                                subPackets.Add(ReadPacket(s));
                            }

                            return new ListPacket(version, typeId, subPackets);
                        }
                        else
                        {
                            var length = s.ReadInt(11);
                            List<Packet> subPackets = new List<Packet>();
                            for (int i = 0; i < length; i++)
                            {
                                subPackets.Add(ReadPacket(s));
                            }
                            return new ListPacket(version, typeId, subPackets);
                        }
                }
            }

            long PrintVersionSum(Packet p, StringBuilder s)
            {
                switch (p)
                {
                    case LiteralPacket lit:
                        s.Append($"(Lv{lit.Version} = {lit.Value})");
                        return p.Version;
                    case ListPacket list:
                        var typeCode = list.TypeId switch
                        {
                            0 => "SUM",
                            1 => "MULT",
                            2 => "MIN",
                            3 => "MAX",
                            5 => "GT",
                            6 => "LT",
                            7 => "EQ"
                        };
                        s.Append($"({typeCode} v{list.Version} - [ ");
                        long sum = list.Version;
                        foreach (var sub in list.SubPackets)
                        {
                            sum += PrintVersionSum(sub, s);
                            s.Append(" ");
                        }

                        s.Append("])");
                        return sum;
                    default:
                        throw new ArgumentException();
                }
            }

            int? d;
            Console.Write(line);
            Console.Write(" ==> ");
            BitStream dump = new BitStream(line);
            while ((d = dump.Read()).HasValue)
            {
                Console.Write(d.Value);
            }
            Console.WriteLine();

            BitStream str = new BitStream(line);
            var packet = ReadPacket(str);
            StringBuilder b = new StringBuilder();
            var sum = PrintVersionSum(packet, b);
            Console.WriteLine(b);
            Console.WriteLine($"Value = {packet.CalculateValue()} (v{sum})");
        }
    }
}
