// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;

namespace Mediapipe.Allocator
{
    public class HeadAdapter : TrackingAdapterBase
    {
        public HeadAdapter(GameObject partObject, LandmarksPacket landmarksPacket)
                                                            : base(partObject, landmarksPacket){}

        /*  [Landmark Index]
         * 
         *   | Call Index |  Mediapipe Index |        Part       |
         *   |:----------:|:----------------:|:-----------------:|
         *   |     0      |         7        |     left  ear     |
         *   |     1      |         8        |     right ear     |
         *   |     2      |        11        |   left  shoulder  |
         *   |     3      |        12        |   right shoulder  |
         *   |     4      |         0        |        nose       |
         * 
         */

        #region Public properties that may vary depending on the person

        public bool CanRotateAroundXaxis { get; set; } = true;

        /// <summary>
        /// Correction amount for rotation around the X-axis [deg]
        /// </summary>
        public float NodOffset { get; set; } = 30.0f;

        /// <summary>
        /// Increase the angle of lookup by this variable multiple.
        /// </summary>
        public float LookUpGain { get; set; } = 1.5f;

        #endregion

        #region Variables for control

        private Quaternion _chestQuaternion;

        // Neck rotation, This is transmitted to the neck.
        private Vector3 _neckRotation = new();
        public Vector3 NeckRotation => _neckRotation;

        private float _divisionRatioToNeck = 0.5f;
        public float DivisionRatioToNeck 
        { 
            get {  return _divisionRatioToNeck; }
            set { _divisionRatioToNeck = Mathf.Clamp01(value); } 
        }

        #endregion

        public override void ForwardApply(PoseMatrix? parentMatrix = null, Quaternion? parentQuaternion = null)
        {
            _chestQuaternion = parentQuaternion.Value;

            Vector3 earVector = (Landmark(1) - Landmark(0)).normalized;
            Vector3 shoulderMidPoint = (Landmark(2) + Landmark(3)) * 0.5f;
            Vector3 noseShoulderVector = shoulderMidPoint - Landmark(4);

            if(!LandmarkVisibility(4 /* Nose */) || !LandmarkVisibility(2) || !LandmarkVisibility(3))
            {
                ApplyRotation(Quaternion.Euler(InitialTransform()));
                return;
            }

            float LocalRotationAngleX()
            {
                if (!CanRotateAroundXaxis) return InitialTransform().x;

                float x = EulerAngleX(noseShoulderVector, Vector2.right, true, -90.0f + NodOffset);

                if(x < 0.0f)
                {
                    x *= Mathf.Abs(LookUpGain);
                }
                return x;
            }

            Vector3 localRotationAngles = Vector3.zero;
            localRotationAngles.x = LocalRotationAngleX();
            localRotationAngles.y = EulerAngleY(earVector, Vector2.up, false, 90.0f);
            localRotationAngles.z = EulerAngleZ(earVector, Vector2.up, true, -90.0f);

            Quaternion absoluteRotation = Quaternion.Euler(localRotationAngles);

            // Eliminate body rotation
            Quaternion relativeRotation = Quaternion.Inverse(_chestQuaternion) * absoluteRotation;

            // XXX : It works when multiplied by Inverse(_chestQuaternion) twice, not sure why.
            Quaternion beforeDivisionRotation 
                = PreventUnwantedRotation(Quaternion.Inverse(_chestQuaternion) * relativeRotation);

            // If we try to achieve facial movements using only the movement of the head (object),
            // the neck area will look unnatural.
            // Therefore, assign the movement to the neck (object) based on the DivisionRatioToNeck ratio.
            Vector3 beforeDivisionEulerAngles = beforeDivisionRotation.eulerAngles;

            Quaternion afterDivisionRotation
                = Quaternion.Euler(ContinuousAngleValue(beforeDivisionEulerAngles).Mul(1.0f - DivisionRatioToNeck));

            _neckRotation = ContinuousAngleValue(beforeDivisionEulerAngles).Mul(DivisionRatioToNeck);

            ApplyRotation(afterDivisionRotation);
        }

        protected override Quaternion PreventUnwantedRotation(Quaternion smoothedRotationLHS, bool isDebug = false)
        {
            Vector3 rawEulerAngles = smoothedRotationLHS.eulerAngles;

            Vector3 stableEulerAngles = ContinuousAngleValue(rawEulerAngles);

            // Prohibit movements that exceed the range of motion of the human neck.
            stableEulerAngles.x = Mathf.Clamp(stableEulerAngles.x, -40.0f, 90.0f);
            stableEulerAngles.y = Mathf.Clamp(stableEulerAngles.y, -90.0f, 90.0f);
            stableEulerAngles.z = Mathf.Clamp(stableEulerAngles.z, -60.0f, 60.0f);

            // Eliminate unnaturalness that occurs when the absolute value of chestQuaternion.eulerAngles.y exceeds 60,
            // i.e., when the human torso is facing sideways.
            // The unnaturalness here refers to the phenomenon of unintentionally moving
            // between the two directions of facing forward or backward
            // without being able to determine which direction the face is facing.
            // The following program forces the face to look forward or sideways when the body is turned sideways.
            if ((_chestQuaternion.eulerAngles.y < 180.0f) && 
                (_chestQuaternion.eulerAngles.y >  60.0f))
            {
                stableEulerAngles.y = Mathf.Clamp(stableEulerAngles.y, -90.0f, 0.0f);
                stableEulerAngles.z = Mathf.Clamp(stableEulerAngles.z, 0.0f, 60.0f);
            }
            else if ((_chestQuaternion.eulerAngles.y > 180.0f) && 
                     (_chestQuaternion.eulerAngles.y < 300.0f)) /* i.e. _chestQuaternion.eulerAngles.y < - 60.0f */
            {
                stableEulerAngles.y = Mathf.Clamp(stableEulerAngles.y, 0.0f, 90.0f);
                stableEulerAngles.z = Mathf.Clamp(stableEulerAngles.z, -60.0f, 0.0f);
            }

            if (isDebug) GameLogger.Log(stableEulerAngles);

            return Quaternion.Euler(stableEulerAngles);
        }
    }
}// namespace Mediapipe.Allocator
