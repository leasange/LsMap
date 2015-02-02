using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsMap.Map
{
    /// <summary>
    /// 线图层
    /// </summary>
    [Serializable]
    public class LineLayer : Layer
    {
        public LineLayer(string datasourceName, string datatableName)
            : base(datasourceName, datatableName)
        {
        }
    }
}
