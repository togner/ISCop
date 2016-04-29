using System.Collections.Generic;
using log4net;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Dts")]
    public static class DtsExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static List<TaskHost> GetControlFlowObjects<T>(this DtsContainer container)
        {
            List<TaskHost> returnItems = new List<TaskHost>();
            EventsProvider ep = container as EventsProvider;
            if (ep != null)
            {
                foreach (DtsEventHandler eh in ep.EventHandlers)
                {
                    returnItems.AddRange(DtsExtensions.GetControlFlowObjects<T>(eh));
                }
            }
            IDTSSequence sequence = (IDTSSequence)container;
            foreach (Executable exec in sequence.Executables)
            {
                if (exec is IDTSSequence)
                {
                    returnItems.AddRange(DtsExtensions.GetControlFlowObjects<T>((DtsContainer)exec));
                }
                else
                {
                    var th = exec as TaskHost;
                    if (th != null)
                    {
                        if (th.InnerObject is T)
                        {
                            returnItems.Add(th);
                        }
                    }
                }
            }
            return returnItems;
        }

        public static IDTSComponentMetaData100 TraceInputToSource(this IDTSComponentMetaData100 component, MainPipe mainPipe)
        {
            if (mainPipe != null && component != null)
            {
                foreach (IDTSPath100 path in mainPipe.PathCollection)
                {
                    if (path.EndPoint.Component.IdentificationString == component.IdentificationString)
                    {
                        if (path.StartPoint.SynchronousInputID == 0)
                        {
                            //This is the source component.
                            return path.StartPoint.Component;
                        }
                        else
                        {
                            return DtsExtensions.TraceInputToSource(path.StartPoint.Component, mainPipe);
                        }
                    }
                }
            }
            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static bool TryGetCustomPropertyValue<T>(this IDTSComponentMetaData100 metadata, string propertyName, out T value)
        {
            value = default(T);
            if (metadata != null)
            {
                try
                {
                    value = DtsExtensions.GetCustomPropertyValue<T>(metadata, propertyName);
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }

        public static T GetCustomPropertyValue<T>(this IDTSComponentMetaData100 metadata, string propertyName)
        {
            if (metadata == null)
            {
                return default(T);
            }
            IDTSCustomProperty100 prop = metadata.CustomPropertyCollection[propertyName];
            return (T)prop.Value;
        }
    }
}
