using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsMap.Data
{
    [Serializable]
    public abstract class Datasource
    {
        private string _name = null;//数据源名称
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        private bool _isOpen = false;//是否打开
        public bool IsOpen
        {
            get { return _isOpen; }
            internal set { _isOpen = value; }
        }
        private List<Datatable> _tables = new List<Datatable>();//数据集
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<Datatable> Tables
        {
            get { return _tables; }
        }

        private DatasourceType _datasourceType = DatasourceType.File;
        public LsMap.Data.DatasourceType DatasourceType
        {
            get { return _datasourceType; }
        }
        public Datasource(DatasourceType dsType)
        {
            _datasourceType = dsType;
        }

        public Datatable GetDatatable(string tableName)
        {
            foreach (Datatable item in _tables)
            {
                if (item.TableName==tableName)
                {
                    return item;
                }
            }
            return null;
        }
        public abstract void Open();
        public abstract void Close();
        public abstract List<Datarow> Query(string tableName, MapExtent extent);
        public abstract MapExtent QueryExtent(string tableName);
    }
    public enum DatasourceType
    {
        File,//文件型数据源
    }
}
