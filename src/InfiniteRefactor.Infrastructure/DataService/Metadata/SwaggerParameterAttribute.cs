using System;

namespace InfiniteRefactor.Infrastructure.DataService.Metadata
{
    public class SwaggerParameterAttribute : Attribute
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsRequired { get; set; }
        public string Format { get; set; }
        public string Example { get; set; }
    }
}
