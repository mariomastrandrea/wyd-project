using System;
using System.Collections.Generic;
using System.Linq;

namespace _13_Wyd.ModelClasses.DataStructures
{
    public class PriorityQueue<T> : IPriorityQueue<T>
        where T : IComparable<T>
    {
        private readonly List<T> Queue;
        private readonly HashSet<T> Set;

        public PriorityQueue()
        {
            this.Queue = new List<T>();
            this.Set = new HashSet<T>();
        }

        public bool Enqueue(T element)
        {
            if (element == null)
                throw new NullReferenceException();

            this.Queue.Add(element);
            this.Set.Add(element);
            this.Queue.Sort();

            return true;
        }

        public void EnqueueAll(params T[] elements)
        {
            if(elements == null)
                throw new NullReferenceException();

            this.Queue.AddRange(elements);

            foreach (var element in elements)
                this.Set.Add(element);

            this.Queue.Sort();
        }

        public void EnqueueAll(IEnumerable<T> elements)
        {
            if (elements == null)
                throw new NullReferenceException();

            this.Queue.AddRange(elements);

            foreach (var element in elements)
                this.Set.Add(element);

            this.Queue.Sort();
        }

        public void EnqueueAllIfNotPresent(IEnumerable<T> elements)
        {
            //filter elements already present
            elements = elements.Where(element => !this.Set.Contains(element));  
            this.EnqueueAll(elements);
        }

        public T Peek()
        {
            if (!this.Queue.Any()) return default;

            return this.Queue.First();
        }

        public T Dequeue()
        {
            if (!this.Queue.Any()) return default;

            T firstElement = this.Queue.First();
            this.Queue.RemoveAt(0);

            return firstElement;
        }
    }
}
