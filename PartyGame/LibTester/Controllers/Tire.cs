﻿using Microsoft.Xna.Framework;
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

		float TrailDuration = 5;
		bool TrailActive;

		public Tire(Vector2 pos)
		{
			
			transform = new Transform2(pos);
		}
		//GameObject Skidmark;

		//public void SetTrailActive(bool active)
		//{
		//	if (active && !TrailActive)
		//	{
		//		// These should be pooled and re-used
		//		Skidmark = GameObject.Instantiate(Resources.Load("Skidmark") as GameObject);

		//		//Fix issue where skidmarks draw at 0,0,0 at slow speeds
		//		Skidmark.GetComponent<TrailRenderer>().Clear();

		//		Skidmark.GetComponent<TrailRenderer>().time = TrailDuration;
		//		Skidmark.GetComponent<TrailRenderer>().sortingOrder = 0;
		//		Skidmark.transform.parent = this.transform;
		//		Skidmark.transform.localPosition = Vector2.zero;
		//	}
		//	else if (!active && TrailActive)
		//	{
		//		Skidmark.transform.parent = null;
		//		GameObject.Destroy(Skidmark.gameObject, TrailDuration);
		//	}
		//	TrailActive = active;
		//}
	}
	}