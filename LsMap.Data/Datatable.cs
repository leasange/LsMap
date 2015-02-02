using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsMap.Data
{
    [Serializable]
    public class Datatable
    {
        private Datasource _datasource = null;
        public LsMap.Data.Datasource Datasource
        {
            get { return _datasource; }
            set { _datasource = value; }
        }
        private List<Datarow> _datarows = new List<Datarow>();
        public List<Datarow> Datarows
        {
            get { return _datarows; }
        }
        private string _tableName = null;//数据表名称
        public string TableName
        {
            get { return _tableName; }
        }
        private DatatableType _tableType = DatatableType.Raster;//数据类型
        public LsMap.Data.DatatableType TableType
        {
            get { return _tableType; }
        }
        public Datatable(string tablename, DatatableType type)
        {
            _tableName = tablename;
            _tableType = type;
        }
    }
    public enum DatatableType
    {
        Null,//未知类型
        Point,//点类型
        Raster,//栅格数据
        Line,//线
    }
}
