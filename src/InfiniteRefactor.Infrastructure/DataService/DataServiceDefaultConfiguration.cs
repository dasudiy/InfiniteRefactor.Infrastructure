using InfiniteRefactor.Infrastructure.DataService.Abstractions;
using InfiniteRefactor.Infrastructure.DataService.Common;
using InfiniteRefactor.Infrastructure.DataService.Metadata;

namespace InfiniteRefactor.Infrastructure.DataService
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
