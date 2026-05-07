using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Annotations;
using InfiniteRefactor.Infrastructure.Extensions;

namespace InfiniteRefactor.Infrastructure.DataService.Metadata
{
    public class ParamInfo
    {
        private static Dictionary<Type, IParameterValueReader> _valueReaderCache = new Dictionary<Type, IParameterValueReader>();
        public Type Type { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string RawName { get; set; }
        public bool Optional { get; set; }
        public bool IsOut { get; set; }
        public object DefaultValue { get; set; }
        public IParameterValueReader ValueReader { get; set; }

        public ParamInfo(DataServiceMethodAttribute methodAttribute, ParameterInfo parameterInfo)
        {
            Name = RawName = parameterInfo.Name;
            Type = parameterInfo.ParameterType;
            var attr = parameterInfo.GetCustomAttribute<DataServiceParamAttribute>();
            if (attr != null)
            {
                Name = attr.Name ?? Name;
                ValueReader = GetValueReader(attr.ValueReader, attr.Parameters);
            }
            if (ValueReader == null)
            {
                //增加这个方便一个dataservice里，混合使用不同valuereader.
                //DataServiceMethodAttribute
                ValueReader = GetValueReader(methodAttribute.ParameterValueReader, methodAttribute.ValueReaderParameters);
            }
            if (ValueReader == null)
            {
                //global config + self default;
                ValueReader = DataServiceDefaultConfiguration.DefaultParameterReader ?? new DefaultValueReader();
            }
            DefaultValue = parameterInfo.DefaultValue;
            IsOut = parameterInfo.IsOut;
            Optional = parameterInfo.IsOptional;
        }

        private static IParameterValueReader GetValueReader(Type type, object[] parameters)
        {
            if (type == null) { return null; }
            if (parameters != null && parameters.Length > 0)
            {
                return Activator.CreateInstance(type, parameters).To<IParameterValueReader>();
            }
            else if (!_valueReaderCache.ContainsKey(type))
            {
                lock (_valueReaderCache)
                {
                    if (!_valueReaderCache.ContainsKey(type))
                    {
                        _valueReaderCache[type] = Activator.CreateInstance(type).To<IParameterValueReader>();
                    }
                }
            }
            return _valueReaderCache[type];
        }
    }

    public class DefaultValueReader : IParameterValueReader
    {
        //private ParamInfo paramInfo;
        //public DefaultValueReader(ParamInfo paramInfo)
        //{
        //    this.paramInfo = paramInfo;
        //}

        public object ReadValue(ParamInfo paramInfo, DataServiceRequest request)
        {
            return request.ReadParameter(paramInfo);
        }

        public async Task<object> ReadValueAsync(ParamInfo paramInfo, DataServiceRequest request)
        {
            return await Task.FromResult(request.ReadParameter(paramInfo));
        }
    }

    /// <summary>
    /// ReadValue上直接增加参数ParamInfo，免去繁琐配置
    /// </summary>
    public interface IParameterValueReader
    {
        object ReadValue(ParamInfo paramInfo, DataServiceRequest request);
        System.Threading.Tasks.Task<object> ReadValueAsync(ParamInfo paramInfo, DataServiceRequest request);
    }
    //public class CriteriaParameterReader : IParameterValueReader
    //{
    //    private class SortInfo
    //    {
    //        public string property { get; set; }
    //        public string direction { get; set; }
    //    }

    //    public object ReadValue(DataServiceRequest request)
    //    {
    //        var sort = request.ReadParameter<List<SortInfo>>("sort", null);
    //        var property = (sort != null && sort.Count > 0) ? sort[0].property : string.Empty;
    //        var dir = (sort != null && sort.Count > 0) ? sort[0].direction : string.Empty;

    //        var criteria = ParseCriteria(
    //            request.ReadParameter<int>("start", -1),
    //            request.ReadParameter<int>("limit", -1),
    //            property,
    //            dir,                
    //            request.ReadParameter<string>("filter", null));
    //        return criteria;
    //    }

    //    public static Criteria ParseCriteria(int start, int limit, string sort, string dir, string precidate)
    //    {
    //        var criteria = new Criteria { Start = start, Limit = limit, OrderBy = sort + " " + dir };
    //        if (string.IsNullOrWhiteSpace(precidate)) { return criteria; }
    //        var exps = precidate.Split(new string[] { " and " }, StringSplitOptions.RemoveEmptyEntries);
    //        var pres = new List<string>();
    //        var preValues = new List<object>();
    //        for (var i = 0; i < exps.Length; i++)
    //        {
    //            var j = exps[i].Split('|');
    //            var left = j.First();
    //            var p = left.Split('#').First();
    //            var dataType = left.Split('#').Last();
    //            pres.Add(string.Format(p, "@" + i));
    //            if (j.Length == 2)
    //            {
    //                object right = j.Last();
    //                switch (dataType)
    //                {
    //                    case "int": { right = Convert.ToInt32(right); break; }
    //                    case "decimal": { right = Convert.ToDecimal(right); break; }
    //                    case "bool": { right = Convert.ToBoolean(right); break; }
    //                    case "date": { right = Convert.ToDateTime(right); break; }

    //                }
    //                preValues.Add(right);
    //            }
    //        }
    //        criteria.Precidate = string.Join(" and ", pres.ToArray());
    //        criteria.PrecidateValue = preValues.ToArray();
    //        return criteria;
    //    }
    //}
}
