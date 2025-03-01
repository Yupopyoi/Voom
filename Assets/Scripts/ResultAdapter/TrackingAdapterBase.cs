using UnityEngine;

namespace Mediapipe.Allocator
{
    public abstract class TrackingAdapterBase
    {
        protected GameObject _partObject;
        protected Vector3 _initTransform;
        protected LandmarksPacket _landmarksPacket;
        protected bool[] _unfixAxis = new bool[3];

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

        protected void LandmarkLog(int index)
        {
            if(index < _landmarksPacket.Capacity)
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

        protected void ApplyRotation(float x, float y, float z)
        {
            if (!_unfixAxis[0] || x == float.NaN) x = _initTransform.x;
            if (!_unfixAxis[1] || y == float.NaN) y = _initTransform.y;
            if (!_unfixAxis[2] || z == float.NaN) z = _initTransform.z;

            _partObject.transform.localEulerAngles = new Vector3(x, y, z);
        }
    }
}// namespace Mediapipe.Allocator
