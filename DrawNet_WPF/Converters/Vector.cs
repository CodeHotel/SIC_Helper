using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DrawNet_WPF.Converters
{
    [Serializable]
    [TypeConverter(typeof(VectorConverter))]
    public class Vector
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vector() { }

        public Vector(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Vector operator +(Vector a, Vector b) => new Vector(a.X + b.X, a.Y + b.Y);
        public static Vector operator -(Vector a, Vector b) => new Vector(a.X - b.X, a.Y - b.Y);
        public static Vector operator *(Vector a, double factor) => new Vector(a.X * factor, a.Y * factor);
        public static Vector operator *(Vector a, Vector b) => new Vector(a.X * b.X, a.Y * b.Y);
        public static Vector operator /(Vector a, double divisor) => new Vector(a.X / divisor, a.Y / divisor);
        public static Vector operator /(Vector a, Vector b) => new Vector(a.X / b.X, a.Y / b.Y);
    }


}
