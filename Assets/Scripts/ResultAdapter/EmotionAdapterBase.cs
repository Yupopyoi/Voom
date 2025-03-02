// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.Generic;
using UnityEngine;

namespace Mediapipe.Allocator
{
    public abstract class EmotionAdapterBase
    {
        protected GameObject _faceObject;
        protected SkinnedMeshRenderer _skinnedMeshRenderer;
        protected LandmarksPacket _landmarksPacket;
        protected bool[] _unfixAxis = new bool[3];

        public static int CacheSize { get; private set; } = 30;
        private readonly Queue<Vector3> _rotationCache;

        protected EmotionAdapterBase(GameObject faceObject, LandmarksPacket landmarksPacket)
        {
            _faceObject = faceObject;
            _landmarksPacket = landmarksPacket;
            _rotationCache = new(capacity: CacheSize);

            _skinnedMeshRenderer = _faceObject.GetComponent<SkinnedMeshRenderer>();
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

        protected float PlaneDistance(Vector3 diffVector)
        {
            return (diffVector.x * diffVector.x + diffVector.y * diffVector.y) * 1000.0f;
        }

        protected float BindControlValue(float source /* [0 - 1] */, float scale = 1.0f)
        {
            source *= scale;
            source = Mathf.Clamp01(source);

            return source * 100.0f;
        }

        public abstract void ForwardApply();
    }
} // namespace Mediapipe.Allocator
