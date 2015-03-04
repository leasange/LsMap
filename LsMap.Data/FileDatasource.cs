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
        //初始化文件型数据源
        public FileDatasource(string dataSrcPath)
            : base(DatasourceType.File)
        {
            _datasourcePath = dataSrcPath;
            DoOpenFileDataSrc();
        }
        //初始化文件型数据源
        public FileDatasource(string dataSrcPath, string name):base(DatasourceType.File)
        {
            _datasourcePath = dataSrcPath;
            this.Name = name;
        }
        /// <summary>
        /// 打开文件型数据源
        /// </summary>
        public override void Open()
        {
            try
            {
                if (this.IsOpen)
                {
                    return;
                }
                DoOpenFileDataSrc();
                this.IsOpen = true;
            }
            catch (System.Exception ex)
            {
                this.IsOpen = false;
                throw ex;
            }
        }
        /// <summary>
        /// 关闭文件型数据源
        /// </summary>
        public override void Close()
        {
            this.Tables.Clear();
            this.IsOpen = false;
        }

        public override List<Datarow> Query(string tableName, MapExtent extent)
        {
            string lsdtfile = System.IO.Path.Combine(_datasourcePath, tableName + ".lsdt");
            XmlDocument document = new XmlDocument();
            document.Load(lsdtfile);
            XmlNodeList nodelist = document.SelectNodes("datas/data");

            if (nodelist != null && nodelist.Count > 0)
            {
                return DoQueryFileTableData(nodelist, tableName,extent);
            }
            return new List<Datarow>();
        }

        public override MapExtent QueryExtent(string tableName)
        {
            List<Datarow> rows = Query(tableName, MapExtent.Max);
            if (rows.Count>0)
            {
                MapExtent extent = rows[0].Extent;
                for (int i = 1; i < rows.Count; i++)
                {
                    extent.Combine(rows[i].Extent);
                }
                return extent;
            }
            return MapExtent.None;
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
                dataTable.Datasource = this;
                this.Tables.Add(dataTable);
//                 XmlNodeList nodelist = document.SelectNodes("datas/data");
//                
//                 if (nodelist != null && nodelist.Count > 0)
//                 {
//                     DoParserFileTableData(nodelist, dataTable, item);
//                 }
            }
        }

        internal List<Datarow> DoQueryFileTableData(XmlNodeList nodelist, string tableName, MapExtent extent)
        {
            Datatable table = GetDatatable(tableName);
            if (table!=null)
            {
                switch (table.TableType)
                {
                    case DatatableType.Null:
                        break;
                    case DatatableType.Point:
                        return DoParserPointData(nodelist, tableName, extent);
                    case DatatableType.Raster:
                        return DoParserRasterData(nodelist, tableName, extent);
                    case DatatableType.Line:
                        return DoParserLineData(nodelist, tableName, extent);
                    case DatatableType.Polygon:
                        return DoParserPolygonData(nodelist, tableName, extent);
                    default:
                        break;
                }
            }
            return new List<Datarow>();
        }

        private List<Datarow> DoParserPolygonData(XmlNodeList nodelist, string tableName, MapExtent extent)
        {
            List<Datarow> rows = new List<Datarow>();
            foreach (XmlNode node in nodelist)
            {
                int id;
                List<MapPoint> points;
                if (!DoParserIdAndExtent(node, out id, out points))
                {
                    continue;
                }
                MapPolygon mapPolygon = new MapPolygon();
                mapPolygon.Points = points;
                if (mapPolygon.Extent.IsIntersectWith(extent))
                {
                    Datarow datarow = new Datarow(mapPolygon, mapPolygon.Extent);
                    rows.Add(datarow);
                }
            }
            return rows;
        }
        internal List<Datarow> DoParserPointData(XmlNodeList nodelist, string tableName, MapExtent extent)
        {
            List<Datarow> rows = new List<Datarow>();
            foreach (XmlNode node in nodelist)
            {
                int id;
                MapExtent textent;
                if (!DoParserIdAndExtent(node, out id, out textent))
                {
                    continue;
                }
                if (textent.IsIntersectWith(extent))
                {
                    Datarow datarow = new Datarow(textent.LeftTop, textent);
                    rows.Add(datarow);
                }
            }
            return rows;
        }

        internal List<Datarow> DoParserRasterData(XmlNodeList nodelist, string tableName, MapExtent extent)
        {
            List<Datarow> rows = new List<Datarow>();
            string ddPath = System.IO.Path.Combine(_datasourcePath, tableName + ".lsdd");
            foreach (XmlNode node in nodelist)
            {
                int id;
                MapExtent textent;
                if (!DoParserIdAndExtent(node, out id, out textent))
                {
                    continue;
                }
                if (textent.IsIntersectWith(extent))
                {
                    string fileddPath = ddPath + id;
                    Datarow datarow = new Datarow(fileddPath, textent);
                    rows.Add(datarow);
                }
            }
            return rows;
        }

        internal List<Datarow> DoParserLineData(XmlNodeList nodelist, string tableName, MapExtent extent)
        {
            List<Datarow> rows = new List<Datarow>();
            foreach (XmlNode node in nodelist)
            {
                int id;
                List<MapPoint> points;
                if (!DoParserIdAndExtent(node, out id, out points))
                {
                    continue;
                }
                MapLine mapLine = new MapLine();
                mapLine.Points = points;
                if (mapLine.Extent.IsIntersectWith(extent))
                {
                    Datarow datarow = new Datarow(mapLine, mapLine.Extent);
                    rows.Add(datarow);
                }
            }
            return rows;
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
