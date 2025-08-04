// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;
using VRMController;

namespace Mediapipe.Allocator
{
    public class UpperLegAdapter : TrackingAdapterBase
    {
        protected readonly bool _isLeft;
        protected readonly float RightMinus;

        protected bool _isSittingDown = false;

        public UpperLegAdapter(GameObject partObject, LandmarksPacket landmarksPacket, Sleeve sleeve, bool isLeft)
            : base(partObject, landmarksPacket, sleeve)
        {
            _isLeft = isLeft;
            RightMinus = isLeft ? 1.0f : -1.0f;
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
         */

        private Quaternion _torsoQuaternion;

        // Points
        private Vector3 _hip;
        private Vector3 _knee;
        private Vector3 _oppositeHip;

        public override void ForwardApply(PoseMatrix? parentMatrix = null, Quaternion? parentQuaternion = null)
        {
            if (!LandmarkVisibility(2) && !LandmarkVisibility(3) /* Both hips are not visible */)
            {
                _poseMatrix = NeutralMatrix();

                ApplyRotation(_poseMatrix.RotationLHS);
                return;
            }

            if (parentQuaternion.HasValue) _torsoQuaternion = parentQuaternion.Value;

            _hip = Landmark(2);
            _knee = Landmark(4);
            _oppositeHip = Landmark(3);

            // Vector (Y Axis): hip ¨ knee (direction of the upper leg)
            Vector3 up = (_knee - _hip).normalized;

            // Vector (Z Axis): from hip to opposite hip (used as a reference for body orientation)
            Vector3 sideAxis = (_hip - _oppositeHip).normalized;

            // Vector (X Axis): forward direction = cross(up, sideAxis)
            Vector3 forward = Vector3.Cross(up, sideAxis).normalized * RightMinus;

            // Re-orthogonalized right vector = cross(forward, up)
            Vector3 right = Vector3.Cross(forward, up).normalized * RightMinus;

            // Construct pose matrix with the basis vectors and position at the hip
            _poseMatrix = PoseMatrix.SetBasisAndPosition(right, up, forward, (_hip + _oppositeHip) * 0.5f /* Center of hips */);

            // Get the world-space rotation from the pose matrix
            Quaternion worldRotation = ToSmoothStair(_poseMatrix.RotationLHS);

            // Cancel parent's (= hips') rotation to get local rotation
            Quaternion localRotation = parentQuaternion.HasValue
                ? Quaternion.Inverse(_torsoQuaternion) * worldRotation
                : worldRotation;

            ApplyRotation(PreventUnwantedRotation(localRotation));
        }

        protected override Quaternion PreventUnwantedRotation(Quaternion smoothedRotationLHS, bool isDebug = false)
        {
            var stableEulerAngles = smoothedRotationLHS.eulerAngles;

            if (stableEulerAngles.x > 180.0f) stableEulerAngles.x -= 360.0f;

            // The number 175 is not a mistake
            // Setting the threshold to 180 makes unintended rotation more likely to occur
            if (stableEulerAngles.y > 150.0f) stableEulerAngles.y -= 360.0f;

            if (stableEulerAngles.z > 270.0f) stableEulerAngles.z -= 360.0f;

            // Adjusting the width of the legs (mainly widening)
            if (stableEulerAngles.z > 25.0f && stableEulerAngles.z < 120.0f)
            {
                stableEulerAngles.z = (stableEulerAngles.z - 20.0f) * 1.75f + 20.0f;
            }

            if (isDebug) GameLogger.Log(stableEulerAngles);

            return Quaternion.Euler(stableEulerAngles);
        }

        protected static PoseMatrix NeutralMatrix()
        {
            return PoseMatrix.SetBasisAndPosition(new Vector3(-1.0f, 0.0f, 0.0f),
                                                  new Vector3(0.0f, +1.0f, 0.0f),
                                                  new Vector3(0.0f, 0.0f, -1.0f),
                                                  new Vector3(0.0f, 0.0f, 0.0f));
        }
    }
}// namespace Mediapipe.Allocator
