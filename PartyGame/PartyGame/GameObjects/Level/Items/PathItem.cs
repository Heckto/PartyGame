﻿using AuxLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Xml.Serialization;
using System.Drawing.Design;
using System.ComponentModel;

namespace Game1.GameObjects.Levels
{
    public partial class PathItem : GameObject
    {
        public Vector2[] LocalPoints;
        public Vector2[] WorldPoints;

        public bool IsPolygon;
        public int LineWidth;

        public Color LineColor;
        public ItemTypes ItemType { get; set; } = ItemTypes.None;

        public PathItem() { }

        public override Rectangle getBoundingBox()
        {
            var minX = WorldPoints.Min(min => min.X);
            var maxX = WorldPoints.Max(max => max.X);
            var minY = WorldPoints.Min(min => min.Y);
            var maxY = WorldPoints.Max(max => max.Y);

            return new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
        }



        #region Editable

        [XmlIgnore]
        int pointundermouse = -1;
        int pointgrabbed = -1;
        Vector2 initialpos;

        public PathItem(Vector2[] points)
            : base()
        {
            
            WorldPoints = points;
            LocalPoints = (Vector2[])points.Clone();
            for (var i = 0; i < LocalPoints.Length; i++)
                LocalPoints[i] -= points[0];

            Transform.Position = points[0];
            
            //LineWidth = Constants.Instance.DefaultPathItemLineWidth;
            //LineColor = Constants.Instance.ColorPrimitives;
            LineWidth = 4;
            LineColor = new Color(192, 0, 192, 145);
        }

        public override GameObject clone()
        {
            var result = (PathItem)this.MemberwiseClone();
            result.CustomProperties = new SerializableDictionary(CustomProperties);
            result.LocalPoints = (Vector2[])this.LocalPoints.Clone();
            result.WorldPoints = (Vector2[])this.WorldPoints.Clone();
            result.hovering = false;
            return result;
        }

        public override bool contains(Vector2 worldpos)
        {
            for (var i = 1; i < WorldPoints.Length; i++)
            {
                if (worldpos.DistanceToLineSegment(WorldPoints[i], WorldPoints[i - 1]) <= LineWidth) return true;
            }
            if (IsPolygon)
                if (worldpos.DistanceToLineSegment(WorldPoints[0], WorldPoints[WorldPoints.Length - 1]) <= LineWidth) return true;
            return false;
        }

        /// <summary>
        /// Calculates the WorldPoints based on Position and LocalPoints
        /// </summary>
        public override void OnTransformed()
        {
            for (var i = 0; i < WorldPoints.Length; i++) WorldPoints[i] = LocalPoints[i] + Transform.Position;
        }


        public override bool onMouseOver(Vector2 mouseworldpos, out string msg)
        {
            msg = String.Empty;
            pointundermouse = -1;
            for (var i = 0; i < WorldPoints.Length; i++)
            {
                if (mouseworldpos.DistanceTo(WorldPoints[i]) <= 5)
                {
                    pointundermouse = i;
                    //MainForm.Instance.picturebox.Cursor = Cursors.Hand;
//                    MainForm.Instance.toolStripStatusLabel1.Text = Name + " (Point " + i.ToString() + ": " + WorldPoints[i].ToString() + ")";
                    var pointMsg = Name + " (Point " + i.ToString() + ": " + WorldPoints[i].ToString() + ")";
                    return true;
                }
            }
            
            if (pointundermouse == -1)
                return false;
            return false;
                //MainForm.Instance.picturebox.Cursor = Cursors.Default;
            //return base.onMouseOver(mouseworldpos, out string msg);
        }

        public override void onMouseOut()
        {
            base.onMouseOut();
        }

        public override void onMouseButtonDown(Vector2 mouseworldpos)
        {
            hovering = false;
            if (pointundermouse >= 0)
            {
                pointgrabbed = pointundermouse;
                initialpos = WorldPoints[pointundermouse];
            }
//            else
  //              MainForm.Instance.picturebox.Cursor = Cursors.SizeAll;
            base.onMouseButtonDown(mouseworldpos);
        }

        public override void onMouseButtonUp(Vector2 mouseworldpos)
        {
            if (pointgrabbed == 0)
            {
                LocalPoints[0] = Vector2.Zero;
                for (var i = 1; i < LocalPoints.Length; i++)
                {
                    LocalPoints[i] = WorldPoints[i] - WorldPoints[0];
                }
                Transform.Position = WorldPoints[0];
                OnTransformed();
                //MainForm.Instance.propertyGrid1.Refresh();
            }

            pointgrabbed = -1;
            base.onMouseButtonUp(mouseworldpos);
        }        

        public override void drawInEditor(SpriteBatch sb)
        {
            if (!Visible) return;
            var c = hovering ? new Color(255, 0, 0, 228) : LineColor;
            if (IsPolygon)
                Primitives.Instance.drawPolygon(sb, WorldPoints, c, LineWidth);
            else
                Primitives.Instance.drawPath(sb, WorldPoints, c, LineWidth);

        }

        public override void drawSelectionFrame(SpriteBatch sb, Matrix matrix, Color color)
        {
            var transformedPoints = new Vector2[WorldPoints.Length];
            Vector2.Transform(WorldPoints, ref matrix, transformedPoints);

            if (IsPolygon)
                Primitives.Instance.drawPolygon(sb, transformedPoints, color, 2);
            else
                Primitives.Instance.drawPath(sb, transformedPoints, color, 2);

            foreach (var p in transformedPoints)
            {
                Primitives.Instance.drawCircleFilled(sb, p, 4, color);
            }

            Primitives.Instance.drawBoxFilled(sb, transformedPoints[0].X - 5, transformedPoints[0].Y - 5, 10, 10, color);


        }

        #endregion
    }
}
