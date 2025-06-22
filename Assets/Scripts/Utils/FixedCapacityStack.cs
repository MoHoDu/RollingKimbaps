using System.Collections.Generic;

namespace Utils
{
    public class FixedCapacityStack<T>
    {
        private List<T> _buffer = new List<T>();
        private int _maxSize;

        public FixedCapacityStack(int maxSize)
        {
            _maxSize = maxSize;
        }

        public void Push(T item)
        {
            if (_buffer.Count >= _maxSize)
                _buffer.RemoveAt(0);
            _buffer.Add(item);
        }

        public T Pop()
        {
            if (_buffer.Count == 0)
                return default;
            T item = _buffer[^1];
            _buffer.RemoveAt(_buffer.Count - 1);
            return item;
        }

        public T Peek()
        {
            if (_buffer.Count == 0)
                return default;
            T item = _buffer[^1];
            return item;
        }
        
        public void RemoveAt(int index)
        {
            if (index < 0 || _buffer.Count <= index)
                return;
            _buffer.RemoveAt(index);
        }
        
        public void Clear() => _buffer.Clear();
        
        public int Count => _buffer.Count;
        public T this[int index] => _buffer[index];
        public Stack<T> GetCopied() => new Stack<T>(_buffer.ToArray());
    }
}