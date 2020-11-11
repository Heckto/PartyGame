using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;

namespace AuxLib.Debug
{
    public class DebugMonitor : DrawableGameComponent
    {
        private const int itemSize = 20;
        private Dictionary<string, object> debugData;
        private SpriteFont font;
        private SpriteBatcher spriteBatch;

        private readonly FpsMonitor fpsMonitor;

        public DebugMonitor(Game game) : base(game)
        {
            debugData = new Dictionary<string, object>();
            fpsMonitor = new FpsMonitor();
        }

        public override void Initialize()
        {
            var content = Game.Content;
            spriteBatch = Game.Services.GetService<SpriteBatcher>();
            font = content.Load<SpriteFont>("Font/DiagnosticsFont");
        }

        

        public void AddDebugValue(string name,object value)
        {
            debugData[name] = value;
        }

        public void RemoveDebugValue(string key)
        {
            if (debugData.ContainsKey(key))
                debugData.Remove(key);
        }

        public void Clear()
        {
            debugData.Clear();
        }


        public override void Draw(GameTime gameTime)
        {

            const float maxHeight = 20f;
            var itemCnt = debugData.Count();
            if (itemCnt > 0)
            {
                var vectSize = itemCnt * maxHeight + 3f * maxHeight;
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                spriteBatch.DrawBoxFilled(new Rectangle(50, 50, 400, (int)vectSize), new Color(Color.Black,0.5f));
                var vertIdx = 70;
                var idx = 0;
                foreach (var item in debugData)
                {
                    spriteBatch.DrawString(font, $"{item.Key} : { item.Value}", new Vector2(60, idx++ * itemSize + vertIdx), Color.White);
                }
                spriteBatch.End();
            }
        }

        public override void Update(GameTime gameTime)
        {
            fpsMonitor.Update();
            
            AddDebugValue("FPS", fpsMonitor.Value);
            base.Update(gameTime);
        }
    }
}
