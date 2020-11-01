using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AuxLib;

namespace Game1.GameObjects.Sprite.AnimationEffects
{
    class NoAnimationEffect : IAnimationEffect
    {
        public void Reset() { }

        public void Update(GameTime gameTime) { }

        public void Draw(SpriteBatcher SpriteBatcher, Texture2D texture, Vector2 actualPosition, Rectangle rectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects flipEffects, float layerDepth)
        {
            SpriteBatcher.Draw(texture, actualPosition, rectangle, color, rotation, origin, scale, flipEffects, layerDepth);
        }
        
    }
}
