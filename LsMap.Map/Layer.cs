using LsMap.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsMap.Map
{
    /// <summary>
    /// 图层基类
    /// </summary>
    [Serializable]
    public abstract class Layer
    {
        public string DatasourceName//数据源名称
        {
            get
            {
                if (_datasource != null)
                {
                    return _datasource.Name;
                }
                return null;
            }
        }
        public string DatatableName//数据表名称
        {
            get 
            { 
                if (_datatable!=null)
                {
                    return _datatable.TableName;
                }
                return null;
            }
        }
        private string _aliasName = null;//别名，显示名称
        public string AliasName
        {
            get { 
                if (string.IsNullOrEmpty(_aliasName))
                {
                    return DatatableName + "@" + DatasourceName;
                }
                return _aliasName;
            }
            set { _aliasName = value; }
        }
        private static ulong LAYERID = 0;
        private ulong _layerID = 0;//图层编号，图层新建生成的编号
        public ulong LayerID
        {
            get { return _layerID; }
        }
        private bool _visible = true;//是否可视
        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }
        private MapExtent _extent = MapExtent.None;//地图范围
        public LsMap.Data.MapExtent Extent
        {
            get 
            {
                if (_extent==MapExtent.None&&_datatable!=null)
                {
                   _extent = _datatable.GetDataExtent();
                }
                return _extent; 
            }
        }

        private LsMap.Map.MapObj _map = null;
        public LsMap.Map.MapObj Map
        {
            get { return _map; }
            set { _map = value; }
        }

        private LsMap.Data.Datasource _datasource = null;
        public LsMap.Data.Datasource Datasource
        {
            get { return _datasource; }
            set { _datasource = value; }
        }
        private LsMap.Data.Datatable _datatable = null;
        public LsMap.Data.Datatable Datatable
        {
            get { return _datatable; }
            set { _datatable = value; }
        }

        public Layer(LsMap.Data.Datasource datasource, LsMap.Data.Datatable datatable)
        {
            _datasource = datasource;
            _datatable = datatable;
            _layerID = ++LAYERID;
        }

        public void RefreshInfo()
        {
            if (_datatable!=null)
            {
                _extent = _datatable.GetDataExtent();
            }
        }
    }
}
