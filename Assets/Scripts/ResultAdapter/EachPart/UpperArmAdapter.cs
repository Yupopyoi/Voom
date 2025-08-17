// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;

namespace Mediapipe.Allocator
{
    public class UpperArmAdapter : ArmAdapterBase
    {
        public UpperArmAdapter(GameObject partObject, LandmarksPacket landmarksPacket, bool isLeft)
            : base(partObject, landmarksPacket, isLeft){}

        public float NaturalRotation { get; set; } = 65.0f;

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
            if (!LandmarkVisibility(1))/* The elbow is not visible */
            {
                // Since elbow is not visible, fix arm in lowered position
                var naturalQuaternion = Quaternion.Euler(new Vector3(0.0f, 0.0f, NaturalRotation * RightMinus));

                ApplyRotation(naturalQuaternion);
                return;
            }

            Vector3 shoulder = Landmark(0); // Point: shoulder
            Vector3 elbow = Landmark(1);    // Point: elbow

            // Vector (X Axis) : Elbow -> Shoulder
            Vector3 right = (shoulder - elbow).normalized;

            // Vector (Z Axis) : Defined using upHint
            Vector3 upHint = parentMatrix.Value.Up;
            Vector3 forward = (Vector3.Cross(right, upHint).normalized) * RightMinus;

            // Vector (Y Axis)
            Vector3 up = (Vector3.Cross(forward, right).normalized) * RightMinus;

            _poseMatrix = PoseMatrix.SetBasisAndPosition(right, up, forward, shoulder);

            ApplyRotation(PreventUnwantedRotation(_poseMatrix.RotationLHS));
        }

        protected override Quaternion PreventUnwantedRotation(Quaternion smoothedRotationLHS, bool isDebug = false)
        {
            var stableEulerAngles = smoothedRotationLHS.eulerAngles;

            stableEulerAngles.z -= 10 * RightMinus;

            if (stableEulerAngles.x > 270)
            {
                stableEulerAngles.x = 0.0f;
            }

            stableEulerAngles.y = ContinuousAngleValue(stableEulerAngles.y, 180.0f);
            stableEulerAngles.z = ContinuousAngleValue(stableEulerAngles.z, 180.0f);

            stableEulerAngles.y = Mathf.Clamp(stableEulerAngles.y, -90.0f, 90.0f);
            stableEulerAngles.z = Mathf.Clamp(stableEulerAngles.z, -90.0f, 80.0f);

            if (isDebug) GameLogger.Log(stableEulerAngles);

            return Quaternion.Euler(stableEulerAngles);
        }
    }
}// namespace Mediapipe.Allocator
