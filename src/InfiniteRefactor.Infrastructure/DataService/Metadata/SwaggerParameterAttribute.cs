using System;
using System.Collections.Generic;
using System.Text;

namespace AirMaster.Infrastructure.DataService.Metadata
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
