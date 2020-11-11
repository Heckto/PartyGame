using Microsoft.Xna.Framework;
using tainicom.Aether.Physics2D.Dynamics;

namespace LibTester.Controllers
{
	public class Axis
	{
		public float DistanceToCG { get; set; }
		public float WeightRatio { get; set; }
		public float SlipAngle { get; set; }
		public float FrictionForce
		{
			get
			{
				return (TireLeft.FrictionForce + TireRight.FrictionForce) / 2f;
			}
		}
		public float AngularVelocity
		{
			get
			{
				return TireLeft.AngularVelocity + TireRight.AngularVelocity;
			}
		}
		public float Torque
		{
			get
			{
				return (TireLeft.Torque + TireRight.Torque) / 2f;
			}
		}

		public Tire TireLeft { get; private set; }
		public Tire TireRight { get; private set; }

		public Axis(Body rb, float wheelBase, float distanceToCG, Vector2 left,Vector2 right,float Gravity)
		{			
			DistanceToCG = distanceToCG;

			WeightRatio = DistanceToCG / wheelBase;
			// Weight distribution on each axle and tire
			TireLeft = new Tire(left);
			TireRight = new Tire(right);

			float weight = rb.Mass * (WeightRatio * Gravity);
			TireLeft.RestingWeight = weight;
			TireRight.RestingWeight = weight;
		}

	}
}
