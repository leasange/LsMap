using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LsMap.Map;
using System.Xml;
using LsMap.Data;
using System.Threading;

namespace LsMap.UI
{
    public partial class MapControl : UserControl
    {
        private LsMap.Workspace.Workspace _workspace = null;
        [DefaultValue(null)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public LsMap.Workspace.Workspace Workspace
        {
            get { return _workspace; }
            set
            {
                if (_workspace != null)
                {
                    if (_workspace != value)
                    {
                        if (_isOpen)
                        {
                            CloseMap();
                        }
                        _workspace = value;
                    }
                }
                else
                {
                    _workspace = value;
                }
            }
        }
        private LsMap.Map.Map _map = null;//地图对象
        private bool _isOpen = false;//是否打开
        private double _scale = 1;//比例尺
        public new double Scale//比例尺
        {
            get
            {
                return _scale;
            }
        }
        private MapExtent _extent = MapExtent.Empty;
        private MapAction _mapAction = MapAction.Move;
        private MapShowEngine _mapShowEngine=null;
        private MapDrawEngine _mapDrawEngine = null;

        public LsMap.UI.MapAction MapAction
        {
            get { return _mapAction; }
            set { SetMapAction(value); }
        }
        //当前地图可视范围
        private Size _oldRefreshSize = Size.Empty;
        private Size _oldSize = Size.Empty;
        public MapExtent Extent
        {
            get
            {
                return _extent;
            }
            set
            {
                SetExtent(value);
            }
        }
        [DefaultValue(false), Description("获取地图是否打开")]
        public bool IsOpen
        {
            get { return _isOpen; }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(true)]
        [DefaultValue(null), Description("获取地图对象")]
        public LsMap.Map.Map Map
        {
            get { return _map; }
        }

        #region 自定义事件
        public event EventHandler ScaleChanged = null;
        #endregion

        public MapControl()
        {
            InitializeComponent();
            this.ResizeRedraw = true;
            this.SetStyle(ControlStyles.DoubleBuffer |
              ControlStyles.UserPaint |
              ControlStyles.AllPaintingInWmPaint,
              true);
            this.UpdateStyles();
            _mapShowEngine = new MapShowEngine();
            _mapShowEngine.ShowUpdated += _mapShowEngine_ShowUpdated;
            _mapDrawEngine = new MapDrawEngine(_mapShowEngine);
        }

        #region 加载
        private void MapControl_Load(object sender, EventArgs e)
        {
            _oldSize = this.Size;
            this.MouseWheel += MapControl_MouseWheel;
            this.Refresh();
        }
        #endregion

        #region 方法
        /// <summary>
        /// 打开地图
        /// </summary>
        /// <param name="map">需打开地图对象</param>
        /// <returns>打开成功与否</returns>
        /// <exception cref="System.ArgumentNullException">输入参数为空异常</exception>
        public bool OpenMap(string mapname)
        {
            DoCheckWorkspaceArgument();
            if (String.IsNullOrWhiteSpace(mapname))
            {
                throw new ArgumentNullException("mapname");
            }
            LsMap.Map.Map map = _workspace.GetMap(mapname);
            return DoOpenMap(map);
        }
        public bool OpenMap(int mapindex)
        {
            DoCheckWorkspaceArgument();
            LsMap.Map.Map map = _workspace.Maps[mapindex];
            return DoOpenMap(map);
        }
        internal bool DoOpenMap(LsMap.Map.Map map)
        {
            if (map == null)
            {
                throw new ArgumentNullException("map");
            }
            _map = map;
            _isOpen = true;
            SetExtent(_map.DefaultExtent);
            this.Refresh();
            return true;
        }
        /// <summary>
        /// 获取或更新比例尺
        /// </summary>
        /// <returns></returns>
        private double UpdateScale()
        {
            double dd1 = _extent.Width / _extent.Height;
            double dd2 = this.Width / (double)this.Height;
            float pixPPcmX = 25;
            float pixPPcmY = 25;
            MapHelper.GetScreenDpiPPcm(out pixPPcmX, out pixPPcmY);
            double scale = 1;
            if (dd1 <= dd2)
            {
                scale = this.Height / (pixPPcmY * _extent.Height * 100);
            }
            else
            {
                scale = this.Width / (pixPPcmX * _extent.Width * 100);
            }
            if (_scale != scale)
            {
                _scale = scale;
                if (ScaleChanged != null)
                {
                    ScaleChanged(this, new EventArgs());
                }
            }
            return scale;
        }

        private void SetExtent(MapExtent extent)
        {
            double dd1 = extent.Width / extent.Height;
            double dd2 = this.Width / (double)this.Height;
            if (dd1 <= dd2)
            {
                double ew = extent.Height / this.Height * this.Width;
                MapExtent ex = new MapExtent(0, extent.top, 0, extent.bottom);
                ex.left = extent.left - (ew / 2 - extent.Width / 2);
                ex.right = extent.right + (ew / 2 - extent.Width / 2);
                _extent = ex;
            }
            else
            {
                double eh = extent.Width / this.Width * this.Height;
                MapExtent ex = new MapExtent(extent.left, 0, extent.right, 0);
                ex.top = extent.top - (eh / 2 - extent.Height / 2);
                ex.bottom = extent.bottom + (eh / 2 - extent.Height / 2);
                _extent = ex;
            }
            UpdateScale();
        }

        /// <summary>
        /// 转换为屏幕坐标
        /// </summary>
        /// <param name="mapPoint">地理坐标</param>
        /// <returns>屏幕坐标</returns>
        public PointF ToScreenPoint(MapPoint mapPoint)
        {
            float fl = (float)((mapPoint.x - _extent.left) / _extent.Width * this.Width);
            float ft = (float)((_extent.top - mapPoint.y) / _extent.Height * this.Height);
            return new PointF(fl, ft);
        }
        /// <summary>
        /// 转换为地理坐标
        /// </summary>
        /// <param name="screenPoint">屏幕坐标</param>
        /// <returns>地理坐标</returns>
        public MapPoint ToMapPoint(PointF screenPoint)
        {
            double x = _extent.left + (screenPoint.X * _extent.Width) / this.Width;
            double y = _extent.top - (screenPoint.Y * _extent.Height) / this.Height;
            return new MapPoint(x, y);
        }

        /// <summary>
        /// 检测工作空间是否为空
        /// </summary>
        private void DoCheckWorkspaceArgument()
        {
            if (_workspace == null)
            {
                throw new ArgumentNullException("workspace");
            }
        }
        /// <summary>
        /// 关闭地图
        /// </summary>
        public void CloseMap()
        {
            _map = null;
            _isOpen = false;
        }

        private void UpdateExtentBySizeChanged()
        {
            double nw = _extent.Width / _oldSize.Width * this.Width;
            double nh = _extent.Height / _oldSize.Height * this.Height;
            _extent.right -= _extent.Width - nw;
            _extent.bottom += _extent.Height - nh;
            _oldSize = this.Size;
            if (_oldRefreshSize.Width<this.Width||_oldRefreshSize.Height<this.Height)
            {
                timerDelayRefresh.Stop();
                timerDelayRefresh.Start();
            }
        }

        private void SetMapAction(MapAction mapAction)
        {
            _mapAction = mapAction;
            this.Cursor = _mapAction.upCursor;
        }
        #endregion

        #region 绘制
        private void DoDrawLayer(Layer layer)
        {
            DrawLayerTask task = null;
            if (layer is RasterLayer)
            {
                task = new DrawRasterTask(_workspace, layer as RasterLayer, this.Extent, this.Width, this.Height);
            }
            else if (layer is PointLayer)
            {
                task = new DrawPointTask(_workspace, layer as PointLayer, this.Extent, this.Width, this.Height);
            }
            else if (layer is LineLayer)
            {
                task = new DrawLineTask(_workspace, layer as LineLayer, this.Extent, this.Width, this.Height);
            }
            else if (layer is PolygonLayer)
            {
                task = new DrawPolygonTask(_workspace, layer as PolygonLayer, this.Extent, this.Width, this.Height);
            }
            if (task!=null)
            {
                _mapDrawEngine.InsertTask(-1, task);
            }
           
        }
        /// <summary>
        /// 绘制Logo
        /// </summary>
        private void DoDrawLogo(Graphics g)
        {
            SizeF s = g.MeasureString("LsMap MapControl", this.Font);
            g.ResetTransform();
            g.DrawString("LsMap MapControl", this.Font, new SolidBrush(Color.Blue), new PointF(0, this.Height - s.Height));
        }
        private void _mapShowEngine_ShowUpdated(object sender, EventArgs e)
        {
            if (this.IsDisposed)
            {
                return;
            }
            this.Invoke(new Action(() =>
            {
                base.Refresh();
            }));
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                if (_map != null)
                {
                    if (_isMoving)
                    {
                        _mapShowEngine.Show(e.Graphics, _oldMousePoint.X - _startMousePoint.X, _oldMousePoint.Y - _startMousePoint.Y);
                    }
                    else
                    {
                        _mapShowEngine.Show(e.Graphics);
                    }
                }
                else
                {
                    base.OnPaint(e);
                }
                DoDrawLogo(e.Graphics);
            }
            finally
            {
            }
        }
        #endregion

        #region 事件
        protected override void OnSizeChanged(EventArgs e)
        {
            UpdateExtentBySizeChanged();
            base.OnSizeChanged(e);
        }
        protected override void OnHandleDestroyed(EventArgs e)
        {
            _mapDrawEngine.Dispose();
            _mapShowEngine.Dispose();
            base.OnHandleDestroyed(e);
        }

        private Point _oldMousePoint = Point.Empty;
        private Point _startMousePoint = Point.Empty;
        private Size _moveSize = Size.Empty;
        private bool _isMoving = false;
        private void MapControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_mapAction == MapAction.Move)//移动
                {
                    _isMoving = true;
                    if (_oldMousePoint == Point.Empty)
                    {
                        _oldMousePoint = e.Location;
                    }
                    else
                    {
                        CancelRefresh();
                        int dx = e.Location.X - _oldMousePoint.X;
                        int dy = e.Location.Y - _oldMousePoint.Y;
                        double dnw = _extent.Width / this.Width * dx;
                        double dnh = _extent.Height / this.Height * dy;
                        _extent.left -= dnw;
                        _extent.right -= dnw;
                        _extent.top += dnh;
                        _extent.bottom += dnh;
                        _moveSize.Width += dx;
                        _moveSize.Height += dy;
                        _oldMousePoint = e.Location;
                        this.Refresh();
                    }
                }
            }
        }
        private void CancelRefresh()
        {
            _mapDrawEngine.CancelDrawAndShow();
        }
        public new void Refresh()
        {
            if (_map==null)
            {
                return;
            }
            if (_isMoving)
            {
                base.Refresh();
            }
            else
            {
                _oldRefreshSize = this.Size;
                CancelRefresh();
                for (int i = _map.Layers.Count-1; i >=0; i--)
                {
                    DoDrawLayer(_map.Layers[i]);
                }
            }
           
        }
        public new void Invalidate()
        {
            this.Refresh();
        }
        private void MapControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _oldMousePoint = e.Location;
                _startMousePoint = e.Location;
                this.Cursor = _mapAction.downCursor;
                _moveSize = Size.Empty;
            }
            else if (e.Button == MouseButtons.Right)
            {
                _oldMousePoint = Point.Empty;
                _startMousePoint = Point.Empty;
                _mapAction = MapAction.Default;
            }
            else
            {
                _oldMousePoint = Point.Empty;
                _startMousePoint = Point.Empty;
                _mapAction = MapAction.Move;
            }
        }

        private void MapControl_MouseUp(object sender, MouseEventArgs e)
        {
            _oldMousePoint = Point.Empty;
            _startMousePoint = Point.Empty;
            this.Cursor = _mapAction.upCursor;
            if (_isMoving)
            {
                CancelRefresh();
                _isMoving = false;
                this.Refresh();
            }
        }

        private void MapControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0 && _scale >= 1/0.01)
            {
                return;
            }
            timerDelayRefresh.Stop();
            CancelRefresh();
            MapPoint mapPoint = ToMapPoint(e.Location);
            if (e.Delta > 0)//放大
            {
                double left = (_extent.left + mapPoint.x)* 3/ 4;
                double top = (_extent.top + mapPoint.y) * 3 / 4;
                double bottom = (mapPoint.y + _extent.bottom) * 3 / 4;
                double right = (mapPoint.x + _extent.right) * 3 / 4;
                _extent = new MapExtent(left, top, right, bottom);
            }
            else if (e.Delta < 0)//缩小
            {
                double left = 4d/3 * _extent.left - mapPoint.x;
                double top = 4d / 3 * _extent.top - mapPoint.y;
                double bottom = 4d / 3 * _extent.bottom - mapPoint.y;
                double right = 4d / 3 * _extent.right - mapPoint.x;
                _extent = new MapExtent(left, top, right, bottom);
            }
            UpdateScale();
            timerDelayRefresh.Start();
        }
        //延时绘制100ms
        private void timerDelayRefresh_Tick(object sender, EventArgs e)
        {
            timerDelayRefresh.Stop();
            this.Refresh();
        }
        #endregion

    }
}
