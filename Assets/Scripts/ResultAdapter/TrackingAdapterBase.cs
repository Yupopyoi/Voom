// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.Generic;
using UnityEngine;

namespace Mediapipe.Allocator
{
    interface IPoseAdapter
    {
        void ForwardApply(Rotation? parentRotation = null);
        void ReverseApply(Rotation childRotation);
        Rotation LatestRotation { get; }
        void ChangeCacheSize(int size);
    }

    // This class provides the functions and declarations necessary for the operation of the various parts of the body.
    // ForwardApply is an abstract method and MUST be implemented in all subclasses.
    // For more details, see https://ai.google.dev/edge/mediapipe/solutions/vision/pose_landmarker
    public abstract class TrackingAdapterBase : IPoseAdapter
    {
        protected GameObject _partObject;
        private Transform _partTransform; 

        protected Vector3 _initTransform;
        protected LandmarksPacket _landmarksPacket;
        protected bool[] _unfixAxis = new bool[3];

        public static int CacheSize { get; private set; } = 30; // Length of _rotationCache
        private readonly Queue<Vector3> _rotationCache;

        public Rotation LatestRotation => new(AverageRotation());
        public Rotation WorldRotation => new(_partTransform.rotation.eulerAngles);

        #region Logger (For Debug)

        // Returns the current "localEulerAngles" in string format for log output.
        // You may provide as an argument a string to be prefixed to the log.
        // This should basically be the name of the part, such asÅg[chest]ÅhorÅg[right leg]Åh
        protected string LatestRotationLogString(string prefix = "[TrackingAdapter]")
        {
            var rot = AverageRotation();
            return $"{prefix} x : {rot.x:F1}, y : {rot.y:F1}, z : {rot.z:F1}";
        }

        protected string LandmarkLogString(int index)
        {
            Vector3 landmark = Landmark(index);
            string name = "";
            return $"[{name}] x : {landmark.x:F1}, y : {landmark.y:F1}, z : {landmark.z:F1}";
        }

        #endregion

        protected TrackingAdapterBase(GameObject partObject, LandmarksPacket landmarksPacket, 
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

            _rotationCache = new(capacity: CacheSize);

            _partTransform = _partObject.transform;
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

        /// <summary>
        /// Change the length of the queue that keeps the results of previous calculations.
        /// The queue length is not changed if a negative value is given.
        /// The larger this value, the more stable the operation, but the greater the delay.
        /// Conversely, the smaller this value is, the more likely the model will move unintentionally, but with less delay.
        /// </summary>
        public void ChangeCacheSize(int size)
        {
            if (size < 0) return;
            CacheSize = size;
        }

        /// <summary>
        /// Apply angles from the center of the body outward as they are derived.
        /// For example, when considering arm movement,
        /// the body (torso) is the ÅgparentÅh and we calculate the amount of arm rotation as its ÅgchildÅh.
        /// This is the (abstract) method for such adaptation, and this must be implemented in all parts of the body.
        /// </summary>
        public abstract void ForwardApply(Rotation? parentRotation = null);

        /// <summary>
        /// This is reversed, applying changes from the tip of the body toward the center.
        /// For example, in arm rotation, if you don't know how to rotate a hand tip, we can't implement it completely.
        /// In short, after adapting changes from the center of the body toward the tip,
        /// we now also apply changes from the tip toward the center.
        /// </summary>
        public virtual void ReverseApply(Rotation childRotation) { }

        #region Functions for calculating the amount of rotation

        /// <summary>
        /// Calculates a rotation Quaternion from a reference direction to the target direction.
        /// </summary>
        private Quaternion CalculateRotation(Vector3 direction, Vector3 reference = default)
        {
            if (reference == default) reference = Vector3.right;
            if (direction == Vector3.zero) return Quaternion.identity;
            return Quaternion.FromToRotation(reference, direction.normalized);
        }

        /// <summary>
        /// Calculates the Euler angles (degrees) from the direction vector,
        /// assuming the rotation is from Vector3.right to the direction.
        /// </summary>
        private Vector3 CalculateEulerAngles(Vector3 direction, Vector3 reference = default)
        {
            return CalculateRotation(direction, reference).eulerAngles;
        }

        /// <summary>
        /// Returns signed (x: pitch, y: yaw, z: roll) angles from direction vector.
        /// </summary>
        protected Vector3 CalculateSignedEulerAngles(Vector3 direction, Vector3 reference = default)
        {
            Vector3 euler = CalculateEulerAngles(direction, reference);
            return new Vector3(
                Mathf.DeltaAngle(0f, euler.x),  // pitch
                Mathf.DeltaAngle(0f, euler.y),  // yaw
                Mathf.DeltaAngle(0f, euler.z)   // roll
            );
        }

        /// <summary>
        /// This function applies the calculated rotation values (x,y,z) to the model.
        /// This sets the initial value if an invalid value is specified
        /// or if the rotation around the respective axis is fixed by _unfixAxis.
        /// </summary>
        protected void ApplyRotation(float x, float y, float z, 
                                     bool canApplyX = true, bool canApplyY = true, bool canApplyZ = true)
        {
            if (!_unfixAxis[0] || x == float.NaN) x = _initTransform.x;
            if (!_unfixAxis[1] || y == float.NaN) y = _initTransform.y;
            if (!_unfixAxis[2] || z == float.NaN) z = _initTransform.z;


            // First, add the specified angle values to the end of the queue,
            // and then apply the average value of the queue to the transform of the 3D model.
            // By doing this, we can make the 3D model more stable than if we applied it directly (but there will be some delay).
            AddRotationCache(new Vector3(x, y, z));

            var localEulerAngles = _partObject.transform.localEulerAngles;
            var averageRotation = AverageRotation();

            if (canApplyX) localEulerAngles.x = averageRotation.x;
            if (canApplyY) localEulerAngles.y = averageRotation.y;
            if (canApplyZ) localEulerAngles.z = averageRotation.z;

            _partObject.transform.localEulerAngles = localEulerAngles;
        }

        protected void ApplyRotation(Vector3 rot,
                             bool canApplyX = true, bool canApplyY = true, bool canApplyZ = true)
        {
            ApplyRotation(rot.x, rot.y, rot.z, canApplyX, canApplyY, canApplyZ);
        }

        private void AddRotationCache(Vector3 latestRotation)
        {
            if(_rotationCache.Count >= CacheSize)
            {
                _rotationCache.Dequeue();
            }

            _rotationCache.Enqueue(latestRotation);
        }

        private Vector3 AverageRotation()
        {
            if (_rotationCache.Count == 0)
            {
                return new(_initTransform.x, _initTransform.y, _initTransform.z);
            }

            Vector3 sum = new(0, 0, 0);
            foreach (var value in _rotationCache)
            {
                sum += value;
            }

            return sum / _rotationCache.Count;
        }

        #endregion
    }
}// namespace Mediapipe.Allocator
