using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LsMap.Data
{
    //通用集合类
    public class ComCollection<T> : IList<T>, IEnumerable<T>, System.Collections.IEnumerable
    {
        public event EventHandler<ComCollectionArgs<T>> CollectionEvent = null;

        private List<T> _list = new List<T>();
        public int IndexOf(T item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
            OnCollectionEvent(new ComCollectionArgs<T>(item, ComCollectionEventType.Insert, index));
        }

        public void RemoveAt(int index)
        {
            T item=_list[index];
            _list.RemoveAt(index);
            OnCollectionEvent(new ComCollectionArgs<T>(item, ComCollectionEventType.Remove, index));
        }

        public T this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                if (!_list[index].Equals(value))
                {
                    _list[index] = value;
                    OnCollectionEvent(new ComCollectionArgs<T>(_list[index], ComCollectionEventType.Replace, index));
                }
            }
        }

        public void Add(T item)
        {
            _list.Add(item);
            OnCollectionEvent(new ComCollectionArgs<T>(item, ComCollectionEventType.Add, this.Count-1));
        }

        public void Clear()
        {
            _list.Clear();
            OnCollectionEvent(new ComCollectionArgs<T>(default(T), ComCollectionEventType.Clear,-1));
        }

        public bool Contains(T item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            bool ret= _list.Remove(item);
            if (ret)
            {
                OnCollectionEvent(new ComCollectionArgs<T>(item, ComCollectionEventType.Remove, index));
            }
            return ret;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        protected void OnCollectionEvent(ComCollectionArgs<T> args)
        {
            if (CollectionEvent!=null)
            {
                CollectionEvent(this, args);
            }
        }
    }
    public class ComCollectionArgs<T> : EventArgs
    {
        public ComCollectionEventType eventType = ComCollectionEventType.Add;
        public int index = -1;
        public T item;
        public ComCollectionArgs(T item, ComCollectionEventType eventType, int index = -1)
        {
            this.item = item;
            this.eventType = eventType;
            this.index = index;
        }
    }
    public enum ComCollectionEventType
    {
        Add,
        Insert,
        Replace,
        Remove,
        Clear
    }
}
