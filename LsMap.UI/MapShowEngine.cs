using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Threading;

namespace LsMap.UI
{
    /// <summary>
    /// 地图显示引擎
    /// </summary>
    internal class MapShowEngine:IDisposable
    {
        //图片列表，排在前面的先绘制，依次叠加显示
        private List<MapImage> _mapImages = new List<MapImage>();
        private int _dx = 0, _dy = 0;
        //最后一次显示的图片
        private Bitmap _lastBitmap = null;
	    public System.Drawing.Bitmap LastBitmap
	    {
		    get { return _lastBitmap; }
	    }
        //绘制线程同步位
        private AutoResetEvent _updateResetEvent = new AutoResetEvent(false);
        //等待绘制结束同步位
        private AutoResetEvent _newupdateResetEvent = new AutoResetEvent(false);
        //绘制线程
        private Thread _updateThread = null;
        //需要更新绘制事件
        public event EventHandler ShowUpdated = null;
        //是否激活绘制状态
        private bool _enableUpdate = true;//是否是激活更新状态，当非激活状态时，一切插入、更新操作无效
        public bool EnableUpdate
        {
            get { return _enableUpdate; }
            set { _enableUpdate = value; }
        }
        //是否取消绘制状态位
        private bool _isCancelPaint = false;
        //是否绘制中状态位
        private bool _isPainting = false;
        public MapShowEngine()
        {
            _updateThread = new Thread(new ThreadStart(UpdateShow));
            _updateThread.IsBackground = true;
            _updateThread.Priority = ThreadPriority.Highest;
            _updateThread.Start();
        }
        //绘制线程
        private void UpdateShow()
        {
            while (true)
            {
                DoUpdateShow();
            }
        }
        //开始绘制
        private void DoUpdateShow()
        {
            try
            {
                _updateResetEvent.WaitOne();
                if (_lastBitmap != null)
                {
                    _lastBitmap.Dispose();
                    _lastBitmap = null;
                }
                if (_mapImages.Count > 0)
                {
                    _lastBitmap = new Bitmap(_mapImages[_mapImages.Count - 1].image.Width, _mapImages[_mapImages.Count - 1].image.Height);
                    Graphics g = null;
                    try
                    {
                        g = Graphics.FromImage(_lastBitmap);
                        for (int i = _mapImages.Count-1; i >=0; i--)
                        {
                            MapImage item = _mapImages[i];
                            if (_isCancelPaint)
                            {
                                return;
                            }
                            if (item.canShow)
                            {
                                if (item.isOld)
                                {
                                    g.TranslateTransform(_dx, _dy);
                                    g.DrawImage(item.image, 0, 0);
                                    g.ResetTransform();
                                }
                                else
                                {
                                    g.DrawImage(item.image, 0, 0);
                                }
                            }
                        }
                        g.Dispose();
                        g = null;
                        if (_isCancelPaint)
                        {
                            return;
                        }
                    }
                    finally
                    {
                        if (g!=null)
                        {
                            g.Dispose();
                        }
                    }
                }
                if (ShowUpdated != null)
                {
                    ShowUpdated(this, new EventArgs());
                }
            }
            finally
            {
                _isCancelPaint = false;
                _isPainting = false;
                _newupdateResetEvent.Set();
            }
        }
        //进入绘制状态
        private void SetPaintState()
        {
            _isPainting = true;
            _updateResetEvent.Set();
            _newupdateResetEvent.Reset();
        }
        //在窗口上显示
        public void Show(Graphics g,int dx=0,int dy=0)
        {
            if (_lastBitmap==null)
            {
                return;
            }
            _dx = dx;
            _dy = dy;
            if (dx!=0||dy!=0)
            {
                g.TranslateTransform(dx, dy);
            }
            g.DrawImage(_lastBitmap, 0, 0);
        }
        //插入新图片
        public ulong Insert(ulong id,int layerIndex, Bitmap bitmap, bool canShow, bool isCompelete)
        {
            lock (this)
            {
                if (!_enableUpdate)
                {
                    return 0;
                }
                MapImage mapImage = null;
                AbortPaintWait();
                int index = 0;
                mapImage = Find(id, out index);
                if (mapImage != null)
                {
                    mapImage.image.Dispose();
                    mapImage.image = bitmap;
                    mapImage.canShow = canShow;
                    mapImage.isCompelete = isCompelete;
                    mapImage.isOld = false;
                    if (mapImage.index != layerIndex)
                    {
                        mapImage.index = layerIndex;
                        _mapImages.Sort(Compare);
                    }
                    Console.WriteLine("Insert Find:" + mapImage.index);
                }
                else
                {
                    mapImage = new MapImage(bitmap, canShow, isCompelete);
                    index = 0;
                    mapImage.id = id;
                    mapImage.index = layerIndex;
                    for (int i = _mapImages.Count - 1; i >= 0; i--)
                    {
                        if (_mapImages[i].index <= layerIndex)
                        {
                            index = i + 1;
                            break;
                        }
                    }
                    _mapImages.Insert(index, mapImage);
                    Console.WriteLine("Insert:" + mapImage.index);
                }
                //                 if (isCompelete)
                //                 {
                //                     CombineCompelete(mapImage, index);
                //                 }
                if (canShow && bitmap != null)
                {
                    SetPaintState();
                }
                return mapImage.id;
            }
        }
        //设置图片先后顺序
        public void SetIndex(List<ulong> ids, List<int> layerIndexs)
        {
            lock (this)
            {
                AbortPaintWait();
                for (int i = 0; i < ids.Count; i++)
                {
                    int index;
                    MapImage image = Find(ids[i], out index);
                    if (image != null)
                    {
                        image.index = layerIndexs[i];
                    }
                }
                _mapImages.Sort(Compare);
                SetPaintState();
            }
        }
        //比较图片先后顺序
        private int Compare(MapImage x, MapImage y)
        {
            if (x.index>y.index)
            {
                return 1;
            }
            else if(x.index<y.index)
            {
                return -1;
            }else return 0;
        }
        //合并图片
        public bool Combine(ulong id, Bitmap bitmap, bool canShow, bool isCompelete)
        {
            lock (this)
            {
                if (!_enableUpdate)
                {
                    return false;
                }
                AbortPaintWait();
                int index = -1;
                MapImage mapImage = Find(id, out index);
                if (mapImage == null)
                {
                    return false;
                }
                if (mapImage.image!=null)
                {
                    Combine(mapImage.image, bitmap);
                    bitmap.Dispose();
                }
                else
                {
                    mapImage.image = bitmap;
                }
                mapImage.canShow = canShow;
                mapImage.isCompelete = isCompelete;
//                 if (isCompelete)
//                 {
//                     CombineCompelete(mapImage, index);
//                 }
                if (canShow)
                {
                    SetPaintState();
                }
                return true;
            }
        }
        //替换图片
        public bool Replace(ulong id, Bitmap bitmap, bool canShow, bool isCompelete)
        {
            if (!_enableUpdate)
            {
                return false;
            }
            lock (this)
            {
                AbortPaintWait();
                int index = -1;
                MapImage mapImage = Find(id, out index);
                if (mapImage == null)
                {
                    return false;
                }
                mapImage.image.Dispose();
                mapImage.image = bitmap;
                mapImage.canShow = canShow;
                mapImage.isCompelete = isCompelete;
//                 if (isCompelete)
//                 {
//                     CombineCompelete(mapImage, index);
//                 }
                if (canShow)
                {
                    SetPaintState();
                }
                return true;
            }
        }
        //查找图片
        public MapImage Find(ulong id, out int index)
        {
            index = -1;
            for (int i = 0; i < _mapImages.Count; i++)
            {
                if (_mapImages[i].id == id)
                {
                    index = _mapImages[i].index;
                    return _mapImages[i];
                }
            }
            return null;
        }
        //合并完成项
//         private void CombineCompelete(MapImage mapImage,int index)
//         {
//             if (mapImage==null)
//             {
//                 return;
//             }
//             if (index<0)
//             {
//                 index = _mapImages.Count - 1;
//             }
//             for (int i = index-1; i >=0; i--)
//             {
//                 if (!_mapImages[i].isCompelete)
//                 {
//                     break;
//                 }
//                 if (_mapImages[i].id == mapImage.id - 1 || _mapImages[i].combineIds.Contains(mapImage.id - 1))
//                 {
//                     Combine(_mapImages[i].image, mapImage.image);
//                     mapImage.image.Dispose();
//                     mapImage.image = _mapImages[i].image;
// 
//                     mapImage.combineIds.Add(_mapImages[i].id);
//                     mapImage.combineIds.AddRange(_mapImages[i].combineIds);
// 
//                     _mapImages[i].image = null;
//                     _mapImages[i].Dispose();
//                     _mapImages.RemoveAt(i);
//                     index--;
//                 }
//                 else
//                 {
//                     break;
//                 }
//             }
//             for (int i = index + 1; i <_mapImages.Count; i++)
//             {
//                 if (!_mapImages[i].isCompelete)
//                 {
//                     break;
//                 }
//                 if (_mapImages[i].id == mapImage.id + 1 || _mapImages[i].combineIds.Contains(mapImage.id + 1))
//                 {
//                     Combine(mapImage.image, _mapImages[i].image);
// 
//                     mapImage.combineIds.Add(_mapImages[i].id);
//                     mapImage.combineIds.AddRange(_mapImages[i].combineIds);
// 
//                     _mapImages[i].Dispose();
//                     _mapImages.RemoveAt(i);
//                     i--;
//                 }
//                 else
//                 {
//                     break;
//                 }
//             }
//         }
        //合并两张图片
        private void Combine(Bitmap dstBmp, Bitmap srcBmp)
        {
            Graphics g = Graphics.FromImage(dstBmp);
            g.DrawImage(srcBmp,0,0);
            g.Dispose();
        }
        //清除所有图片
        public void Clear()
        {
            lock (this)
            {
                AbortPaintWait();
                foreach (MapImage item in _mapImages)
                {
                    item.Dispose();
                }
                _mapImages.Clear();
//                 if (_lastBitmap!=null)
//                 {
//                     _lastBitmap.Dispose();
//                     _lastBitmap = null;
//                 }
            }
        }
        //终止绘制，并等待终止
        private void AbortPaintWait()
        {
            _isCancelPaint = true;
            if (_isPainting)
            {
                _newupdateResetEvent.WaitOne();
            }
            _isCancelPaint = false;
        }
        //销毁当前对象
        public void Dispose()
        {
            Clear();
        }
        //移除指定图层
        internal void Remove(ulong id)
        {
            lock (this)
            {
                AbortPaintWait();
                foreach (MapImage item in _mapImages)
                {
                    if (item.id==id)
                    {
                        _mapImages.Remove(item);
                       if (item.canShow)
                       {
                           SetPaintState();
                       }
                       item.Dispose();
                       break;
                    }
                }
            }
        }

        public void InsertOrCombine(List<ulong> ids, List<int> indexs, List<Bitmap> bitmaps,List<bool> isInOrComs, bool canShow, bool isComplete)
        {
            lock (this)
            {
                if (!_enableUpdate)
                {
                    return;
                }
                AbortPaintWait();
                int ind;
                for (int i = 0; i < ids.Count; i++)
                {
                    MapImage mapImage = Find(ids[i], out ind);
                    if (mapImage != null)
                    {
                        //Console.WriteLine("InsertOrCombine:Find 1:" + mapImage.index);
                        if (isInOrComs[i])
                        {
                            mapImage.image.Dispose();
                            mapImage.image = bitmaps[i];
                        }
                        else
                        {
                            Combine(mapImage.image, bitmaps[i]);
                            bitmaps[i].Dispose();
                        }
                        mapImage.isOld = false;
                        mapImage.index = indexs[i];
                       // Console.WriteLine("InsertOrCombine:Find 2:" + mapImage.index);
                        mapImage.canShow = canShow;
                        mapImage.isCompelete = isComplete;
                    }
                    else
                    {
                        mapImage = new MapImage(bitmaps[i], canShow, isComplete);
                        ind = 0;
                        mapImage.id = ids[i];
                        mapImage.index = indexs[i];
                        for (int j = _mapImages.Count - 1; j >= 0; j--)
                        {
                            if (_mapImages[j].index <= indexs[i])
                            {
                                ind = j + 1;
                                break;
                            }
                        }
                        _mapImages.Insert(ind, mapImage);
                       // Console.WriteLine("InsertOrCombine:Insert:" + mapImage.index);
                    }
                }
                _mapImages.Sort(Compare);
                if (canShow)
                {
                    SetPaintState();
                }
            }
        }

        //将现有图片全部设置为过期图片
        public void SetAllOld()
        {
            lock (this)
            {
                foreach (var item in _mapImages)
                {
                    item.isOld = true;
                }
            }
        }
    }
    internal class MapImage:IDisposable
    {
        public Bitmap image = null;//图片
        public ulong id = 0;//编号
        public int index = 0;//图层顺序
        public bool canShow=true;//是否可视
        public bool isCompelete = true;//是否完成
        public bool isOld = false;//是否已过期
        //public List<int> combineIds = new List<int>();//混合ID
        public MapImage(Bitmap image,bool canShow,bool isCompelete)
        {
            this.image = image;
            this.canShow=canShow;
            this.isCompelete = isCompelete;
        }
        public void Dispose()
        {
            if (image!=null)
            {
                image.Dispose();
                image = null;
            }
            id = 0;
        }
    }
}
