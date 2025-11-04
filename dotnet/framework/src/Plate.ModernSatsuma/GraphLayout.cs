using System;
using System.Collections.Generic;
using System.Globalization;

namespace Plate.ModernSatsuma
{
    /// <summary>
    /// An immutable point whose coordinates are double.
    /// </summary>
    public struct PointD : IEquatable<PointD>
    {
        public double X { get; }
        public double Y { get; }

        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(PointD other) => X == other.X && Y == other.Y;
        public override bool Equals(object? obj) => obj is PointD other && Equals(other);
        public static bool operator ==(PointD a, PointD b) => a.Equals(b);
        public static bool operator !=(PointD a, PointD b) => !(a == b);
        public override int GetHashCode() => HashCode.Combine(X, Y);

        public string ToString(IFormatProvider provider) => 
            string.Format(provider, "({0} {1})", X, Y);

        public override string ToString() => ToString(CultureInfo.CurrentCulture);

        /// <summary>
        /// Returns the vector sum of two points.
        /// </summary>
        public static PointD operator +(PointD a, PointD b) => new(a.X + b.X, a.Y + b.Y);

        /// <summary>
        /// Added for CLS compliancy.
        /// </summary>
        public static PointD Add(PointD a, PointD b) => a + b;

        /// <summary>
        /// Returns the Euclidean distance from another point.
        /// </summary>
        public double Distance(PointD other)
        {
            double dx = X - other.X;
            double dy = Y - other.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }

    /// <summary>
    /// Attempts to draw a graph to the plane such that a certain equilibrium is attained.
    /// Models the graph as electrically charged nodes connected with springs.
    /// Nodes are attracted by the springs and repelled by electric forces.
    ///
    /// By default, the springs behave logarithmically, and (as in reality) the electric repulsion force is inversely
    /// proportional to the square of the distance.
    /// The formulae for the attraction/repulsion forces can be configured through <see cref="SpringForce"/> and <see cref="ElectricForce"/>.
    ///
    /// The algorithm starts from a given configuration (e.g. a random placement)
    /// and lets the forces move the graph to an equilibrium.
    /// Simulated annealing is used to ensure good convergence.
    /// Each convergence step requires O(n²) time, where n is the number of the nodes.
    ///
    /// Force-directed layout algorithms work best for graphs with a few nodes (under about 100).
    /// Not only because of the running time, but also the probability of running into a poor local minimum 
    /// is quite high for a large graph. This decreases the chance that a nice arrangement is attained.
    ///
    /// Example:
    /// <code>
    /// var g = new CompleteGraph(7);
    /// var layout = new ForceDirectedLayout(g);
    /// layout.Run();
    /// foreach (var node in g.Nodes())
    ///     Console.WriteLine("Node "+node+" is at "+layout.NodePositions[node]);
    /// </code>
    /// </summary>
    public sealed class ForceDirectedLayout
    {
        /// <summary>
        /// The default initial temperature for the simulated annealing.
        /// </summary>
        public const double DefaultStartingTemperature = 0.2;

        /// <summary>
        /// The temperature where the simulated annealing should stop.
        /// </summary>
        public const double DefaultMinimumTemperature = 0.01;

        /// <summary>
        /// The ratio between two successive temperatures in the simulated annealing.
        /// </summary>
        public const double DefaultTemperatureAttenuation = 0.95;

        /// <summary>
        /// The input graph.
        /// </summary>
        public IGraph Graph { get; }

        /// <summary>
        /// The current layout, which assigns positions to the nodes.
        /// </summary>
        public Dictionary<Node, PointD> NodePositions { get; }

        /// <summary>
        /// The function defining the attraction force between two connected nodes.
        /// Arcs are viewed as springs that want to bring the two connected nodes together.
        /// The function takes a single parameter, which is the distance of the two nodes.
        ///
        /// The default force function is 2·ln(d).
        /// </summary>
        public Func<double, double> SpringForce { get; set; }

        /// <summary>
        /// The function defining the repulsion force between two nodes.
        /// Nodes are viewed as electrically charged particles which repel each other.
        /// The function takes a single parameter, which is the distance of the two nodes.
        ///
        /// The default force function is 1/d².
        /// </summary>
        public Func<double, double> ElectricForce { get; set; }

        /// <summary>
        /// The current temperature in the simulated annealing.
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// The temperature attenuation factor used during the simulated annealing.
        /// </summary>
        public double TemperatureAttenuation { get; set; }

        public ForceDirectedLayout(IGraph graph, Func<Node, PointD>? initialPositions = null)
        {
            Graph = graph;
            NodePositions = new Dictionary<Node, PointD>();
            SpringForce = d => 2 * Math.Log(d);
            ElectricForce = d => 1 / (d * d);
            TemperatureAttenuation = DefaultTemperatureAttenuation;

            Initialize(initialPositions);
        }

        /// <summary>
        /// Initializes the layout with the given one and resets the temperature.
        /// </summary>
        /// <param name="initialPositions">If null, a random layout is used.</param>
        public void Initialize(Func<Node, PointD>? initialPositions = null)
        {
            if (initialPositions == null)
            {
                // Make a random layout
                var r = new Random();
                initialPositions = _ => new PointD(r.NextDouble(), r.NextDouble());
            }

            foreach (var node in Graph.Nodes())
                NodePositions[node] = initialPositions(node);

            // Reset the temperature
            Temperature = DefaultStartingTemperature;
        }

        /// <summary>
        /// Performs an optimization step.
        /// </summary>
        public void Step()
        {
            var forces = new Dictionary<Node, PointD>();

            foreach (var u in Graph.Nodes())
            {
                PointD uPos = NodePositions[u];
                double xForce = 0, yForce = 0;

                // Attraction forces
                foreach (var arc in Graph.Arcs(u))
                {
                    PointD vPos = NodePositions[Graph.Other(arc, u)];
                    double d = uPos.Distance(vPos);
                    double force = Temperature * SpringForce(d);
                    xForce += (vPos.X - uPos.X) / d * force;
                    yForce += (vPos.Y - uPos.Y) / d * force;
                }

                // Repulsion forces
                foreach (var v in Graph.Nodes())
                {
                    if (v == u) continue;
                    PointD vPos = NodePositions[v];
                    double d = uPos.Distance(vPos);
                    double force = Temperature * ElectricForce(d);
                    xForce += (uPos.X - vPos.X) / d * force;
                    yForce += (uPos.Y - vPos.Y) / d * force;
                }

                forces[u] = new PointD(xForce, yForce);
            }

            foreach (var node in Graph.Nodes())
                NodePositions[node] += forces[node];

            Temperature *= TemperatureAttenuation;
        }

        /// <summary>
        /// Runs the algorithm until a low temperature is reached.
        /// </summary>
        public void Run(double minimumTemperature = DefaultMinimumTemperature)
        {
            while (Temperature > minimumTemperature)
                Step();
        }
    }
}
