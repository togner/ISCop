using System;
using System.Collections.Generic;
using log4net;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;

namespace BIDSHelper.SSIS
{
    internal class PackageHelper
    {
        /// <summary>
        /// All managed components in the data flow share the same wrapper, identified by this GUID.
        /// The specific type of managed component is identified by the UserComponentTypeName 
        /// custom property of the component.
        /// </summary>
        private const string ManagedComponentWrapper = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}"; //{bf01d463-7089-41ee-8f05-0a6dc17ce633}";
        private const string ManagedComponentTypeNameField = "UserComponentTypeName";

        public const string PackageCreationName = "Package";
        public const string EventHandlerCreationName = "EventHandler";
        public const string ConnectionCreationName = "Connection";
        public const string SequenceCreationName = "Sequence";
        public const string ForLoopCreationName = "ForLoop";
        public const string ForEachLoopCreationName = "ForEachLoop";

        private static readonly ILog Logger = LogManager.GetLogger(typeof(PackageHelper));
        private static Dictionary<string, ComponentInfo> _componentInfos = new Dictionary<string, ComponentInfo>();
        private static Dictionary<string, ComponentInfo> _controlInfos = new Dictionary<string, ComponentInfo>();

        public static List<TaskHost> GetControlFlowObjects<T>(DtsContainer container)
        {
            List<TaskHost> returnItems = new List<TaskHost>();
            EventsProvider ep = container as EventsProvider;
            if (ep != null)
            {
                foreach (DtsEventHandler eh in ep.EventHandlers)
                {
                    returnItems.AddRange(GetControlFlowObjects<T>(eh));
                }
            }
            IDTSSequence sequence = (IDTSSequence)container;
            foreach (Executable exec in sequence.Executables)
            {
                if (exec is IDTSSequence)
                {
                    returnItems.AddRange(GetControlFlowObjects<T>((DtsContainer)exec));
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

        // Gets the cached Pipeline ComponentInfo object
        public static ComponentInfo GetComponentInfo(IDTSComponentMetaData100 component)
        {
            if (_componentInfos.Count == 0)
            {
                Application application = new Application();
                PipelineComponentInfos pipelineComponentInfos = application.PipelineComponentInfos;
                foreach (PipelineComponentInfo pipelineComponentInfo in pipelineComponentInfos)
                {
                    if (pipelineComponentInfo.ID == ManagedComponentWrapper)
                    {
                        _componentInfos.Add(pipelineComponentInfo.CreationName, new ComponentInfo(pipelineComponentInfo));
                    }
                    else
                    {
                        _componentInfos.Add(pipelineComponentInfo.ID, new ComponentInfo(pipelineComponentInfo));
                    }
                }
            }
            ComponentInfo result = null;
            var key = PackageHelper.GetComponentKey(component);
            if (!_componentInfos.TryGetValue(key, out result))
            {
                PackageHelper.Logger.WarnFormat("Component '{0}' has unsupported type '{1}'.", component.Name, key);
            }
            return result;
        }

        // Gets the cached Control Flow ComponentInfo object
        public static ComponentInfo TryGetControlFlowInfo(IDTSComponentMetaData100 component)
        {
            if (_controlInfos.Count == 0)
            {
                Application application = new Application();
                TaskInfos taskInfos = application.TaskInfos;
                foreach (TaskInfo taskInfo in taskInfos)
                {
                    ComponentInfo info = new ComponentInfo(taskInfo);
                    _controlInfos.Add(taskInfo.CreationName, info);

                    // Tasks can be created using the creation name or 
                    // ID, so need both when they differ
                    if (taskInfo.CreationName != taskInfo.ID)
                    {
                        _controlInfos.Add(taskInfo.ID, info);                            
                    }
                }
            }
            ComponentInfo result = null;
            var key = PackageHelper.GetComponentKey(component);
            if (!_controlInfos.TryGetValue(key, out result))
            {
                PackageHelper.Logger.WarnFormat("Component '{0}' has unsupported type '{1}'.", component.Name, key);
            }
            return result;
        }

        /// <summary>
        /// Get a Type from a TypeCode
        /// </summary>
        /// <remarks>
        /// SSIS does not support the type UInt16 for variables, since this is actually used to store Char types.
        /// </remarks>
        /// <param name="typeCode">TypeCode to lookup Type</param>
        /// <returns>Type matching TypeCode supplied.</returns>
        public static Type GetTypeFromTypeCode(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return typeof(bool);
                case TypeCode.Byte:
                    return typeof(byte);
                case TypeCode.Char:
                    return typeof(byte);
                case TypeCode.DateTime:
                    return typeof(DateTime);
                case TypeCode.DBNull:
                    return typeof(DBNull);
                case TypeCode.Decimal:
                    return typeof(decimal);
                case TypeCode.Double:
                    return typeof(double);
                case TypeCode.Empty:
                    return null;
                case TypeCode.Int16:
                    return typeof(short);
                case TypeCode.Int32:
                    return typeof(int);
                case TypeCode.Int64:
                    return typeof(long);
                case TypeCode.Object:
                    return typeof(object);
                case TypeCode.SByte:
                    return typeof(sbyte);
                case TypeCode.Single:
                    return typeof(float);
                case TypeCode.String:
                    return typeof(string);
                case TypeCode.UInt16:
                    return typeof(char); // Assign a char, get a UInt16 with SSIS variables
                case TypeCode.UInt32:
                    return typeof(uint);
                case TypeCode.UInt64:
                    return typeof(ulong);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the unique container key, based on the creation name.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <returns>The container key</returns>
        public static string GetContainerKey(DtsContainer container)
        {
            string containerKey = container.CreationName;
            if (container is Package)
            {
                containerKey = PackageHelper.PackageCreationName;
            }
            else if (container is DtsEventHandler)
            {
                containerKey = PackageHelper.EventHandlerCreationName;
            }
            else if (container is Sequence)
            {
                containerKey = PackageHelper.SequenceCreationName;
            }
            else if (container is ForLoop)
            {
                containerKey = PackageHelper.ForLoopCreationName;
            }
            else if (container is ForEachLoop)
            {
                containerKey = PackageHelper.ForEachLoopCreationName;
            }
            return containerKey;
        }

        public static string GetComponentKey(IDTSComponentMetaData100 component)
        {
            string key = component.ComponentClassID;
            if (component.ComponentClassID == PackageHelper.ManagedComponentWrapper)
            {
                key = component.CustomPropertyCollection[PackageHelper.ManagedComponentTypeNameField].Value.ToString();
            }
            return key;
        }

        public static IDTSComponentMetaData100 TraceInputToSource(MainPipe mainPipe, IDTSComponentMetaData100 component)
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
                        return TraceInputToSource(mainPipe, path.StartPoint.Component);
                    }
                }
            }
            return null;
        }
    }

    public enum SourceAccessMode : int
    {
        OpenRowSet = 0,
        OpenRowSetVariable = 1,
        SqlCommand = 2,
        SqlCommandVariable = 3
    }
}
