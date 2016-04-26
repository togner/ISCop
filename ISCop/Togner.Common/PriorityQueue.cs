using System;
using System.Collections.Generic;
using System.Linq;

namespace Togner.Common
{
    /// <summary>
    /// Priority queue implementation with int priority. The higher the number, 
    /// the higher the priority.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class PriorityQueue<TValue> : PriorityQueue<TValue, int> { }

    /// <summary>
    /// Simple priority queue implemented as sorted dictionary of queues.
    /// </summary>
    /// <typeparam name="TValue">The type of values this queue will hold.</typeparam>
    /// <typeparam name="TPriority">The type of value of the priority object.</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class PriorityQueue<TValue, TPriority> where TPriority : IComparable
    {
        /// <summary>
        /// Gets the number of elements in the queue.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the queue is empty or not.
        /// </summary>
        public bool IsEmpty 
        { 
            get 
            { 
                return this.Count == 0; 
            } 
        }

        private SortedDictionary<TPriority, Queue<TValue>> _dictionary = new SortedDictionary<TPriority, Queue<TValue>>();

        /// <summary>
        /// Adds an object to the queue with the default priority.
        /// </summary>
        /// <param name="value">The object to add. The value can be null for reference types.</param>
        public void Enqueue(TValue value)
        {
            this.Enqueue(value, default(TPriority));
        }

        /// <summary>
        /// Adds an object to the queue with specified priority.
        /// </summary>
        /// <param name="value">The object to add. The value can be null for reference types.</param>
        /// <param name="priority">The priority of the added object</param>
        public void Enqueue(TValue value, TPriority priority)
        {
            this.Count++;
            if (!this._dictionary.ContainsKey(priority))
            {
                this._dictionary[priority] = new Queue<TValue>();
            }
            this._dictionary[priority].Enqueue(value);
        }

        /// <summary>
        /// Removes and returns the object with the highest priority.
        /// Items with same priority are dequeued in the same order that they were added.
        /// </summary>
        /// <returns>The object that is removed from the queue.</returns>
        /// <exception cref="System.InvalidOperationException">The queue is empty.</exception>
        public TValue Dequeue()
        {
            this.Count--;
            var item = this._dictionary.Last();
            if (item.Value.Count == 1)
            {
                this._dictionary.Remove(item.Key);
            }
            return item.Value.Dequeue();
        }

        public bool Contains(TValue value)
        {
            return this.Contains(value, null);
        }

        /// <summary>
        /// Determines whether the queue contains a specified element.
        /// Optionally, non-default equality comaprer can be used.
        /// </summary>
        /// <param name="value">The value to locate in the queue.</param>
        /// <param name="comparer">An optional equality comparer to compare values.</param>
        /// <returns>True if the queue contains an element that has the specified value; otherwise, false.</returns>
        public bool Contains(TValue value, IEqualityComparer<TValue> comparer)
        {
            if (comparer != null)
            {
                return this._dictionary.Values.Any(queue => queue.Contains(value, comparer));
            }
            else
            {
                return this._dictionary.Values.Any(queue => queue.Contains(value));
            }
        }
    }
}
