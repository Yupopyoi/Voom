using UnityEngine;

namespace Mediapipe.Allocator
{
    public class ChestAdapter : TrackingAdapterBase
    {
        public ChestAdapter(GameObject partObject, LandmarksPacket landmarksPacket)
            : base(partObject, landmarksPacket) { }

        /*  [Landmark Index]
         * 
         *    Call Index    Mediapipe Index         Part
         *        0               11           left  shoulder
         *        1               12           right shoulder
         *        2               23             left  hip
         *        3               24             right hip
         */

        /// <summary>
        /// Correcting stooped posture.
        /// The smaller the number (less than 0), the more it is corrected.
        /// Conversely, the larger the value, the more the model becomes stooped.
        /// </summary>
        private float _shigureUI = -5.0f;

        // Points
        private Vector3 _leftShoulder;
        private Vector3 _rightShoulder;
        private Vector3 _leftHip;
        private Vector3 _rightHip;
        private Vector3 _nose;

        public override void ForwardApply(PoseMatrix? parentMatrix = null, Quaternion? parentQuaternion = null)
        {
            _leftShoulder = Landmark(0);
            _rightShoulder = Landmark(1);
            _leftHip = Landmark(2);
            _rightHip = Landmark(3);
            _nose = Landmark(4);

            // Center of torso
            Vector3 chestCenterPoint = (_leftShoulder + _rightShoulder) * 0.5f;
            Vector3 hipCenterPoint = (_leftHip + _rightHip) * 0.5f;

            Vector3 up = (hipCenterPoint - chestCenterPoint).normalized;

            if(Dimension == OperationDimension.TwoDimension)
            {
                up = Vector3.up;
            }

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

                ApplyRotation(PreventUnwantedRotation(stableRotationLHS));
            }
        }

        protected override Quaternion PreventUnwantedRotation(Quaternion smoothedRotationLHS, bool isDebug = false)
        {
            var stableEulerAngles = smoothedRotationLHS.eulerAngles;

            // Preventing excessive forward leaning
            stableEulerAngles.x -= _shigureUI;

            if (stableEulerAngles.x > 180.0f) stableEulerAngles.x -= 360.0f;

            stableEulerAngles.x *= -1.0f;

            if (stableEulerAngles.y > 180.0f) stableEulerAngles.y -= 360.0f;
            if (stableEulerAngles.z > 180.0f) stableEulerAngles.z -= 360.0f;

            if (isDebug) GameLogger.Log(stableEulerAngles);

            return Quaternion.Euler(stableEulerAngles);
        }
    }
}// namespace Mediapipe.Allocator
