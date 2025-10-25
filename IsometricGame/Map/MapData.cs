// Directory: Map
// MapData.cs (Adicionado MapTrigger e Triggers)

using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Xna.Framework; // Adicionado para Vector3

namespace IsometricGame.Map
{
    public class TileMappingEntry
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("assetName")]
        public string AssetName { get; set; }

        [JsonProperty("solid")]
        public bool Solid { get; set; }
    }

    public class MapLayer
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("zLevel")] public int ZLevel { get; set; }

        [JsonProperty("data")] public List<int> Data { get; set; }
    }

    // --- NOVA CLASSE PARA TRIGGERS ---
    public class MapTrigger
    {
        [JsonProperty("id")] // Opcional, mas útil para debug ou lógica específica
        public string Id { get; set; }

        [JsonProperty("position")] // Posição do trigger no mapa atual (X, Y, Z)
        public Vector3 Position { get; set; }

        [JsonProperty("targetMap")] // Nome do arquivo JSON do mapa de destino
        public string TargetMap { get; set; }

        [JsonProperty("targetPosition")] // Posição de spawn no mapa de destino (X, Y, Z)
        public Vector3 TargetPosition { get; set; }

        [JsonProperty("radius")] // Raio de ativação (opcional, default pode ser 0.5f)
        public float Radius { get; set; } = 0.5f; // Valor padrão se não especificado no JSON
    }
    // --- FIM DA NOVA CLASSE ---

    public class MapData
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("tileMapping")] public List<TileMappingEntry> TileMapping { get; set; }

        [JsonProperty("layers")] public List<MapLayer> Layers { get; set; }

        // --- NOVA LISTA DE TRIGGERS ---
        [JsonProperty("triggers")]
        public List<MapTrigger> Triggers { get; set; } = new List<MapTrigger>(); // Inicializa para evitar null
        // --- FIM DA NOVA LISTA ---
    }
}