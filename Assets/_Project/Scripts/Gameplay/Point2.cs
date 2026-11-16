using System;

namespace MacacosMeMordam.Gameplay
{
    /// <summary>
    /// Two-dimensional point independent of Unity types.
    /// Source: design/gdd/voyage-system.md.
    /// </summary>
    public readonly struct Point2 : IEquatable<Point2>
    {
        /// <summary>Creates a point from finite board-local coordinates.</summary>
        public Point2(double x, double y)
        {
            if (!IsFiniteValue(x))
            {
                throw new ArgumentOutOfRangeException(nameof(x), "Point coordinates must be finite.");
            }

            if (!IsFiniteValue(y))
            {
                throw new ArgumentOutOfRangeException(nameof(y), "Point coordinates must be finite.");
            }

            X = x;
            Y = y;
        }

        /// <summary>Gets the horizontal coordinate.</summary>
        public double X { get; }

        /// <summary>Gets the vertical coordinate.</summary>
        public double Y { get; }

        /// <summary>Gets the origin point.</summary>
        public static Point2 Zero => new Point2(0d, 0d);

        /// <summary>Calculates squared Euclidean distance without taking a square root.</summary>
        public double DistanceSquaredTo(Point2 other)
        {
            double xDifference = X - other.X;
            double yDifference = Y - other.Y;
            return (xDifference * xDifference) + (yDifference * yDifference);
        }

        /// <inheritdoc />
        public bool Equals(Point2 other) => X.Equals(other.X) && Y.Equals(other.Y);

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is Point2 other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        /// <summary>Compares two points by coordinate value.</summary>
        public static bool operator ==(Point2 left, Point2 right) => left.Equals(right);

        /// <summary>Compares two points by coordinate value.</summary>
        public static bool operator !=(Point2 left, Point2 right) => !left.Equals(right);

        internal static bool IsFiniteValue(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
