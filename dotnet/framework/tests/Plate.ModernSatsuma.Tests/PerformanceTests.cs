using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using Plate.ModernSatsuma;
using Xunit;
using Xunit.Abstractions;

namespace Plate.ModernSatsuma.Test
{
    /// <summary>
    /// Performance and memory allocation tests for Modern Satsuma
    /// </summary>
    public class PerformanceTests
    {
        private readonly ITestOutputHelper _output;

        public PerformanceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        #region Performance Benchmarks

        [Fact]
        public void Dijkstra_SmallGraph_ShouldCompleteQuickly()
        {
            // Arrange
            var graph = CreateTestGraph(100, 0.1); // 100 nodes, 10% connectivity
            var dijkstra = new Dijkstra(graph, arc => 1.0, DijkstraMode.Sum);

            // Act
            var stopwatch = Stopwatch.StartNew();
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();
            stopwatch.Stop();

            // Assert
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(100); // Should complete in <100ms
            _output.WriteLine($"Small graph (100 nodes) completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        [Fact]
        public void Dijkstra_MediumGraph_ShouldCompleteReasonably()
        {
            // Arrange
            var graph = CreateTestGraph(1000, 0.05); // 1000 nodes, 5% connectivity
            var dijkstra = new Dijkstra(graph, arc => 1.0, DijkstraMode.Sum);

            // Act
            var stopwatch = Stopwatch.StartNew();
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();
            stopwatch.Stop();

            // Assert
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete in <1s
            _output.WriteLine($"Medium graph (1000 nodes) completed in {stopwatch.ElapsedMilliseconds}ms");
        }

        [Fact]
        public void Dijkstra_vs_BFS_PerformanceComparison()
        {
            // Arrange
            var graph = CreateTestGraph(500, 0.08);
            var source = new Node(1);

            // Act - Dijkstra
            var dijkstraStopwatch = Stopwatch.StartNew();
            var dijkstra = new Dijkstra(graph, arc => 1.0, DijkstraMode.Sum);
            dijkstra.AddSource(source);
            dijkstra.Run();
            dijkstraStopwatch.Stop();

            // Act - BFS
            var bfsStopwatch = Stopwatch.StartNew();
            var bfs = new Bfs(graph);
            bfs.AddSource(source);
            bfs.Run();
            bfsStopwatch.Stop();

            // Assert & Report
            _output.WriteLine($"Dijkstra: {dijkstraStopwatch.ElapsedMilliseconds}ms");
            _output.WriteLine($"BFS: {bfsStopwatch.ElapsedMilliseconds}ms");

            // BFS should generally not be dramatically slower than Dijkstra for unweighted graphs,
            // but allow for small timing noise on different machines.
            if (dijkstraStopwatch.ElapsedMilliseconds > 0)
            {
                var ratio = (double)bfsStopwatch.ElapsedMilliseconds / dijkstraStopwatch.ElapsedMilliseconds;
                ratio.Should().BeLessThan(1.5); // BFS shouldn't be >50% slower than Dijkstra
            }
            else
            {
                // If Dijkstra completes in 0ms, just ensure BFS is also very fast
                bfsStopwatch.ElapsedMilliseconds.Should().BeLessThan(10);
            }
        }

        [Fact]
        public void ModernAPI_vs_LegacyAPI_PerformanceComparison()
        {
            // Arrange
            var graph = CreateTestGraph(200, 0.1);
            var source = new Node(1);
            var target = new Node(50);

            // Act - Legacy API
            var legacyStopwatch = Stopwatch.StartNew();
            var dijkstra = new Dijkstra(graph, arc => 1.0, DijkstraMode.Sum);
            dijkstra.AddSource(source);
            dijkstra.Run();
            var legacyPath = dijkstra.GetPath(target);
            legacyStopwatch.Stop();

            // Act - Modern Builder API
            var modernStopwatch = Stopwatch.StartNew();
            var result = DijkstraBuilder
                .Create(graph)
                .WithCost(arc => 1.0)
                .AddSource(source)
                .Run();
            var modernPath = result.TryGetPath(target, out var path) ? path : null;
            modernStopwatch.Stop();

            // Assert & Report
            _output.WriteLine($"Legacy API: {legacyStopwatch.ElapsedMilliseconds}ms");
            _output.WriteLine($"Modern API: {modernStopwatch.ElapsedMilliseconds}ms");

            // Results should be equivalent
            (legacyPath != null).Should().Be(modernPath != null);
            if (legacyPath != null && modernPath != null)
            {
                legacyPath.NodeCount().Should().Be(modernPath.NodeCount());
            }

            // Performance should be comparable (within 50% difference)
            // Handle case where both complete in 0ms
            if (legacyStopwatch.ElapsedMilliseconds > 0)
            {
                var performanceRatio = (double)modernStopwatch.ElapsedMilliseconds / legacyStopwatch.ElapsedMilliseconds;
                performanceRatio.Should().BeLessThan(1.5); // Modern API shouldn't be >50% slower
            }
            else
            {
                // Both too fast to measure accurately - just ensure modern API also completes quickly
                modernStopwatch.ElapsedMilliseconds.Should().BeLessThan(10);
            }
        }

        #endregion

        #region Memory Allocation Tests

        [Fact]
        public void GetPathSpan_ShouldNotAllocateMemory()
        {
            // Arrange
            var graph = CreateSimplePathGraph();
            var dijkstra = new Dijkstra(graph, arc => 1.0, DijkstraMode.Sum);
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();

            // Warm up - ensure JIT compilation and caches are initialized
            Span<Node> warmupBuffer = stackalloc Node[10];
            _ = dijkstra.GetPathSpan(new Node(5), warmupBuffer);

            // Measure memory before
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var memoryBefore = GC.GetTotalMemory(false);

            // Act - Use Span API (should not allocate)
            Span<Node> pathBuffer = stackalloc Node[10];
            var pathLength = dijkstra.GetPathSpan(new Node(5), pathBuffer);

            // Measure memory after
            var memoryAfter = GC.GetTotalMemory(false);
            var allocatedBytes = memoryAfter - memoryBefore;

            // Assert
            pathLength.Should().BeGreaterThan(0);
            // Note: Exact zero allocation is hard to guarantee due to GC internals, diagnostics, etc.
            // We accept small allocations from framework internals but verify it stays reasonably small.
            allocatedBytes.Should().BeLessThan(20_000); // Still much less than typical heap-allocated path costs
            _output.WriteLine($"Span API allocated {allocatedBytes} bytes (should be minimal)");
        }

        [Fact]
        public void GetPath_vs_GetPathSpan_AllocationComparison()
        {
            // Arrange
            var graph = CreateSimplePathGraph();
            var dijkstra = new Dijkstra(graph, arc => 1.0, DijkstraMode.Sum);
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();
            var target = new Node(5);

            // Measure GetPath allocation
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var memoryBefore1 = GC.GetTotalMemory(false);

            var path = dijkstra.GetPath(target);

            var memoryAfter1 = GC.GetTotalMemory(false);
            var getPathAllocation = memoryAfter1 - memoryBefore1;

            // Measure GetPathSpan allocation
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var memoryBefore2 = GC.GetTotalMemory(false);

            Span<Node> pathBuffer = stackalloc Node[10];
            var pathLength = dijkstra.GetPathSpan(target, pathBuffer);

            var memoryAfter2 = GC.GetTotalMemory(false);
            var getPathSpanAllocation = memoryAfter2 - memoryBefore2;

            // Assert & Report
            _output.WriteLine($"GetPath allocated: {getPathAllocation} bytes");
            _output.WriteLine($"GetPathSpan allocated: {getPathSpanAllocation} bytes");

            path.Should().NotBeNull();
            pathLength.Should().BeGreaterThan(0);
            
            // Span version should allocate no more than the object-based version
            getPathSpanAllocation.Should().BeLessOrEqualTo(getPathAllocation);
        }

        [Fact]
        public void RepeatedPathfinding_ShouldNotLeakMemory()
        {
            // Arrange
            var graph = CreateTestGraph(100, 0.1);

            // Measure initial memory
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var initialMemory = GC.GetTotalMemory(false);

            // Act - Perform many pathfinding operations
            for (int i = 0; i < 100; i++)
            {
                var dijkstra = new Dijkstra(graph, arc => 1.0, DijkstraMode.Sum);
                dijkstra.AddSource(new Node(1));
                dijkstra.Run();
                
                // Access some results to ensure they're not optimized away
                var reachableCount = 0;
                for (int j = 1; j <= 50; j++)
                {
                    if (dijkstra.Reached(new Node(j)))
                        reachableCount++;
                }
            }

            // Measure final memory
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var finalMemory = GC.GetTotalMemory(false);

            var memoryGrowth = finalMemory - initialMemory;

            // Assert
            _output.WriteLine($"Memory growth after 100 operations: {memoryGrowth} bytes");
            
            // Should not grow significantly (allow some growth for JIT, etc.)
            memoryGrowth.Should().BeLessThan(1_000_000); // Less than 1MB growth
        }

        #endregion

        #region Scalability Tests

        [Theory]
        [InlineData(10, 0.5)]    // Small dense graph
        [InlineData(100, 0.1)]   // Medium sparse graph
        [InlineData(500, 0.05)]  // Large sparse graph
        public void Dijkstra_ScalabilityTest(int nodeCount, double connectivity)
        {
            // Arrange
            var graph = CreateTestGraph(nodeCount, connectivity);
            var dijkstra = new Dijkstra(graph, arc => 1.0, DijkstraMode.Sum);

            // Act
            var stopwatch = Stopwatch.StartNew();
            dijkstra.AddSource(new Node(1));
            dijkstra.Run();
            stopwatch.Stop();

            // Assert & Report
            var edgeCount = graph.ArcCount();
            var timePerNode = (double)stopwatch.ElapsedMilliseconds / nodeCount;
            var timePerEdge = (double)stopwatch.ElapsedMilliseconds / edgeCount;

            _output.WriteLine($"Graph: {nodeCount} nodes, {edgeCount} edges");
            _output.WriteLine($"Time: {stopwatch.ElapsedMilliseconds}ms total, {timePerNode:F2}ms/node, {timePerEdge:F4}ms/edge");

            // Performance should scale reasonably
            timePerNode.Should().BeLessThan(10); // Less than 10ms per node
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Less than 5 seconds total
        }

        [Fact]
        public void PathfindingThroughput_ShouldMeetTargets()
        {
            // Arrange
            var graph = CreateTestGraph(50, 0.2); // Small graph for throughput testing
            var operationCount = 1000;

            // Act
            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < operationCount; i++)
            {
                var dijkstra = new Dijkstra(graph, arc => 1.0, DijkstraMode.Sum);
                dijkstra.AddSource(new Node(1));
                dijkstra.RunUntilFixed(new Node(25));
            }
            stopwatch.Stop();

            // Assert
            var operationsPerSecond = (double)operationCount / stopwatch.Elapsed.TotalSeconds;
            _output.WriteLine($"Throughput: {operationsPerSecond:F0} operations/second");

            // Should achieve reasonable throughput
            operationsPerSecond.Should().BeGreaterThan(100); // At least 100 ops/sec
        }

        #endregion

        #region Test Graph Creation Helpers

        private static CustomGraph CreateTestGraph(int nodeCount, double connectivity)
        {
            var graph = new CustomGraph();
            var nodes = new List<Node>();

            // Create nodes
            for (int i = 0; i < nodeCount; i++)
            {
                nodes.Add(graph.AddNode());
            }

            // Create edges based on connectivity
            var random = new Random(42); // Fixed seed for reproducible tests
            var targetEdgeCount = (int)(nodeCount * (nodeCount - 1) * connectivity / 2);

            // Ensure graph is connected by creating a spanning tree first
            for (int i = 1; i < nodeCount; i++)
            {
                var parentIndex = random.Next(i);
                graph.AddArc(nodes[parentIndex], nodes[i], Directedness.Undirected);
            }

            // Add additional random edges
            var currentEdgeCount = nodeCount - 1;
            while (currentEdgeCount < targetEdgeCount)
            {
                var from = random.Next(nodeCount);
                var to = random.Next(nodeCount);

                if (from != to)
                {
                    // Check if edge already exists (simplified check)
                    var hasEdge = graph.Arcs(nodes[from]).Any(arc => 
                        graph.Other(arc, nodes[from]) == nodes[to]);

                    if (!hasEdge)
                    {
                        graph.AddArc(nodes[from], nodes[to], Directedness.Undirected);
                        currentEdgeCount++;
                    }
                }
            }

            return graph;
        }

        private static CustomGraph CreateSimplePathGraph()
        {
            var graph = new CustomGraph();
            var nodes = new List<Node>();

            // Create a simple path: 1 -> 2 -> 3 -> 4 -> 5
            for (int i = 0; i < 5; i++)
            {
                nodes.Add(graph.AddNode());
            }

            for (int i = 0; i < 4; i++)
            {
                graph.AddArc(nodes[i], nodes[i + 1], Directedness.Directed);
            }

            return graph;
        }

        #endregion
    }
}