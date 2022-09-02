using System;
using System.Collections.Generic;

namespace _13_Wyd.ModelClasses.DataStructures
{
    public interface IPriorityQueue<T> where T : IComparable<T>
    {
        public bool Enqueue(T element);
        public void EnqueueAll(params T[] elements);
        public void EnqueueAll(IEnumerable<T> elements);
        public void EnqueueAllIfNotPresent(IEnumerable<T> elements);
        public T Peek();
        public T Dequeue();
    }
}
