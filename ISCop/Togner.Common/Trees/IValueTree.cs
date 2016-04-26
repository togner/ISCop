using System.Collections.ObjectModel;

namespace Togner.Common.Trees
{
    /// <summary>
    /// Describes a simple tree structure, containing Children, knowing its parent, and having a generic Value.
    /// </summary>
    /// <typeparam name="T">The type of the value for this tree.</typeparam>
    public interface IValueTree<T> : ITree
    {
        /// <summary>
        /// Gets or sets the Value of this tree.
        /// </summary>
        T Value { get; set; }

        #region ITree interface functions
        /// <summary>
        /// Gets the collection of children of this tree.
        /// </summary>
        new ReadOnlyCollection<IValueTree<T>> Children
        {
            get;
        }

        /// <summary>
        /// Gets the parent of this tree.
        /// </summary>
        new IValueTree<T> Parent { get; }

        /// <summary>
        /// Adds a new child to this tree.
        /// </summary>
        /// <returns>The newly added child.</returns>
        new IValueTree<T> AddChild();
        #endregion ITree interface functions
    }
}
