using UnityEngine;

namespace Mediapipe.Allocator
{
    public class HipAdapter : TrackingAdapterBase
    {
        public HipAdapter(GameObject partObject, LandmarksPacket landmarksPacket, bool unfixX = false, bool unfixY = false, bool unfixZ = true)
            : base(partObject, landmarksPacket, unfixX, unfixY, unfixZ) { }

        /*  [Landmark Index]
         * 
         *    Call Index    Mediapipe Index         Part
         *        0               23             left  hip
         *        1               24             right hip
         */

        public override void ForwardApply(Rotation? parentRotation = null)
        {
            Vector3 leftHip = Landmark(0);
            Vector3 rightHip = Landmark(1);

            Vector3 hipVec = leftHip - rightHip;

            Vector3 calculatedEulerAngles = CalculateSignedEulerAngles(hipVec);

            Vector3 hipRotationValue = new(calculatedEulerAngles.x /* No effect now, we have to add landmarks of hips! */,
                                             Mathf.Clamp(calculatedEulerAngles.y, -100f, 100f),
                                             calculatedEulerAngles.z);

            ApplyRotation(hipRotationValue);
        }
    }
}// namespace Mediapipe.Allocator
