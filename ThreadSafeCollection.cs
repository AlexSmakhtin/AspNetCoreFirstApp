using System.Collections;
using System.Runtime.CompilerServices;
using System.Text;

namespace AspNetCoreFirstApp
{
    public class ThreadSafeCollection<T> : IList<T>
    {
        private readonly object lockObject = new();
        private readonly List<T> items;

        public bool IsFixedSize => throw new NotImplementedException();

        public bool IsReadOnly => throw new NotImplementedException();

        public int Count => GetCount();

        private int GetCount()
        {
            lock (lockObject)
            {
                return items.Count;
            }
        }

        public bool IsSynchronized => throw new NotImplementedException();

        public object SyncRoot => throw new NotImplementedException();

        T IList<T>.this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public T this[int index]
        {
            get
            {
                lock (lockObject)
                    return items[index];
            }
            set
            {
                lock (lockObject)
                    items[index] = value;
            }
        }

        public ThreadSafeCollection()
        {
            items = new List<T>();
        }

        public void Clear()
        {
            lock (lockObject)
            {
                items.Clear();
            }
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            lock (lockObject)
            {
                items.RemoveAt(index);
            }
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Add(T item)
        {
            lock (lockObject)
                items.Add(item);
        }
        void ICollection<T>.Add(T item)
        {
            throw new NotImplementedException();
        }
        public override string ToString()
        {
            lock (lockObject)
            {
                var stringBuilder = new StringBuilder();
                foreach (var item in items)
                    stringBuilder.Append($"{item}\n");
                return stringBuilder.ToString();
            }
        }
    }
}
