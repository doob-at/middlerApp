using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace JintTsDefinition
{
    public class TestROC : IList
    {
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public int Count { get; }
        public bool IsSynchronized { get; }
        public object SyncRoot { get; }
        public int Add(object value)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public bool IsFixedSize { get; }
        bool IList.IsReadOnly { get; }

        public object this[int index]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }
}
