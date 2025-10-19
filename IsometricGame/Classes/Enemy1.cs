using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace IsometricGame.Classes
{
    public class Enemy1 : EnemyBase
    {
        // --- MODIFICAÇÃO: Passa Vector3 para o construtor base ---
        // Adiciona um Z=0 padrão se chamado com Vector2 (mantém compatibilidade)
        public Enemy1(Vector2 worldPosXY)
            : this(new Vector3(worldPosXY.X, worldPosXY.Y, 0)) { }

        // Novo construtor que aceita Vector3
        public Enemy1(Vector3 worldPos)
            : base(worldPos, new List<string> {
                "enemy1_idle_south", "enemy1_idle_west",
                "enemy1_idle_north", "enemy1_idle_east" // Garanta que estes estão no AssetManager
            })
        {
            Life = 1;
            Weight = 1;
            Speed = 3.0f;
        }
    }
}