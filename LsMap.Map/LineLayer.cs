using LsMap.Data;
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
        public LineLayer(Datasource datasource, Datatable datatable)
            : base(datasource, datatable)
        {
        }
    }
}
