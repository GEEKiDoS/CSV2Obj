using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSV2Obj
{
    public static class ExtMethods
    {
        public static CultureInfo defaultInfo = CultureInfo.CreateSpecificCulture("en-US");

        public static string ToStdString(this float f)
        {
            return f.ToString(defaultInfo);
        }

        public static string ToStdString(this float f, string format)
        {
            return f.ToString(format, defaultInfo);
        }
    }

    public class float3
    {
        float[] data = new float[3] { 0, 0, 0 };

        public float x
        {
            get => data[0];
            set => data[0] = value;
        }

        public float y
        {
            get => data[1];
            set => data[1] = value;
        }

        public float z
        {
            get => data[2];
            set => data[2] = value;
        }

        public float this[int index]
        {
            get => data[index];
            set => data[index] = value;
        }

        public override string ToString()
        {
            return $"{x.ToStdString("N6")} {y.ToStdString("N6")} {z.ToStdString("N6")}";
        }

        public string ToString(string format)
        {
            if(format == "xy")
            {
                return $"{x.ToStdString("N6")} {y.ToStdString("N6")}";
            }

            return ToString();
        }

        public static bool operator ==(float3 left, float3 right)
        {
            for (int i = 0; i < 3; i++)
                if (left[i] != right[i])
                    return false;

            return true;
        }

        public static bool operator !=(float3 left, float3 right)
        {
            return !(left == right);
        }

        public static float3 operator *(float3 left, float3 right)
        {
            return new float3 {
                x = left.y * right.z - left.z * right.y,
                y = left.z * right.x - left.x * right.z,
                z = left.x * right.y - left.y - right.x,
            };
        }

        public static float3 operator *(float3 left, float right)
        {
            return new float3
            {
                x = left.x * right,
                y = left.y * right,
                z = left.z * right,
            };
        }

        public static float3 operator /(float3 left, float right)
        {
            return new float3
            {
                x = left.x / right,
                y = left.y / right,
                z = left.z / right
            };
        }

        public static float3 operator +(float3 left, float3 right)
        {
            return new float3
            {
                x = left.x + right.x,
                y = left.y + right.y,
                z = left.z + right.z,
            };
        }

        public static float3 operator -(float3 left, float3 right)
        {
            return new float3
            {
                x = left.x - right.x,
                y = left.y - right.y,
                z = left.z - right.z,
            };
        }

        public float Dot(float3 other)
        {
            return this.x * other.x + this.y * other.y + this.z * other.z;
        }

        public float Mod
        {
            get
            {
                return Convert.ToSingle(Math.Sqrt(x * x + y * y + z * z));
            }
        }

        public static float3 operator ~(float3 v0)
        {
            var len = v0.Mod;

            v0.x /= len;
            v0.y /= len;
            v0.z /= len;

            return v0;
        }

        public float3 Copy()
        {
            return new float3
            {
                x = x,
                y = y,
                z = z
            };
        }

        public float DistanseTo(float3 other)
        {
            var target = other - this;

            return target.Mod;
        }
    }

    public class Vertex
    {
        public int index;
        public float3 pos;

        public override string ToString()
        {
            return $"v {pos}";
        }
    }

    public class UV
    {
        public int index;
        public float3 uv;

        public override string ToString()
        {
            return $"vt {uv: xy}";
        }
    }

    public class Normal
    {
        public int index;
        public float3 dir;

        public override string ToString()
        {
            return $"vn {dir}";
        }
    }

    public class Face
    {
        public int vc;

        public Vertex[] v;
        public UV[] t;
        public Normal[] n;

        public override string ToString()
        {
            var sb = new StringBuilder("f ");

            for(int i = 0; i < vc; i++)
            {
                if(n == null)
                    sb.Append($"{v[i].index}/{t[i].index} ");
                else
                    sb.Append($"{v[i].index}/{t[i].index}/{n[i].index} ");
            }

            return sb.ToString().TrimEnd(' ');
        }
    }
}
