using LsMap.Data;
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
        public PointLayer(Datasource datasource, Datatable datatable)
            : base(datasource, datatable)
        {
        }
    }
}
