using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LsMap.UI
{
    public partial class MapListControl : TreeView
    {
        private LsMap.Workspace.Workspace _workSpace = null;
        public LsMap.Workspace.Workspace WorkSpace
        {
            get { return _workSpace; }
            set {
                if (value!=null&&value!=_workSpace)
                {
                    if (_workSpace!=null)
                    {
                        _workSpace.Maps.CollectionEvent -= Maps_CollectionEvent;
                    }
                    _workSpace = value; 
                    _workSpace.Maps.CollectionEvent+=Maps_CollectionEvent;
                    UpdateMapNodes();
                }
                else
                {
                    if (_workSpace != null)
                    {
                        _workSpace.Maps.CollectionEvent -= Maps_CollectionEvent;
                    }
                    ClearMapNodes();
                    _workSpace = value;
                }
            }
        }

        public MapListControl()
        {
            InitializeComponent();
            ClearMapNodes();
        }
        private void ClearMapNodes()
        {
            if (this.Nodes.Count>0)
            {
                this.Nodes[0].Nodes.Clear();
                while (this.Nodes.Count >= 2)
                {
                    this.Nodes.RemoveAt(this.Nodes.Count - 1);
                }
            }
            else
            {
                this.Nodes.Add(new TreeNode("地图"));
            }
        }
        private void UpdateMapNodes()
        {
            ClearMapNodes();
            if (_workSpace!=null)
            {
                foreach (var item in _workSpace.Maps)
                {
                    TreeNode node = new TreeNode(item.Name);
                    node.Tag = item;
                    this.Nodes[0].Nodes.Add(node);
                }
            }
            this.ExpandAll();
        }
        private void Maps_CollectionEvent(object sender, Data.ComCollectionArgs<Map.MapObj> e)
        {
            UpdateMapNodes();
        }
    }
}
