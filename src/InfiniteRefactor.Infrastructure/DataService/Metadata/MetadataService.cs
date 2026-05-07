using System;
using System.Linq;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Annotations;
using InfiniteRefactor.Infrastructure.DataService.Common.Processor;
using InfiniteRefactor.Infrastructure.Extensions;
using Newtonsoft.Json;
using NLog;

namespace InfiniteRefactor.Infrastructure.DataService.Metadata
{
    [DataService(Name = "Metadata")]
    [SwaggerIgnore]
    public class MetadataService
    {
        private static Logger log = LogManager.GetCurrentClassLogger();

        public static Func<DataServiceContext, ServiceInfo, ActionInfo, bool> ValidateIntercept { get; set; }

        [DataServiceMethod]
        [RawOutput]
        public SwaggerDef Swagger()
        {
            var def = new SwaggerDef();
            def.info.title = DataServiceHost.Instance.Title;
            def.info.version = DataServiceHost.Instance.Version;

            var url = DataServiceContext.Current?.Request?.Url ?? new Uri("http://localhost/api/Metadata/Swagger");
            def.host = url.Host;
            if (!url.IsDefaultPort)
            {
                def.host += ":" + url.Port;
            }

            def.schemes = new string[] { url.Scheme };            
            if (DataServiceContext.Current == null)
            {
                def.basePath = url.AbsolutePath.Substring(0, url.AbsolutePath.IndexOf("Metadata"));
            }
            else
            {
                def.basePath = url.AbsolutePath.Substring(0, url.AbsolutePath.IndexOf(DataServiceContext.Current.RouteInfo.ServiceName));
            }


            foreach (var service in DataServiceHost.Instance.Services)
            {
                if (service.Value.ServiceType.GetCustomAttribute<SwaggerIgnoreAttribute>() != null) { continue; }

                log.Trace($"[Service]开始生成{service.Key}...");
                //判断Service
                if (ValidateIntercept != null && !ValidateIntercept(DataServiceContext.Current, service.Value, null)) { continue; }

                def.tags.Add(service.Value.GetTag());

                foreach (var method in service.Value.Actions)
                {
                    if (method.Value.MethodInfo.GetCustomAttribute<SwaggerIgnoreAttribute>() != null) { continue; }
                    log.Trace($"[Method]开始生成{method.Key}...");
                    //判断Method
                    if (ValidateIntercept != null && !ValidateIntercept(DataServiceContext.Current, service.Value, method.Value)) { continue; }
                    var param = method.Value.Parameters;

                    var path = new PathItem();
                    path.post = new Operation();
                    path.post.summary = method.Value.Summary;
                    path.post.description = method.Value.Description;
                    path.post.tags = new string[] { service.Key };
                    path.post.operationId = method.Key;


                    foreach (var parameter in param.Where(p => p.IsOut == false))
                    {
                        log.Trace($"[Parameter]开始生成{parameter.Name}...");
                        path.post.parameters.Add(GetParameter(def, parameter.Name, !parameter.Optional, parameter.Description, parameter.Type, ParameterInType.body, false));
                    }

                    log.Trace($"[Response]开始生成{method.Value.MethodInfo.ReturnType}...");
                    path.post.responses["200"] = new Response
                    {
                        schema = GetSchema(def, method.Value.MethodInfo.ReturnType, false, null)
                    };
                    def.paths.Add($"/{service.Key}/{method.Key}", path);
                }
            }
            return def;
        }

        private Parameter GetParameter(SwaggerDef def, string name, bool? required, string description, Type type, ParameterInType? inType, bool isDefinition)
        {
            log.Trace($"[Parameter]获取{name}参数定义...");
            var result = new Parameter { name = name, required = required, description = description, _in = inType };
            switch (type.Name)
            {
                case "Nullable`1":
                    return GetParameter(def, name, false, description, type.GetGenericArguments().First(), inType, isDefinition);
                case "Byte":
                case "SByte":
                case "Int16":
                case "Int32":
                case "Int64":
                case "UInt16":
                case "UInt32":
                case "UInt64":
                    result.type = "integer";
                    break;
                case "Double":
                case "Single":
                case "Decimal":
                    result.type = "number";
                    break;
                case "String":
                case "DateTime":
                    result.type = "string";
                    break;
                case "Boolean":
                    result.type = "boolean";
                    break;
                case "List`1":
                    return new ArrayParameter { name = name, required = required, description = description, type = "array", items = GetParameter(def, null, null, null, type.GetGenericArguments().First(), null, isDefinition), _in = inType };
                case "Dictionary`2":
                    return null;
                default:
                    if (type.FullName.StartsWith("System"))
                    {
                        return new EnumParameter { name = name, required = required, description = description, type = "string", _in = inType };
                    }
                    if (type.IsArray)
                    {
                        return new ArrayParameter { name = name, required = required, description = description, type = "array", items = GetParameter(def, null, null, null, type.GetElementType(), null, isDefinition), _in = inType };
                    }
                    else if (type.IsEnum)
                    {
                        return new EnumParameter { name = name, required = required, description = description, type = "string", _enum = type.GetEnumNames(), _in = inType };
                    }
                    else
                    {
                        var obj = new ObjectParameter { name = name, required = required, description = description, schema = GetSchema(def, type, isDefinition, null), _in = inType };
                        if (obj.schema._ref != null && isDefinition)
                        {
                            obj._ref = obj.schema._ref;
                            obj.schema = null;
                        }
                        return obj;
                    }
            }
            return result;
        }

        public Schema GetSchema(SwaggerDef def, Type type, bool isDefinition, SwaggerParameterAttribute parameterAttribute)
        {
            log.Trace($"[Schema]获取{type.FullName}的Schema定义...");
            switch (type.Name)
            {
                case "Nullable`1":
                case "Task`1":
                    return GetSchema(def, type.GetGenericArguments().First(), isDefinition, parameterAttribute);
                case "Byte":
                case "SByte":
                case "Int16":
                case "Int32":
                case "Int64":
                case "UInt16":
                case "UInt32":
                case "UInt64":
                    return new Parameter { type = "integer", name = parameterAttribute?.Name, description = parameterAttribute?.Description, format = parameterAttribute?.Format, required = parameterAttribute?.IsRequired, example = parameterAttribute?.Example };
                case "Double":
                case "Single":
                case "Decimal":
                    return new Parameter { type = "number", name = parameterAttribute?.Name, description = parameterAttribute?.Description, format = parameterAttribute?.Format, required = parameterAttribute?.IsRequired, example = parameterAttribute?.Example };
                case "String":
                case "DateTime":
                    return new Parameter { type = "string", name = parameterAttribute?.Name, description = parameterAttribute?.Description, format = parameterAttribute?.Format, required = parameterAttribute?.IsRequired, example = parameterAttribute?.Example };
                case "Boolean":
                    return new Parameter { type = "boolean", name = parameterAttribute?.Name, description = parameterAttribute?.Description, format = parameterAttribute?.Format, required = parameterAttribute?.IsRequired, example = parameterAttribute?.Example };
                case "List`1":
                    return new ArrayParameter { type = "array", items = GetParameter(def, parameterAttribute?.Name, parameterAttribute?.IsRequired, parameterAttribute?.Description, type.GetGenericArguments().First(), null, isDefinition) };
                case "Dictionary`2":
                    return new Parameter { type = "object", name = parameterAttribute?.Name, description = parameterAttribute?.Description, format = parameterAttribute?.Format, required = parameterAttribute?.IsRequired, additionalProperties = new Parameter { type = "string" }, example = parameterAttribute?.Example };
                default:
                    if (type.IsArray)
                    {
                        return new ArrayParameter { type = "array", name = parameterAttribute?.Name, description = parameterAttribute?.Description, format = parameterAttribute?.Format, required = parameterAttribute?.IsRequired, example = parameterAttribute?.Example, items = GetParameter(def, parameterAttribute?.Name, parameterAttribute?.IsRequired, parameterAttribute?.Description, type.GetElementType(), null, isDefinition) };
                    }
                    else if (type.IsEnum)
                    {
                        return new EnumParameter { type = "string", _enum = type.GetEnumNames(), name = parameterAttribute?.Name, description = parameterAttribute?.Description, format = parameterAttribute?.Format, required = parameterAttribute?.IsRequired, example = parameterAttribute?.Example };
                    }
                    else
                    {
                        if (type.FullName.StartsWith("System"))
                        {
                            return new Parameter { type = "string", name = parameterAttribute?.Name, description = parameterAttribute?.Description, format = parameterAttribute?.Format, required = parameterAttribute?.IsRequired, example = parameterAttribute?.Example };
                        }
                        if (!def.definitions.Any(d => d.Value.NetType == type))
                        {
                            CreateSchema(def, type);
                        }
                        return new Schema { _ref = "#/definitions/" + def.definitions.FirstOrDefault(d => d.Value.NetType == type).Key };
                    }
            }
        }

        private void CreateSchema(SwaggerDef def, Type type)
        {
            log.Trace($"[Schema]创建{type.FullName}定义...");
            var schema = new Schema();
            //.Replace('+', '.'),使得内部类也可以正常展示
            def.definitions[type.ToString().Replace('+', '.')] = schema;

            schema.NetType = type;
            schema.properties = type.GetProperties().Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null && p.GetCustomAttribute<SwaggerIgnoreAttribute>() == null).Select(p =>
            new
            {
                //.Replace('+', '.'),使得内部类也可以正常展示
                key = p.Name.Replace('+', '.'),
                value = GetSchema(def, p.PropertyType, true, p.GetCustomAttribute<SwaggerParameterAttribute>())
            }).ToDictionary(g => g.key, g => g.value);
        }
    }
}
