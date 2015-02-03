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
                showEngine.Combine(task.id, e.bitmap, e.canShow, true);
                task.TaskComplete -= task_TaskComplete;
                task.TaskUpdate -= task_TaskUpdate;
                tasks.Remove(task);
                task.Dispose();
            }
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
        public Layer layer;
        public abstract event EventHandler<DrawTaskEventArgs> TaskComplete;
        public abstract event EventHandler<DrawTaskEventArgs> TaskUpdate;
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
            ThreadPool.QueueUserWorkItem(new WaitCallback((o)=>
                {
                    Run();
                }));
        }
        internal abstract void Run();
        public abstract void Dispose();
    }
    internal class DrawPointTask : DrawLayerTask
    {
        public DrawPointTask(Workspace.Workspace workspace, PointLayer layer, MapExtent extent, int width, int height)
            : base(workspace,layer,extent,width,height)
        {
        }

        public override void Dispose()
        {
            
        }

        internal override void Run()
        {
            if (layer != null)
            {
                LsMap.Data.Datatable dt = MapDrawHelper.GetTable(workspace,layer);
                if (dt != null)
                {
                    try
                    {
                        List<AutoResetEvent> autoResets = new List<AutoResetEvent>();
                        int threadCount = 4;
                        int c = (int)Math.Ceiling(dt.Datarows.Count / (double)threadCount);
                        for (int i = 0; i < threadCount; i++)
                        {
                            int start = i * c;
                            int end = (i + 1) * c;
                            if (end > dt.Datarows.Count)
                            {
                                end = dt.Datarows.Count;
                            }
                            AutoResetEvent autoReset = new AutoResetEvent(false);
                            autoResets.Add(autoReset);
                            Thread th = new Thread(new ThreadStart(() =>
                            {
                                Bitmap bitmap = new Bitmap(this.width, this.height);
                                Graphics g = Graphics.FromImage(bitmap);
                                for (int j = start; j < end; j++)
                                {
                                    if (this.state==-1)
                                    {
                                        g.Dispose();
                                        bitmap.Dispose();
                                        autoReset.Set();
                                        return;
                                    }
                                    PointF point = MapDrawHelper.ToScreenPoint((MapPoint)dt.Datarows[j].Data,extent,width,height);
                                    g.DrawImage(Properties.Resources.qiuji_online, point.X, point.Y, 12, 14);

                                }
                                g.Dispose();
                                DrawTaskEventArgs args = new DrawTaskEventArgs(bitmap, true);
                                if (autoResets.Count==threadCount)
                                {
                                    if (TaskComplete != null)
                                    {
                                        TaskComplete(this, args);
                                    }
                                }
                                else
                                {
                                    if (TaskUpdate!=null)
                                    {
                                        TaskUpdate(this, args);
                                    }
                                }
                                autoReset.Set();
                            }));
                            th.IsBackground = true;
                            th.Start();

                        }
                        foreach (AutoResetEvent item in autoResets)
                        {
                            item.WaitOne();
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }

        public override event EventHandler<DrawTaskEventArgs> TaskComplete;

        public override event EventHandler<DrawTaskEventArgs> TaskUpdate;
    }
    internal class DrawLineTask : DrawLayerTask
    {
        public DrawLineTask(Workspace.Workspace workspace, LineLayer layer, MapExtent extent, int width, int height)
            : base(workspace,layer,extent,width,height)
        {
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        internal override void Run()
        {
            throw new NotImplementedException();
        }

        public override event EventHandler<DrawTaskEventArgs> TaskComplete;

        public override event EventHandler<DrawTaskEventArgs> TaskUpdate;
    }
    internal class DrawPolygonTask : DrawLayerTask
    {
        public DrawPolygonTask(Workspace.Workspace workspace, PolygonLayer layer, MapExtent extent, int width, int height)
            : base(workspace,layer,extent,width,height)
        {
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        internal override void Run()
        {
            throw new NotImplementedException();
        }

        public override event EventHandler<DrawTaskEventArgs> TaskComplete;

        public override event EventHandler<DrawTaskEventArgs> TaskUpdate;
    }
    internal class DrawRasterTask : DrawLayerTask
    {
        public DrawRasterTask(Workspace.Workspace workspace, RasterLayer layer, MapExtent extent, int width, int height)
            : base(workspace,layer,extent,width,height)
        {
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        internal override void Run()
        {
            throw new NotImplementedException();
        }

        public override event EventHandler<DrawTaskEventArgs> TaskComplete;

        public override event EventHandler<DrawTaskEventArgs> TaskUpdate;
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
    internal class AsyncThread
    {
        public AsyncThread()
        {}
        public void Start(WaitCallback ts)
        {
            ThreadPool.QueueUserWorkItem(ts);
        }
    }
}
