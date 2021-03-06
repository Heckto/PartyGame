﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AuxLib;

namespace Game1.HUD
{
    public abstract class HUDComponent
    {
        public bool IsAlive = true;
        public bool Enabled = true;
        public bool Visible = true;

        public abstract void Draw(SpriteBatcher SpriteBatcher, GameTime gameTime);
        public abstract void Update(GameTime gameTime);
    }
}
