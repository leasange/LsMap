using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsMap.Data
{
    /// <summary>
    /// 几何图型
    /// </summary>
    [Serializable]
    public abstract class Geometry
    {
        public abstract MapExtent Extent { get; }
        public abstract MapPoint Center { get; }
        public abstract DatatableType GeoType { get; }
    }
}
