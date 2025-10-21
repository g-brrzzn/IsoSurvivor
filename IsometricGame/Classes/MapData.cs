using System.Collections.Generic;
using Newtonsoft.Json;
namespace IsometricGame.Classes{
    public class TileMappingEntry
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("assetName")]
        public string AssetName { get; set; }
    }

    public class MapLayer
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("zLevel")]        public int ZLevel { get; set; }

        [JsonProperty("data")]        public List<int> Data { get; set; }
    }

    public class MapData
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("tileMapping")]        public List<TileMappingEntry> TileMapping { get; set; }

        [JsonProperty("layers")]        public List<MapLayer> Layers { get; set; }
    }
}