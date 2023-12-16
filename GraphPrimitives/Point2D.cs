using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AVGraphPrimitives
{
    /// <summary>
    /// Represents a point in 2D space
    /// </summary>
    public class Point2D
    {
        public double X, Y;

        public Point2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static double Distance(Point2D pt1, Point2D pt2)
        {
            // Distance Between X Coordinates
            double x_distance = (pt1.X - pt2.X) * (pt1.X - pt2.X);
            // Distance Between Y Coordinates
            double y_distance = (pt1.Y - pt2.Y) * (pt1.Y - pt2.Y);
            // Distance Between 2d Points
            double total_distance = Math.Sqrt(x_distance + y_distance);
            return total_distance;
        }
    }
}
