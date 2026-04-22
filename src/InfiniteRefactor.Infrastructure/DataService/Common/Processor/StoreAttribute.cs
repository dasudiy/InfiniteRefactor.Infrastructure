using System;

namespace AirMaster.Infrastructure.DataService.Common.Processor
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class StoreAttribute : Attribute
    {
        private Type _modelType;

        public StoreAttribute(Type modelType)
        {
            _modelType = modelType;
        }

        internal string Name { get { return _modelType.Name; } }

        internal string GetStoreName(string moduleName)
        {
            return moduleName + ".store." + _modelType.Name + "s";
        }

        internal string GetModelName(string moduleName)
        {
            return moduleName + ".model." + _modelType.Name;
        }

        public StoreAction Action { get; set; }
    }

    public enum StoreAction
    {
        read,
        create,
        update,
        destory
    }
}
