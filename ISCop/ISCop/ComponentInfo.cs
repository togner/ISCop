using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Dts.Pipeline.Wrapper;
using Microsoft.SqlServer.Dts.Runtime;

namespace ISCop
{
    public class ComponentInfo 
    {
        public const string SortTransformName = "Sort";
        public const string OleDbSourceName = "OLE DB Source";
        public const string AdoNetSourceName = "ADO NET Source";
        public const string LookupName = "Lookup";
        public const string ScriptName = "Script Component";
        public const string OleDbDestinationName = "OLE DB Destination";
        private const string ManagedComponentClassID = "{874F7595-FB5F-40FF-96AF-FBFF8250E3EF}";

        public string Id { get; protected set; }
        public string Name { get; protected set; }
        public string CreationName { get; protected set; }
        public DTSPipelineComponentType ComponentType { get; protected set; }

        private static List<PipelineComponentInfo> _infoCache;
        private static HashSet<ComponentInfo> _componentCache = new HashSet<ComponentInfo>();
        protected ComponentInfo(PipelineComponentInfo pipelineComponent)
        {
            if (pipelineComponent == null)
            {
                throw new ArgumentNullException("pipelineComponent");
            }
            this.Id = pipelineComponent.ID;
            this.Name = pipelineComponent.Name;
            this.CreationName = pipelineComponent.CreationName;
            this.ComponentType = pipelineComponent.ComponentType;
        }

        public static ComponentInfo Create(IDTSComponentMetaData100 metadata)
        {
            if (metadata == null)
            {
                return null;
            }
            string id = metadata.ComponentClassID == ComponentInfo.ManagedComponentClassID
                ? metadata.GetCustomPropertyValue<string>(CustomPropertyNames.UserComponentTypeName)
                : metadata.ComponentClassID;
            var comp = ComponentInfo._componentCache.FirstOrDefault(c => c.Id == id);
            if (comp == null)
            {
                var pipelineComp = ComponentInfo.Find(id, null, null, null);
                if (pipelineComp != null)
                {
                    comp = new ComponentInfo(pipelineComp);
                    ComponentInfo._componentCache.Add(comp);
                }
            }
            return comp;
        }

        protected static PipelineComponentInfo Find(string id, string name, string creationName, DTSPipelineComponentType? componentType)
        {
            if (ComponentInfo._infoCache == null)
            {
                ComponentInfo._infoCache = new Application().PipelineComponentInfos.OfType<PipelineComponentInfo>().ToList();
            }
            var info = _infoCache.FirstOrDefault(p =>
                (id == null || p.ID == id)
                && (name == null || p.Name == name)
                && (creationName == null || p.CreationName == creationName)
                && (!componentType.HasValue || p.ComponentType == componentType.Value));
            return info;
        }
    }
}
