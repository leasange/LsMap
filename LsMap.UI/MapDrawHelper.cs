using LsMap.Data;
using LsMap.Map;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsMap.UI
{
    internal class MapDrawHelper
    {
        public static Datatable GetTable(Workspace.Workspace ws,Layer layer)
        {
            LsMap.Data.Datasource ds = ws.GetDatasource(layer.DatasourceName);
            if (ds != null)
            {
                LsMap.Data.Datatable dt = ws.GetDatatable(ds, layer.DatatableName);
                return dt;
            }
            return null;
        }
        public static PointF ToScreenPoint(MapPoint mapPoint,MapExtent extent,int width,int height)
        {
            float fl = (float)((mapPoint.x - extent.left) / extent.Width * width);
            float ft = (float)((extent.top - mapPoint.y) / extent.Height * height);
            return new PointF(fl, ft);
        }
    }
}
