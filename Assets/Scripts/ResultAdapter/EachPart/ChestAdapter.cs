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
        public float ShigureUI { get; set; } = - 5.0f;

        /// <summary>
        /// Prevents excessive rotation around the y-axis.
        /// If this value is set to less than 0.01, rotation around the y-axis will not occur.
        /// The recommended value is 0.8f.
        /// </summary>
        public float RotationalResistanceAroundYaxis { get; set; } = 0.8f;

        // Points
        private Vector3 _leftShoulder;
        private Vector3 _rightShoulder;
        private Vector3 _leftHip;
        private Vector3 _rightHip;

        public override void ForwardApply(PoseMatrix? parentMatrix = null, Quaternion? parentQuaternion = null)
        {
            if (!LandmarkVisibility(0) || !LandmarkVisibility(1))
            {
                ApplyRotation(Quaternion.Euler(InitialTransform()));
                return;
            }

            _leftShoulder = Landmark(0);
            _rightShoulder = Landmark(1);

            if (Is2D())
            {
                // In 2D mode (sitting mode), "chest" is responsible only for rotation around the y-axis.
                // This is defined by the direction in which "Shoulder Vector" is looking in global space.
                
                Vector3 shoulderVector = (_leftShoulder - _rightShoulder).normalized;

                Quaternion rotation = Quaternion.FromToRotation(Vector3.right, shoulderVector);
                
                Vector3 eulerAngles = rotation.eulerAngles;

                if(RotationalResistanceAroundYaxis < 0.01f)
                {
                    eulerAngles.y = 0.0f;
                }
                else
                {
                    eulerAngles.y = ToSmoothStair(ContinuousAngleValue(eulerAngles.y), 
                                                  90.0f / RotationalResistanceAroundYaxis, 0.02f) * RotationalResistanceAroundYaxis;
                }

                // Not applicable
                eulerAngles.x = 0;
                eulerAngles.z = 0;

                ApplyRotation(PreventUnwantedRotation(Quaternion.Euler(eulerAngles)));
                return;
            }

            // 3D Mode (All Body Tracking) ------------------------------

            _leftHip = Landmark(2);
            _rightHip = Landmark(3);

            // Center of torso
            Vector3 chestCenterPoint = (_leftShoulder + _rightShoulder) * 0.5f;
            Vector3 hipCenterPoint = (_leftHip + _rightHip) * 0.5f;

            Vector3 right = (_rightShoulder - _leftShoulder).normalized;
            Vector3 upHint = (hipCenterPoint - chestCenterPoint).normalized;

            Vector3 forward = Vector3.Cross(upHint, right).normalized;
            Vector3 up = Vector3.Cross(right,forward).normalized;
            forward = Vector3.Cross(up, right).normalized;

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
            stableEulerAngles.x -= ShigureUI;

            if (stableEulerAngles.x > 180.0f) stableEulerAngles.x -= 360.0f;

            stableEulerAngles.x *= -1.0f;

            if (stableEulerAngles.y > 180.0f) stableEulerAngles.y -= 360.0f;
            if (stableEulerAngles.z > 180.0f) stableEulerAngles.z -= 360.0f;

            if (isDebug) GameLogger.Log(stableEulerAngles);

            return Quaternion.Euler(stableEulerAngles);
        }
    }
}// namespace Mediapipe.Allocator
