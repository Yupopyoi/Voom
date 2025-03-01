using UnityEngine;

namespace Mediapipe.Allocator
{
    public class ChestAdapter : TrackingAdapterBase
    {
        public ChestAdapter(GameObject partObject, LandmarksPacket landmarksPacket, bool unfixX = false, bool unfixY = false, bool unfixZ = true)
            : base(partObject, landmarksPacket, unfixX, unfixY, unfixZ) { }

        public override void ForwardApply(Rotation? parentRotation = null)
        {
            ApplyRotation(float.NaN, float.NaN, 10.0f);
        }
    }
}// namespace Mediapipe.Allocator
