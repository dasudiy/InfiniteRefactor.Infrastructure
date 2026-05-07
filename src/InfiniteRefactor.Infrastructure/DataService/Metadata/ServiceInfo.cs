using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Abstractions.Processor;
using InfiniteRefactor.Infrastructure.DataService.Annotations;

namespace InfiniteRefactor.Infrastructure.DataService.Metadata
{
    public class ServiceInfo
    {
        public Type ServiceType { get; private set; }
        public Dictionary<string, ActionInfo> Actions { get; private set; }
        public IServiceActivator Creator { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ExternalDocumentUrl { get; set; }
        public string ExternalDocumentDescription { get; set; }
        public string RawName { get; set; }
        public List<IPreProcessor> PreProcessors { get; private set; }
        public List<IPostProcessor> PostProcessors { get; private set; }
        public SessionRequireType SessionRequireType { get; private set; }
        public bool UseAsyncHandle { get; private set; }

        public ServiceInfo(Type type, IServiceActivator defaultCreator, IEnumerable<IPreProcessor> preprocessor, IEnumerable<IPostProcessor> postprocessor)
        {
            Name = RawName = type.Name;
            var attr = type.GetCustomAttribute<DataServiceAttribute>();

            Name = attr.Name ?? Name;
            Creator = attr.InstanceRetriver != null ? Activator.CreateInstance(attr.InstanceRetriver) as IServiceActivator : defaultCreator;
            //if (type.IsInterface && attr.InstanceType == null)
            //{
            //    throw new ArgumentNullException("接口服务InstanceType不能为null");
            //}
            ServiceType = attr.InstanceType ?? type;
            Description = attr.Description;            
            this.SessionRequireType = attr.SessionRequireType;
            this.UseAsyncHandle = attr.UseAsyncHandle;

            var processors = type.GetCustomAttributes<ProcessorAttribute>();
            PreProcessors = new List<IPreProcessor>(preprocessor);
            PreProcessors.AddRange(processors.Where(a => a is IPreProcessor).Cast<IPreProcessor>());

            PostProcessors = new List<IPostProcessor>(postprocessor);
            PostProcessors.AddRange(processors.Where(a => a is IPostProcessor).Cast<IPostProcessor>());

            Actions = GetActionInfos(type);
        }

        public Dictionary<string, ActionInfo> GetActionInfos(Type dataServiceType)
        {
            var result = new Dictionary<string, ActionInfo>();
            var methods = from m in dataServiceType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                          where m.GetCustomAttribute<DataServiceMethodAttribute>() != null
                          select m;
            foreach (var method in methods)
            {
                var actionInfo = new ActionInfo(method, PreProcessors, PostProcessors);
                //actionInfo.PreProcessors.AddRange(PreProcessors);
                //actionInfo.PostProcessors.AddRange(PostProcessors);
                result[actionInfo.Name] = actionInfo;
            }
            return result;
        }

        //public void Invoke(ActionInfo action, DataServiceContext context)
        //{
        //    if (DataServiceHost.PreProcessContext(context, PreProcessors))
        //    {
        //        context.ServiceInstance = GetInstance();
        //        context.ServiceInstanceRequireDispose = Creator.RequireDispose;
        //        action.Invoke(context);
        //    }
        //    DataServiceHost.PostProcessContext(context, PostProcessors);
        //}

        internal object GetInstance()
        {
            return Creator.GetInstance(ServiceType);
        }
    }
}
