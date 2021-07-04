using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Palantir.Homematic
{
    public record ChannelInformation
    {
        public string Address { get; init; }

        public int AesActive { get; init; }
        
        public string AvailableFirmware { get; init; }
        
        public object Children { get; init; }
        
        public int Direction { get; init; }
        
        public string Firmware { get; init; }
        
        public int Flags { get; init; }
        
        public string Group { get; init; }
        
        public string Identifier { get; init; }
        
        public int Index { get; init; }
        
        public string Interface { get; init; }
        
        public string LinkSourceRoles { get; init; }
        
        public string LinkTargetRoles { get; init; }
        
        public IEnumerable<string> Paramsets { get; } = new List<string>();
        
        public string Parent { get; init; }
        
        public string ParentType { get; init; }
        
        public int RfAddress { get; init; }
        
        public int Roaming { get; init; }
        
        public int RxMode { get; init; }
        
        public string Team { get; init; }
        
        public object TeamChannels { get; init; }
        
        public string TeamTag { get; init; }
        
        public string Title { get; init; }
        
        public string Type { get; init; }
        
        public int Version { get; init; }

        [JsonPropertyName("~links")]
        public IEnumerable<Link> Links { get; init; } = new List<Link>();
    }
}
