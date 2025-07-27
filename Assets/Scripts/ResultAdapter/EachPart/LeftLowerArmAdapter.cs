// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Unity.VisualScripting;
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
         *   |     2      |         15        |   left wrist   |
         */

        readonly float _notRotationThreshold = 0.7f; // Range : 1.0 <=> _verticalThreshold
        readonly float _verticalThreshold = 0.4f;    // Range : _notRotationThreshold <=> 0.0f
        readonly float _maxRotation = 170.0f; // [deg]

        Vector3 _armRotation = new(float.NaN, float.NaN, float.NaN);

        public override void ForwardApply(Rotation? parentRotation = null)
        {
            Vector3 upperArm = (Landmark(1) - Landmark(0)).normalized;
            Vector3 lowerArm = (Landmark(1) - Landmark(2)).normalized;
            //lowerArm.z = ThresholdQuadratic(lowerArm.z, 0.5f);

            if(_sleeve == Sleeve.Short)
            {
                ShortSleeveApply(upperArm, lowerArm);
            }
            else
            {
                LongSleeveApply(lowerArm, parentRotation.GetValueOrDefault());
            }

            ApplyRotation(_armRotation);
        }

        private void ShortSleeveApply(Vector3 upper, Vector3 lower)
        {
            // Dot product of upper arm and lower arm.
            // The larger this value, the more the arm is bent.
            // The maximum value of the dot product is 1,
            // because the vectors representing the upper arm and lower arm are normalized.
            float lowerArmY = Vector3.Dot(lower, upper);
            lowerArmY = lowerArmY > 0.0f ? lowerArmY : 0.0f;

            // |    lowerArmY    | armRotation.z [deg] |
            // |:---------------:|:-------------------:|
            // |       1.0       |          0          |
            // |        :        |          :          |
            // |    nThreshold   |          0          |
            // |        :        |          :          |
            // |        :        |        Lerp         |
            // |        :        |          :          |
            // |    vThreshold   |         90          |
            // |        :        |          :          |
            // |        :        |        Lerp         |
            // |        :        |          :          |
            // |       0.0       | maxRotation ( > 90) |

            float zRotation;
            if (lowerArmY > _notRotationThreshold)
            {
                zRotation = 0.0f;
            }
            else if (lowerArmY > _verticalThreshold)
            {
                float range = _notRotationThreshold - _verticalThreshold;
                float clampedZRotation = (lowerArmY - _verticalThreshold) / range;

                zRotation = Mathf.Lerp(90.0f, 0.0f, clampedZRotation);
            }
            else /* lowerArmY <= _fullRotationThreshold */
            {
                float range = _verticalThreshold;
                float clampedZRotation = lowerArmY / range;

                zRotation = Mathf.Lerp(_maxRotation, 90.0f, clampedZRotation);
            }

            _armRotation.z = zRotation;
        }
    
        private void LongSleeveApply(Vector3 lower, Rotation parent)
        {
            /*
            Vector3 calculatedEulerAngles = CalculateSignedEulerAngles(lower);

            Vector3 armRotationRawValue = new(0.0f,
                                              Mathf.Clamp(calculatedEulerAngles.y, -100f, 100f),
                                              Mathf.Clamp(calculatedEulerAngles.z, -100f, 100f));

            Vector3 propagatedRotation = parent.ToVector3;

            _armRotation = armRotationRawValue - propagatedRotation;
            */

            Quaternion worldRot = Quaternion.FromToRotation(Vector3.right, lower);

            // 親ボーンからの相対回転に変換
            Quaternion parentWorldRot = _partObject.transform.parent.rotation;
            Quaternion localRot = Quaternion.Inverse(parentWorldRot) * worldRot;

            // 適用
            _partObject.transform.localRotation = localRot;

            // optional: ログ確認
            GameLogger.Log(localRot.eulerAngles); // X,Y,Z全部出る
        }
    }
}// namespace Mediapipe.Allocator
