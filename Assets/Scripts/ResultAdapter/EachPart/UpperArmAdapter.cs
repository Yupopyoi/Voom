// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;
using VRMController;

namespace Mediapipe.Allocator
{
    public class UpperArmAdapter : ArmAdapterBase
    {
        public UpperArmAdapter(GameObject partObject, LandmarksPacket landmarksPacket, Sleeve sleeve, bool isLeft)
            : base(partObject, landmarksPacket, sleeve, isLeft){}

        /*  [Landmark Index]
         * 
         *   | Call Index |  Mediapipe Index [L/R] |        Part       |
         *   |:----------:|:----------------------:|:-----------------:|
         *   |     0      |        11 / 12         |      shoulder     |
         *   |     1      |        13 / 14         |       elbow       |
         *   |     2      |        12 / 11         | opposite shoulder |
         *   |     3      |        15 / 16         |       wrist       |
         *   |     4      |        23 / 24         |        hip        |
         *   |     5      |        24 / 23         |    opposite hip   |
         */

        public override void ForwardApply(PoseMatrix? parentMatrix = null, Quaternion? parentQuaternion = null)
        {
            Vector3 shoulder = Landmark(0); // Point: shoulder
            Vector3 elbow = Landmark(1);    // Point: elbow
            Vector3 hip = Landmark(4);      // Point: hip

            // Vector (X Axis) : Elbow -> Shoulder
            Vector3 right = (shoulder - elbow).normalized;

            // Although normalization is lost,
            // multiplying y by 1.2 makes the hand rise steadily to the top.
            right.y *= 1.2f;

            // Vector (Z Axis) : Defined using upHint
            Vector3 upHint = (hip - shoulder).normalized;
            Vector3 forward = (Vector3.Cross(right, upHint).normalized) * RightMinus;

            // Vector (Y Axis)
            Vector3 up = (Vector3.Cross(forward, right).normalized) * RightMinus;

            _poseMatrix = PoseMatrix.SetBasisAndPosition(right, up, forward, shoulder);

            // Make it less sensitive to small movements.
            // This prevents meaningless vibrations from occurring in the model when you are stationary.
            Quaternion stableRotationLHS = ToSmoothStair(_poseMatrix.RotationLHS);

            ApplyRotation(PreventUnwantedRotation(stableRotationLHS));
        }
    }
}// namespace Mediapipe.Allocator
