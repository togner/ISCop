using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Togner.Common.Tools
{
    /// <summary>
    /// Contains extension methods to WPF classes.
    /// </summary>
    public static class WpfExtensions
    {
        #region FindDescendants
        /// <summary>
        /// Finds all descendants - logical and visual - of type T of an element.
        /// Uses GetChildObjects: Framework and content elements are traversed using logical tree.
        /// </summary>
        /// <typeparam name="T">Type of the descendants we are looking for.</typeparam>
        /// <param name="element">Root element whose descendants are being searched.</param>
        /// <returns>Descendants of type T of the element parent, or default(T).</returns>
        public static IEnumerable<T> FindDescendants<T>(this DependencyObject element) where T : DependencyObject
        {
            if (element != null)
            {
                FrameworkElement templatableElement = element as FrameworkElement;
                if (templatableElement != null)
                {
                    templatableElement.ApplyTemplate();
                }

                var children = WpfExtensions.GetChildObjects(element);
                foreach (var child in children)
                {
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T descendant in WpfExtensions.FindDescendants<T>(child))
                    {
                        yield return descendant;
                    }
                }
            }
        }

        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetChild"/> method, which also
        /// supports content elements. Do note, that for content elements,
        /// this method falls back to the logical tree of the element.
        /// </summary>
        /// <param name="parent">The item to be processed.</param>
        /// <returns>The submitted item's child elements, if available.</returns>
        public static IEnumerable<DependencyObject> GetChildObjects(this DependencyObject parent)
        {
            if (parent == null)
            {
                yield break;
            }

            if (parent is ContentElement || parent is FrameworkElement)
            {
                //use the logical tree for content / framework elements
                foreach (object obj in LogicalTreeHelper.GetChildren(parent))
                {
                    var depObj = obj as DependencyObject;
                    if (depObj != null)
                    {
                        yield return (DependencyObject)obj;
                    }
                }
            }
            else
            {
                //use the visual tree per default
                int count = VisualTreeHelper.GetChildrenCount(parent);
                for (int childIndex = 0; childIndex < count; childIndex++)
                {
                    yield return VisualTreeHelper.GetChild(parent, childIndex);
                }
            }
        }
        #endregion

        #region FindVisualDescendants
        /// <summary>
        /// Finds all visual descendants of type T of an element.
        /// </summary>
        /// <typeparam name="T">Type of the descendants we are looking for.</typeparam>
        /// <param name="element">Root element whose descendants are being searched.</param>
        /// <returns>Descendants of type T of the element parent, or default(T).</returns>
        public static IEnumerable<T> FindVisualDescendants<T>(this DependencyObject element) where T : Visual
        {
            if (element != null)
            {
                FrameworkElement templatableElement = element as FrameworkElement;
                if (templatableElement != null)
                {
                    templatableElement.ApplyTemplate();
                }

                int numVisuals = VisualTreeHelper.GetChildrenCount(element);
                for (int i = 0; i < numVisuals; i++)
                {
                    Visual visual = (Visual)VisualTreeHelper.GetChild(element, i);

                    T child = visual as T;
                    if (child != null)
                    {
                        yield return child;
                    }

                    foreach (T descendant in WpfExtensions.FindVisualDescendants<T>(visual))
                    {
                        yield return descendant;
                    }
                }
            }
        }

        /// <summary>
        /// Finds first visual descendant of type T of an element.
        /// </summary>
        /// <typeparam name="T">Type of the descendant we are looking for.</typeparam>
        /// <param name="element">Root element whose descendants are being searched.</param>
        /// <returns>First descendant of type T of the element parent, or default(T).</returns>
        public static T FindVisualDescendant<T>(this DependencyObject element) where T : Visual
        {
            FrameworkElement templatableElement = element as FrameworkElement;
            if (templatableElement != null)
            {
                templatableElement.ApplyTemplate();
            }

            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual visual = (Visual)VisualTreeHelper.GetChild(element, i);
                child = visual as T;
                if (child == null)
                {
                    child = visual.FindVisualDescendant<T>();
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        /// <summary>
        /// Finds first visual descendant with specified name of an element.
        /// </summary>
        /// <param name="element">Element whose descendants are being searched.</param>
        /// <param name="name">Name of the visual descendant.</param>
        /// <returns>Null if no descendant matches the name; itself or the descendant.</returns>
        public static DependencyObject FindVisualDescendant(this DependencyObject element, string name)
        {
            DependencyObject result;
            if (element == null)
            {
                return null;
            }
            if (name == null)
            {
                return null;
            }
            if (name.Equals(element.GetValue(FrameworkElement.NameProperty)))
            {
                return element;
            }
            FrameworkElement templatableElement = element as FrameworkElement;
            if (templatableElement != null)
            {
                templatableElement.ApplyTemplate();
            }
            int numVisuals = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < numVisuals; i++)
            {
                DependencyObject visual = VisualTreeHelper.GetChild(element, i);
                if ((result = visual.FindVisualDescendant(name)) != null)
                {
                    return result;
                }
            }
            return null;
        }
        #endregion FindVisualDescendant

        #region FindAncestor
        /// <summary>
        /// Finds all ancestors - logical and visual - of type T of an element.
        /// </summary>
        /// <typeparam name="T">Type of the ancestors we are looking for.</typeparam>
        /// <param name="element">Root element whose ancestors are being searched.</param>
        /// <returns>Ancestors of type T of the element parent, or default(T).</returns>
        public static IEnumerable<T> FindAncestors<T>(this DependencyObject element) where T : DependencyObject
        {
            DependencyObject parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    yield return correctlyTyped;
                }

                if (parent is ContentElement || parent is FrameworkElement)
                {
                    parent = LogicalTreeHelper.GetParent(parent);
                }
                else
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
            }
        }
        #endregion

        #region FindVisualAncestor
        /// <summary>
        /// Finds all visual ancestor of type T of an element.
        /// Uses logical tree for traversal of non-visual elements.
        /// </summary>
        /// <typeparam name="T">Type of the visual ancestor to look for.</typeparam>
        /// <param name="element">Element whose ancestor is being looked for.</param>
        /// <returns>Visual ancestor of type T or null if the element has no ancestor of type T.</returns>
        public static IEnumerable<T> FindVisualAncestors<T>(this DependencyObject element) where T : DependencyObject
        {
            DependencyObject parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    yield return correctlyTyped;
                }

                if (parent is Visual)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
                else
                {
                    //e.g. ContentElement
                    parent = LogicalTreeHelper.GetParent(parent);
                }
            }
        }

        /// <summary>
        /// Finds first visual ancestor of type T of an element.
        /// Uses logical tree for traversal of non-visual elements.
        /// </summary>
        /// <typeparam name="T">Type of the visual ancestor to look for.</typeparam>
        /// <param name="element">Element whose ancestor is being looked for.</param>
        /// <returns>Visual ancestor of type T or null if the element has no ancestor of type T.</returns>
        public static T FindVisualAncestor<T>(this DependencyObject element) where T : DependencyObject
        {
            DependencyObject parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                if (parent is Visual)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }
                else
                {
                    //e.g. ContentElement
                    parent = LogicalTreeHelper.GetParent(parent);
                }
            }
            return null;
        }
        #endregion FindVisualAncestor

        /// <summary>
        /// Gets all attached properties on a dependency object.
        /// </summary>
        /// <param name="element">DependencyObject to get the attached properties on.</param>
        /// <returns>List of attached dependency properties.</returns>
        public static Collection<DependencyProperty> GetAttachedProperties(this DependencyObject element)
        {
            Collection<DependencyProperty> attached = new Collection<DependencyProperty>();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(element,
                new Attribute[] { new PropertyFilterAttribute(PropertyFilterOptions.All) }))
            {
                DependencyPropertyDescriptor dependencyProperty = DependencyPropertyDescriptor.FromProperty(property);
                if (dependencyProperty != null && dependencyProperty.IsAttached)
                {
                    attached.Add(dependencyProperty.DependencyProperty);
                }
            }
            return attached;
        }

        /// <summary>
        /// Prints all visual descendants of a visual element.
        /// </summary>
        /// <param name="element">The element whose visual descendants should be printed.</param>
        /// <param name="level">The level in the visual tree this visual belongs to (0 - root).</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "Element must be of Visual type - we are using visual tree to search for its descendants.")]
        public static void PrintVisualDescendants(this Visual element, int level)
        {
            for (int child = 0; child < VisualTreeHelper.GetChildrenCount(element); child++)
            {
                //Retrieve child visual at specified index value.
                Visual childVisual = (Visual)VisualTreeHelper.GetChild(element, child);

                //Indent the child visual object.
                for (int i = 0; i < level; i++)
                {
                    Debug.Write("  ");
                }
                Debug.Write(childVisual.ToString() + "\n");

                //Enumerate children of the child visual object.
                childVisual.PrintVisualDescendants(level + 1);
            }
        }

        /// <summary>
        /// Prints all logical descendants of a dependency object.
        /// </summary>
        /// <param name="element">The dependency object whose logical descendants should be printed.</param>
        /// <param name="level">The level in the logical tree this dependency object belongs to (0 - root).</param>
        public static void PrintLogicalDescendants(this DependencyObject element, int level)
        {
            IEnumerable children = LogicalTreeHelper.GetChildren(element);
            foreach (DependencyObject child in children)
            {
                for (int i = 0; i < level; i++)
                {
                    Debug.Write("  ");
                }
                Debug.Write(child.ToString() + "\n");
                child.PrintLogicalDescendants(level + 1);
            }
        }

        /// <summary>
        /// Enumeration of all bindings set on an element. 
        /// Uses reflection to find all bound properties on element. Slow when getting properties of a type of an element for the first time.
        /// </summary>
        /// <param name="element">Element whose bindings should be enumerated.</param>
        /// <returns>Enumeration of all bindings set on an element.</returns>
        public static IEnumerable<BindingExpressionBase> EnumerateBindings(this DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            PropertyFilterAttribute propertyAttributes = new PropertyFilterAttribute(PropertyFilterOptions.SetValues | PropertyFilterOptions.UnsetValues | PropertyFilterOptions.Valid);
            PropertyDescriptorCollection descriptors = TypeDescriptor.GetProperties(element, new Attribute[] { propertyAttributes });
            foreach (PropertyDescriptor descriptor in descriptors)
            {
                DependencyPropertyDescriptor dependencyDescriptor = DependencyPropertyDescriptor.FromProperty(descriptor);
                if (dependencyDescriptor != null && BindingOperations.IsDataBound(element, dependencyDescriptor.DependencyProperty))
                {
                    BindingExpressionBase binding = BindingOperations.GetBindingExpressionBase(element, dependencyDescriptor.DependencyProperty);
                    yield return binding;
                }
            }
            /*
            //this only works for locally set properties!
            LocalValueEnumerator propertyEnumerator = element.GetLocalValueEnumerator();
            while (propertyEnumerator.MoveNext())
            {
                LocalValueEntry entry = propertyEnumerator.Current;
                if (BindingOperations.IsDataBound(element, entry.Property))
                {
                    BindingExpressionBase binding = (entry.Value as BindingExpressionBase);
                    yield return binding;
                }
            }
            */
        }
    }
}
