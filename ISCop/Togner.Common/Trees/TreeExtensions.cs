using System;
using System.Collections.Generic;
using System.Linq;

namespace Togner.Common.Trees
{
    /// <summary>
    /// Contains extension methods to the ITree interface.
    /// </summary>
    public static class TreeExtensions
    {
        /// <summary>
        /// Describes a predicate on a tree.
        /// </summary>
        /// <param name="tree">The tree on which to check the predicate.</param>
        /// <returns>Whether the predicate is satisfied.</returns>
        private delegate bool LeafCondition(ITree tree);

        /// <summary>
        /// Checks whether this tree is the root of the tree.
        /// </summary>
        /// <typeparam name="T">The type of the tree.</typeparam>
        /// <param name="source">The tree on which to check.</param>
        /// <returns>Whether this tree is root.</returns>
        public static bool IsRoot<T>(this T source)
            where T : class, ITree
        {
            return source.Parent as T == source;
        }

        /// <summary>
        /// Checks whether this tree is a leaf.
        /// </summary>
        /// <typeparam name="T">The type of the tree.</typeparam>
        /// <param name="source">The tree on which to check.</param>
        /// <returns>Whether this tree is a leaf.</returns>
        public static bool IsLeaf<T>(this T source)
            where T : ITree
        {
            return source.Children.Count == 0;
        }

        /// <summary>
        /// Gets the next sibling of this tree, or null if the tree has no next sibling.
        /// </summary>
        /// <typeparam name="T">The type of the tree.</typeparam>
        /// <param name="source">The tree on which to find the next sibling.</param>
        /// <returns>The next sibling, or null.</returns>
        public static T GetNextSibling<T>(this T source)
            where T : class, ITree
        {
            //As IndexOf() is a costly operation, this could be heavily optimized by caching the index of a tree in its parents children collection.
            if (source.IsRoot())
            {
                return null;
            }
            int index = source.Parent.Children.IndexOf(source) + 1;
            if (index < source.Parent.Children.Count)
            {
                return source.Parent.Children.ElementAt(index) as T;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the previous sibling of this tree, or null if the tree has no previous sibling.
        /// </summary>
        /// <typeparam name="T">The type of the tree.</typeparam>
        /// <param name="source">The tree on which to find the previous sibling.</param>
        /// <returns>The previous sibling, or null.</returns>
        public static T GetPreviousSibling<T>(this T source)
            where T : class, ITree
        {
            if (source.IsRoot())
            {
                return null;
            }
            int index = source.Parent.Children.IndexOf(source) - 1;
            if (index >= 0)
            {
                return source.Parent.Children.ElementAt(index) as T;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the root of the tree.
        /// </summary>
        /// <typeparam name="T">The type of the tree.</typeparam>
        /// <param name="source">The tree we want the root of.</param>
        /// <returns>The root of this tree.</returns>
        public static T GetRoot<T>(this T source)
            where T : class, ITree
        {
            T elm = source;
            while (!elm.IsRoot())
            {
                elm = elm.Parent as T;
            }
            return elm;
        }

        /// <summary>
        /// Gets the depth of this tree.
        /// </summary>
        /// <param name="source">The tree we want the depth of.</param>
        /// <returns>The depth of the tree.</returns>
        public static int Depth(this ITree source)
        {
            if (source == null)
            {
                return -1;
            }
            int depth = 0;
            foreach (ITree child in source.Children)
            {
                depth = Math.Max(depth, child.Depth());
            }
            if (!source.IsLeaf())
            {
                depth += 1;
            }
            return depth;
        }

        /// <summary>
        /// Gets all the leafs of a tree.
        /// </summary>
        /// <typeparam name="T">The type of the tree.</typeparam>
        /// <param name="source">The tree we want the leafs of.</param>
        /// <returns>The leafs of the tree.</returns>
        public static IList<T> Leafs<T>(this T source)
            where T : ITree
        {
            return source.TreesOfDepth(0);
        }

        /// <summary>
        /// Gets all the trees of a certain level that are child to this tree.
        /// </summary>
        /// <typeparam name="T">The type of the tree.</typeparam>
        /// <param name="source">The tree we want subtrees of.</param>
        /// <param name="level">The level of the searched subtrees.</param>
        /// <returns>The subtrees of given level.</returns>
        public static IList<T> TreesOfLevel<T>(this T source, int level)
            where T : ITree
        {
            List<T> result = new List<T>();
            if (source.Level == level)
            {
                result.Add(source);
                return result;
            }
            foreach (T tree in source.Children)
            {
                if (tree.Level < level)
                {
                    result.AddRange(tree.TreesOfLevel(level));
                }
                else if (tree.Level == level)
                {
                    result.Add(tree);
                }
            }
            return result;
        }

        /// <summary>
        /// Gets all the trees of a certain depth that are child to this tree.
        /// </summary>
        /// <typeparam name="T">The type of the tree.</typeparam>
        /// <param name="source">The tree we want subtrees of.</param>
        /// <param name="depth">The depth of the searched subtrees.</param>
        /// <returns>The subtrees of given depth.</returns>
        public static IList<T> TreesOfDepth<T>(this T source, int depth)
            where T : ITree
        {
            List<T> result = new List<T>();
            if (source.Depth() == depth)
            {
                result.Add(source);
                return result;
            }
            foreach (T tree in source.Children)
            {
                int currentTreeDepth = tree.Depth();
                if (currentTreeDepth == depth)
                {
                    result.Add(tree);
                }
                if (currentTreeDepth > depth)
                {
                    result.AddRange(tree.TreesOfDepth(depth));
                }
            }
            return result;
        }
    }
}
