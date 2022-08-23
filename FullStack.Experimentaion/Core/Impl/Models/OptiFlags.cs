using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FullStack.Experimentaion.Core.Impl.Models.OptiFeature;

namespace FullStack.Experimentaion.Core.Impl.Models
{
    public class OptiFlag
    {
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("outlier_filtering_enabled")]
        public bool OutlierFilteringEnabled { get; set; }
        [JsonProperty("variable_definitions")]
        public VariableDefinitions VariableDefinition { get; set; }




        public class VariableDefinitions
        {
            [JsonProperty("amount")]
            public Variable Amount { get; set; }
            [JsonProperty("message")]
            public Variable Message { get; set; }
            [JsonProperty("advanced")]
            public Variable Advanced { get; set; }
        }

    }
}
