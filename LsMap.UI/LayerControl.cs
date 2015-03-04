using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using LsMap.Map;

namespace LsMap.UI
{
    public partial class LayerControl : TreeView
    {
        private MapControl _mapControl = null;
        public MapControl MapControl
        {
            get { return _mapControl; }
            set {
                if (value != null && value != _mapControl)
                {
                    if (_mapControl != null)
                    {
                        _mapControl.MapOpen -= _mapControl_MapOpen;
                    }
                    _mapControl = value;
                    _mapControl.MapOpen += _mapControl_MapOpen;
                    if (_mapControl.IsOpen)
                    {
                        DoMapOpen();
                    }
                }
                else
                {
                    if (_mapControl != null)
                    {
                        _mapControl.MapOpen -= _mapControl_MapOpen;
                    }
                    _mapControl = value;
                    ClearNodes();
                }
            }
        }

        public LayerControl()
        {
            InitializeComponent();
            this.BackColor = Color.White;
            this.CheckBoxes = true;
            tsmiZoomToLayer.Click += tsmiZoomToLayer_Click;
            tsmiRefresh.Click += tsmiRefresh_Click;
            ClearNodes();
            this.AllowDrop = true;
        }

        private void _mapControl_MapOpen(object sender, EventArgs e)
        {
            DoMapOpen();
        }
        private void DoMapOpen()
        {
            ClearNodes();
            if (_mapControl==null&&!_mapControl.IsOpen)
            {
                return;
            }
            foreach (Layer item in _mapControl.Map.Layers)
            {
                TreeNode node = new TreeNode(item.AliasName);
                node.Checked = item.Visible;
                node.Tag = item;
                this.Nodes[0].Nodes.Add(node);
            }
            this.ExpandAll();
        }

        private void ClearNodes()
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
                this.Nodes.Add(new TreeNode("图层"));
            }
        }

        private void tsmiZoomToLayer_Click(object sender, EventArgs e)
        {
            if (_mapControl!=null)
            {
                Layer layer = GetSelectedLayer();
                if (layer==null)
                {
                    if (this.SelectedNode!=null&&this.SelectedNode==this.Nodes[0])
                    {
                        _mapControl.ZoomToAllLayer();
                    }
                    return;
                }
                _mapControl.ZoomToLayer(layer);
            }
        }

        private void tsmiRefresh_Click(object sender, EventArgs e)
        {
            if (_mapControl != null)
            {
                Layer layer = GetSelectedLayer();
                if (layer == null)
                {
                    return;
                }
                _mapControl.Refresh(layer);
            }
        }

        private Layer GetSelectedLayer()
        {
            TreeNode n = this.SelectedNode;
            if (n!=null&&n.Tag is Layer)
            {
                return (Layer)n.Tag;
            }
            return null;
        }

        private void LayerControl_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            this.SelectedNode = e.Node;
            if (e.Button== MouseButtons.Right)
            {
                cmsMenu.Show(Cursor.Position);
            }
        }

        private void LayerControl_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action == TreeViewAction.ByMouse&&e.Node.Tag is Layer)
            {
                Layer layer = (Layer)e.Node.Tag;
                if (layer.Visible!=e.Node.Checked)
                {
                    layer.Visible = e.Node.Checked;
                    _mapControl.Refresh(layer);
                }
                this.Refresh();
            }
        }

        private void LayerControl_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button==MouseButtons.Left)
            {
                TreeNode node = e.Item as TreeNode;
                if (node==null||node.Tag==null)
                {
                    return;
                }
                this.SelectedNode = node;
                DoDragDrop(e.Item, DragDropEffects.Move);
            }
        }

        private void LayerControl_DragEnter(object sender, DragEventArgs e)
        {
            TreeNode node = e.Data.GetData(typeof(TreeNode)) as TreeNode;
            if (node != null && node.TreeView == this)
            {
                e.Effect = e.AllowedEffect;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void LayerControl_DragDrop(object sender, DragEventArgs e)
        {
            TreeNode dataNode = e.Data.GetData(typeof(TreeNode)) as TreeNode;
            if (dataNode != null && dataNode.TreeView == this)
            {
                TreeNode node = this.GetNodeAt(this.PointToClient(Cursor.Position));
                if (node == null)
                {
                    return;
                }
                
                //根节点
                if (node == this.Nodes[0])
                {
                    if (dataNode.Index>0)
                    {
                        this.Nodes[0].Nodes.Remove(dataNode);
                        this.Nodes[0].Nodes.Insert(0, dataNode);
                        this.SelectedNode = dataNode;
                        DoMapLayerMove(dataNode.Tag as Layer, dataNode.Index);
                    }
                }
                else
                {
                    if (node.Index+1==dataNode.Index)
                    {
                        return;
                    }
                    this.Nodes[0].Nodes.Remove(dataNode);
                    this.Nodes[0].Nodes.Insert(node.Index + 1, dataNode);
                    this.SelectedNode = dataNode;
                    DoMapLayerMove(dataNode.Tag as Layer, dataNode.Index);
                }
            }
        }
        private void DoMapLayerMove(Layer layer, int toIndex)
        {
            if (layer!=null&&_mapControl!=null)
            {
                _mapControl.MoveLayer(layer, toIndex);
            }
        }
    }
}
