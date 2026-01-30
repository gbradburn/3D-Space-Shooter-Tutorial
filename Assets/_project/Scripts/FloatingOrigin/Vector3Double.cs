using System;
using UnityEngine;

namespace MidniteOilSoftware.SpaceShooter.FloatingOrigin
{
    [Serializable]
    public struct Vector3Double
    {
        public double x;
        public double y;
        public double z;

        public Vector3Double(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3Double(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public static Vector3Double zero => new(0, 0, 0);
        public static Vector3Double one => new(1, 1, 1);

        public double magnitude => Math.Sqrt(x * x + y * y + z * z);
        public double sqrMagnitude => x * x + y * y + z * z;

        public Vector3 ToVector3() => new((float)x, (float)y, (float)z);

        public static Vector3Double operator +(Vector3Double a, Vector3Double b) =>
            new(a.x + b.x, a.y + b.y, a.z + b.z);

        public static Vector3Double operator -(Vector3Double a, Vector3Double b) =>
            new(a.x - b.x, a.y - b.y, a.z - b.z);

        public static Vector3Double operator *(Vector3Double a, double d) =>
            new(a.x * d, a.y * d, a.z * d);

        public static Vector3Double operator /(Vector3Double a, double d) =>
            new(a.x / d, a.y / d, a.z / d);

        public static Vector3Double operator -(Vector3Double a) =>
            new(-a.x, -a.y, -a.z);

        public static implicit operator Vector3Double(Vector3 v) => new(v);

        public override string ToString() => $"({x:F2}, {y:F2}, {z:F2})";

        public string ToString(string format) => $"({x.ToString(format)}, {y.ToString(format)}, {z.ToString(format)})";
    }
}
