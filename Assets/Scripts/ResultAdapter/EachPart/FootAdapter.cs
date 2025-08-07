// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;
using VRMController;

namespace Mediapipe.Allocator
{
    public class FootAdapter : TrackingAdapterBase
    {
        protected readonly bool _isLeft;

        public FootAdapter(GameObject partObject, LandmarksPacket landmarksPacket, Sleeve sleeve, bool isLeft)
            : base(partObject, landmarksPacket, sleeve)
        {
            _isLeft = isLeft;
        }

        /*  [Landmark Index]
         * 
         *   | Call Index |  Mediapipe Index [L/R] |        Part       |
         *   |:----------:|:----------------------:|:-----------------:|
         *   |     0      |        25 / 26         |        knee       |
         *   |     1      |        27 / 28         |       ankle       |
         *   |     2      |        29 / 30         |        heel       |
         *   |     3      |        31 / 32         |     foot index    |
         */

        public override void ForwardApply(PoseMatrix? parentMatrix = null, Quaternion? parentQuaternion = null)
        {
            if (!LandmarkVisibility(0) && !LandmarkVisibility(1))
            {
                _poseMatrix = NeutralMatrix();

                ApplyRotation(_poseMatrix.RotationLHS);
                return;
            }

            if (!LandmarkVisibility(2, 0.7f) && !LandmarkVisibility(3, 0.7f)) // The knee or/and the ancle are not visible
            {
                _poseMatrix = NeutralMatrix();

                ApplyRotation(_poseMatrix.RotationLHS);
                return;
            }

            Vector3 knee = Landmark(0);
            Vector3 ankle = Landmark(1);
            Vector3 heel = Landmark(2);
            Vector3 footIndex = Landmark(3);

            Vector3 lowerLeg = (ankle - knee).normalized;
            Vector3 footVector = (footIndex - heel).normalized;

            //float absDot = Mathf.Abs(Vector2.Dot(upperLeg, lowerLeg));

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
             *   
             */  
        }
    }
}// namespace Mediapipe.Allocator
