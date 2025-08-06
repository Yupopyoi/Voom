// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRMController;

namespace Mediapipe.Allocator
{
    public struct PoseMatrix
    {
        public Matrix4x4 Matrix;

        public PoseMatrix(Matrix4x4 matrix)
        {
            this.Matrix = matrix;
        }

        public PoseMatrix Inverse => new(Matrix.inverse);

        public static PoseMatrix Identity => new(Matrix4x4.identity);

        public static PoseMatrix SetBasisAndPosition(Vector3 right, Vector3 up, Vector3 forward, Vector3 pos)
        {
            PoseMatrix m = Identity;
            m.SetColumn(0, new Vector4(right.x, right.y, right.z, 0));       // X+
            m.SetColumn(1, new Vector4(up.x, up.y, up.z, 0));                // Y+
            m.SetColumn(2, new Vector4(forward.x, forward.y, forward.z, 0)); // Z+
            m.SetColumn(3, new Vector4(pos.x, pos.y, pos.z, 1));

            return m;
        }

        public static PoseMatrix operator *(PoseMatrix a, PoseMatrix b)
        {
            return new PoseMatrix(a.Matrix * b.Matrix);
        }

        public void SetColumn(int columnIndex, Vector4 vec4)
        {
            Matrix.SetColumn(columnIndex, vec4);
        }

        public Quaternion RotationLHS
        {
            get
            {
                Vector3 forward = Matrix.GetColumn(2);  // Z Axis
                Vector3 up = Matrix.GetColumn(1);       // Y Axis

                if(forward.magnitude == 0.0f) return new Quaternion();
                if(up.magnitude == 0.0f) return new Quaternion();

                // Right Hand System → Left Hand System
                forward = -forward;

                return Quaternion.LookRotation(forward, up);
            }
        }
    }

    interface IPoseAdapter
    {
        void ForwardApply(PoseMatrix? parentMatrix = null, Quaternion? parentQuaternion = null);
        void ReverseApply(INamedVector childMessage);
    }

    // This class provides the functions and declarations necessary for the operation of the various parts of the body.
    // ForwardApply is an abstract method and MUST be implemented in all subclasses.
    // For more details, see https://ai.google.dev/edge/mediapipe/solutions/vision/pose_landmarker
    public abstract class TrackingAdapterBase : MonoBehaviour, IPoseAdapter
    {
        private readonly GameObject _partObject;
        private static Sleeve _sleeve;

        private Vector3 _initTransform;
        private LandmarksPacket _landmarksPacket;

        protected PoseMatrix _poseMatrix;

        protected const int CACHE_SIZE = 50;
        private static int _validCacheSize = 15;
        private readonly Queue<Quaternion> _quaternionCache;

        private Quaternion _latestQuaternion;

        public Quaternion LatestQuaternion => _latestQuaternion;

        public PoseMatrix PoseMatrix { get { return _poseMatrix; } set { _poseMatrix = value; } }

        public Vector3 PartObjectPosition => _partObject.transform.position;

        public static int ValidCacheSize
        { 
            get { return _validCacheSize; }
            set 
            { 
                if (value > CACHE_SIZE) _validCacheSize = CACHE_SIZE;
                else if (value < 1) _validCacheSize = 1;
                else _validCacheSize = value;
            }
        }

        protected TrackingAdapterBase(GameObject partObject, LandmarksPacket landmarksPacket, Sleeve sleeve)
        {
            _partObject = partObject;

            _initTransform.x = _partObject.transform.localEulerAngles.x;
            _initTransform.y = _partObject.transform.localEulerAngles.y;
            _initTransform.z = _partObject.transform.localEulerAngles.z;

            _landmarksPacket = landmarksPacket;

            _quaternionCache = new(capacity: CACHE_SIZE);

            _sleeve = sleeve;

            _latestQuaternion = _partObject.transform.rotation;
        }

        protected string PartName()
        {
            return _partObject.name;
        }

        protected bool IsPartNameContains(string partName)
        {
            return PartName().Contains(partName);
        }

        /// <summary>
        /// Returns the current coordinates of the landmark selected by the argument.
        /// If the landmark does not exist, a Vector3 with all 0 elements is returned.
        /// </summary>
        protected Vector3 Landmark(int index)
        {
            if (index < _landmarksPacket.Capacity)
            {
                float x = _landmarksPacket.GetLandmark(index).x;
                float y = _landmarksPacket.GetLandmark(index).y;
                float z = _landmarksPacket.GetLandmark(index).z;

                return new Vector3(x, y, z);
            }
            else
            {
                Debug.Log($"The index exceeds the bounds of the List. | index : {index}");

                return Vector3.zero;
            }
        }

        protected bool LandmarkVisibility(int index, float threshold = 0.9f)
        {
            if (index < _landmarksPacket.Capacity)
            {
                if (_landmarksPacket.GetLandmark(index).visibility == null) return false;

                if((float)_landmarksPacket.GetLandmark(index).visibility < threshold)
                {
                    return false;
                }

                return true;
            }
            else
            {
                Debug.Log($"The index exceeds the bounds of the List. | index : {index}");

                return false;
            }
        }

        protected float LandmarkVisibilityValue(int index)
        {
            if (index < _landmarksPacket.Capacity)
            {
                if (_landmarksPacket.GetLandmark(index).visibility == null) return 0.0f;
                return _landmarksPacket.GetLandmark(index).visibility.Value;
            }
            else
            {
                Debug.Log($"The index exceeds the bounds of the List. | index : {index}");

                return 0.0f;
            }
        }

        protected float LandmarkPresenceValue(int index)
        {
            if (index < _landmarksPacket.Capacity)
            {
                if (_landmarksPacket.GetLandmark(index).presence == null) return 0.0f;
                return _landmarksPacket.GetLandmark(index).presence.Value;
            }
            else
            {
                Debug.Log($"The index exceeds the bounds of the List. | index : {index}");

                return 0.0f;
            }
        }

        /// <summary>
        /// Apply angles from the center of the body outward as they are derived.
        /// For example, when considering arm movement,
        /// the body (torso) is the “parent” and we calculate the amount of arm rotation as its “child”.
        /// This is the (abstract) method for such adaptation, and this must be implemented in all parts of the body.
        /// </summary>
        public abstract void ForwardApply(PoseMatrix? parentMatrix = null, Quaternion? parentQuaternion = null);

        /// <summary>
        /// This is reversed, applying changes from the tip of the body toward the center.
        /// For example, in arm rotation, if you don't know how to rotate a hand tip, we can't implement it completely.
        /// In short, after adapting changes from the center of the body toward the tip,
        /// we now also apply changes from the tip toward the center.
        /// </summary>
        public virtual void ReverseApply(INamedVector childMessage) { }

        protected virtual Quaternion PreventUnwantedRotation(Quaternion smoothedRotationLHS, bool isDebug = false) 
        {
            if (isDebug) GameLogger.Log(smoothedRotationLHS);

            return smoothedRotationLHS; 
        }

        #region Static Utils

        /// <summary>
        /// Hyperbolic tangent
        /// </summary>
        /// <param name="x"></param>
        /// <returns>tanh(x)</returns>
        protected static float Tanh(float x)
        {
            float ep = Mathf.Exp(x);
            float em = Mathf.Exp(-x);
            return (ep - em) / (ep + em);
        }

        /// <summary>
        /// This function returns a smooth staircase function around zero.
        /// It looks like two sigmoid functions connected together.
        /// This makes the model more stable and enables smooth movement.
        /// If you want to check the shape of the graph, try entering this equation into GeoGebra.
        /// f(x)=(a/2)*(tanh(k(x+(a/2)))+tanh(k(x-(a/2))))
        /// </summary>
        /// <param name="value">x of f(x)</param>
        /// <param name="range">The value at which input and output are equal.(convergence value)</param>
        /// <param name="k">The larger this is, the closer the function is to a step function.</param>
        /// <param name="wide">Change the value at which the rate of change is greatest.</param>
        /// <returns></returns>
        protected static float ToSmoothStair(float value, float range = 90.0f, float k = 0.04f, float wide = 1.0f)
        {
            float mid = range * 0.5f;
            return mid * (Tanh(k * (value + mid * wide)) + Tanh(k * (value - mid * wide)));
        }

        protected static Vector3 ToSmoothStair(Vector3 value, float range = 90.0f, float k = 0.04f, float wide = 1.0f)
        {
            return new Vector3(ToSmoothStair(value.x, range, k, wide),
                               ToSmoothStair(value.y, range, k, wide),
                               ToSmoothStair(value.z, range, k, wide));
        }

        protected static Quaternion ToSmoothStair(Quaternion value, float range = 1.0f, float k = 4.0f, float wide = 1.0f)
        {
            return new Quaternion(ToSmoothStair(value.x, range, k, wide),
                                  ToSmoothStair(value.y, range, k, wide),
                                  ToSmoothStair(value.z, range, k, wide),
                                  ToSmoothStair(value.w, range, k, wide));
        }

        public static float ThresholdLerp(float x, float a, float max = 1.0f)
        {
            if (x < a) return 0.0f;
            return (x - a) / (max - a);
        }

        public static float ThresholdQuadratic(float x, float a, float max = 1.0f)
        {
            float t = ThresholdLerp(x, a, max);
            return t * t;
        }

        protected static PoseMatrix NeutralMatrix()
        {
            return PoseMatrix.SetBasisAndPosition(new Vector3(-1.0f, 0.0f, 0.0f),
                                                  new Vector3(0.0f, +1.0f, 0.0f),
                                                  new Vector3(0.0f, 0.0f, -1.0f),
                                                  new Vector3(0.0f, 0.0f, 0.0f));
        }

        #endregion

        #region Functions for calculating the amount of rotation

        // Call this function to apply the rotation angle.
        protected void ApplyRotation(Quaternion q, bool isDebug = false)
        {
            AddQuaternionCache(q);
            Quaternion averageQuaternion = AverageQuaternion(isDebug);

            if (_partObject != null)
            {
                _partObject.transform.localRotation = averageQuaternion;
                _latestQuaternion = averageQuaternion;
            }
        }

        private void AddQuaternionCache(Quaternion q)
        {
            if (_quaternionCache.Count >= CACHE_SIZE)
            {
                _quaternionCache.Dequeue();
            }

            _quaternionCache.Enqueue(q);
        }

        private Quaternion AverageQuaternion(bool isDebug = false)
        {
            if (_quaternionCache.Count == 0)
            {
                return Quaternion.Euler(_initTransform);
            }

            // Selecting items to use from the most recent cache
            List<Quaternion> candidates = _quaternionCache
                .Skip(Mathf.Max(0, _quaternionCache.Count - _validCacheSize))
                .ToList();

            Quaternion baseQ = LatestQuaternion;
            List<float> angles = candidates.Select(q => Quaternion.Angle(baseQ, q)).ToList();

            float mean = angles.Average();
            float variance = angles.Select(a => (a - mean) * (a - mean)).Average();
            float stddev = Mathf.Sqrt(variance);

            float threshold = mean + stddev; // Exclude values greater than 1σ.

            // Take the average excluding outliers.
            Vector4 sum = Vector4.zero;
            int count = 0;

            for (int i = 0; i < candidates.Count; i++)
            {
                if (Mathf.Abs(angles[i]) <= threshold) {
                    count++;
                }
            }

            // Prioritize newer data over older data.
            float finalCount = 0;
            for (int i = candidates.Count - count; i< candidates.Count; i++)
            {
                if (Mathf.Abs(angles[i]) <= threshold)
                {
                    Quaternion q = candidates[i];
                    sum.x += q.x;
                    sum.y += q.y;
                    sum.z += q.z;
                    sum.w += q.w;
                    finalCount++;
                }
            }

            if (finalCount == 0) return baseQ;

            Quaternion avgQ = new(sum.x / finalCount, sum.y / finalCount, sum.z / finalCount, sum.w / finalCount);

            if (Quaternion.Dot(avgQ, baseQ) < 0f)
            {
                avgQ = avgQ.Negate();
            }

            return avgQ;
        }

        #endregion
    }

    #region Extensions

    public static class QuaternionExtensions
    {
        public static Quaternion Negate(this Quaternion q)
        {
            return new Quaternion(-q.x, -q.y, -q.z, -q.w);
        }

        public static Quaternion Mul(this Quaternion q, float a)
        {
            return new Quaternion(q.x * a, q.y * a, q.z * a, q.w * a);
        }
    }

    public static class VectorExtensions
    {
        public static Vector2 Add(this Vector2 vector, float addValue)
        {
            return vector + new Vector2(addValue, addValue);
        }

        public static Vector3 Add(this Vector3 vector, float addValue)
        {
            return vector + new Vector3(addValue, addValue, addValue);
        }

        public static Vector2 Mul(this Vector2 vector, float mulValue)
        {
            return new Vector2(vector.x * mulValue, vector.y * mulValue);
        }

        public static Vector3 Mul(this Vector3 vector, float mulValue)
        {
            return new Vector3(vector.x * mulValue, vector.y * mulValue, vector.z * mulValue);
        }
    }

    #endregion

}// namespace Mediapipe.Allocator
