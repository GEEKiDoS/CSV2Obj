using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSV2Obj
{
    public class Vertex
    {
        public int index;
        public float x;
        public float y;
        public float z;

        public override string ToString()
        {
            return $"v {x} {y} {z}";
        }

        public static bool operator ==(Vertex left, Vertex right)
        {
            if(left.x == right.x &&
               left.y == right.y &&
               left.z == right.z)
            {
                return true;
            }

            return false;
        }

        public static bool operator !=(Vertex left, Vertex right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            if(obj is Vertex vtx)
                return vtx == this;

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class UV
    {
        public int index;
        public float u;
        public float v;

        public override string ToString()
        {
            return $"vt {u} {v}";
        }
    }

    public class Normal
    {
        public int index;
        public float x;
        public float y;
        public float z;

        public void Normalize()
        {
            var max = Math.Max(x, Math.Max(y, z));

            x /= max;
            y /= max;
            z /= max;
        }

        public override string ToString()
        {
            return $"vn {x} {y} {z}";
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
