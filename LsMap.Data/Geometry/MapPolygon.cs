using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsMap.Data
{
    /// <summary>
    /// 面对象
    /// </summary>
    public class MapPolygon:Geometry
    {
        private List<MapPoint> _points = new List<MapPoint>();//首尾互连的点集合
        public List<MapPoint> Points
        {
            get { return _points; }
            internal set { _points = value; }
        }

        public override MapExtent Extent
        {
            get
            {
                return (MapExtent)MapExtent.FromPoints(_points);
            }
        }

        public override MapPoint Center
        {
            get
            {
                return this.Extent.Center;
            }
        }

        public override DatatableType GeoType
        {
            get { return DatatableType.Polygon; }
        }
    }
}
