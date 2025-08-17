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
         *    Call Index    Mediapipe Index         Part
         *        0                7             left  ear
         *        1                8             right ear
　　　　 *        2               11           left  shoulder
         *        3               12           right shoulder
　　　　 *        4                0                nose
         */

        private Quaternion _chestQuaternion;

        public bool CanRotateAroundXaxis { get; set; } = true;
        public float NodOffset { get; set; } = 20.0f;

        // Neck rotation, This is transmitted to the neck.
        private Vector3 _neckRotation = new();
        public Vector3 NeckRotation => _neckRotation;

        private float _divisionRatioToNeck = 0.5f;
        public float DivisionRatioToNeck 
        { 
            get {  return _divisionRatioToNeck; }
            set { _divisionRatioToNeck = Mathf.Clamp01(value); } 
        }

        public override void ForwardApply(PoseMatrix? parentMatrix = null, Quaternion? parentQuaternion = null)
        {
            _chestQuaternion = parentQuaternion.Value;

            Vector3 earVector = (Landmark(1) - Landmark(0)).normalized;

            Vector3 shoulderMidPoint = (Landmark(2) + Landmark(3)) * 0.5f;

            Vector3 noseShoulderVector = Landmark(4) - shoulderMidPoint;

            static float EulerAngleZ(Vector3 earVector)
            {
                Vector2 spineVectorProjectedXYPlane = ((Vector2)earVector).normalized;

                float cos = Vector2.Dot(spineVectorProjectedXYPlane, Vector2.up /* y-axis */);

                return Mathf.Acos(cos) * Mathf.Rad2Deg - 90.0f;
            }

            static float EulerAngleY(Vector3 earVector)
            {
                Vector2 spineVectorProjectedXZPlane = new Vector2(earVector.x, earVector.z).normalized;

                float cos = Vector2.Dot(spineVectorProjectedXZPlane, Vector2.up);

                return - Mathf.Acos(cos) * Mathf.Rad2Deg + 90.0f;
            }

            float EulerAngleX(Vector3 noseShoulderVector)
            {
                if (!CanRotateAroundXaxis) return 0.0f;

                Vector2 spineVectorProjectedYZPlane = new Vector2(noseShoulderVector.y, noseShoulderVector.z).normalized;

                float cos = Vector2.Dot(spineVectorProjectedYZPlane, Vector2.right);

                return - Mathf.Acos(cos) * Mathf.Rad2Deg + 90.0f + NodOffset;
            }

            Quaternion absoluteRotation = Quaternion.Euler(new Vector3(EulerAngleX(noseShoulderVector), 
                                                                       EulerAngleY(earVector), 
                                                                       EulerAngleZ(earVector)));

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

            // Apply
            ApplyRotation(afterDivisionRotation);
        }

        protected override Quaternion PreventUnwantedRotation(Quaternion smoothedRotationLHS, bool isDebug = false)
        {
            Vector3 rawEulerAngles = smoothedRotationLHS.eulerAngles;

            Vector3 stableEulerAngles = ContinuousAngleValue(rawEulerAngles);

            // Prohibit movements that exceed the range of motion of the human neck.
            stableEulerAngles.x = Mathf.Clamp(stableEulerAngles.x, -20.0f, 90.0f);
            stableEulerAngles.y = Mathf.Clamp(stableEulerAngles.y, -90.0f, 90.0f);
            stableEulerAngles.z = Mathf.Clamp(stableEulerAngles.z, -60.0f, 60.0f);
            
            if (isDebug) GameLogger.Log(stableEulerAngles);

            return Quaternion.Euler(stableEulerAngles);
        }
    }
}// namespace Mediapipe.Allocator
