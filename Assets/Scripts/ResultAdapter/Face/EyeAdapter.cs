// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;

namespace Mediapipe.Allocator
{
    public class EyeAdapter : EmotionAdapterBase
    {
        public EyeAdapter(GameObject faceObject, LandmarksPacket landmarksPacket)
            : base(faceObject, landmarksPacket) { }

        bool KeepBothEyesSameMovement { get; set; } = true;

        bool HideEyeHighlight { get; set; } = false;

        float ClosedEyeSize { get; set; } = 0.3f;

        float EyeSizeScale { get; set; } = 1.5f;

        float ClosedEyeOffset { get; set; } = 0.3f;

        int JoySorrowBorder { get; set; } = 90;

        int SpreadFinishBorder { get; set; } = 200;

        float SurprisedMaxValue { get; set; } = 40.0f;

        public float AnglyEyebrowOffset { get; set; } = 0.5f;

        public float AnglyEyebrowScale { get; set; } = 1.0f;

        /* ### Landmark Index

            | Index | MP Index |              Part             |
            |:-----:|:--------:|:-----------------------------:|
            |   0   |   473    |       Right eye central       |
            |   1   |   468    |       Left  eye central       |
            |   2   |   386    |   Right eye (Upper central)   |
            |   3   |   374    |   Right eye (Lower central)   |
            |   4   |   159    |   Left  eye (Upper central)   |
            |   5   |   145    |   Left  eye (Lower central)   |            
            |  (6)  |   263    |     Right eye (Left  edge)    |
            |  (7)  |   362    |     Right eye (Right edge)    |
            |  (8)  |   132    |     Left eye (Left  edge)     |
            |  (9)  |    33    |     Left eye (Right edge)     |
            |:-----:|:--------:|:-----------------------------:|
            |  10   |   475    |     Iris of Right eye UC      |
            |  11   |   477    |     Iris of Right eye LC      |
            |  12   |   470    |     Iris of Left eye UC       |
            |  13   |   472    |     Iris of Left eye LC       |
            |:-----:|:--------:|:-----------------------------:|
            |  14   |   334    |    Center of Right eyebrow    |
            |  15   |   105    |    Center of Left  eyebrow    |
         */

        /* ### Controlling Parameters
                
            | Index |  Parameter's Name  |                      Description                      |
            |:-----:|:------------------:|:-----------------------------------------------------:|
            |   12  |  Fcl_EYE_Angly     |  Expressing "angly" by opening both eyes              |
            |   18  |  Fcl_EYE_Joy_R     |  General-purpose eye closing control (Right eye)      |
            |   19  |  Fcl_EYE_Joy_L     |  General-purpose eye closing control (Left  eye)      |
            |   20  |  Fcl_EYE_Sorrow    |  General-purpose eye closing control (Both  eyes)     |
            |   21  |  Fcl_EYE_Surprised |  Expressing "surprise" by opening both eyes           |
            |   22  |  Fcl_EYE_Spread    |  General-purpose eye more-opening control (Both eyes) |

         */

        public override void ForwardApply()
        {
            if(HideEyeHighlight)
            {
                _skinnedMeshRenderer.SetBlendShapeWeight(24, 100.0f);
            }
            else
            {
                _skinnedMeshRenderer.SetBlendShapeWeight(24,   0.0f);
            }

            Vector3 binocularVector = Landmark(0) - Landmark(1);
            float binocularDistance = PlaneDistance(binocularVector);

            // If the face is too far from the camera, it will move unnaturally; in this case, do not move the eyes.
            if (binocularDistance < 1.0f)
            {
                _skinnedMeshRenderer.SetBlendShapeWeight(18, 0.0f);
                _skinnedMeshRenderer.SetBlendShapeWeight(19, 0.0f);
                _skinnedMeshRenderer.SetBlendShapeWeight(20, 0.0f);
                _skinnedMeshRenderer.SetBlendShapeWeight(21, 0.0f);
                _skinnedMeshRenderer.SetBlendShapeWeight(22, 0.0f);
                return;
            }

            #region Right eye measurement

            Vector3 VerticalRightEyeVector = Landmark(3) - Landmark(2);
            float verticalRightEyeLength = PlaneDistance(VerticalRightEyeVector);

            float verticalRightEyeValue = (verticalRightEyeLength - ClosedEyeSize * 0.1f) / binocularDistance;

            float verticalRightEyeOverallInput = 100.0f - BindControlValue(verticalRightEyeValue, EyeSizeScale * 10.0f);

            float rightJoyValue;
            float rightSorrowValue;
            float right_a;

            if (verticalRightEyeOverallInput < JoySorrowBorder)
            {
                rightJoyValue = 0.0f;

                right_a = -100.0f / (JoySorrowBorder * JoySorrowBorder);
            }
            else
            {
                rightJoyValue = (verticalRightEyeOverallInput - JoySorrowBorder) * 100.0f / (100.0f - JoySorrowBorder);

                right_a = -100.0f / ((JoySorrowBorder - 100.0f) * (JoySorrowBorder - 100.0f));
            }

            // y = a(x - JoySorrowBorder)^2 + c
            rightSorrowValue = right_a * (verticalRightEyeOverallInput - JoySorrowBorder) * (verticalRightEyeOverallInput - JoySorrowBorder) + 100.0f;

            #endregion

            #region Left eye measurement

            Vector3 VerticalLeftEyeVector = Landmark(5) - Landmark(4);
            float verticalLeftEyeLength = PlaneDistance(VerticalLeftEyeVector);
                          
            float verticalLeftEyeValue = (verticalLeftEyeLength - ClosedEyeSize * 0.1f) / binocularDistance;
                          
            float verticalLeftEyeOverallInput = 100.0f - BindControlValue(verticalLeftEyeValue, EyeSizeScale * 10.0f);

            float leftJoyValue;
            float leftSorrowValue;
            float left_a;

            if (verticalLeftEyeOverallInput < JoySorrowBorder)
            {
                leftJoyValue = 0.0f;

                left_a = -100.0f / (JoySorrowBorder * JoySorrowBorder);
            }
            else
            {
                leftJoyValue = (verticalLeftEyeOverallInput - JoySorrowBorder) * 100.0f / (100.0f - JoySorrowBorder);

                left_a = -100.0f / ((JoySorrowBorder - 100.0f) * (JoySorrowBorder - 100.0f));
            }

            // y = a(x - JoySorrowBorder)^2 + c
            leftSorrowValue = left_a * (verticalLeftEyeOverallInput - JoySorrowBorder) * (verticalLeftEyeOverallInput - JoySorrowBorder) + 100.0f;

            #endregion

            #region Spread/Surprised

            float eyeSize = (verticalLeftEyeValue + verticalRightEyeValue) * EyeSizeScale * 1000.0f;
            float spreadValue;
            float surprisedValue = 0.0f;

            if (eyeSize > SpreadFinishBorder)
            {
                spreadValue = 100.0f;
                surprisedValue = SurprisedMaxValue;
            }
            else if (eyeSize > 100.0f)
            {
                float a = 100.0f / ((SpreadFinishBorder - 100.0f) * (SpreadFinishBorder - 100.0f));
                spreadValue = a * (eyeSize - 100.0f) * (eyeSize - 100.0f);
                surprisedValue = spreadValue * SurprisedMaxValue / 100.0f;
            }
            else
            {
                spreadValue = Mathf.Min(rightJoyValue, leftJoyValue) * 0.4f; /* kawaii */
            }

            #endregion

            #region Angly

            Vector3 leftEyebrowVector = Landmark(0) - Landmark(14);
            Vector3 rightEyebrowVector = Landmark(1) - Landmark(15);

            float eyebrowToEyeLengthAverage = (PlaneDistance(leftEyebrowVector) + PlaneDistance(rightEyebrowVector)) * 0.5f;
            float eyebrowToEyeLengthRatio = eyebrowToEyeLengthAverage / binocularDistance - AnglyEyebrowOffset;

            float anglyValue = BindControlValue(-eyebrowToEyeLengthRatio, AnglyEyebrowScale * 3.0f /* Make the SurpriseEyebrowScale roughly 1.5 */);

            #endregion

            #region Others

            if (KeepBothEyesSameMovement)
            {
                rightJoyValue = Mathf.Min(rightJoyValue, leftJoyValue);
                leftJoyValue = rightJoyValue;

                verticalRightEyeValue = (verticalLeftEyeValue + verticalRightEyeValue) * 0.5f;
                verticalLeftEyeValue = verticalRightEyeValue;
            }

            float sorrowValue = Mathf.Max(rightSorrowValue, leftSorrowValue) - ClosedEyeOffset * 100.0f;
            if (sorrowValue < 0.0f) sorrowValue = 0.0f;
            else if (sorrowValue > 100.0f) sorrowValue = 100.0f;

            #endregion

            #region Adaptation

            _skinnedMeshRenderer.SetBlendShapeWeight(12, anglyValue);
            _skinnedMeshRenderer.SetBlendShapeWeight(18, rightJoyValue);
            _skinnedMeshRenderer.SetBlendShapeWeight(19, leftJoyValue);
            _skinnedMeshRenderer.SetBlendShapeWeight(20, sorrowValue);
            _skinnedMeshRenderer.SetBlendShapeWeight(21, surprisedValue);
            _skinnedMeshRenderer.SetBlendShapeWeight(22, spreadValue);

            #endregion
        }
    }
}// namespace Mediapipe.Allocator
