using UnityEngine;
using UnityEngine.UIElements.Experimental;
using VRMController;

namespace Mediapipe.Allocator
{
    public class ChestAdapter : TrackingAdapterBase
    {
        public ChestAdapter(GameObject partObject, LandmarksPacket landmarksPacket, Sleeve sleeve, bool unfixX = false, bool unfixY = false, bool unfixZ = true)
            : base(partObject, landmarksPacket, sleeve, unfixX, unfixY, unfixZ) { }

        /*  [Landmark Index]
         * 
         *    Call Index    Mediapipe Index         Part
         *        0               11           left  shoulder
         *        1               12           right shoulder
         *        2               23             left  hip
         *        3               24             right hip
         */

        // Points
        private Vector3 _leftShoulder;
        private Vector3 _rightShoulder;
        private Vector3 _leftHip;
        private Vector3 _rightHip;

        public override void ForwardApply(PoseMatrix? parentMatrix = null)
        {
            _leftShoulder = Landmark(0);
            _rightShoulder = Landmark(1);
            _leftHip = Landmark(2);
            _rightHip = Landmark(3);

            // Center of torso
            Vector3 chestCenterPoint = (_leftShoulder + _rightShoulder) * 0.5f;
            Vector3 hipCenterPoint = (_leftHip + _rightHip) * 0.5f;

            Vector3 up = (hipCenterPoint - chestCenterPoint).normalized;
            Vector3 right = (_rightShoulder - _leftShoulder).normalized;
            Vector3 forward = Vector3.Cross(up, right).normalized;
            right = Vector3.Cross(forward, up).normalized;

            _poseMatrix = PoseMatrix.SetBasisAndPosition(right, up, forward, chestCenterPoint);

            // Extracting relative rotation from the hip
            if (parentMatrix.HasValue)
            {
                // Hip^-1 * Chest = Local Rotation
                PoseMatrix localMatrix = parentMatrix.Value.Inverse *_poseMatrix;

                // Make it less sensitive to small movements.
                // This prevents meaningless vibrations from occurring in the model when you are stationary.
                Quaternion stableRotationLHS = ToSmoothStair(localMatrix.RotationLHS);

                ApplyRotation(stableRotationLHS);
            }
        }
    }
}// namespace Mediapipe.Allocator
