// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace Mediapipe.Allocator
{
    public abstract class TrackingAdapterBase
    {
        protected GameObject _partObject;
        protected Vector3 _initTransform;
        protected LandmarksPacket _landmarksPacket;
        protected bool[] _unfixAxis = new bool[3];

        public static int CacheSize { get; private set; } = 30;
        private readonly Queue<Vector3> _rotationCache;

        public Vector3 LatestRotation => AverageRotation();

        protected TrackingAdapterBase(GameObject partObject, LandmarksPacket landmarksPacket, bool unfixX = true, bool unfixY = true, bool unfixZ = false)
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
        }

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

        public void ChangeCacheSize(int size)
        {
            if (size < 0) return;
            CacheSize = size;
        }

        protected void LandmarkLog(int index)
        {
            if (index < _landmarksPacket.Capacity)
            {
                Debug.Log(_landmarksPacket.GetLandmark(index).ToString());
            }
            else
            {
                Debug.Log($"The index exceeds the bounds of the List. | index : {index}");
            }
        }

        public abstract void ForwardApply(Rotation? parentRotation = null);

        public virtual void ReverseApply(Rotation childRotation) { }

        protected float CalculateRotationAngle /* [deg] */ (Vector3 diffVector3, int axis1 = 0, int axis2 = 1 /* x = 0, y = 1, z = 2 */)
        {
            if (axis1 < 0 || axis1 > 2) return float.NaN;
            if (axis2 < 0 || axis2 > 2) return float.NaN;
            if (axis1 == axis2) return float.NaN;

            float length1 = diffVector3.x;
            float length2 = diffVector3.y;

            if (axis1 == 1) length1 = diffVector3.y;
            else if (axis1 == 2) length1 = diffVector3.z;

            if (axis2 == 0) length2 = diffVector3.x;
            else if (axis1 == 2) length1 = diffVector3.z;

            float angleRad = Mathf.Atan2(length2, length1);

            return angleRad * Mathf.Rad2Deg;
        }

        protected void ApplyRotation(float x, float y, float z)
        {
            if (!_unfixAxis[0] || x == float.NaN) x = _initTransform.x;
            if (!_unfixAxis[1] || y == float.NaN) y = _initTransform.y;
            if (!_unfixAxis[2] || z == float.NaN) z = _initTransform.z;

            AddRotationCache(new Vector3(x, y, z));

            _partObject.transform.localEulerAngles = AverageRotation();
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
    }
}// namespace Mediapipe.Allocator
