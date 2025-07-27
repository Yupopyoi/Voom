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

        Vector3 armRotation;

        readonly float _notRotationThreshold = 0.8f; // Range : 1.0 <=> _verticalThreshold
        readonly float _verticalThreshold = 0.4f;    // Range : _notRotationThreshold <=> 0.0f

        readonly float _correctionCoefficientForForwardRotation = 60.0f;

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

        public override void ForwardApply(Rotation? parentRotation = null)
        {
            Vector3 leftShoulder = Landmark(0);
            Vector3 leftElbow = Landmark(1);

            Vector3 leftArmVec = leftElbow - leftShoulder;

            Vector3 calculatedEulerAngles = CalculateSignedEulerAngles(leftArmVec);

            Vector3 armRotationRawValue = new(0.0f, /* Implement in ReverseApply() */
                                              Mathf.Clamp(calculatedEulerAngles.y, -100f, 100f),
                                              Mathf.Clamp(calculatedEulerAngles.z, -100f, 100f));

            Vector3 propagatedRotation /* From parents ( = Chest ) */ = parentRotation.GetValueOrDefault().ToVector3;

            armRotation = armRotationRawValue - propagatedRotation;
        }

        float _prevTwistAngle = 0f;
        public override void ReverseApply(INamedVector palmVectors)
        {
            Vector3 upperArm = (Landmark(1) - Landmark(0)).normalized;
            Vector3 lowerArm = (Landmark(1) - Landmark(3)).normalized;
            lowerArm.z = ThresholdQuadratic(lowerArm.z, 0.5f);

            ((PalmVectors)palmVectors).ConvertToVerticalPalmVector(lowerArm, true);

            // Correction of forward rotation by LowerArm
            float lowerRawArmY = Vector3.Dot(lowerArm, Vector3.up); // [-1.0, 1.0]

            float lowerArmY = lowerRawArmY > 0.0f ? lowerRawArmY : 0.0f;
            armRotation.y -= _correctionCoefficientForForwardRotation * lowerArmY;

            #region Legacy

            /*
            // Dot product of upper arm and lower arm.
            // The larger this value, the more the arm is bent.
            // The maximum value of the dot product is 1,
            // because the vectors representing the upper arm and lower arm are normalized.
            float armDot = Vector3.Dot(lowerArm, upperArm);
            armDot = armDot > 0.0f ? armDot : 0.0f;

            // |      armDot     | armRotation.z [deg] | applicationRatio |
            // |                 |     (Lower Arm)     |                  |
            // |:---------------:|:-------------------:|:----------------:|
            // |       1.0       |          0          |         0        |
            // |        :        |          :          |         :        |
            // |    nThreshold   |          0          |         0        |
            // |        :        |          :          |         :        |
            // |        :        |        Lerp         |       Lerp       |
            // |        :        |          :          |         :        |
            // |    vThreshold   |         90          |         1        |
            // |        :        |          :          |         :        |
            // |        :        |        Lerp         |         :        |
            // |        :        |          :          |         :        |
            // |       0.0       | maxRotation ( > 90) |         1        |


            float spineLowerArmDot = Vector3.Dot(SpineVector(), lowerArm);
            float twistAngle = spineLowerArmDot * 90.0f;
            
            void ApplyTwist(float currentTwistAngle)
            {
                float deltaTwist = currentTwistAngle - _prevTwistAngle;
                //GameLogger.Log(currentTwistAngle, _prevTwistAngle,deltaTwist);
                _partObject.transform.Rotate(
                    _partObject.transform.right, // Ôü
                    deltaTwist,
                    Space.World
                );

                _prevTwistAngle = currentTwistAngle;
            }

            GameLogger.Log(spineLowerArmDot * 45);
            ApplyTwist(spineLowerArmDot * 45);
            //Vector3 upperArmVec = (elbowWorld - shoulderWorld).normalized;
            
            float armDot = Vector3.Dot(upperArm, lowerArm);

            // abs(armDot) > 0.5 : Ignore the palm vector
            //                     In this case, the X rotation of the upper arm depends on
            //                     the forward and backward rotation of the lower arm.
            // abs(armDot) < 0.2 : Ignore the dot product (LowerArm) 
            //                     In this case, the X rotation of the upper arm depends on
            //                     the palm vector.
            // otherwise : Blending two elements

            if (Mathf.Abs(armDot) < 0.2) // In other words, in your arms are bent.
            {
                // lowerArm.z represents the magnitude of the front-back rotation of the lower arm (normalized).

                // |    lowerArm.z    | armRotation.x [deg] |
                // |:----------------:|:-------------------:|
                // |        1.0       |           0         |
                // |         :        |          :          | (y = -45x + 45)
                // |        0.0       |          45         |
                // |         :        |          :          | (y = -90x + 45)
                // |       -1.0       |         135         |

                if(lowerArm.z > 0.0f)
                {
                    armRotation.x = - 45.0f * lowerArm.z + 45.0f;
                }
                else // lowerArm.z <= 0.0f
                {
                    armRotation.x = -90.0f * lowerArm.z + 45.0f;
                }

                GameLogger.Log(armRotation.x);
            }
            else if(Mathf.Abs(armDot) > 0.1)
            {
                // | leftPalmVector.y | armRotation.x [deg] |
                // |:----------------:|:-------------------:|
                // |        1.0       |          45         |
                // |         :        |           :         |
                // |         :        |         Lerp        |
                // |         :        |           :         |
                // |       -1.0       |           0         |

                armRotation.x = (leftPalmVector.y + 1.0f) * 45 / 2.0f;

                //GameLogger.Log(armDot);
            }
            else
            {
                GameLogger.Log("");
            }
            */

            #endregion
            
            ApplyRotation(ToSmoothStair(armRotation));
    }

        private Vector3 SpineVector()
        {
            Vector3 shoulderPos = (Landmark(2) + Landmark(0)) * 0.5f;
            Vector3 hipPos = (Landmark(5) + Landmark(4)) * 0.5f;
            return (hipPos - shoulderPos).normalized;
        }
        
    }
}// namespace Mediapipe.Allocator
