using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsMap.Map
{
    /// <summary>
    /// 点图层
    /// </summary>
    [Serializable]
    public class PointLayer : Layer
    {
        public PointLayer(string datasourceName, string datatableName)
            : base(datasourceName, datatableName)
        {
        }
    }
}
