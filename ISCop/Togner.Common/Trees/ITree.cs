using System.Collections.ObjectModel;

namespace Togner.Common.Trees
{
    /// <summary>
    /// Simple Tree interface.
    /// </summary>
    public interface ITree
    {
        /// <summary>
        /// Gets the collection of children of this tree.
        /// </summary>
        ReadOnlyCollection<ITree> Children
        {
            get;
        }

        /// <summary>
        /// Gets the parent of this tree.
        /// </summary>
        ITree Parent { get; }

        /// <summary>
        /// Adds a new child to this tree.
        /// </summary>
        /// <returns>The newly added child.</returns>
        ITree AddChild();

        /// <summary>
        /// Gets the level of the tree.
        /// The level represents the number of parents a tree has (e.g. 0 if root, 1 if child of root, ...)
        /// </summary>
        int Level
        {
            get;
        }
    }
}
