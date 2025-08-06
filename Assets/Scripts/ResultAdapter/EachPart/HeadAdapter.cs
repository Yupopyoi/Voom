// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;
using VRMController;

namespace Mediapipe.Allocator
{
    public class HeadAdapter : TrackingAdapterBase
    {
        public HeadAdapter(GameObject partObject, LandmarksPacket landmarksPacket, Sleeve sleeve)
                                                            : base(partObject, landmarksPacket, sleeve) { }

        /*  [Landmark Index]
         * 
         *    Call Index    Mediapipe Index         Part
         *        0                7             left  ear
         *        1                8             right ear
　　　　 *        2               11           left  shoulder
         *        3               12           right shoulder
         */

        private Quaternion _chestQuaternion;

        // Points
        private Vector3 _leftEar;
        private Vector3 _rightEar;
        private Vector3 _leftShoulder;
        private Vector3 _rightShoulder;

        public override void ForwardApply(PoseMatrix? parentMatrix = null, Quaternion? parentQuaternion = null)
        {
            _chestQuaternion = parentQuaternion.Value;

            _leftEar = Landmark(0);
            _rightEar = Landmark(1);
            _leftShoulder = Landmark(2);
            _rightShoulder = Landmark(3);

            // Base direction vector (left to right ear)
            Vector3 right = (_rightEar - _leftEar).normalized;

            // Upward vector (from midpoint of shoulders to midpoint of ears)
            Vector3 shoulderMid = (_leftShoulder + _rightShoulder) * 0.5f;
            Vector3 earMid = (_leftEar + _rightEar) * 0.5f;
            Vector3 up = (earMid - shoulderMid).normalized;

            // Forward vector derived from cross product
            Vector3 forward = Vector3.Cross(up, right).normalized;

            // Recalculate up vector to ensure orthogonality
            up = Vector3.Cross(forward, right).normalized;

            // Construct and apply PoseMatrix
            _poseMatrix = PoseMatrix.SetBasisAndPosition(right, up, forward, earMid);

            Quaternion stable = ToSmoothStair(_poseMatrix.RotationLHS);
            ApplyRotation(PreventUnwantedRotation(stable));
        }

        protected override Quaternion PreventUnwantedRotation(Quaternion smoothedRotationLHS, bool isDebug = false)
        {
            var stableEulerAngles = smoothedRotationLHS.eulerAngles;
            var chestEulerAngles = _chestQuaternion.eulerAngles;

            #region Rotation around the midline (stableEulerAngles.y)

            // Cancel out the rotation of the torso
            float cancellationAmountOfChestRotation =
                chestEulerAngles.y > 180.0 ? chestEulerAngles.y - 360.0f : chestEulerAngles.y;

            stableEulerAngles.y -= cancellationAmountOfChestRotation * 2.0f;

            // Make movements continuous
            // A change from around 0 degrees (e.g., 10 degrees)
            // to around 360 degrees (e.g., 350 degrees) means almost a full rotation.
            // To prevent this, subtract 360 in advance from the value around 360 degrees.
            if (stableEulerAngles.y > 180.0)
            {
                stableEulerAngles.y -= 360.0f;
            }

            // Prohibit movements that are impossible for humans to do.
            stableEulerAngles.y = Mathf.Clamp(stableEulerAngles.y, -75.0f, 75.0f);

            #endregion

            #region Nod (stableEulerAngles.x)

            // For reasons unknown, rotation in the y direction causes unintended rotation in the x direction as well.
            // This is corrected here.
            float cancellationOfYRotation = Mathf.Abs(stableEulerAngles.y) * 0.2f;
            stableEulerAngles.x += cancellationOfYRotation;

            // Rotate the head slightly negative, because in the standard state it will face downward.
            stableEulerAngles.x -= 10.0f; // [deg]

            float cancellationOfXRotation = chestEulerAngles.x > 180.0 ? chestEulerAngles.x - 360.0f : chestEulerAngles.x;
            stableEulerAngles.x -= cancellationOfXRotation;

            // The output of MediaPipe itself is not sufficient to achieve sufficient movement,
            // therefore, movement is amplified.
            float nodSensitivity = stableEulerAngles.x > 0.0f ? 2.0f : 5.0f;

            // Prohibit movements that are impossible for humans to do.
            stableEulerAngles.x = Mathf.Clamp(stableEulerAngles.x * nodSensitivity, -60.0f, 25.0f);

            #endregion

            #region Tilt (stableEulerAngles.z)

            // Make movements continuous
            if (stableEulerAngles.z > 180.0)
            {
                stableEulerAngles.z -= 360.0f;
            }

            // Prohibit movements that are impossible for humans to do.
            stableEulerAngles.z = Mathf.Clamp(stableEulerAngles.z, -20.0f, 20.0f);

            #endregion

            if (isDebug) GameLogger.Log(stableEulerAngles);

            return Quaternion.Euler(stableEulerAngles);
        }
    }
}// namespace Mediapipe.Allocator
