using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LsMap.Data
{
    [Serializable]
    public class FileDatasource:Datasource
    {
        private string _datasourcePath = null;
        public string DatasourcePath
        {
            get { return _datasourcePath; }
        }
        //打开文件型数据源
        public FileDatasource(string dataSrcPath)
            : base(DatasourceType.File)
        {
            _datasourcePath = dataSrcPath;
            DoOpenFileDataSrc();
        }
        //打开文件型数据源
        public FileDatasource(string dataSrcPath, string name):base(DatasourceType.File)
        {
            _datasourcePath = dataSrcPath;
            this.Name = name;
            DoOpenFileDataSrc();
        }
        internal void DoOpenFileDataSrc()
        {
            //数据库中所有数据
            string[] allfiles = System.IO.Directory.GetFiles(_datasourcePath);
            if (allfiles==null||allfiles.Length==0)
            {
                return;
            }
            //表文件
            IEnumerable<string> lsdtfiles = allfiles.Where(delegate(string s)
            {
                return s.EndsWith(".lsdt");
            });
            if (lsdtfiles.Count() == 0)
            {
                return;
            }
            foreach (string item in lsdtfiles)
            {
                XmlDocument document = new XmlDocument();
                document.Load(item);

                XmlNode root = document.SelectSingleNode("datas");
                if (root==null)
                {
                    continue;
                }

                XmlAttribute attritype = root.Attributes["type"];
                if (attritype == null)
                {
                    continue;
                }

                string datatype = attritype.Value;

                DatatableType type= DatatableType.Null;
                if (!Enum.TryParse<DatatableType>(datatype,true, out type))
                {
                    continue;
                }
                string tablename= System.IO.Path.GetFileNameWithoutExtension(item);
                Datatable dataTable = new Datatable(tablename,type);
                this.Tables.Add(dataTable);
                XmlNodeList nodelist = document.SelectNodes("datas/data");
               
                if (nodelist != null && nodelist.Count > 0)
                {
                    DoParserFileTableData(nodelist, dataTable, item);
                }
            }
        }

        internal void DoParserFileTableData(XmlNodeList nodelist, Datatable dataTable, string dataTableFilePath)
        {
            switch (dataTable.TableType)
            {
                case DatatableType.Null:
                    break;
                case DatatableType.Point:
                    DoParserPointData(nodelist, dataTable, dataTableFilePath);
                    break;
                case DatatableType.Raster:
                    DoParserRasterData(nodelist, dataTable, dataTableFilePath);
                    break;
                case DatatableType.Line:
                    DoParserLineData(nodelist, dataTable, dataTableFilePath);
                    break;
                default:
                    break;
            }
        }
        internal void DoParserPointData(XmlNodeList nodelist, Datatable dataTable, string dataTableFilePath)
        {
            foreach (XmlNode node in nodelist)
            {
                int id;
                MapExtent extent;
                if (!DoParserIdAndExtent(node, out id, out extent))
                {
                    continue;
                }
                Datarow datarow = new Datarow(extent.LeftTop, extent);
                dataTable.Datarows.Add(datarow);
            }
        }

        internal void DoParserRasterData(XmlNodeList nodelist, Datatable dataTable, string dataTableFilePath)
        {
            string ddPath = dataTableFilePath.Substring(0, dataTableFilePath.Length - 2) + "dd";
            foreach (XmlNode node in nodelist)
            {
                int id;
                MapExtent extent;
                if (!DoParserIdAndExtent(node, out id, out extent))
                {
                    continue;
                }
                string fileddPath = ddPath + id;
                Datarow datarow = new Datarow(fileddPath, extent);
                dataTable.Datarows.Add(datarow);
            }
        }
        internal void DoParserLineData(XmlNodeList nodelist, Datatable dataTable, string dataTableFilePath)
        {
            foreach (XmlNode node in nodelist)
            {
                int id;
                List<MapPoint> points;
                if (!DoParserIdAndExtent(node, out id, out points))
                {
                    continue;
                }

                Datarow datarow = new Datarow(points, (MapExtent)MapExtent.FromPoints(points));
                dataTable.Datarows.Add(datarow);
            }
        }
        internal bool DoParserIdAndExtent(XmlNode dataNode, out int id, out string extent)
        {
            id = 0;
            extent = null;
            XmlAttribute attri = dataNode.Attributes["id"];
            if (attri == null)
            {
                return false;
            }
            if (!int.TryParse(attri.Value, out id))
            {
                return false;
            }

            XmlNode extentnode = dataNode.SelectSingleNode("extent");
            if (extentnode == null || String.IsNullOrWhiteSpace(extentnode.InnerText))
            {
                return false;
            }
            extent = extentnode.InnerText;
            return true;
        }

        internal bool DoParserIdAndExtent(XmlNode dataNode, out int id, out List<MapPoint> points)
        {
            string strextent;
            points = new List<MapPoint>();
            if (DoParserIdAndExtent(dataNode, out id, out strextent))
            {
                string[] strs = strextent.Split(';');
                foreach (string item in strs)
                {
                    if (String.IsNullOrWhiteSpace(item))
                    {
                        continue;
                    }
                    MapExtent? me = MapExtent.FromString(item);
                    if (me == null)
                    {
                        continue;
                    }
                    MapPoint mp = ((MapExtent)me).LeftTop;
                    points.Add(mp);
                }
            }
            return (points.Count != 0);
        }

        internal bool DoParserIdAndExtent(XmlNode dataNode,out int id,out MapExtent extent)
        {
            string strextent;
            extent = MapExtent.Empty;
            if (DoParserIdAndExtent(dataNode, out id, out strextent))
            {
                MapExtent? me = MapExtent.FromString(strextent);
                if (me == null)
                {
                    return false;
                }
                extent = (MapExtent)me;
                return true;
            }
            return false;
        }
    }
}
