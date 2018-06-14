using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CoreFX.TestUtils.TestFileSetup
{
    public class XUnitTestAssembly
    {
        [JsonRequired]
        [JsonProperty("name")]
        public string Name;

        [JsonRequired]
        [JsonProperty("exclusions")]
        public Exclusions Exclusions;

        // Used to assign a test url or to override it via the json file definition
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string Url;

    }

    public class Exclusions
    {
        [JsonProperty("namespaces")]
        public Exclusion[] Namespaces;

        [JsonProperty("classes")]
        public Exclusion[] Classes;

        [JsonProperty("methods")]
        public Exclusion[] Methods;
    }

    public class Exclusion
    {
        [JsonRequired]
        [JsonProperty("name", Required = Required.DisallowNull)]
        public string Name;

        [JsonRequired]
        [JsonProperty("reason", Required = Required.DisallowNull)]
        public string Reason;
    }
}
