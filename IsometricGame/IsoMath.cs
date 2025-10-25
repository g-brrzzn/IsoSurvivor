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

            screenY -= worldPosition.Z * Constants.TileHeightFactor;

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
            float maxXY = Math.Max(1f, Constants.WorldSize.X + Constants.WorldSize.Y);            float currentXY = worldPosition.X + worldPosition.Y;

            float normalized = currentXY / maxXY;            return MathHelper.Clamp(1f - normalized, 0f, 1f);
        }
    }
}
