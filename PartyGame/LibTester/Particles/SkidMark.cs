using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Particles;
using MonoGame.Extended.Particles.Modifiers;
using MonoGame.Extended.Particles.Modifiers.Containers;
using MonoGame.Extended.Particles.Modifiers.Interpolators;
using MonoGame.Extended.Particles.Profiles;
using MonoGame.Extended.TextureAtlases;

namespace LibTester.Particles
{
    public class SkidMarkEffect : ParticleEffect
    {
        Texture2D _particleTexture;
        public SkidMarkEffect(ContentManager content) : base(autoTrigger : false)
        {
            _particleTexture = content.Load<Texture2D>("Particles/Square");

            Emitters = new List<ParticleEmitter>
            {
                new ParticleEmitter(new TextureRegion2D(_particleTexture), 2000, TimeSpan.FromSeconds(3.5),
                    Profile.Point())
                {
                    Parameters = new ParticleReleaseParameters
                    {
                        Scale = new Range<float>(0.1f,0.2f),
                        Quantity = 1,
                        Rotation = new Range<float>(-2f, 2f)                        
                    },
                    Modifiers =
                    {
                        new AgeModifier
                        {
                            Interpolators =
                            {
                                new ColorInterpolator
                                {                                    
                                    StartValue = new HslColor(0f, 1.0f, 0.0f),
                                    EndValue = new HslColor(0.0f, 0.0f, 0.0f)
                                }
                            }
                        },
                    }
                }
            };
        }
    }
}
