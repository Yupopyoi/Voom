using UnityEngine;

namespace Mediapipe.Allocator
{
    public class LeftUpperArmAdapter : TrackingAdapterBase
    {
        public LeftUpperArmAdapter(GameObject partObject, LandmarksPacket landmarksPacket, bool unfixX = false, bool unfixY = false, bool unfixZ = true)
            : base(partObject, landmarksPacket, unfixX, unfixY, unfixZ) { }

        /*  [Landmark Index]
         * 
         *    Call Index    Mediapipe Index         Part
         *        0               11           left  shoulder
         *        1               13             left elbow
         *        2               12           right shoulder
         */

        public override void ForwardApply(Rotation? parentRotation = null)
        {
            Vector3 leftShoulder = Landmark(0);
            Vector3 leftElbow = Landmark(1);

            Vector3 leftArmVec = leftElbow - leftShoulder;

            Vector3 calculatedEulerAngles = CalculateSignedEulerAngles(leftArmVec);

            Vector3 armRotationRawValue = new(float.NaN, /* ToDo : Implementation of x-axis rotation */
                                              Mathf.Clamp( calculatedEulerAngles.y, -100f, 100f),
                                              Mathf.Clamp( calculatedEulerAngles.z, -100f, 100f));

            Vector3 propagatedRotation /* From parents ( = Chest) */ = parentRotation.GetValueOrDefault().ToVector3;

            Vector3 armRotation = armRotationRawValue - propagatedRotation;

            ApplyRotation(ToSmoothStair(armRotation));
        }
    }
}// namespace Mediapipe.Allocator
