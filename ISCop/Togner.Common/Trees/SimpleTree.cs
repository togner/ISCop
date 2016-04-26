using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace Togner.Common.Trees
{
    /// <summary>
    /// Straightforward implementation of the ITree interface.
    /// </summary>
    /// <typeparam name="T">The type of the Value of this tree.</typeparam>
    [Serializable]
    public class SimpleTree<T> : IValueTree<T>
    {
        /// <summary>
        /// Initializes a new instance of the SimpleTree class.
        /// </summary>
        public SimpleTree()
        {
            this.Parent = this;
            this._children = new List<IValueTree<T>>();
            this.Level = 0;
        }

        /// <summary>
        /// Initializes a new instance of the SimpleTree class.
        /// </summary>
        /// <param name="parent">The parent of the new tree.</param>
        protected SimpleTree(IValueTree<T> parent)
        {
            this.Parent = parent;
            this.Level = parent == null ? 0 : parent.Level + 1;
            this._children = new List<IValueTree<T>>();
        }

        #region ITree<T> Members

        private List<IValueTree<T>> _children;

        /// <summary>
        /// Gets the collection of children of this tree.
        /// </summary>
        public System.Collections.ObjectModel.ReadOnlyCollection<IValueTree<T>> Children
        {
            get
            {
                return this._children.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets or sets the Value of this tree.
        /// </summary>
        public T Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the parent of this tree.
        /// </summary>
        public IValueTree<T> Parent
        {
            get;
            private set;
        }

        /// <summary>
        /// Adds a new child to this tree.
        /// </summary>
        /// <returns>The newly added child.</returns>
        public IValueTree<T> AddChild()
        {
            IValueTree<T> _result = new SimpleTree<T>(this);
            this._children.Add(_result);
            return _result;
        }

        /// <summary>
        /// Gets the level of the tree.
        /// The level represents the number of parents a tree has (e.g. 0 if root, 1 if child of root, ...)
        /// </summary>
        public int Level
        {
            get;
            private set;
        }

        #endregion

        #region ITree Members

        /// <summary>
        /// Gets the collection of children of this tree.
        /// </summary>
        ReadOnlyCollection<ITree> ITree.Children
        {
            get
            {
                List<ITree> result = new List<ITree>();
                foreach (IValueTree<T> child in this._children)
                {
                    result.Add(child);
                }
                return result.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the parent of this tree.
        /// </summary>
        ITree ITree.Parent
        {
            get { return this.Parent; }
        }

        /// <summary>
        /// Adds a new child to this tree.
        /// </summary>
        /// <returns>The newly added child.</returns>
        ITree ITree.AddChild()
        {
            return this.AddChild();
        }

        #endregion

        /// <summary>
        /// ToSString override displaying the type of the tree and its value.
        /// </summary>
        /// <returns>A string representation of this object.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "SimpleTree : {0}", this.Value);
        }
    }
}
