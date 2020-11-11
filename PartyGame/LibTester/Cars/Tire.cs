using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibTester.Controllers
{
	public class Tire
	{
		public Transform2 transform {get;set;}
		public float RestingWeight { get; set; }
		public float ActiveWeight { get; set; }
		public float Grip { get; set; }
		public float FrictionForce { get; set; }
		public float AngularVelocity { get; set; }
		public float Torque { get; set; }

		public float Radius = 0.5f;

		public Tire(Vector2 pos)
		{
			
			transform = new Transform2(pos,MathHelper.ToRadians(90));
		}
	}
}
