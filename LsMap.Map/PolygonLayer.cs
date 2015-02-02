using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsMap.Map
{
    /// <summary>
    /// 面图层
    /// </summary>
    [Serializable]
    public class PolygonLayer : Layer
    {
        public PolygonLayer(string datasourceName, string datatableName)
            : base(datasourceName, datatableName)
        {
        }
    }
}
