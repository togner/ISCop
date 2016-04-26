using System;
using System.Collections.Generic;

namespace Togner.Common.Trees
{
    /// <summary>
    /// This class prefix enumerates any ITree implementation.
    /// </summary>
    public sealed class TreeEnumerator : IEnumerator<ITree>
    {
        private ITree _tree;
        private bool _rootEnumerated;

        /// <summary>
        /// Initializes a new instance of the TreeEnumerator class.
        /// This enumerator will prefix iterate a tree structure.
        /// </summary>
        /// <param name="tree">The tree structure to iterate.</param>
        public TreeEnumerator(ITree tree)
        {
            this._tree = tree;
            this.Reset();
        }

        #region IEnumerator<ITree> Members

        /// <summary>
        /// Gets the current tree node.
        /// </summary>
        public ITree Current
        {
            get;
            private set;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes of the resources.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion

        #region IEnumerator Members

        /// <summary>
        /// Gets the current tree node.
        /// </summary>
        object System.Collections.IEnumerator.Current
        {
            get { return this._tree; }
        }

        /// <summary>
        /// Moves to the next tree node.
        /// </summary>
        /// <returns>Whether the Iterator could advance to the next element.</returns>
        public bool MoveNext()
        {
            if (!this._rootEnumerated)
            {
                this._rootEnumerated = true;
                return (this.Current != null);
            }

            ITree next;
            if (this.Current.Children.Count > 0)
            {
                next = this.Current.Children[0];
            }
            else
            {
                next = this.Current.GetNextSibling();
                while (next == null && this.Current.Parent != this._tree)
                {
                    this.Current = this.Current.Parent;
                    next = this.Current.GetNextSibling();
                }
            }
            this.Current = next;

            return this.Current != null;
        }

        /// <summary>
        /// Returns this iterator to its initial position.
        /// </summary>
        public void Reset()
        {
            this._rootEnumerated = false;
            this.Current = this._tree.Children.Count > 0 ? this._tree.Children[0] : null;
        }

        #endregion
    }
}