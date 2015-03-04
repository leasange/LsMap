using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using LsMap.Data;

namespace LsMap.Workspace
{
    [Serializable]
    public partial class Workspace : ISerializable
    {
        private ComCollection<LsMap.Map.MapObj> _maps = new ComCollection<LsMap.Map.MapObj>();//地图集合
        private ComCollection<LsMap.Data.Datasource> _datasources = new ComCollection<LsMap.Data.Datasource>();//数据源集合
        public ComCollection<LsMap.Data.Datasource> Datasources
        {
            get { return _datasources; }
        }

        [Description("获取地图集合")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ComCollection<LsMap.Map.MapObj> Maps
        {
            get { return _maps; }
        }

        public Workspace()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 供反序列化调用的构造函数
        /// </summary>
        protected Workspace(SerializationInfo info, StreamingContext context)
        {

        }

        public bool Open(WorkspaceConnection con)
        {
            bool ret = con.Open();
            if (ret)
            {
                _maps.Clear();
                _datasources.Clear();
                _maps = con._maps;
                _datasources = con._datasources;
            }
            return ret;
        }

        public bool Save()
        {
            return true;
        }
        public bool SaveAsFile(string wsFileName)
        {
            FileStream fs = new FileStream(wsFileName, FileMode.OpenOrCreate);
            BinaryFormatter bin = new BinaryFormatter();
            bin.Serialize(fs, this);
            return true;
        }
        static internal Workspace OpenFileWorkspace(string wsFileName)
        {
            FileStream fs = new FileStream(wsFileName, FileMode.Open);
            BinaryFormatter bin = new BinaryFormatter();
            object obj = bin.Deserialize(fs);
            if (obj != null)
            {
                Workspace ws = (Workspace)obj;
                return ws;
            }
            return null;
        }

        /// <summary>
        /// 获取地图
        /// </summary>
        /// <param name="mapname">地图名称</param>
        /// <returns>返回地图，不存在则为null</returns>
        public LsMap.Map.MapObj GetMap(string mapname)
        {
            foreach (var item in _maps)
            {
                if (item.Name == mapname)
                {
                    return item;
                }
            }
            return null;
        }
        /// <summary>
        /// 获取数据源
        /// </summary>
        /// <param name="datasourceName">数据源名称</param>
        /// <returns>返回数据源</returns>
        public LsMap.Data.Datasource GetDatasource(string datasourceName)
        {
            foreach (var item in _datasources)
            {
                if (item.Name == datasourceName)
                {
                    return item;
                }
            }
            return null;
        }
        /// <summary>
        /// 获取表
        /// </summary>
        /// <param name="datasource">数据源</param>
        /// <param name="datatableName">表名称</param>
        /// <returns>返回数据源</returns>
        public LsMap.Data.Datatable GetDatatable(LsMap.Data.Datasource datasource, string datatableName)
        {
            if (datasource != null)
            {
                foreach (var item in datasource.Tables)
                {
                    if (item.TableName == datatableName)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        #region 序列化操作
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {

        }
        #endregion

    }
    
}
