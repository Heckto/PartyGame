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
        public SkidMarkEffect(ContentManager content,GraphicsDevice graphics) : base(autoTrigger : false)
        {
            
            _particleTexture = new Texture2D(graphics, 8, 8);
            Color[] data = new Color[8 * 8];
            for (int pixel = 0; pixel < data.Length; pixel++)
            {
                //the function applies the color according to the specified pixel
                data[pixel] = Color.Black;
            }
            _particleTexture.SetData(data);

            Emitters = new List<ParticleEmitter>
            {
                new ParticleEmitter(new TextureRegion2D(_particleTexture), 2500, TimeSpan.FromSeconds(2.5),
                    Profile.Point())
                {
                    Parameters = new ParticleReleaseParameters
                    {
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
                                    StartValue = new HslColor(0.33f, 0.5f, 0.5f),
                                    EndValue = new HslColor(0.5f, 0.9f, 1.0f)
                                }
                            }
                        },
                    }
                }
            };
        }
    }
}
