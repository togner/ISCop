using System;
using System.Reflection;
using Microsoft.SqlServer.Dts.Runtime;

namespace BIDSHelper.SSIS
{
    public class ComponentInfo
    {
        private DTSPipelineComponentType _componentType;
        private string _id;
        private string _name;
        private string _creationName;

        public ComponentInfo(PipelineComponentInfo componentInfo)
        {
            if (componentInfo == null)
            {
                throw new ArgumentNullException("componentInfo");
            }
            this._componentType = componentInfo.ComponentType;
            this._id = componentInfo.ID;
            this._name = componentInfo.Name;
            this._creationName = componentInfo.CreationName;
        }

        public ComponentInfo(TaskInfo componentInfo)
        {
            if (componentInfo == null)
            {
                throw new ArgumentNullException("componentInfo");
            }
            this._id = componentInfo.ID;
            this._name = componentInfo.Name;
            this._creationName = componentInfo.CreationName;
        }
        
        public DTSPipelineComponentType ComponentType
        {
            get { return this._componentType; }
        }

        public string Id
        {
            get { return this._id; }
        }

        public string Name
        {
            get { return this._name; }
        }

        public string CreationName
        {
            get { return this._creationName; }
        }
    }
}
