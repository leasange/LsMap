using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsMap.Data
{
    /// <summary>
    /// 行数据
    /// </summary>
   [Serializable]
    public class Datarow
    {
        private List<object> _attributes = new List<object>();
        public List<object> Attributes
        {
            get { return _attributes; }
        }
        private object _data = null;
        public object Data
        {
            get { return _data; }
            //set { _data = value; }
        }
        private MapExtent _extent = MapExtent.Empty;
        public LsMap.Data.MapExtent Extent
        {
            get { return _extent; }
            //set { _extent = value; }
        }
        public Datarow(object data, MapExtent extent)
        {
            _data = data;
            _extent = extent;
        }
    }
}
