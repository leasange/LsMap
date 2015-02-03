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

        private AutoResetEvent _invalidateResetEvent = new AutoResetEvent(false);
        private Thread _invalidateThread = null;

        public LsMap.UI.MapAction MapAction
        {
            get { return _mapAction; }
            set { SetMapAction(value); }
        }
        //当前地图可视范围
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

            _invalidateThread = new Thread(new ThreadStart(RefreshThread));
            _invalidateThread.Start();
        }

        private void _mapShowEngine_ShowUpdated(object sender, EventArgs e)
        {
            Graphics g = this.CreateGraphics();
            g.DrawImage(_mapShowEngine.LastBitmap, 0, 0);
            g.Dispose();
        }

        #region 加载
        private Size _oldSize = Size.Empty;
        private void MapControl_Load(object sender, EventArgs e)
        {
            _oldSize = this.Size;
            this.MouseWheel += MapControl_MouseWheel;
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

        private void UpdateSize()
        {
            double nw = _extent.Width / _oldSize.Width * this.Width;
            double nh = _extent.Height / _oldSize.Height * this.Height;
            _extent.right -= _extent.Width - nw;
            _extent.bottom += _extent.Height - nh;
            _oldSize = this.Size;
        }

        private void SetMapAction(MapAction mapAction)
        {
            _mapAction = mapAction;
            this.Cursor = _mapAction.upCursor;
        }
        #endregion

        #region 绘制
        enum RefreahState
        {
            None,//空闲状态
            Refreshing,//绘制中
            Cancelfreshing,//取消绘制中
        }
        private RefreahState _cancelRefresh = RefreahState.None;
        private void RefreshThread()
        {
            try
            {
                while (true)
                {
                    _invalidateResetEvent.WaitOne();

                    while (_cancelRefresh != RefreahState.None)
                    {
                        Thread.Sleep(10);
                    }

                    _cancelRefresh = RefreahState.Refreshing;
                    this.Invoke(new Action(() =>
                    {
                        base.Refresh();
                    }));
                }
            }
            catch
            {
                return;
            }
        }

        private void DoDrawLayer(Layer layer, Graphics g)
        {
            if (CheckNeedPaintReturn())
            {
                return;
            }

            if (layer is RasterLayer)
            {
                DoDrawLayer(layer as RasterLayer, g);
            }
            else if (layer is PointLayer)
            {
                DoDrawLayer(layer as PointLayer, g);
            }
            else if (layer is LineLayer)
            {
                DoDrawLayer(layer as LineLayer, g);
            }
            else if (layer is PolygonLayer)
            {
                DoDrawLayer(layer as PolygonLayer, g);
            }
        }
        /// <summary>
        /// 根据图层获取对应的数据库表
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        private LsMap.Data.Datatable GetLayerDataTable(Layer layer)
        {
            if (layer != null)
            {
                LsMap.Data.Datasource ds = _workspace.GetDatasource(layer.DatasourceName);
                if (ds != null)
                {
                    LsMap.Data.Datatable dt = _workspace.GetDatatable(ds, layer.DatatableName);
                    return dt;
                }
            }
            return null;
        }
        /// <summary>
        /// 绘制RasterLayer图层
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="g"></param>
        private void DoDrawLayer(RasterLayer layer, Graphics g)
        {
            if (layer != null)
            {
                LsMap.Data.Datatable dt = GetLayerDataTable(layer);
                if (dt != null)
                {
                    try
                    {
                        foreach (Datarow item in dt.Datarows)
                        {
                            if (CheckNeedPaintReturn())
                            {
                                return;
                            }
                            PointF p1 = ToScreenPoint(item.Extent.LeftTop);
                            PointF p2 = ToScreenPoint(item.Extent.RightBottom);
                            Image image = null;
                            switch (dt.TableType)
                            {
                                case DatatableType.Raster:
                                    if (item.Data is string)
                                    {
                                        image = Image.FromFile((string)item.Data);
                                    }
                                    break;
                                default:
                                    break;
                            }
                            if (image == null)
                            {
                                continue;
                            }
                            g.DrawImage(image, p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);
                            image.Dispose();
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }
        /// <summary>
        /// 绘制PointLayer图层
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="g"></param>
        private void DoDrawLayer(PointLayer layer, Graphics g)
        {
            if (layer != null)
            {
                LsMap.Data.Datatable dt = GetLayerDataTable(layer);
                if (dt != null)
                {
                    try
                    {
                        Random rand = new Random();
                        DateTime dtStart = DateTime.Now;
                        List<AutoResetEvent> autoResets = new List<AutoResetEvent>();
                        List<Bitmap> bitmaps = new List<Bitmap>();
                        
                        int c = (int)Math.Ceiling(dt.Datarows.Count / 4d);
                        for (int i = 0; i < 4; i++)
                        {
                            int start = i * c;
                            int end = (i + 1) * c;
                            if (end>dt.Datarows.Count)
                            {
                                end = dt.Datarows.Count;
                            }
                            AutoResetEvent autoReset = new AutoResetEvent(false);
                           // Bitmap bitmap = new Bitmap(this.Width, this.Height);
                            autoResets.Add(autoReset);
                           // bitmaps.Add(bitmap);
                            Thread th = new Thread(new ThreadStart(() =>
                            {
                               // Graphics gt = Graphics.FromImage(bitmap);
                                Graphics tem = this.CreateGraphics();
                                for (int j = start; j < end; j++)
                                {
                                    if (CheckNeedPaintReturn())
                                    {
                                        //gt.Dispose();
                                        autoReset.Set();
                                        return;
                                    }
                                    PointF point = ToScreenPoint((MapPoint)dt.Datarows[j].Data);
                                    
                                    //gt.DrawImage(Properties.Resources.qiuji_online, point.X, point.Y, 12, 14);
                                    
                                    tem.DrawImage(Properties.Resources.qiuji_online, point.X, point.Y, 12, 14);
                                   
                                }
                                tem.Dispose();
                             //   gt.Dispose();
                                autoReset.Set();
                            }));
                            th.IsBackground = true;
                            th.Start();

                        }
//                         foreach (AutoResetEvent item in autoResets)
//                         {
//                             item.WaitOne();
//                         }
                        foreach (Bitmap item in bitmaps)
                        {
                            g.DrawImage(item, 0, 0);
                            item.Dispose();
                        }
                        bitmaps.Clear();
                        Console.WriteLine("绘制摄像头所有时间：" + (DateTime.Now - dtStart).TotalSeconds);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }
        private void DoDrawLayer(LineLayer layer, Graphics g)
        {
            if (layer != null)
            {
                LsMap.Data.Datatable dt = GetLayerDataTable(layer);
                if (dt != null)
                {
                    try
                    {
                        foreach (Datarow item in dt.Datarows)
                        {
                            if (CheckNeedPaintReturn())
                            {
                                return;
                            }
                            if (item.Data == null)
                            {
                                continue;
                            }
                            MapLine mapLine = (MapLine)item.Data;
                            List<PointF> screenpoints = new List<PointF>();
                            foreach (MapPoint p in mapLine.Points)
                            {
                                screenpoints.Add(ToScreenPoint(p));
                            }
                            g.DrawLines(new Pen(Color.Orange, 2), screenpoints.ToArray());
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

        }
        private void DoDrawLayer(PolygonLayer layer, Graphics g)
        {
            if (layer != null)
            {
                LsMap.Data.Datatable dt = GetLayerDataTable(layer);
                if (dt != null)
                {
                    try
                    {
                        foreach (Datarow item in dt.Datarows)
                        {
                            if (CheckNeedPaintReturn())
                            {
                                return;
                            }
                            if (item.Data == null)
                            {
                                continue;
                            }
                            MapPolygon mapPolygon = (MapPolygon)item.Data;
                            List<PointF> screenpoints = new List<PointF>();
                            foreach (MapPoint p in mapPolygon.Points)
                            {
                                screenpoints.Add(ToScreenPoint(p));
                            }
                            g.FillPolygon(new SolidBrush(Color.FromArgb(100,Color.Blue)), screenpoints.ToArray());
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

        }
        /// <summary>
        /// 绘制Logo
        /// </summary>
        private void DoDrawLogo(Graphics g)
        {
            SizeF s = g.MeasureString("LsMap MapControl", this.Font);
            g.DrawString("LsMap MapControl", this.Font, new SolidBrush(Color.Blue), new PointF(0, this.Height - s.Height));
        }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
        }
        private Bitmap _lastBitMap = null;
        private void ClearLastBitMap()
        {
            if (_lastBitMap != null)
            {
                _lastBitMap.Dispose();
                _lastBitMap = null;
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            try
            {
                if (_map != null)
                {
                    if (CheckNeedPaintReturn())
                    {
                        DrawDefault(e.Graphics);
                        return;
                    }
                    if (_lastBitMap == null || !_isMoving)
                    {
                        if (CheckNeedPaintReturn())
                        {
                            DrawDefault(e.Graphics);
                            return;
                        }
                        Bitmap bmp = new Bitmap(this.Width, this.Height);
                        Graphics g = Graphics.FromImage(bmp);
                        foreach (Layer layer in _map.Layers)
                        {
                            DoDrawLayer(layer, g);
                        }
                        DoDrawLogo(g);
                        g.Dispose();
                        e.Graphics.DrawImage(bmp, 0, 0);
                        ClearLastBitMap();
                        _lastBitMap = bmp;
                    }
                    else if (_isMoving)
                    {
                        _lastDx = _oldMousePoint.X - _startMousePoint.X;
                        _lastDy = _oldMousePoint.Y - _startMousePoint.Y;
                        DrawDefault(e.Graphics);
                    }
                    else
                    {
                        _lastDx = 0;
                        _lastDy = 0;
                        e.Graphics.DrawImage(_lastBitMap, 0, 0);
                    }
                }
                else
                {
                    _lastDx = 0;
                    _lastDy = 0;
                    base.OnPaint(e);
                }
            }
            finally
            {
                _cancelRefresh = RefreahState.None;
            }
        }
        private int _lastDx = 0, _lastDy = 0;
        private void DrawDefault(Graphics g)
        {
            if (_lastBitMap != null)
            {
                g.TranslateTransform(_lastDx, _lastDy);
                g.DrawImage(_lastBitMap, 0, 0);
            }
        }
        #endregion

        #region 事件
        //private void MapControl_SizeChanged(object sender, EventArgs e)
        //{
        //    UpdateSize();
        //    this.Refresh();
        //}
        protected override void OnResize(EventArgs e)
        {
            UpdateSize();
            //ClearLastBitMap();
            base.OnResize(e);
        }
        //         protected override void OnSizeChanged(EventArgs e)
        //         {
        //             base.OnSizeChanged(e);
        //         }
        protected override void OnHandleDestroyed(EventArgs e)
        {
            _invalidateThread.Abort();
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
                        this.RefreshX();
                    }
                }
            }
        }
        public new void Refresh()
        {
            RefreshX();
        }
        private void RefreshX()
        {
            _invalidateResetEvent.Set();
        }
        private void CancelRefresh()
        {
            Console.WriteLine("CancelRefresh..");
            _invalidateResetEvent.Reset();
            if (_cancelRefresh == RefreahState.Refreshing)
            {
                _cancelRefresh = RefreahState.Cancelfreshing;
            }
        }
        private bool CheckNeedPaintReturn()
        {
            return (_cancelRefresh == RefreahState.Cancelfreshing);
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
                this.RefreshX();
            }
        }

        private void MapControl_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0 && _scale >= 1/0.01)
            {
                return;
            }
            timerWheelRefresh.Stop();
            CancelRefresh();
            MapPoint mapPoint = ToMapPoint(e.Location);
            if (e.Delta > 0)//放大
            {
                double left = (_extent.left + mapPoint.x) / 2;
                double top = (_extent.top + mapPoint.y) / 2;
                double bottom = (mapPoint.y + _extent.bottom) / 2;
                double right = (mapPoint.x + _extent.right) / 2;
                _extent = new MapExtent(left, top, right, bottom);
            }
            else if (e.Delta < 0)//缩小
            {
                double left = 2 * _extent.left - mapPoint.x;
                double top = 2 * _extent.top - mapPoint.y;
                double bottom = 2 * _extent.bottom - mapPoint.y;
                double right = 2 * _extent.right - mapPoint.x;
                _extent = new MapExtent(left, top, right, bottom);
            }
            UpdateScale();
            timerWheelRefresh.Start();
        }
        #endregion

        private void timerWheelRefresh_Tick(object sender, EventArgs e)
        {
            timerWheelRefresh.Stop();
            this.RefreshX();
        }

    }
}
