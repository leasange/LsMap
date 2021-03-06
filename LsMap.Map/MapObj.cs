﻿using LsMap.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsMap.Map
{
    /// <summary>
    /// 地图对象
    /// </summary>
   [Serializable]
    public class MapObj
    {
        private string _name = null;//地图名称
        private LsMap.Data.MapExtent _defaultExtent = LsMap.Data.MapExtent.Empty;//默认地图范围
        public LsMap.Data.MapExtent DefaultExtent
        {
            get { return _defaultExtent; }
            set { _defaultExtent = value; }
        }
        private List<Layer> _layers = new List<Layer>();//图层列表

        private ComCollection<LsMap.Data.Datasource> _datasources = new ComCollection<LsMap.Data.Datasource>();//数据源集合
        public ComCollection<LsMap.Data.Datasource> Datasources
        {
            get { return _datasources; }
            set { _datasources = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(true)]
        public List<Layer> Layers
        {
            get { return _layers; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public MapObj()
        {
            _name = "未知名称";
        }
        public MapObj(string name)
        {
            _name = name;
        }
    }
}
