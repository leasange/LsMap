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

    }
}
