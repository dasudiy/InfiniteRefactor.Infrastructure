using System;
using System.Collections.Generic;
using System.Text;
using AirMaster.Infrastructure.DataService.Abstractions;
using AirMaster.Infrastructure.DataService.Common;
using AirMaster.Infrastructure.DataService.Metadata;

namespace AirMaster.Infrastructure.DataService
{
    public static class DataServiceDefaultConfiguration
    {
        /// <summary>
        /// 暂时没用
        /// </summary>
        public static DataServiceRouter DefaultServiceRouter { get; set; } = new DefaultServiceRouter();
        /// <summary>
        /// 配置默认参数Reader，可以省很多代码
        /// 比如 Login([DataServiceParam(ValueReader = typeof(PostDataReader<LoginRequest>))] LoginRequest request)
        /// Login(LoginRequest request)
        /// </summary>
        public static IParameterValueReader DefaultParameterReader { get; set; } = new DefaultValueReader();
    }
}
