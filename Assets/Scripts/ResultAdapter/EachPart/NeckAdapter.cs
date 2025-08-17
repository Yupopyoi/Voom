using UnityEngine;

namespace Mediapipe.Allocator
{
    public class NeckAdapter : TrackingAdapterBase
    {
        public NeckAdapter(GameObject partObject, LandmarksPacket landmarksPacket)
            : base(partObject, landmarksPacket) { }

        private Vector3 _rotation = new();
        public void SetRotation(Vector3 rotation)
        {
            _rotation = rotation + InitialTransform();
        }

        public override void ForwardApply(PoseMatrix? parentMatrix = null, Quaternion? parentQuaternion = null)
        {
            ApplyRotation(Quaternion.Euler(_rotation));
        }
    }
}// namespace Mediapipe.Allocator
