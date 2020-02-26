using UnityEngine;
using System.Collections;

namespace HandshakeVR
{
    /// <summary>
    /// Anything involving math that isn't covered by Unity's Mathf class gets put here.
    /// Contains neat features such as linear interpolation, cartesian->spherical projection,
    /// Vector averaging, and more.
    /// </summary>
	public static class MathSupplement
	{
		#region Linear Interpolation Functions
		/// <summary>
		/// Lerp with a sine tvalue
		/// </summary>
		public static float Sinerp(float from, float to, float t)
		{
			return Mathf.Lerp(from, to, Mathf.Sin(t * Mathf.PI * 0.5f));
		}

		/// <summary>
		/// Lerp with a cosine tvalue
		/// </summary>
		public static float Coserp(float from, float to, float t)
		{
			return Mathf.Lerp(from, to, (1f - Mathf.Cos(t * Mathf.PI * 0.5f)));
		}

		/// <summary>
		/// lerp with an exponential tvalue
		/// </summary>
		public static float Exerp(float from, float to, float t)
		{
			return Mathf.Lerp(from, to, t * t);
		}
		
		public static float InverseLerp(float low, float high, float value)
		{
			return (value - low) / (high - low);
		}

        public static double InverseLerp(long low, long high, float value)
        {
            return (value - low) / (high - low);
        }

        public static double InverseLerp(double low, double high, double value)
		{
			return (value - low) / (high - low);
		}
		
		public static float Lerp(float A, float B, float t)
		{
			return A + (B - A) * t;
		}

		public static double Lerp(double A, double B, double t)
		{
			return A + (B - A) * t;
		}
		#endregion

		// The vector3 interpolators below are basically the same as using some of NGUI's tweeners with preset animation
		// curve values. Still useful though.
		#region Vector3 Interpolation Functions
		public static Vector3 Sinerp(Vector3 to, Vector3 from, float tValue)
		{
			return new Vector3(Sinerp(to.x, from.x, tValue),
				Sinerp(to.y, from.y, tValue),
				Sinerp(to.z, from.z, tValue));
		}

		public static Vector3 Coserp(Vector3 to, Vector3 from, float tValue)
		{
			return new Vector3(Coserp(to.x, from.x, tValue),
				Coserp(to.y, from.y, tValue),
				Coserp(to.z, from.z, tValue));
		}

		public static Vector3 Exerp(Vector3 to, Vector3 from, float tValue)
		{
			return new Vector3(Exerp(to.x, from.x, tValue),
				Exerp(to.y, from.y, tValue),
				Exerp(to.z, from.z, tValue));
		}

        public static float UnclampedLerp(float a, float b, float t)
        {
            return t * b + (1 - t) * a;
        }

        public static Vector3 UnclampedLerp(Vector3 to, Vector3 from, float tValue)
        {
            return new Vector3(
               UnclampedLerp(to.x, from.x, tValue),
               UnclampedLerp(to.y, from.y, tValue),
               UnclampedLerp(to.z, from.z, tValue));
        }
		#endregion

		#region Spherical Coordinates
		public static void SphericalToCartesian(float radius, float polar, float elevation, out Vector3 outCart)
		{
			float a = radius * Mathf.Cos(elevation);
			outCart.x = a * Mathf.Cos(polar);
			outCart.y = radius * Mathf.Sin(elevation);
			outCart.z = a * Mathf.Sin(polar);
		}

		public static void CartesianToSpherical(Vector3 cartCoords, out float outRadius, out float outPolar, out float outElevation)
		{
			if (cartCoords.x == 0)
				cartCoords.x = Mathf.Epsilon;
			outRadius = Mathf.Sqrt((cartCoords.x * cartCoords.x)
							+ (cartCoords.y * cartCoords.y)
							+ (cartCoords.z * cartCoords.z));
			outPolar = Mathf.Atan(cartCoords.z / cartCoords.x);
			if (cartCoords.x < 0)
				outPolar += Mathf.PI;
			outElevation = Mathf.Asin(cartCoords.y / outRadius);
		}
		#endregion

		#region Spatial Relationships
		/// <summary>
		/// Gets a normalized direction from one point to another.
		/// </summary>
		/// <param name="to">Destination</param>
		/// <param name="from">source</param>
		/// <returns>A normalized direction from the source to the destination</returns>
		public static Vector3 Direction(Vector3 to, Vector3 from)
		{
			return (to - from).normalized; // dono why but this is backwards?
		}

		public static Vector3 CenterOfPoints(Vector3[] points)
		{
			Vector3 average = new Vector3();

			foreach (Vector3 target in points) average += target;

			average /= points.Length;

			return average;
		}

		public static Vector3 CenterOfGameObjects(GameObject[] objects)
		{
			Vector3 average = new Vector3();

			foreach (GameObject target in objects) average += target.transform.position;

			average /= objects.Length;

			return average;
		}

		/// <summary>
		/// Find the distance from the center to the furthest point in the array.
		/// </summary>
		/// <param name="points">Input vector 3 array.</param>
		/// <returns>The distance from the center to the furthest point.</returns>
		public static float DistanceOfPoints(Vector3[] points)
		{
			Vector3 center = CenterOfPoints(points);

			float dist = 0;

			foreach (Vector3 point in points)
			{
				float newDist = Vector3.Distance(point, center);
				if (newDist > dist) dist = newDist;
			}

			return dist;
		}

        public static Vector3 GetClosestPointOnLineSegment(Vector3 A, Vector3 B, Vector3 P)
        {
            Vector3 AP = P - A;       //Vector from A to P   
            Vector3 AB = B - A;       //Vector from A to B  

            float magnitudeAB = AB.sqrMagnitude;    //Magnitude of AB vector (it's length squared)
                                // NOTE: sqrMag above might just be mag.
            float ABAPproduct = Vector2.Dot(AP, AB);    //The DOT product of a_to_p and a_to_b     
            float distance = ABAPproduct / magnitudeAB; //The normalized "distance" from a to your closest point  

            if (distance < 0)     //Check if P projection is over vectorAB     
            {
                return A;
            }
            else if (distance > 1)
            {
                return B;
            }
            else
            {
                return A + AB * distance;
            }
        }
		
		#endregion

        public static Vector2 GetNormal(Vector2 a, Vector2 b)
        {
            Vector3 v = b - a;

            return new Vector2(-v.y, v.x) / Mathf.Sqrt((v.x * v.x) + (v.y * v.y));
        }

        public static float Repeat(float t, float length)
        {
            return t - Mathf.Floor(t / length) * length;
        }
    }
}