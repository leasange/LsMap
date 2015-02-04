using LsMap.Data;
using LsMap.Map;
using LsMap.Workspace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LsMap
{
    public partial class FrmMain : Form
    {
        private Workspace.Workspace workspace;
        public FrmMain()
        {
            InitializeComponent();
            InitMap();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            this.mapControl.Workspace = this.workspace;
            this.mapControl.OpenMap(0);
        }

        private void InitMap()
        {
            workspace = new Workspace.Workspace();
//             WorkspaceConnection con = new WorkspaceConnection("E:\\lsmap.lsws",null,null);
//             this.workspace.Open(con);
//             return;

            //添加数据源
            LsMap.Data.Datasource dsrc = new LsMap.Data.FileDatasource(Application.StartupPath + @"\map.lsdb","test");
            dsrc.Open();
            this.workspace.Datasources.Add(dsrc);

            //添加地图
            Map.Map map = new Map.Map("test");
            map.Datasources = this.workspace.Datasources;
            map.DefaultExtent = new LsMap.Data.MapExtent(-50, 150, 250, -50);

            PointLayer pointlayer = new PointLayer("test", "point");
            map.Layers.Add(pointlayer);
            pointlayer.Map = map;

            LineLayer linelayer = new LineLayer("test", "line");
            map.Layers.Add(linelayer);
            linelayer.Map = map;

            PolygonLayer polygonlayer = new PolygonLayer("test", "polygon");
            map.Layers.Add(polygonlayer);
            polygonlayer.Map = map;

            RasterLayer rasterLayer = new RasterLayer("test", "test");
            map.Layers.Add(rasterLayer);
            rasterLayer.Map = map;


            this.workspace.Maps.Add(map);  
        }

        private void mapControl_MouseMove(object sender, MouseEventArgs e)
        {
            MapPoint mp = mapControl.ToMapPoint(e.Location);
            tssbMousePosition.Text = "x=" + mp.x.ToString(".0000") + ",y=" + mp.y.ToString(".0000");
        }

        private void mapControl_ScaleChanged(object sender, EventArgs e)
        {
            tsslMapScale.Text = "1:"+(1/mapControl.Scale).ToString(".00");
        }

        private void tsmiSaveWorkspace_Click(object sender, EventArgs e)
        {
            this.workspace.SaveAsFile("E:\\lsmap.lsws");
        }

        private List<MapPoint> _mapPoints = new List<MapPoint>();
        private void mapControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button== MouseButtons.Left)
            {
                MapPoint mp = mapControl.ToMapPoint(e.Location);
                _mapPoints.Add(mp);
            }
            else
            {
                string temp = "";
                foreach (MapPoint item in _mapPoints)
                {
                    temp += item.x + "," + item.y + ";";
                }
                _mapPoints.Clear();
                Console.WriteLine(temp);
            }
        }
    }
}
