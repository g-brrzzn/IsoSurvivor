using Microsoft.Xna.Framework;
using System;

namespace IsometricGame
{
    public static class IsoMath
    {
        public static Vector2 WorldToScreen(Vector3 worldPosition)
        {
            float screenX = (worldPosition.X - worldPosition.Y) * (Constants.IsoTileSize.X / 2f);
            float screenY = (worldPosition.X + worldPosition.Y) * (Constants.IsoTileSize.Y / 2f);

            // --- ADIÇÃO: Subtrai a altura Z da posição Y na tela ---
            // Cada unidade de Z "levanta" o sprite na tela
            screenY -= worldPosition.Z * Constants.TileHeightFactor;
            // --- FIM DA ADIÇÃO ---

            return new Vector2(screenX, screenY);
        }
        public static Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            float tileWidth = Constants.IsoTileSize.X;
            float tileHeight = Constants.IsoTileSize.Y;

            float worldX = (screenPosition.X / (tileWidth / 2f) + screenPosition.Y / (tileHeight / 2f)) / 2f;
            float worldY = (screenPosition.Y / (tileHeight / 2f) - (screenPosition.X / (tileWidth / 2f))) / 2f;
            return new Vector2(worldX, worldY);
        }
        public static float GetDepth(Vector3 worldPosition)
        {
            // Define um peso para Z. Multiplicar por WorldSize.X garante que
            // Z tenha um impacto significativo, maior que apenas X ou Y sozinhos.
            float zWeight = Constants.WorldSize.X;

            float maxPossibleUnits = Constants.WorldSize.X + Constants.WorldSize.Y + Constants.MaxZLevel * zWeight;
            float currentUnits = worldPosition.X + worldPosition.Y + worldPosition.Z * zWeight;

            // Normaliza (0.0 para topo/frente, 1.0 para base/fundo)
            float normalizedDepth = currentUnits / maxPossibleUnits;

            // Inverte (1.0 para topo/frente - desenhado primeiro, 0.0 para base/fundo - desenhado por último)
            return Math.Clamp(1.0f - normalizedDepth, 0.0f, 1.0f);
        }
    }
}