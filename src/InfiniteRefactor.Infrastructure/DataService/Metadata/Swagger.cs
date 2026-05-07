//using Newtonsoft.Json;
//using Newtonsoft.Json.Converters;

using System;
using System.Collections.Generic;

namespace InfiniteRefactor.Infrastructure.DataService.Metadata
{
    internal static class SwaggerHelper
    {
        internal static Tag GetTag(this ServiceInfo serviceInfo)
        {
            var tag = new Tag { name = serviceInfo.Name, description = serviceInfo.Description };
            if (!string.IsNullOrWhiteSpace(serviceInfo.ExternalDocumentUrl))
            {
                tag.externalDocs = new ExternalDoc { url = serviceInfo.ExternalDocumentUrl, description = serviceInfo.ExternalDocumentDescription };
            }
            return tag;
        }
    }

    public class SwaggerIgnoreAttribute : Attribute
    {

    }

    public class SwaggerDef
    {
        public string swagger { get; set; } = "2.0";
        public Info info { get; set; } = new Info();
        public string host { get; set; } = "nicepnr.com";
        public string basePath { get; set; } = "/";
        public string[] schemes { get; set; } = new string[] { "http" };
        public string[] consumes { get; set; } = new string[] { "application/json" };
        public string[] produces { get; set; } = new string[] { "application/json" };
        public Dictionary<string, PathItem> paths { get; set; } = new Dictionary<string, PathItem>();
        public Dictionary<string, Schema> definitions { get; set; } = new Dictionary<string, Schema>();
        public List<Tag> tags { get; set; } = new List<Tag>();
    }

    public class Tag
    {
        public string name { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string description { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public ExternalDoc externalDocs { get; set; }
    }

    public class ExternalDoc
    {
        public string url { get; set; }
        public string description { get; set; }
    }

    public class Info
    {
        public string version { get; set; } = "1.0.0";
        public string title { get; set; } = "Test api";
        public License license { get; set; } = new License();
    }

    public class License
    {
        public string name { get; set; } = "Apache 2.0";
    }

    public class Schema
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "$ref", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("$ref")]
        public string _ref { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Dictionary<string, Schema> properties { get; set; }
        [Newtonsoft.Json.JsonIgnore,System.Text.Json.Serialization.JsonIgnore]
        public Type NetType { get; internal set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Parameter additionalProperties { get; set; }
    }

    public class PathItem
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "$ref", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("$ref")]
        public string _ref { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Operation get { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Operation post { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Operation put { get; set; }
    }

    public class Operation
    {
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string summary { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string description { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public ExternalDoc externalDocs { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string operationId { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string[] tags { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public List<Parameter> parameters { get; private set; } = new List<Parameter>();
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Dictionary<string, Response> responses { get; private set; } = new Dictionary<string, Response>();
    }

    public class Response
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "$ref", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("$ref")]
        public string _ref { get; set; }
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string description { get; set; } = "";
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Dictionary<string, Header> headers { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Schema schema { get; set; }
    }

    public class Header
    {
        public string type { get; set; }
        public string description { get; set; }
    }

    public enum ParameterInType
    {
        body,
        query,
        path,
        header,
        form
    }

    public class Parameter : Schema
    {
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string name { get; set; }
        [Newtonsoft.Json.JsonProperty(PropertyName = "in", NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonPropertyName("in")]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public ParameterInType? _in { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string description { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool? required { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string type { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string format { get; set; }
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string example { get; set; }
    }

    public class ArrayParameter : Parameter
    {
        public Parameter items { get; set; }
    }

    public class EnumParameter : Parameter
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "enum")]
        [System.Text.Json.Serialization.JsonPropertyName("enum")]
        public string[] _enum { get; set; }
    }

    public class ObjectParameter : Parameter
    {
        [Newtonsoft.Json.JsonProperty(NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public Schema schema { get; set; }
    }
}
