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

namespace LsMap.UI
{
    public partial class LayerControl : TreeView
    {
        public LayerControl()
        {
            InitializeComponent();
            this.BackColor = Color.White;
            this.DrawMode = TreeViewDrawMode.OwnerDrawAll;
        }
        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            base.OnDrawNode(e);
            //屏蔽无效节点
            if (e.Node.Bounds.Left == 0)
            {
                return;
            }

            const int int_RowWidth = 9;
            const int int_ExpandeboxWidth = 8;

            //if (this.CheckBoxes)
            //{
            TreeNode node = e.Node;
            int x = node.Bounds.X;
            Pen penText = new Pen(this.ForeColor, 1);
            Pen penDotline = new Pen(Color.Gray, 1);

            Font textFont = this.Font;
            if (e.Node.NodeFont!=null)
            {
                textFont = e.Node.NodeFont;
            }
            
            //绘制文字
            //Brush brush = new SolidBrush(this.BackColor);
            Size textSize=e.Graphics.MeasureString(node.Text, textFont).ToSize();
            Rectangle textRect = new Rectangle(node.Bounds.Left + 50, e.Bounds.Top+2, textSize.Width, textSize.Height);
            if (e.Node.TreeView.SelectedNode == node)
            {
                if (e.State == TreeNodeStates.Selected)
                {
                   // brush = new SolidBrush(Color.FromArgb(236, 233, 216));
                    //e.Graphics.FillRectangle(brush, new Rectangle(textRect.Location, textSize));
                    TextRenderer.DrawText(e.Graphics, node.Text, textFont, textRect, this.ForeColor, Color.Blue, TextFormatFlags.Left);
                }
                else
                {
                   // brush = new SolidBrush(Color.FromArgb(10, 36, 106));
                    //e.Graphics.FillRectangle(brush, new Rectangle(textRect.Location, textSize));
                   // Pen penDotline2 = new Pen(Color.Black, 1);
                    //penDotline2.DashStyle = DashStyle.Dot;
                    //e.Graphics.DrawRectangle(penDotline2, textRect);
                    TextRenderer.DrawText(e.Graphics, node.Text, textFont, textRect, Color.White, Color.Blue, TextFormatFlags.Left);
                }
            }
            else
            {
                TextRenderer.DrawText(e.Graphics, node.Text, textFont, textRect, this.ForeColor, Color.Transparent, TextFormatFlags.Left);
            }

            //画image
            if (this.ImageList != null && this.ImageList.Images.Count != 0)
            {
                //画image
                Rectangle imagebox = new Rectangle(x - 3 - 16, node.Bounds.Y + 1, 16, 14);
                int index = node.ImageIndex;
                string imagekey = node.ImageKey;
                if (imagekey != "" && this.ImageList.Images.ContainsKey(imagekey))
                {
                    e.Graphics.DrawImage(this.ImageList.Images[imagekey], imagebox);
                }
                else
                {
                    if (index < 0) index = 0;
                    else if (index > this.ImageList.Images.Count - 1) index = 0;
                    e.Graphics.DrawImage(this.ImageList.Images[index], imagebox);
                }
                e.Graphics.DrawRectangle(new Pen(Color.Black, 1), imagebox);

                x -= 19;
            }

            ////画checkbox
            //if (this.CheckBoxes)
            //{
            //    Rectangle rcCheck = new Rectangle(node.Bounds.X - int_CheckBoxWidth, node.Bounds.Y, int_CheckBoxWidth, int_CheckBoxWidth);
            //    using (Bitmap bmp = new Bitmap(node.Checked ? Resource.Images.CheckBox_Check : Resource.Images.CheckBox))
            //    {
            //        e.Graphics.DrawImage(bmp, rcCheck, 0, bmp.Height / 4, bmp.Width, bmp.Height / 4, Color.Magenta, true);
            //    }
            //    currt_X = currt_X - int_CheckBoxWidth - 3;
            //}

            //画线
            if (node.TreeView.ShowLines)
            {
                penDotline.DashStyle = DashStyle.Dot;
                e.Graphics.DrawLine(penDotline, x, node.Bounds.Top + node.Bounds.Height / 2 + 1, x - int_RowWidth, node.Bounds.Top + node.Bounds.Height / 2 + 1);
                if (node.NextNode != null)
                {
                    if (node.TreeView.Nodes[0] == node)
                    {
                        e.Graphics.DrawLine(penDotline, x - 9, node.Bounds.Bottom, x - 9, node.Bounds.Bottom - node.Bounds.Height / 2 - 1);
                    }
                    else
                    {
                        e.Graphics.DrawLine(penDotline, x - 9, node.Bounds.Top, x - 9, node.Bounds.Bottom);
                    }
                }
                else if (node.TreeView.Nodes[0] == node) { }
                else
                {
                    e.Graphics.DrawLine(penDotline, x - 9, node.Bounds.Top, x - 9, node.Bounds.Top + node.Bounds.Height / 2 + 2);
                }
                int level = node.Level;
                TreeNode parentnode = new TreeNode();
                if (level != 0)
                {
                    parentnode = node.Parent;
                    while (level > 0)
                    {
                        if (parentnode.NextNode != null)
                        {
                            e.Graphics.DrawLine(penDotline, parentnode.Bounds.X - (node.Bounds.X - x + 9), parentnode.Bounds.Bottom, parentnode.Bounds.X - (node.Bounds.X - x + 9), parentnode.NextNode.Bounds.Top);
                        }
                        parentnode = parentnode.Parent;
                        level--;
                    }
                }
                x = x - 9;
            }
            //画+ -框
            if (e.Node.Nodes.Count > 0)
            {
                Rectangle rcExpandebox = new Rectangle(x - int_ExpandeboxWidth / 2, node.Bounds.Y + int_ExpandeboxWidth / 2, int_ExpandeboxWidth, int_ExpandeboxWidth);
                e.Graphics.FillRectangle(new SolidBrush(node.TreeView.BackColor), new Rectangle(rcExpandebox.X, rcExpandebox.Y, 9, 9));
                penDotline.DashStyle = DashStyle.Solid;
                e.Graphics.DrawRectangle(penDotline, rcExpandebox);
                e.Graphics.DrawLine(penText, rcExpandebox.Left + 2, rcExpandebox.Top + 4, rcExpandebox.Right - 2, rcExpandebox.Top + 4);
                if (!node.IsExpanded)
                {
                    e.Graphics.DrawLine(penText, rcExpandebox.Left + 4, rcExpandebox.Top + 2, rcExpandebox.Left + 4, rcExpandebox.Top + 6);
                }
            }

            
            //}
            //else { e.DrawDefault = true; }
        }
    }
}
