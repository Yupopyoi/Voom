// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;
using VRMController;

namespace Mediapipe.Allocator
{
    public class LeftLowerArmAdapter : TrackingAdapterBase
    {
        public LeftLowerArmAdapter(GameObject partObject, LandmarksPacket landmarksPacket, Sleeve sleeve, bool unfixX = false, bool unfixY = false, bool unfixZ = true)
                : base(partObject, landmarksPacket, sleeve, unfixX, unfixY, unfixZ) { }

        /*  [Landmark Index]
         * 
         *   | Call Index |  Mediapipe Index  |      Part      |
         *   |:----------:|:-----------------:|:--------------:|
         *   |     0      |         11        |  left shoulder |
         *   |     1      |         13        |   left elbow   |
         *   |     2      |         12        | right shoulder |
         *   |     3      |         15        |   left wrist   |
         *   |     4      |         23        |    left hip    |
         *   |     5      |         24        |   right hip    |
         */

        public override void ForwardApply(PoseMatrix? parentMatrix = null)
        {
            if (!LandmarkVisibility(3) /* The wrist is outside the screen */)
            {
                // As a result of extending the arm, the wrist is considered to be outside the screen,
                // so the model's arm is fixed straight.
                _poseMatrix = NeutralArmMatrix();

                ApplyRotation(_poseMatrix.RotationLHS);
                return;
            }

            if (IsInTorsoArea(Landmark(3)) /* The wrist is in front of the torso */)
            {
                _poseMatrix = BentArmMatrix();

                ApplyRotation(_poseMatrix.RotationLHS);
                return;
            }

            Vector3 elbow = Landmark(1);
            Vector3 wrist = Landmark(3);

            Vector3 shoulderCenterPoint = (Landmark(0) + Landmark(2)) * 0.5f;

            // Vector (X Axis) : Wrist -> Elbow
            Vector3 right = (elbow - wrist).normalized;

            // Vector (Provisional Y Axis)FShoulder Center -> ElbowiUpper Armj
            Vector3 upperArm = (elbow - shoulderCenterPoint).normalized;

            // Vector (Provisional Z Axis)
            Vector3 forward = Vector3.Cross(right, upperArm).normalized;

            // Vector (Y Axis)
            Vector3 up = Vector3.Cross(forward, right).normalized;

            // Vector (Z Axis)
            forward = Vector3.Cross(right, up).normalized;

            // Although normalization is lost,
            // multiplying y by 1.2 makes the hand rise steadily to the top.
            right.y *= 1.2f;

            _poseMatrix = PoseMatrix.SetBasisAndPosition(right, up, forward, elbow);

            var lowerArmPlaneVector = (Vector2)right.normalized;
            var upperArmPlaneVector = (Vector2)upperArm.normalized;

            if (Mathf.Abs(Vector2.Dot(lowerArmPlaneVector, upperArmPlaneVector)) > 0.9f)
            {
                _poseMatrix = NeutralArmMatrix();

                ApplyRotation(_poseMatrix.RotationLHS);
                return;
            }

            // Make it less sensitive to small movements.
            // This prevents meaningless vibrations from occurring in the model when you are stationary.
            Quaternion stableRotationLHS = ToSmoothStair(_poseMatrix.RotationLHS);

            ApplyRotation(PreventUnwantedRotation(stableRotationLHS));
        }

        protected override Quaternion PreventUnwantedRotation(Quaternion smoothedRotationLHS)
        {
            var stableEulerAngles = smoothedRotationLHS.eulerAngles;

            if (stableEulerAngles.x > 270)
            {
                stableEulerAngles.x = 0.0f;
            }
            if (stableEulerAngles.y > 180)
            {
                stableEulerAngles.y = 0.0f;
            }
            if (stableEulerAngles.z > 180)
            {
                stableEulerAngles.z -= 360.0f;
            }

            return Quaternion.Euler(stableEulerAngles);
        }

        private Vector4 TorsoArea() 
        {
            Vector3 shoulderCenterPoint = (Landmark(0) + Landmark(2)) * 0.5f;
            Vector3 hipCenterPoint = (Landmark(4) + Landmark(5)) * 0.5f;

            Vector3 shoulderVector = (Landmark(0) - Landmark(2)) * 0.7f;
            Vector3 hipVector = (Landmark(4) - Landmark(5)) * 0.7f;

            float x1 = Mathf.Min(hipCenterPoint.x - hipVector.x, shoulderCenterPoint.x - shoulderVector.x);
            float y1 = Mathf.Max(shoulderCenterPoint.y - shoulderVector.y, shoulderCenterPoint.y + shoulderVector.y);
            float x2 = Mathf.Max(hipCenterPoint.x + hipVector.x, shoulderCenterPoint.x + shoulderVector.x);
            float y2 = Mathf.Min(hipCenterPoint.y - hipVector.y, hipCenterPoint.y + hipVector.y);

            return new Vector4(x1, y1, x2, y1 + (y2 - y1) * 0.5f);
        }

        private bool IsInTorsoArea(Vector2 point)
        {
            var torsoArea = TorsoArea();

            if (point.x < torsoArea.x || point.x > torsoArea.z) return false;
            if (point.y < torsoArea.y || point.y > torsoArea.w) return false;

            return true;
        }

        private static PoseMatrix NeutralArmMatrix()
        {
            return PoseMatrix.SetBasisAndPosition(new Vector3(-1.0f, 0.0f, 0.0f),
                                                  new Vector3(0.0f, +1.0f, 0.0f),
                                                  new Vector3(0.0f, 0.0f, -1.0f),
                                                  new Vector3(0.0f, 0.0f, 0.0f));
        }

        private static PoseMatrix BentArmMatrix(float bendAmount = -1.0f /* < 0.0f */)
        {
            return PoseMatrix.SetBasisAndPosition(new Vector3(-1.0f, 0.0f, 0.0f),
                                                  new Vector3(bendAmount, 0.0f, 0.0f),
                                                  new Vector3(0.0f, -1.0f, 0.0f),
                                                  new Vector3(0.0f, 0.0f, 0.0f));
        }
    }
}// namespace Mediapipe.Allocator
