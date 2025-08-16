using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace StorySpoiler.Models
{
    internal class ApiResponseDTO
    {
        [JsonPropertyName ("msg")]
        public string Msg { get; set; }

        [JsonPropertyName("storyID")]
        public string StoryId { get; set; }
    }
}