using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsMap.Data
{
    /// <summary>
    /// 线对象
    /// </summary>
    public class MapLine:Geometry
    {
        private List<MapPoint> _points = new List<MapPoint>();
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
            get { return DatatableType.Line; }
        }
    }
}
