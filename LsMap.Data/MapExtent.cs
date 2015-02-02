using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsMap.Data
{
    [Serializable]
    public struct MapExtent
    {
        public double left;
        public double top;
        public double bottom;
        public double right;
        public static readonly MapExtent Empty;
        public MapExtent(double left, double top, double right, double bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }
        public MapExtent(MapPoint lefttop, MapPoint rightbottom):this(lefttop.x,lefttop.y,rightbottom.x,rightbottom.y)
        {}
        static MapExtent()
        {
            Empty=new MapExtent(0,0,0,0);
        }
        public MapPoint LeftTop
        {
            get
            {
                return new MapPoint(left, top);
            }
        }
        public MapPoint RightBottom
        {
            get
            {
                return new MapPoint(right, bottom);
            }
        }
        public double Width
        {
            get { return right - left; }
        }
        public double Height
        {
            get { return top - bottom; }
        }
        public static bool operator ==(MapExtent left, MapExtent right)
        {
            return ((left.left == right.left) && (left.right == right.right) && (left.bottom == right.bottom) && (left.top == right.top));
        }
        public static bool operator !=(MapExtent left, MapExtent right)
        {
            return !(left == right);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is MapExtent))
            {
                return false;
            }
            MapExtent extent = (MapExtent)obj;
            return (extent==this);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0},{1},{2},{3}", new object[] { this.left, this.top,this.right,this.bottom });
        }
        public static MapExtent? FromString(string extent)
        {
            if (String.IsNullOrWhiteSpace(extent))
            {
                return null;
            }
            string[] strs = extent.Split(',');
            if (strs.Length>4||strs.Length==3)
            {
                return null;
            }
            double l, t, r, b;

            if (strs.Length==1)
            {
                if (double.TryParse(strs[0],out l))
                {
                    return new MapExtent(l, l, l, l);
                }
            }
            else if (strs.Length == 2)
            {
                if (double.TryParse(strs[0].Trim(), out l) && double.TryParse(strs[1].Trim(), out t))
                {
                    return new MapExtent(l, t, l, t);
                }
            }
            else if (double.TryParse(strs[0].Trim(), out l) && double.TryParse(strs[1].Trim(), out t) && double.TryParse(strs[2].Trim(), out r) && double.TryParse(strs[3].Trim(), out b))
            {
                return new MapExtent(l, t, r, b);
            }
            return null;
        }

        public void Combine(MapExtent extent)
        {
            if (this.left>extent.left)
            {
                this.left = extent.left;
            }
            if (this.right<extent.right)
            {
                this.right = extent.right;
            }
            if (this.top<extent.top)
            {
                this.top = extent.top;
            }
            if (this.bottom>extent.bottom)
            {
                this.bottom = extent.bottom;
            }
        }
        public void Combine(MapPoint point)
        {
            MapExtent ex = MapExtent.FromPoint(point);
            Combine(ex);
        }
        public static MapExtent FromPoint(MapPoint point)
        {
            return new MapExtent(point.x, point.y, point.x, point.y);
        }
        public static MapExtent? FromPoints(List<MapPoint> points)
        {
           if (points==null||points.Count==0)
           {
               return null;
           }
           MapExtent me = FromPoint(points[0]);
           for (int i = 1; i < points.Count; i++)
           {
               me.Combine(points[i]);
           }
           return me;
        }
    }
}
