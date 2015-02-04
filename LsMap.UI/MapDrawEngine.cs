using LsMap.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing;
using LsMap.Data;

namespace LsMap.UI
{
    /// <summary>
    /// 地图绘制引擎
    /// </summary>
    internal class MapDrawEngine:IDisposable
    {
        private List<DrawLayerTask> tasks = new List<DrawLayerTask>();
        private MapShowEngine showEngine = null;
        public MapDrawEngine(MapShowEngine showEngine)
        {
            this.showEngine = showEngine;
        }
        public void InsertTask(int index,DrawLayerTask task)
        {
            lock (this)
            {
                if (index == -1)
                {
                    tasks.Add(task);
                }
                else
                {
                    tasks.Insert(index, task);
                }
                task.TaskComplete += task_TaskComplete;
                task.TaskUpdate += task_TaskUpdate;
                task.Start();
            }
        }

        private void task_TaskUpdate(object sender, DrawTaskEventArgs e)
        {
            lock (this)
            {
                DrawLayerTask task = (DrawLayerTask)sender;
                if (task.state==0)//初始状态
                {
                    showEngine.Insert(task.id,e.bitmap,e.canShow,false);
                    task.state = 1;
                }
                else
                {
                    showEngine.Combine(task.id, e.bitmap, e.canShow, false);
                }
            }
        }

        private void task_TaskComplete(object sender, DrawTaskEventArgs e)
        {
            lock (this)
            {
                DrawLayerTask task = (DrawLayerTask)sender;
                if (task.state==0)
                {
                    showEngine.Insert(task.id, e.bitmap, e.canShow, true);
                    task.state = 1;
                }
                else showEngine.Combine(task.id, e.bitmap, e.canShow, true);
                task.TaskComplete -= task_TaskComplete;
                task.TaskUpdate -= task_TaskUpdate;
                tasks.Remove(task);
                task.Dispose();
            }
        }

        public void CancelDrawAndShow()
        {
            ClearTask();
            showEngine.Clear();
        }

        public void ClearTask()
        {
            lock (this)
            {
                foreach (DrawLayerTask item in tasks)
                {
                    item.Dispose();
                }
                tasks.Clear();
                DrawLayerTask.ResetId();
            }
        }
        public void Dispose()
        {
            ClearTask();
        }
    }
    internal abstract class DrawLayerTask:IDisposable
    {
        private static int index = 0;
        public Workspace.Workspace workspace;
        public int width=0;
        public int height = 0;
        public MapExtent extent;
        public int id = -1;
        public int state = 0;//0 初始状态 -1 终止状态
        public int MAX_ONCE_COUNT = 1000;
        
        public Layer layer;
        internal LsMap.Data.Datatable dataTable;
        internal List<AsyncThread> asyncThreads = new List<AsyncThread>();
        public event EventHandler<DrawTaskEventArgs> TaskComplete;
        public event EventHandler<DrawTaskEventArgs> TaskUpdate;
        public DrawLayerTask(Workspace.Workspace workspace, Layer layer, MapExtent extent, int width, int height)
        {
            this.workspace=workspace;
            this.layer = layer;
            id = index++;
            this.width = width;
            this.height = height;
            this.extent = extent;
        }
        public static void ResetId()
        {
            index = 0;
        }
        public void Start()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback((obj)=>
                {
                    Run();
                }));
        }
        internal void DoTaskCompleteEvent(DrawTaskEventArgs e)
        {
            if (TaskComplete!=null)
            {
                TaskComplete(this,e);
            }
        }
        internal void DoTaskUpdateEvent(DrawTaskEventArgs e)
        {
            if (TaskUpdate != null)
            {
                TaskUpdate(this, e);
            }
        }
        internal virtual void CountThread(LsMap.Data.Datatable dt, out int threadCount, out int onceCount)
        {
            threadCount = 0;
            onceCount = 0;
            if (dt != null)
            {
                threadCount = (int)Math.Ceiling(dt.Datarows.Count / (double)MAX_ONCE_COUNT);
                if (threadCount == 0)
                {
                    return;
                }
                onceCount = (int)Math.Ceiling(dt.Datarows.Count / (double)threadCount);
            }
        }
        internal virtual void Run()
        {
            if (layer == null)
            {
                return;
            }
            dataTable = MapDrawHelper.GetTable(workspace, layer);
            int threadCount = 0;
            int onceCount = 0;
            CountThread(dataTable, out threadCount, out onceCount);
            if (threadCount == 0)
            {
                return;
            }
            DrawTaskEventArgs lastArgs = null;
            for (int i = 0; i < threadCount; i++)
            {
                int start = i * onceCount;
                int end = (i + 1) * onceCount;
                if (end > dataTable.Datarows.Count)
                {
                    end = dataTable.Datarows.Count;
                }
                AsyncThread asyncth = new AsyncThread();
                asyncth.Call += delegate(object o)
                {
                    Bitmap bitmap = new Bitmap(this.width, this.height);
                    Graphics g = Graphics.FromImage(bitmap);
                    for (int j = start; j < end; j++)
                    {
                        if (this.state == -1)
                        {
                            g.Dispose();
                            bitmap.Dispose();
                            return;
                        }
                        Draw(g, dataTable.Datarows[j]);
                    }
                    g.Dispose();
                    DrawTaskEventArgs args = new DrawTaskEventArgs(bitmap, true);
                    if ((int)o + 1 == threadCount)
                    {
                        lastArgs = args;
                    }
                    else
                    {
                        DoTaskUpdateEvent(args);
                    }
                };
                asyncth.Start(i);
                asyncThreads.Add(asyncth);
            }
            foreach (AsyncThread item in asyncThreads)
            {
                item.Wait();
                item.Dispose();
            }
            asyncThreads.Clear();
            if (lastArgs != null)
            {
                DoTaskCompleteEvent(lastArgs);
            }
        }
        internal abstract void Draw(Graphics g, Datarow row);
        public virtual void Dispose()
        {
            this.state = -1;
        }
    }
    internal class DrawPointTask : DrawLayerTask
    {
        public DrawPointTask(Workspace.Workspace workspace, PointLayer layer, MapExtent extent, int width, int height)
            : base(workspace,layer,extent,width,height)
        {
        }

        internal override void Draw(Graphics g, Datarow row)
        {
            PointF point = MapDrawHelper.ToScreenPoint((MapPoint)row.Data, extent, width, height);
            g.DrawImage(Properties.Resources.qiuji_online, point.X, point.Y, 12, 14);
        }
    }
    internal class DrawLineTask : DrawLayerTask
    {
        public DrawLineTask(Workspace.Workspace workspace, LineLayer layer, MapExtent extent, int width, int height)
            : base(workspace,layer,extent,width,height)
        {
        }

        internal override void Draw(Graphics g, Datarow row)
        {
            if (row.Data == null)
            {
                return;
            }
            MapLine mapLine = (MapLine)row.Data;
            List<PointF> screenpoints = new List<PointF>();
            foreach (MapPoint p in mapLine.Points)
            {
                screenpoints.Add(MapDrawHelper.ToScreenPoint(p,extent,width,height));
            }
            g.DrawLines(new Pen(Color.Orange, 2), screenpoints.ToArray());
        }
    }
    internal class DrawPolygonTask : DrawLayerTask
    {
        public DrawPolygonTask(Workspace.Workspace workspace, PolygonLayer layer, MapExtent extent, int width, int height)
            : base(workspace,layer,extent,width,height)
        {
        }

        internal override void Draw(Graphics g, Datarow row)
        {
            if (row.Data == null)
            {
                return;
            }
            MapPolygon mapPolygon = (MapPolygon)row.Data;
            List<PointF> screenpoints = new List<PointF>();
            foreach (MapPoint p in mapPolygon.Points)
            {
                screenpoints.Add(MapDrawHelper.ToScreenPoint(p, extent, width, height));
            }
            g.FillPolygon(new SolidBrush(Color.FromArgb(100, Color.Blue)), screenpoints.ToArray());
        }
    }
    internal class DrawRasterTask : DrawLayerTask
    {
        public DrawRasterTask(Workspace.Workspace workspace, RasterLayer layer, MapExtent extent, int width, int height)
            : base(workspace, layer, extent, width, height)
        {
        }

        internal override void Draw(Graphics g, Datarow row)
        {
            PointF p1 = MapDrawHelper.ToScreenPoint(row.Extent.LeftTop,extent,width,height);
            PointF p2 = MapDrawHelper.ToScreenPoint(row.Extent.RightBottom, extent, width, height);
            Image image = null;
            switch (dataTable.TableType)
            {
                case DatatableType.Raster:
                    if (row.Data is string)
                    {
                        image = Image.FromFile((string)row.Data);
                    }
                    break;
                default:
                    break;
            }
            if (image == null)
            {
                return;
            }
            g.DrawImage(image, p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);
            image.Dispose();
        }
    }
    internal class DrawTaskEventArgs : EventArgs
    {
        public Bitmap bitmap=null;
        public bool canShow = false;
        public DrawTaskEventArgs(Bitmap bitmap, bool canShow)
        {
            this.bitmap = bitmap;
            this.canShow = canShow;
        }
    }
    internal class AsyncThread:IDisposable
    {
        private AutoResetEvent autoReset = new AutoResetEvent(false);
        public event WaitCallback Call = null;
        public AsyncThread()
        {}
        public void Start(object state)
        {
            ThreadPool.QueueUserWorkItem(DoCall, state);
        }
        public void DoCall(object state)
        {
            try
            {
                if (Call != null)
                {
                    Call(state);
                }
                autoReset.Set();
            }
            catch
            {}
        }
        public void Wait()
        {
            try
            {
                autoReset.WaitOne();
            }
            catch
            {}
        }
        public void Dispose()
        {
            autoReset.Dispose();
        }
    }
}
