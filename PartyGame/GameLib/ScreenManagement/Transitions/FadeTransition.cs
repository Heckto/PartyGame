using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuxLib.Debug;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AuxLib.ScreenManagement.Transitions
{
    public class FadeTransition : Transition
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly SpriteBatcher _spriteBatch;

        public FadeTransition(GraphicsDevice graphicsDevice, Color color, float duration = 1.0f)
            : base(duration)
        {
            Color = color;

            _graphicsDevice = graphicsDevice;
            _spriteBatch = new SpriteBatcher(graphicsDevice);
        }

        public override void Dispose()
        {
            _spriteBatch.Dispose();
        }

        public Color Color { get; }

        public override void Draw(GameTime gameTime)
        {
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.DrawBoxFilled(_graphicsDevice.Viewport.Bounds, Color * Value);
            _spriteBatch.End();
        }
    }
}
