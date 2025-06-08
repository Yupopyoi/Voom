// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;

namespace Mediapipe.Allocator
{
    public abstract class EmotionAdapterBase
    {
        protected GameObject _faceObject;
        protected SkinnedMeshRenderer _skinnedMeshRenderer;
        protected LandmarksPacket _landmarksPacket;
        protected bool[] _unfixAxis = new bool[3];

        protected const float MeshInputMax = 100.0f;
        protected const float MeshInputMin =   0.0f;

        protected EmotionAdapterBase(GameObject faceObject, LandmarksPacket landmarksPacket)
        {
            _faceObject = faceObject;
            _landmarksPacket = landmarksPacket;

            if (IsEmptyGameObject(_faceObject)) return;

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

        protected void LandmarkLog(int index) /* For Debug */
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

        protected float BindControlValue(float source /* [0 - 1] */, float scale = 1.0f, float maxValue = 100.0f)
        {
            source *= scale;
            source = Mathf.Clamp01(source);

            return source * maxValue;
        }

        protected float Sigmoid(float x, float a, float b, float k = 0.1f) /* x = [ a , b ] */
        {
            float c = (b + a) * 0.5f;
            if(k == default)
            {
                k = 8.0f / (b - a);
            }

            float exp = -k * (x - c);
            return 100.0f / (1.0f + Mathf.Exp(exp));
        }

        protected float Sigmoid(float x, float k = 0.1f) /* x = [ 0 , 100 ] */
        {
            return Sigmoid(x, 0.0f, 100.0f, k);
        }

        protected bool IsEmptyGameObject(GameObject obj)
        {
            if(obj == null) return true;

            var components = obj.GetComponents<Component>();
            if (components.Length > 1) return false;

            if (obj.transform.childCount > 0) return false;

            return true;
        }

        public abstract void ForwardApply();
    }
} // namespace Mediapipe.Allocator
