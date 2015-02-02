using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace LsMap.Data
{
    /// <summary>
    /// 点（单位：米）
    /// </summary>
    [Serializable]
    public class MapPoint : Geometry
    {
        public static readonly MapPoint Empty;
        public double x;
        public double y;
        public MapPoint(double x,double y)
        {
            this.x = x;
            this.y = y;
        }
        public static bool operator ==(MapPoint left, MapPoint right)
        {
            return ((left.x == right.x) && (left.y == right.y));
        }
        public static bool operator !=(MapPoint left, MapPoint right)
        {
            return !(left == right);
        }
        public override bool Equals(object obj)
        {
            if (!(obj is MapPoint))
            {
                return false;
            }
            MapPoint point = (MapPoint)obj;
            return ((point.x == this.x) && (point.y == this.y));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{{X={0}, Y={1}}}", new object[] { this.x, this.y });
        }

        static MapPoint()
        {
            Empty = new MapPoint(0,0);
        }
        /// <summary>
        /// 地理坐标转为屏幕坐标
        /// </summary>
        /// <param name="scale">比例尺,如 1:1000000 或1/1000000</param>
        /// <param name="mapExtentLeft">地理坐标范围左边最小值(单位:米)</param>
        /// <param name="mapExtentTop">地理坐标范围上边最小值(单位:米)</param>
        /// <param name="screenLeft">屏幕坐标范围左边最小值(单位:像素)</param>
        /// <param name="screenTop">屏幕坐标范围上边最小值(单位:像素)</param>
        /// <returns>屏幕坐标</returns>
        public PointF ToScreenPoint(double scale, double mapExtentLeft, double mapExtentTop, float screenLeft, float screenTop)
        {
            float dpixX = 25;
            float dpixY = 25;
            MapHelper.GetScreenDpiPPcm(out dpixX,out dpixY);

            float dx = (float)((this.x - mapExtentLeft) * 100 * scale * dpixX + screenLeft);

            float dy = (float)((mapExtentTop - this.y) * 100 * scale * dpixY + screenTop);

            return new PointF(dx, dy);
        }

        public override MapExtent Extent
        {
            get
            {
                return new MapExtent(x, y, x, y);
            }
        }
        public override MapPoint Center
        {
            get
            {
                return new MapPoint(x, y);
            }
        }
    }
}
