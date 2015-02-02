using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsMap.Map
{
    /// <summary>
    /// 栅格图层
    /// </summary>
    [Serializable]
    public class RasterLayer:Layer
    {
        public RasterLayer(string datasourceName, string datatableName)
            : base(datasourceName, datatableName)
        {
        }
    }
}
