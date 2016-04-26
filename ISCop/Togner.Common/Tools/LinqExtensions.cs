using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Togner.Common.Tools
{
	/// <summary>
	/// Linq extension functions.
	/// </summary>
	public static class LinqExtensions
	{
		/// <summary>
		/// Applies action to each element in a sequence.
		/// </summary>
		/// <typeparam name="T">The type of the elements of the source.</typeparam>
		/// <param name="source">The System.Collections.Generic.IEnumerable[T] containing the elements to apply the action on.</param>
		/// <param name="action">The action to apply on each element.</param>
		public static void ForAll<T>(this IEnumerable<T> source, Action<T> action)
		{
            if (source != null && action != null)
            {
                foreach (T element in source)
                {
                    action(element);
                }
            }
		}

        /// <summary>
        /// Performs left outer join on two sequences based on equality of keys.
        /// The default equality comparer is used to compare keys.
        /// </summary>
        /// <typeparam name="TOuter">The type of the elements of the first sequence.</typeparam>
        /// <typeparam name="TLeft">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <param name="outer">The first sequence to join.</param>
        /// <param name="left">The sequence to left outer join to the first sequence.</param>
        /// <param name="outerKeySelector">A function to extract the join key from each element of the first sequence.</param>
        /// <param name="leftKeySelector">A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector">A function to create a result element from an element from the first sequence and the left outer joined element from the second sequence.</param>
        /// <returns>An enumerable that contains elements of type TResult that are obtained by performing a left outer join on two sequences.</returns>
        public static IEnumerable<TResult> LeftOuterJoin<TOuter, TLeft, TKey, TResult>(this IEnumerable<TOuter> outer, IEnumerable<TLeft> left, Func<TOuter, TKey> outerKeySelector, Func<TLeft, TKey> leftKeySelector, Func<TOuter, TLeft, TResult> resultSelector)
        {
            return
              from result in outer
              join row in left on outerKeySelector(result) equals leftKeySelector(row) into joined
              from joinResult in joined.DefaultIfEmpty()
              select resultSelector(result, joinResult);
        }

        /// <summary>
        /// Concatenate the values of the sequence using the delimiter.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the sequence.</typeparam>
        /// <param name="source">The sequence to concatenate.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns>New string which is the concatenation of the source sequence.</returns>
        public static string ToString<T>(this IEnumerable<T> source, object delimiter)
        {
            return source.Aggregate(new StringBuilder(), (builder, value) => builder.Append(value, delimiter), builder => builder.ToString());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> source, int batchSize)
        {
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    yield return LinqExtensions.YieldBatchElements(enumerator, batchSize - 1);
                }
            }
        }

        private static IEnumerable<T> YieldBatchElements<T>(IEnumerator<T> source, int batchSize)
        {
            yield return source.Current;
            for (int i = 0; i < batchSize && source.MoveNext(); i++)
            {
                yield return source.Current;
            }
        }
	}
}
