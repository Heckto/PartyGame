﻿using AuxLib;
using Game1.GameObjects.Levels;
using Game1.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
//using ProjectMercury;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Game1.GameObjects.ParticleEffects
{

    //[XmlInclude(typeof(FireEffect))]
    //public class ParticleEffectObject : GameObject, IDrawableItem, IUpdateableItem
    //{

    //    protected ParticleEffect effect;

    //    public ParticleEffectObject() : base()
    //    {
    //        _Material = RenderMaterial.DefaultParticleMaterial;
    //    }

    //    public Vector2 Size = new Vector2(20, 20);

    //    public override GameObject clone()
    //    {            
    //        throw new NotImplementedException();
    //    }

    //    public override bool contains(Vector2 worldpos)
    //    {
    //        var r = getBoundingBox();
    //        return r.Contains(worldpos);
    //    }

    //    //public void Draw(SpriteBatcher sb, Matrix mat)
    //    //{   sb.Draw(this.effect, ref mat);
    //    //    //batch.Draw(this.effect, ref mat);
    //    //}

    //    public void Draw(SpriteBatcher sb)
    //    {
    //        for (var i = 0; i < effect.Count; i++)
    //        {
    //            if (effect[i].ParticleTexture != null && effect[i].ActiveParticlesCount > 0)
    //            {
    //                // Calculate the source rectangle and origin offset of the Particle texture...
    //                var source = new Rectangle(0, 0, effect[i].ParticleTexture.Width, effect[i].ParticleTexture.Height);
    //                var origin = new Vector2(source.Width / 2f, source.Height / 2f);

    //                for (var j = 0; j < effect[i].ActiveParticlesCount; j++)
    //                {
    //                    var particle = effect[i].Particles[j];

    //                    float scale = particle.Scale / effect[i].ParticleTexture.Width;
    //                    sb.Draw(effect[i].ParticleTexture, particle.Position, source, new Color(particle.Colour), particle.Rotation, origin, scale, SpriteEffects.None, 0f);
    //                }

    //            }
    //        }
            
    //    }

    //    public override void Initialize()
    //    {
    //        Material = RenderMaterial.DefaultParticleMaterial;
    //    }

    //    public override void drawInEditor(SpriteBatcher sb)
    //    {
    //        if (!Visible) return;
    //        var c = hovering ? new Color(255, 0, 0, 228) : Color.Yellow;
    //        Primitives.Instance.drawBox(sb, boundingrectangle, c, 2);
    //    }

    //    public override void drawSelectionFrame(SpriteBatcher sb, Matrix matrix, Color color)
    //    {
    //        Primitives.Instance.drawBox(sb, this.boundingrectangle.Transform(matrix), color, 2);
    //        var poly = boundingrectangle.Transform(matrix).ToPolygon();
    //        foreach (var p in poly)
    //        {
    //            Primitives.Instance.drawCircleFilled(sb, p, 4, color);
    //        }
    //        Primitives.Instance.drawBoxFilled(sb, poly[0].X - 5, poly[0].Y - 5, 10, 10, color);
    //    }

    //    public override Rectangle getBoundingBox()
    //    {
    //        return new Rectangle((int)(Transform.Position.X), (int)(Transform.Position.Y), (int)Size.X, (int)Size.Y);
    //    }

    //    public override void OnTransformed()
    //    {
    //        //polygon = new Vector2[4];

    //        Transform.GetLocalMatrix(out var mat);

    //        var leftTop = new Vector2(0, 0);
    //        var rightTop = new Vector2(Size.X, 0);
    //        var leftBottom = new Vector2(0, Size.Y);
    //        var rightBottom = new Vector2(Size.X, Size.Y);

    //        //var Origin = new Vector2(Width / 2, Height / 2);

    //        // Transform all four corners into work space
    //        leftTop = Vector2.Transform(leftTop, mat);
    //        rightTop = Vector2.Transform(rightTop, mat);
    //        leftBottom = Vector2.Transform(leftBottom, mat);
    //        rightBottom = Vector2.Transform(rightBottom, mat);

    //        //polygon[0] = leftTop;
    //        //polygon[1] = rightTop;
    //        //polygon[3] = leftBottom;
    //        //polygon[2] = rightBottom;

    //        // Find the minimum and maximum extents of the rectangle in world space
    //        var min = Vector2.Min(Vector2.Min(leftTop, rightTop),
    //                                  Vector2.Min(leftBottom, rightBottom));
    //        var max = Vector2.Max(Vector2.Max(leftTop, rightTop),
    //                                  Vector2.Max(leftBottom, rightBottom));

    //        // Return as a rectangle
    //        boundingrectangle = new Rectangle((int)min.X, (int)min.Y,
    //                             (int)(max.X - min.X), (int)(max.Y - min.Y));
    //    }

    //    public void Update(GameTime gameTime, Level lvl)
    //    {
    //        this.effect.Trigger(Transform.Position);
    //        this.effect.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
    //    }
    //}
}
