using UnityEngine;

namespace Mediapipe.Allocator
{
    public class ChestAdapter : TrackingAdapterBase
    {
        public ChestAdapter(GameObject partObject, LandmarksPacket landmarksPacket, bool unfixX = false, bool unfixY = false, bool unfixZ = true)
            : base(partObject, landmarksPacket, unfixX, unfixY, unfixZ) { }

        /*  [Landmark Index]
         * 
         *    Call Index    Mediapipe Index        Part
         *        0               11           left  shoulder
         *        1               12           right shoulder
         */

        public override void ForwardApply(Rotation? parentRotation = null)
        {
            Vector3 leftShoulder  = Landmark(0);
            Vector3 rightShoulder = Landmark(1);

            float z = CalculateRotationAngle(leftShoulder - rightShoulder);

            ApplyRotation(float.NaN, float.NaN, z);
        }
    }
}// namespace Mediapipe.Allocator
