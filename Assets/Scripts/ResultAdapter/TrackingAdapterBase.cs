// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.Generic;
using UnityEngine;
using VRMController;

namespace Mediapipe.Allocator
{
    public struct PoseMatrix
    {
        public Matrix4x4 Matrix;
        public bool isValid;

        public PoseMatrix(Matrix4x4 matrix, bool valid = true)
        {
            this.Matrix = matrix;
            this.isValid = valid;
        }

        public bool IsValid { get { return isValid; }
            set { isValid = value; } }

        public Vector3 Position => Matrix.GetColumn(3);
        public Vector3 Forward => Matrix.GetColumn(2);
        public Vector3 Up => Matrix.GetColumn(1);
        public Vector3 Right => Matrix.GetColumn(0);

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
        void ForwardApply(PoseMatrix? parentMatrix = null);
        void ReverseApply(INamedVector childMessage);
    }

    // This class provides the functions and declarations necessary for the operation of the various parts of the body.
    // ForwardApply is an abstract method and MUST be implemented in all subclasses.
    // For more details, see https://ai.google.dev/edge/mediapipe/solutions/vision/pose_landmarker
    public abstract class TrackingAdapterBase : IPoseAdapter
    {
        private GameObject _partObject;
        private Sleeve _sleeve;

        private Vector3 _initTransform;
        private LandmarksPacket _landmarksPacket;
        private bool[] _unfixAxis = new bool[3];

        protected PoseMatrix _poseMatrix;

        private const int CACHE_SIZE = 50;
        private int _validCacheSize = 15;
        private readonly Queue<Quaternion> _quaternionCache;

        public Quaternion LatestQuaternion => AverageQuaternion();

        public PoseMatrix PoseMatrix { get { return _poseMatrix; } set { _poseMatrix = value; } }

        public int ValidCacheSize
        { 
            get { return _validCacheSize; }
            set 
            { 
                if (value > CACHE_SIZE) _validCacheSize = CACHE_SIZE;
                else if (value < 1) _validCacheSize = 1;
                else _validCacheSize = value;
            }
        }

        protected TrackingAdapterBase(GameObject partObject, LandmarksPacket landmarksPacket, Sleeve sleeve,
                                      bool unfixX = true, bool unfixY = true, bool unfixZ = false)
        {
            _partObject = partObject;

            _initTransform.x = _partObject.transform.localEulerAngles.x;
            _initTransform.y = _partObject.transform.localEulerAngles.y;
            _initTransform.z = _partObject.transform.localEulerAngles.z;

            _landmarksPacket = landmarksPacket;

            _unfixAxis[0] = unfixX;
            _unfixAxis[1] = unfixY;
            _unfixAxis[2] = unfixZ;

            _quaternionCache = new(capacity: CACHE_SIZE);

            _sleeve = sleeve;
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

        /// <summary>
        /// Apply angles from the center of the body outward as they are derived.
        /// For example, when considering arm movement,
        /// the body (torso) is the “parent” and we calculate the amount of arm rotation as its “child”.
        /// This is the (abstract) method for such adaptation, and this must be implemented in all parts of the body.
        /// </summary>
        public abstract void ForwardApply(PoseMatrix? parentMatrix = null);

        /// <summary>
        /// This is reversed, applying changes from the tip of the body toward the center.
        /// For example, in arm rotation, if you don't know how to rotate a hand tip, we can't implement it completely.
        /// In short, after adapting changes from the center of the body toward the tip,
        /// we now also apply changes from the tip toward the center.
        /// </summary>
        public virtual void ReverseApply(INamedVector childMessage) { }

        protected virtual Quaternion PreventUnwantedRotation(Quaternion smoothedRotationLHS) { return smoothedRotationLHS; }

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

        #endregion

        #region Functions for calculating the amount of rotation

        protected void ApplyRotation(Quaternion q, bool isDebug = false)
        {
            AddQuaternionCache(q);
            Quaternion averageQuaternion = AverageQuaternion(isDebug);

            if (_partObject != null)
            {
                _partObject.transform.localRotation = averageQuaternion;
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

            Vector4 sum = new(0, 0, 0, 0);

            int n = 1;
            foreach (var value in _quaternionCache)
            {
                if (n++ < CACHE_SIZE - _validCacheSize) continue;

                sum.x += value.x;
                sum.y += value.y;
                sum.z += value.z;
                sum.w += value.w;
            }

            int c = _validCacheSize;

            Quaternion averageQuaternion = new(sum.x / c, sum.y / c, sum.z / c, sum.w / c);

            if (averageQuaternion.w < 0f)
            {
                averageQuaternion = averageQuaternion.Negate();
            }

            if (isDebug)
            {
                GameLogger.Log(averageQuaternion, 2);
                Debug.Log(averageQuaternion.eulerAngles.ToString());
            }

            return averageQuaternion;
        }

        #endregion
    }

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
}// namespace Mediapipe.Allocator
