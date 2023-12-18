using System;
using System.Collections.Generic;

namespace AppFrame.Tools
{
    public class ObjectPool<T>
    {
        private readonly Stack<T> m_Stack;
        private readonly int m_MaxSize;
        private readonly Func<T> m_Create;
        
        
        public int TotalCount { get; private set; }
        public int ActiveCount => this.TotalCount - this.InactiveCount;
        public int InactiveCount => this.m_Stack.Count;
        
        public ObjectPool(Func<T> create, int defaultSize = 10, int maxSize = 1000)
        {
            if (maxSize <= 0)
            {
                throw new ArgumentOutOfRangeException("Max Size must be greater than 0", nameof (maxSize));
            }
            if (create == null)
            {
                throw new ArgumentNullException(nameof(create));
            }
            this.m_Stack = new Stack<T>(defaultSize);
            this.m_Create = create;
            this.m_MaxSize = maxSize;
        }
        
        public T Get(Action<T> get = null)
        {
            T obj;
            if (this.m_Stack.Count > 0)
            {
                obj = this.m_Stack.Pop();
            }
            else
            {
                obj = this.m_Create();
                ++this.TotalCount;
            }
            get?.Invoke(obj);
            return obj;
        }
        
        public void GiveBack(T obj, Action<T> giveback = null, Action<T> destroy = null)
        {
            giveback?.Invoke(obj);
            if (this.InactiveCount < this.m_MaxSize)
            {
                this.m_Stack.Push(obj);
            }
            else
            {
                destroy?.Invoke(obj);
            }
        }
        
        public void Clear(Action<T> clear = null)
        {
            foreach (T obj in this.m_Stack)
            {
                clear?.Invoke(obj);
            }
            this.m_Stack.Clear();
            this.TotalCount = 0;
        }
    }
}
