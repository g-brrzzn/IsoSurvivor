using Microsoft.Xna.Framework;
using System;

namespace IsometricGame
{
    public static class IsoMath
    {
        public static Vector2 WorldToScreen(Vector2 worldPosition)
        {
            float screenX = (worldPosition.X - worldPosition.Y) * (Constants.IsoTileSize.X / 2f);
            float screenY = (worldPosition.X + worldPosition.Y) * (Constants.IsoTileSize.Y / 2f);
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
        public static float GetDepth(Vector2 worldPosition)
        {
            float totalWorldUnits = Constants.WorldSize.X + Constants.WorldSize.Y;
            float currentWorldUnits = worldPosition.X + worldPosition.Y;

            return Math.Clamp(currentWorldUnits / totalWorldUnits, 0f, 1f);
        }
    }
}