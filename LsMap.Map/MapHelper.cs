using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsMap.Map
{
    public class MapHelper
    {
        /// <summary>
        /// 获取屏幕每厘米多少像素
        /// </summary>
        /// <param name="dpiPixPcm_X">X方向 每厘米多少像素</param>
        /// <param name="dpiPixPcm_Y">Y方向 每厘米多少像素</param>
        public static void GetScreenDpiPPcm(out float dpiPixPcm_X, out float dpiPixPcm_Y)
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            dpiPixPcm_X = (float)(g.DpiX / 2.539999918d);//X每厘米多少像素
            dpiPixPcm_Y = (float)(g.DpiY / 2.539999918d);//Y每厘米多少像素
            g.Dispose();
        }
        public static Layer GetLayer(LsMap.Data.Datasource datasource, LsMap.Data.Datatable datatable)
        {
            Layer layer = null;
            switch (datatable.TableType)
            {
                case LsMap.Data.DatatableType.Null:
                    break;
                case LsMap.Data.DatatableType.Point:
                    layer = new PointLayer(datasource, datatable);
                    break;
                case LsMap.Data.DatatableType.Raster:
                    layer = new RasterLayer(datasource, datatable);
                    break;
                case LsMap.Data.DatatableType.Line:
                    layer = new LineLayer(datasource, datatable);
                    break;
                case LsMap.Data.DatatableType.Polygon:
                    layer = new PolygonLayer(datasource, datatable);
                    break;
                default:
                    break;
            }
            return layer;
        }
    }
}
