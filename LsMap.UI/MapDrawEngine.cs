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

        public void InsertTask(DrawLayerTask task)
        {
            lock (this)
            {
                foreach (DrawLayerTask item in tasks)
                {
                    if (item.layer.LayerID == task.layer.LayerID)
                    {
                        RemoveTask(item);
                        break;
                    }
                }
                tasks.Add(task);
                task.TaskComplete += task_TaskComplete;
                task.TaskUpdate += task_TaskUpdate;
            }
        }

        private void task_TaskUpdate(object sender, DrawTaskEventArgs e)
        {
            DrawLayerTask task = (DrawLayerTask)sender;
            bool canShow = true;
            lock (this)
            {
                int index = tasks.IndexOf(task);
                for (int i = 0; i < index; i++)
                {
                    if (tasks[i].layer.Datatable.TableType != DatatableType.Point && tasks[i].layer.Datatable.TableType!=DatatableType.Line)
                    {
                        canShow = false;
                        break;
                    }
                }
            }
            if (task.state == 0)//初始状态
            {
                showEngine.Insert(task.layer.LayerID, task.layerIndex, e.bitmap, canShow, false);
                task.state = 1;
            }
            else
            {
                showEngine.Combine(task.layer.LayerID, e.bitmap, canShow, false);
            }
        }

        private void task_TaskComplete(object sender, DrawTaskEventArgs e)
        {
            lock (this)
            {
                DrawLayerTask task = (DrawLayerTask)sender;
                List<DrawLayerTask> ptasks = new List<DrawLayerTask>();
                Console.WriteLine("task_TaskComplete:" + task.layer.AliasName);
//                 if (task.layer.DatatableName=="point")
//                 {
//                     Console.WriteLine("task_TaskComplete:" + tasks.Count);
//                 }
                if (e != null)
                {
                    int index = tasks.Count - 1;
                    if (task.state == 0)
                    {
                        task.state = 3;//insert
                    }
                    else
                    {
                        task.state = 2;//combine
                    }
                    task.drawBitmap = e.bitmap;
                    int lastIndex = -1;
                    for (int i = 0; i < tasks.Count; i++)
                    {
                        if (tasks[i] != task)
                        {
                            if (i < index)
                            {
                                if (tasks[i].state != 2 && tasks[i].state!=3)
                                {
                                    return;
                                }
                                else
                                {
                                    lastIndex = i;
                                }
                            }
                            else
                            {
                                if (tasks[i].state != 2 && tasks[i].state != 3 && tasks[i].layer.Datatable.TableType != DatatableType.Point && tasks[i].layer.Datatable.TableType != DatatableType.Line)
                                {
                                    return;
                                }
                                else if (tasks[i].state != 2 && tasks[i].state != 3)
                                {
                                    lastIndex = i - 1;
                                    break;
                                }
                                else
                                {
                                    lastIndex = i;
                                }
                            }
                        }
                        else
                        {
                            index = i;
                            lastIndex = i;
                        }
                    }

//                     index = tasks.IndexOf(task);
//                     if (index != 0)
//                     {
//                         if (task.state==0)
//                         {
//                             task.state = 3;
//                         }
//                         else task.state = 2;
//                         task.drawBitmap = e.bitmap;
//                         return;
//                     }
//                     else
//                     {
                        //ptasks.Add(task);
                        List<ulong> ids = new List<ulong>();
                        List<int> indexs = new List<int>();
                        List<Bitmap> bitmaps = new List<Bitmap>();
                        List<bool> isInOrComs = new List<bool>();

//                         ids.Add(task.layer.LayerID);
//                         indexs.Add(task.layerIndex);
//                         bitmaps.Add(e.bitmap);
//                         if (task.state==0)
//                         {
//                             isInOrComs.Add(true);
//                         }
//                         else
//                         {
//                             isInOrComs.Add(false);
//                         }

                        for (int i = 0; i <=/*tasks.Count-1*/lastIndex; i++)
                        {
//                             if (tasks[i].state!=2&&tasks[i].state!=3)
//                             {
//                                 break;
//                             }
//                             else
//                             {
                                ids.Add(tasks[i].layer.LayerID);
                                indexs.Add(tasks[i].layerIndex);
                                bitmaps.Add(tasks[i].drawBitmap);
                                tasks[i].drawBitmap = null;
                                ptasks.Add(tasks[i]);
                                if (tasks[i].state==3)
                                {
                                    isInOrComs.Add(true);
                                }
                                else
                                {
                                    isInOrComs.Add(false);
                                }
                                Console.WriteLine("complete :" + tasks[i].layer.AliasName);
                            //}
                        }

                        showEngine.InsertOrCombine(ids, indexs, bitmaps, isInOrComs, true, true);
//                         if (task.state == 0)
//                         {
//                             showEngine.Insert(task.layer.LayerID, task.layerIndex, e.bitmap, e.canShow, true);
//                             task.state = 2;
//                         }
//                         else showEngine.Combine(task.layer.LayerID, e.bitmap, e.canShow, true);
                    //}
                }
                else
                {
                    ptasks.Add(task);
                    showEngine.Remove(task.layer.LayerID);
                }
                RemoveTask(ptasks.ToArray());
            }
        }

        public void ClearDrawAndShow()
        {
            ClearTask();
            showEngine.SetAllOld();
            //showEngine.Clear();
        }

        public void RemoveTask(params DrawLayerTask[] ptasks)
        {
            lock (this)
            {
                if (tasks!=null)
                {
                    foreach (var task in ptasks)
                    {
                        task.TaskComplete -= task_TaskComplete;
                        task.TaskUpdate -= task_TaskUpdate;
                        tasks.Remove(task);
                       // Console.WriteLine("removetask:" + task.layer.Datatable.TableType);
                        task.Dispose();
                    }
                }
            }
        }

        public void ClearTask()
        {
            lock (this)
            {
                foreach (DrawLayerTask item in tasks)
                {
                    item.TaskComplete -= task_TaskComplete;
                    item.TaskUpdate -= task_TaskUpdate;
                    item.Dispose();
                }
                tasks.Clear();
            }
        }

        public void Dispose()
        {
            ClearTask();
        }

        public void StartAllTask()
        {
            lock (this)
            {
                foreach (var item in tasks)
                {
                    item.Start();
                }
            }
        }
    }
    internal abstract class DrawLayerTask:IDisposable
    {
        public Workspace.Workspace workspace;
        public int width=0;
        public int height = 0;
        public MapExtent extent;
        public int state = -1;//-1 退出状态 0 运行状态  1 运行结束状态 2 完成状态1 3 完成状态2
        public int MAX_ONCE_COUNT = 2000;
        public Layer layer;
        public int layerIndex;
        public Bitmap drawBitmap = null;
        internal List<AsyncThread> asyncThreads = new List<AsyncThread>();
        public event EventHandler<DrawTaskEventArgs> TaskComplete;
        public event EventHandler<DrawTaskEventArgs> TaskUpdate;
        public DrawLayerTask(Workspace.Workspace workspace, Layer layer, MapExtent extent, int width, int height)
        {
            this.workspace=workspace;
            this.layer = layer;
            this.width = width;
            this.height = height;
            this.extent = extent;
        }
        public void Start()
        {
            if (state==-1)
            {
                state = 0;
                ThreadPool.QueueUserWorkItem(new WaitCallback((obj) =>
                {
                    Run();
                }));
            }
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
        internal virtual void CountThread(List<Datarow> rows, out int threadCount, out int onceCount)
        {
            threadCount = 0;
            onceCount = 0;
            if (rows != null)
            {
                threadCount = (int)Math.Ceiling(rows.Count / (double)MAX_ONCE_COUNT);
                if (threadCount == 0)
                {
                    return;
                }
                onceCount = (int)Math.Ceiling(rows.Count / (double)threadCount);
            }
        }
        internal virtual void Run()
        {
            DrawTaskEventArgs lastArgs = null;
            try
            {
                if (layer == null||!layer.Visible)
                {
                    return;
                }
                //计算范围
                List<Datarow> rows = layer.Datatable.Query(extent);
                int threadCount = 0;
                int onceCount = 0;
                CountThread(rows, out threadCount, out onceCount);
                if (threadCount == 0)
                {
                    return;
                }
                for (int i = 0; i < threadCount; i++)
                {
                    if (this.state == -1)
                    {
                        return;
                    }
                    int start = i * onceCount;
                    int end = (i + 1) * onceCount;
                    if (end > rows.Count)
                    {
                        end = rows.Count;
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
                            Draw(g, rows[j]);
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
            }
            finally
            {
                asyncThreads.Clear();
                DoTaskCompleteEvent(lastArgs);
            }
        }
        internal abstract void Draw(Graphics g, Datarow row);
        public virtual void Dispose()
        {
            this.state = -1;
            if (drawBitmap!=null)
            {
                drawBitmap.Dispose();
                drawBitmap = null;
            }
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
            switch (this.layer.Datatable.TableType)
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
        //private static int countOfThread=0;
        private Thread thCall = null;
        public AsyncThread()
        {}
        public void Start(object state)
        {
            thCall = new Thread(DoCall);
            thCall.IsBackground = true;
            thCall.Start(state);
            //ThreadPool.QueueUserWorkItem(DoCall, state);
        }
        public void DoCall(object state)
        {
            try
            {
                //countOfThread++;
                if (Call != null)
                {
                    Call(state);
                }
                autoReset.Set();
            }
            catch
            { }
//             finally
//             {
//                 countOfThread--;
//                 Console.WriteLine("线程数:" + countOfThread);
//             }
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
            if (thCall != null && thCall.IsAlive)
            {
                thCall.Abort();
            }
        }
    }
}
