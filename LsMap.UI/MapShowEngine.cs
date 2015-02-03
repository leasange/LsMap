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
    internal class MapShowEngine
    {
        private List<MapImage> _mapImages = new List<MapImage>();
        private Bitmap _lastBitmap = null;//最后一次显示的图片
	    public System.Drawing.Bitmap LastBitmap
	    {
		    get { return _lastBitmap; }
	    }
        private AutoResetEvent _updateResetEvent = new AutoResetEvent(false);
        private AutoResetEvent _newupdateResetEvent = new AutoResetEvent(false);
        private Thread _updateThread = null;
        public event EventHandler ShowUpdated = null;
        private bool _enableUpdate = true;//是否是激活更新状态，当非激活状态时，一切插入、更新操作无效
        public bool EnableUpdate
        {
            get { return _enableUpdate; }
            set { _enableUpdate = value; }
        }
        private bool _isCancelPaint = false;
        private bool _isPainting = false;
        public MapShowEngine()
        {
            _updateThread = new Thread(new ThreadStart(UpdateShow));
            _updateThread.IsBackground = true;
            _updateThread.Start();
        }
        private void UpdateShow()
        {
            while (true)
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
                        _lastBitmap = new Bitmap(_mapImages[0].image.Width, _mapImages[0].image.Height);
                        Graphics g = Graphics.FromImage(_lastBitmap);
                        foreach (MapImage item in _mapImages)
                        {
                            if (_isCancelPaint)
                            {
                                return;
                            }
                            if (item.canShow)
                            {
                                g.DrawImage(item.image, 0, 0);
                            }
                        }
                        if (_isCancelPaint)
                        {
                            return;
                        }
                        if (ShowUpdated != null)
                        {
                            ShowUpdated(this, new EventArgs());
                        }
                    }
                }
                finally
                {
                    _isCancelPaint = false;
                    _isPainting = false;
                    _newupdateResetEvent.Set();
                }
            }
        }
        public int Insert(int id, Bitmap bitmap, bool canShow, bool isCompelete)//添加新图片
        {
            if (!_enableUpdate)
            {
                return 0;
            } 
            lock (this)
            {
                AbortPaintWait();
                MapImage mapImage = new MapImage(bitmap, canShow, isCompelete);
                int index = 0;
                if (id == -1)
                {
                    _mapImages.Add(mapImage);
                    mapImage.id = _mapImages.Count-1;
                    index = mapImage.id;
                }
                else
                {
                    mapImage.id = id;
                    for (int i = _mapImages.Count-1; i >= 0; i--)
                    {
                        if (_mapImages[i].id<id)
                        {
                            index = i+1;
                            break;
                        }
                    }
                    _mapImages.Insert(index, mapImage);
                }
                if (isCompelete)
                {
                    CombineCompelete(mapImage, index);
                }
                if (canShow&&bitmap!=null)
                {
                    _updateResetEvent.Set();
                }
                return mapImage.id;
            }
        }
        public bool Combine(int id, Bitmap bitmap, bool canShow, bool isCompelete)
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
                if (isCompelete)
                {
                    CombineCompelete(mapImage, index);
                }
                if (canShow)
                {
                    _updateResetEvent.Set();
                }
                return true;
            }
        }
        public bool Replace(int id, Bitmap bitmap, bool canShow, bool isCompelete)
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
                if (isCompelete)
                {
                    CombineCompelete(mapImage, index);
                }
                if (canShow)
                {
                    _updateResetEvent.Set();
                }
                return true;
            }
        }
        public MapImage Find(int id,out int index)
        {
            index = -1;
            for (int i = 0; i < _mapImages.Count; i++)
            {
                if (_mapImages[i].id == id)
                {
                    index = i;
                    return _mapImages[i];
                }
            }
            return null;
        }
        //合并完成项
        private void CombineCompelete(MapImage mapImage,int index)
        {
            if (mapImage==null)
            {
                return;
            }
            if (index<0)
            {
                index = _mapImages.Count - 1;
            }
            for (int i = index-1; i >=0; i--)
            {
                if (!_mapImages[i].isCompelete)
                {
                    break;
                }
                if (_mapImages[i].id == mapImage.id - 1 || _mapImages[i].combineIds.Contains(mapImage.id - 1))
                {
                    Combine(_mapImages[i].image, mapImage.image);
                    mapImage.image.Dispose();
                    mapImage.image = _mapImages[i].image;

                    mapImage.combineIds.Add(_mapImages[i].id);
                    mapImage.combineIds.AddRange(_mapImages[i].combineIds);

                    _mapImages[i].image = null;
                    _mapImages[i].Dispose();
                    _mapImages.RemoveAt(i);
                    index--;
                }
                else
                {
                    break;
                }
            }
            for (int i = index + 1; i <_mapImages.Count; i++)
            {
                if (!_mapImages[i].isCompelete)
                {
                    break;
                }
                if (_mapImages[i].id == mapImage.id + 1 || _mapImages[i].combineIds.Contains(mapImage.id + 1))
                {
                    Combine(mapImage.image, _mapImages[i].image);

                    mapImage.combineIds.Add(_mapImages[i].id);
                    mapImage.combineIds.AddRange(_mapImages[i].combineIds);

                    _mapImages[i].Dispose();
                    _mapImages.RemoveAt(i);
                    i--;
                }
                else
                {
                    break;
                }
            }
        }
        private void Combine(Bitmap dstBmp, Bitmap srcBmp)
        {
            Graphics g = Graphics.FromImage(dstBmp);
            g.DrawImage(srcBmp,0,0);
            g.Dispose();
        }
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
            }
        }
        private void AbortPaintWait()
        {
            _isCancelPaint = true;
            if (_isPainting)
            {
                _newupdateResetEvent.WaitOne();
            }
        }
    }
    internal class MapImage:IDisposable
    {
        public Bitmap image = null;//图片
        public int id = 0;//编号
        public bool canShow=true;//是否可视
        public bool isCompelete = true;//是否完成
        public List<int> combineIds = new List<int>();//混合ID
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
