namespace LsMap.UI
{
    partial class LayerControl
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.imageList = new System.Windows.Forms.ImageList();
            this.cmsMenu = new System.Windows.Forms.ContextMenuStrip();
            this.tsmiZoomToLayer = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmiRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.cmsMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // cmsMenu
            // 
            this.cmsMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmiZoomToLayer,
            this.tsmiRefresh});
            this.cmsMenu.Name = "cmsMenu";
            this.cmsMenu.Size = new System.Drawing.Size(137, 48);
            // 
            // tsmiZoomToLayer
            // 
            this.tsmiZoomToLayer.Name = "tsmiZoomToLayer";
            this.tsmiZoomToLayer.Size = new System.Drawing.Size(136, 22);
            this.tsmiZoomToLayer.Text = "缩放到图层";
            // 
            // tsmiRefresh
            // 
            this.tsmiRefresh.Name = "tsmiRefresh";
            this.tsmiRefresh.Size = new System.Drawing.Size(136, 22);
            this.tsmiRefresh.Text = "刷新图层";
            // 
            // LayerControl
            // 
            this.ImageIndex = 0;
            this.ImageList = this.imageList;
            this.LineColor = System.Drawing.Color.Black;
            this.SelectedImageIndex = 0;
            this.Size = new System.Drawing.Size(220, 400);
            this.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.LayerControl_AfterCheck);
            this.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.LayerControl_ItemDrag);
            this.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.LayerControl_NodeMouseClick);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.LayerControl_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.LayerControl_DragEnter);
            this.cmsMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imageList;
        private System.Windows.Forms.ContextMenuStrip cmsMenu;
        private System.Windows.Forms.ToolStripMenuItem tsmiZoomToLayer;
        private System.Windows.Forms.ToolStripMenuItem tsmiRefresh;
    }
}
