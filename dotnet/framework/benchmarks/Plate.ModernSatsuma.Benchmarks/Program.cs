using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Plate.ModernSatsuma;

namespace Plate.ModernSatsuma.Benchmarks
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<DijkstraBenchmarks>();
        }
    }

    [MemoryDiagnoser]
    public class DijkstraBenchmarks
    {
        private IGraph _graph = null!;
        private Node _source;
        private Node _target;
        private Dictionary<Arc, double> _weights = null!;

        [GlobalSetup]
        public void Setup()
        {
            var graph = new CustomGraph();
            var random = new Random(1234);
            const int nodeCount = 200;

            var nodes = new List<Node>(nodeCount);
            for (int i = 0; i < nodeCount; i++)
            {
                nodes.Add(graph.AddNode());
            }

            _weights = new Dictionary<Arc, double>();

            // Create a sparse directed random graph
            for (int i = 0; i < nodeCount; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    int toIndex = random.Next(nodeCount);
                    if (toIndex == i)
                    {
                        continue;
                    }

                    var arc = graph.AddArc(nodes[i], nodes[toIndex], Directedness.Directed);
                    _weights[arc] = 0.1 + random.NextDouble();
                }
            }

            _graph = graph;
            _source = nodes[0];
            _target = nodes[nodeCount - 1];
        }

        [Benchmark]
        public void Dijkstra_ShortestPath()
        {
            var dijkstra = new Dijkstra(_graph, arc => _weights[arc], DijkstraMode.Sum);
            dijkstra.AddSource(_source);
            dijkstra.RunUntilFixed(_target);
        }
    }
}
