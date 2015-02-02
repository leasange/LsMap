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
        private string _datasourceName = null;//数据源名称
        public virtual string DatasourceName
        {
            get { return _datasourceName; }
        }
        private string _datatableName = null;//数据表名称
        public string DatatableName
        {
            get { return _datatableName; }
        }
        private string _aliasName = null;//别名，显示名称
        public string AliasName
        {
            get { return _aliasName; }
            set { _aliasName = value; }
        }

        private LsMap.Map.Map _map = null;
        public LsMap.Map.Map Map
        {
            get { return _map; }
            set { _map = value; }
        }

        private LsMap.Data.Datasource _datasource = null;
        public LsMap.Data.Datasource Datasource
        {
            get { return _datasource; }
        }

        public Layer(string datasourceName, string datatableName)
        {
            _datasourceName = datasourceName;
            _datatableName = datatableName;
        }
    }
}
