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
        public override MapExtent Extent
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override MapPoint Center
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override DatatableType GeoType
        {
            get { return DatatableType.Polygon; }
        }
    }
}
