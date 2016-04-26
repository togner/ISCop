using System.Reflection;
using Microsoft.SqlServer.Dts.Runtime;

namespace BIDSHelper.SSIS
{
    public class ComponentInfo
    {
        private DTSPipelineComponentType componentType;
        private string id;
        private string name;
        private string creationName;

        public ComponentInfo(PipelineComponentInfo componentInfo)
        {
            this.componentType = componentInfo.ComponentType;
            this.id = componentInfo.ID;
            this.name = componentInfo.Name;
            this.creationName = componentInfo.CreationName;
        }

        public ComponentInfo(TaskInfo componentInfo)
        {
            this.id = componentInfo.ID;
            this.name = componentInfo.Name;
            this.creationName = componentInfo.CreationName;
        }

        private static Assembly GetComponentAssembly(IDTSName name)
        {
            Assembly assembly = null;
            try
            {
                string assemblyName = name.ID.Remove(0, 1 + name.ID.IndexOf(',')).Trim();
                
                // Check for GUID as string, and exit
                if (assemblyName.StartsWith("{"))
                {
                    return null;
                }

                assembly = Assembly.Load(assemblyName);
            }
            catch { }

            return assembly;
        }
        
        public DTSPipelineComponentType ComponentType
        {
            get { return this.componentType; }
        }

        public string ID
        {
            get { return this.id; }
        }

        public string Name
        {
            get { return this.name; }
        }

        public string CreationName
        {
            get { return this.creationName; }
        }
    }
}
