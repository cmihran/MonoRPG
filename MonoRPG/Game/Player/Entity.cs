using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoRPG.Desktop.Game.Player {
    public interface Entity {
        
        Color Color { get; }

        GameRPG Game { get; }

        void Draw(GameTime gameTime);

        void Update(GameTime gameTime);

    }
}
