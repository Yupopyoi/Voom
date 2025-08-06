// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;
using VRMController;

namespace Mediapipe.Allocator
{
    public class LowerLegAdapter : TrackingAdapterBase
    {
        protected readonly bool _isLeft;

        private readonly float _straightThreshold = 0.5f;

        public LowerLegAdapter(GameObject partObject, LandmarksPacket landmarksPacket, Sleeve sleeve, bool isLeft)
            : base(partObject, landmarksPacket, sleeve)
        {
            _isLeft = isLeft;
        }

        /*  [Landmark Index]
         * 
         *   | Call Index |  Mediapipe Index [L/R] |        Part       |
         *   |:----------:|:----------------------:|:-----------------:|
         *   |     0      |        11 / 12         |      shoulder     |
         *   |     1      |        12 / 11         | opposite shoulder |
         *   |     2      |        23 / 24         |        hip        |
         *   |     3      |        24 / 23         |    opposite hip   |
         *   |     4      |        25 / 26         |        knee       |
         *   |     5      |        27 / 28         |       ankle       |
         *   |     6      |        28 / 27         |   opposite ankle  |
         */

        public override void ForwardApply(PoseMatrix? parentMatrix = null, Quaternion? parentQuaternion = null)
        {
            if (!LandmarkVisibility(2) && !LandmarkVisibility(3)) // Both hips are not visible
            {
                _poseMatrix = NeutralMatrix();

                ApplyRotation(_poseMatrix.RotationLHS);
                return;
            }

            Vector3 knee = Landmark(4);
            Vector3 ankle = Landmark(5);
            Vector3 hip = Landmark(2);

            Vector3 upperLeg = (knee - hip).normalized;
            Vector3 lowerLeg = (ankle - knee).normalized;

            float absDot = Mathf.Abs(Vector2.Dot(upperLeg, lowerLeg));

            /*
             *   |  absDot |  Rotation Euler X |
             *   |:-------:|:-----------------:|
             *   |   0.0   |        90         |
             *   |    :    |         :         |
             *   |    :    |       Lerp        |
             *   |    :    |         :         |
             *   |    t    |         0         | (t is 0.5 by default.)
             *   |    :    |         :         |
             *   |   1.0   |         0         |
             */

            float rotationEulerX = 0.0f;
            if(absDot < _straightThreshold)
            {
                rotationEulerX = 90.0f - (absDot / _straightThreshold) * 90.0f;
            }

            Vector3 rotationEulerAngles = new(rotationEulerX, 0.0f, 0.0f); 

            ApplyRotation(PreventUnwantedRotation(Quaternion.Euler(rotationEulerAngles)));
        }
    }
}// namespace Mediapipe.Allocator
