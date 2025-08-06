// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;
using VRMController;

namespace Mediapipe.Allocator
{
    public class LowerArmAdapter : ArmAdapterBase
    {
        public LowerArmAdapter(GameObject partObject, LandmarksPacket landmarksPacket, Sleeve sleeve, bool isLeft)
                : base(partObject, landmarksPacket, sleeve, isLeft) { }

        /*  [Landmark Index]
         * 
         *   | Call Index |  Mediapipe Index [L/R] |        Part       |
         *   |:----------:|:----------------------:|:-----------------:|
         *   |     0      |        11 / 12         |      shoulder     |
         *   |     1      |        13 / 14         |       elbow       |
         *   |     2      |        12 / 11         | opposite shoulder |
         *   |     3      |        15 / 16         |       wrist       |
         */

        public override void ForwardApply(PoseMatrix? parentMatrix = null, Quaternion? parentQuaternion = null)
        {
            if (!LandmarkVisibility(3) /* The wrist is outside the screen */)
            {
                // As a result of extending the arm, the wrist is considered to be outside the screen,
                // so the model's arm is fixed straight.
                _poseMatrix = NeutralMatrix();

                ApplyRotation(_poseMatrix.RotationLHS);
                return;
            }

            Vector3 elbow = Landmark(1);
            Vector3 wrist = Landmark(3);

            Vector3 shoulderCenterPoint = (Landmark(0) + Landmark(2)) * 0.5f;

            // Vector (X Axis) : Wrist -> Elbow
            Vector3 right = (elbow - wrist).normalized;

            // Vector (Provisional Y Axis)：Shoulder Center -> Elbow（ = Upper Arm）
            Vector3 upperArm = (elbow - shoulderCenterPoint).normalized;

            // Vector (Provisional Z Axis)
            Vector3 forward = Vector3.Cross(right, upperArm).normalized;

            // Vector (Y Axis)
            Vector3 up = (Vector3.Cross(forward, right).normalized) * RightMinus;

            // Vector (Z Axis)
            forward = (Vector3.Cross(right, up).normalized) * RightMinus;

            // Although normalization is lost,
            // multiplying y by 1.2 makes the hand rise steadily to the top.
            right.y *= 1.2f;

            _poseMatrix = PoseMatrix.SetBasisAndPosition(right, up, forward, elbow);

            var lowerArmPlaneVector = (Vector2)right.normalized;
            var upperArmPlaneVector = (Vector2)upperArm.normalized;

            float dot = Mathf.Abs(Vector2.Dot(lowerArmPlaneVector, upperArmPlaneVector));

            // Mixing the straight state and the curved state detected by Mediapipe using Slerp.
            Quaternion finalRotation 
                //= Quaternion.Slerp(ToSmoothStair(_poseMatrix.RotationLHS), NeutralMatrix().RotationLHS, ToSmoothStair(dot, 1.0f, 4));
                = Quaternion.Slerp(_poseMatrix.RotationLHS, NeutralMatrix().RotationLHS, ToSmoothStair(dot, 1.0f, 4));

            ApplyRotation(PreventUnwantedRotation(finalRotation));
        }

        protected override Quaternion PreventUnwantedRotation(Quaternion smoothedRotationLHS, bool isDebug = false)
        {
            var stableEulerAngles = smoothedRotationLHS.eulerAngles;

            if (stableEulerAngles.x > 180.0f) stableEulerAngles.x -= 360.0f;
            if (stableEulerAngles.y > 180.0f) stableEulerAngles.y -= 360.0f;
            if (stableEulerAngles.z > 180.0f) stableEulerAngles.z -= 360.0f;

            if (isDebug) GameLogger.Log(stableEulerAngles);

            return Quaternion.Euler(stableEulerAngles);
        }
    }
}// namespace Mediapipe.Allocator
