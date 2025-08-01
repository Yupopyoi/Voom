// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;
using VRMController;

namespace Mediapipe.Allocator
{
    public class LeftUpperArmAdapter : TrackingAdapterBase
    {
        public LeftUpperArmAdapter(GameObject partObject, LandmarksPacket landmarksPacket, Sleeve sleeve, bool unfixX = false, bool unfixY = false, bool unfixZ = true)
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
            Vector3 shoulder = Landmark(0); // Point: left shoulder
            Vector3 elbow = Landmark(1);    // Point: left elbow
            Vector3 hip = Landmark(4);      // Point: left hip

            // Vector (X Axis) : Elbow -> Shoulder
            Vector3 right = (shoulder - elbow).normalized;

            // Although normalization is lost,
            // multiplying y by 1.2 makes the hand rise steadily to the top.
            right.y *= 1.2f;

            // Vector (Z Axis) : Defined using upHint
            Vector3 upHint = (hip - shoulder).normalized;
            Vector3 forward = Vector3.Cross(right, upHint).normalized;

            // Vector (Y Axis)
            Vector3 up = Vector3.Cross(forward, right).normalized;

            _poseMatrix = PoseMatrix.SetBasisAndPosition(right, up, forward, shoulder);

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

            stableEulerAngles.z = Mathf.Clamp(stableEulerAngles.z, -90.0f, 90.0f);

            return Quaternion.Euler(stableEulerAngles);
        }

        private static PoseMatrix LiftingArmMatrix()
        {
            return PoseMatrix.SetBasisAndPosition(new Vector3(-1.0f, 0.0f, 0.0f),
                                                  new Vector3(0.0f, +1.0f, 0.0f),
                                                  new Vector3(0.0f, 0.0f, -1.0f),
                                                  new Vector3(0.0f, 0.0f, 0.0f));
        }

    }
}// namespace Mediapipe.Allocator
