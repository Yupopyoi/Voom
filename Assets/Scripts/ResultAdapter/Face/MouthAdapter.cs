// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;

namespace Mediapipe.Allocator
{
    public class MouthAdapter : EmotionAdapterBase
    {
        public MouthAdapter(GameObject faceObject, LandmarksPacket landmarksPacket)
            : base(faceObject, landmarksPacket) { }

        #region General Properties

        public float OverallOperatingScale { get; set; } = 1.0f;

        public float HorizontalMouthSizeScale { get; set; } = 1.0f;

        public float SensitivityVerticalOpen { get; set; } = 1.0f;

        public float SensitivitySorrow { get; set; } = 1.0f;

        public float SensitivityFunny { get; set; } = 1.0f;

        public float SensitivityAngly { get; set; } = 1.0f;

        public float VerticalOpenMax { get; set; } = 70.0f;

        public float SorrowMax { get; set; } = 100.0f;

        public float OverallOpenMax { get; set; } = 100.0f;

        public float PoutingMax { get; set; } = 80.0f;

        public float FunnyMax { get; set; } = 80.0f;

        public float AnglyMax { get; set; } = 100.0f;

        public float MouthSizeOffset { get; set; } = 1.0f;

        public float SurpriseEyebrowOffset { get; set; } = 1.0f;

        public float SurpriseEyebrowScale { get; set; } = 1.0f;

        #endregion

        /* ### Landmark Index

            | Index | MP Index |              Part             |
            |:-----:|:--------:|:-----------------------------:|
            |   0   |    13    |         Upper central         |
            |   1   |    14    |         Lower central         |
            |   2   |   306    |           Left  edge          |
            |   3   |    78    |           Right edge          |
            |  (4)  |   311    |       Upper Left  middle      |
            |  (5)  |    81    |       Upper Right middle      |
            |  (6)  |   402    |       Lower Left  middle      |
            |  (7)  |   178    |       Lower Right middle      |
            |  (8)  |    17    | Lowest point of the lower lip |
            |:-----:|:--------:|:-----------------------------:|
            |   9   |   473    |           Left  eye           |
            |  10   |   468    |           Right eye           |
            |:-----:|:--------:|:-----------------------------:|
            |  11   |   334    |    Center of Left  eyebrow    |
            |  12   |   105    |    Center of Right eyebrow    
        |
         */

        /* ### Controlling Parameters
                
            | Index |  Parameter's Name  |                     Description                    |
            |:-----:|:------------------:|:--------------------------------------------------:|
            |   33  |  Fcl_MTH_Joy       |  General-purpose mouth opening control (Vertical)  |
            |   35  |  Fcl_MTH_Surprised |  General-purpose mouth opening control (Vertical)  |
            |   34  |  Fcl_MTH_Sorrow    |  General-purpose mouth opening control (Horizontal)|
            |   32  |  Fcl_MTH_Fun       |  Raise the corner of mouth                         |
            |   28  |  Fcl_MTH_Angly     |  Droop the corner of mouth                         |

         */

        float _binocularDistance;
        float _verticalOpening;
        float _horizontalLength;
        float _funnyValue;
        float _anglyValue;
        float _surpriseValue;

        public override void ForwardApply()
        {
            Vector3 binocularVector = Landmark(9) - Landmark(10);
            _binocularDistance = PlaneDistance(binocularVector);

            CalculateGeneralOpeningAmount();

            CalculateRaisingCornersAmount();

            CalculateSurpriseAmount();

            Adapt();

            // Local functions

            // Numeric literals defined within these local functions are intended
            // to make the properties "clean numbers", such as 1.0f
            
            void CalculateGeneralOpeningAmount()
            {

                float VerticalOpening()
                {
                    Vector3 verticalMouthVector = Landmark(0) - Landmark(1);
                    float verticalMouthLength = PlaneDistance(verticalMouthVector);

                    return BindControlValue(verticalMouthLength / _binocularDistance, SensitivityVerticalOpen, VerticalOpenMax);
                }

                float Sorrow()
                {
                    Vector3 horizontalMouthVector = Landmark(2) - Landmark(3);
                    float horizontalMouthLength = PlaneDistance(horizontalMouthVector);

                    float adjustedLength = (_binocularDistance - horizontalMouthLength * HorizontalMouthSizeScale * 1.6f) / horizontalMouthLength;

                    return BindControlValue(adjustedLength, SensitivitySorrow * 0.5f, SorrowMax);
                }

                float v = VerticalOpening();
                float h = Sorrow();

                // Adjustment of vertical/horizontal opening amount
                // This prevents the mouth from opening too wide, which would be unnatural.
                float sum = v + h;
                float adjustedRatio = sum > OverallOpenMax ? (OverallOpenMax / sum) : 1.0f;

                _verticalOpening = v * adjustedRatio;
                _horizontalLength = h * adjustedRatio;
            }

            void CalculateRaisingCornersAmount()
            {
                Vector3 leftRaisingCorner = Landmark(0) - Landmark(2);
                Vector3 rightRaisingCorner = Landmark(0) - Landmark(3);

                float raisingCornerLengthAverage = (PlaneDistance(leftRaisingCorner) + PlaneDistance(rightRaisingCorner)) * 0.5f;
                float raisingCornerLengthRatio = raisingCornerLengthAverage / _binocularDistance - MouthSizeOffset * 0.2f;

                float correctionValueOfCorners = ((100.0f - _verticalOpening) / 100.0f) * ((100.0f - _horizontalLength) / 100.0f);

                if (raisingCornerLengthRatio > 0.0f /* Funny */)
                {
                    _funnyValue = BindControlValue(raisingCornerLengthRatio, SensitivityFunny * 17.5f, FunnyMax) * correctionValueOfCorners;
                    _anglyValue = 0.0f;
                }
                else /* Angly */
                {
                    _funnyValue = 0.0f;
                    _anglyValue = BindControlValue(- raisingCornerLengthRatio - 0.05f, SensitivityAngly * 22.5f, AnglyMax) * correctionValueOfCorners;
                }
            }

            void CalculateSurpriseAmount()
            {
                Vector3 leftEyebrowVector = Landmark(9) - Landmark(11);
                Vector3 rightEyebrowVector = Landmark(10) - Landmark(12);

                float eyebrowToEyeLengthAverage = (PlaneDistance(leftEyebrowVector) + PlaneDistance(rightEyebrowVector)) * 0.5f;
                float eyebrowToEyeLengthRatio = eyebrowToEyeLengthAverage / _binocularDistance - SurpriseEyebrowOffset * 0.7f;

                _surpriseValue = BindControlValue(eyebrowToEyeLengthRatio, SurpriseEyebrowScale * 3.0f, 1.0f);
            }

            void Adapt()
            {

                _skinnedMeshRenderer.SetBlendShapeWeight(33 /* Fcl_MTH_Joy */ , _verticalOpening * (1.0f - _surpriseValue) * 1.4f * OverallOperatingScale);

                _skinnedMeshRenderer.SetBlendShapeWeight(35 /* Fcl_MTH_Surprised */, _verticalOpening * _surpriseValue * 0.5f * OverallOperatingScale);

                _skinnedMeshRenderer.SetBlendShapeWeight(34 /* Fcl_MTH_Sorrow */ , _horizontalLength * OverallOperatingScale);

                _skinnedMeshRenderer.SetBlendShapeWeight(32 /* Fcl_MTH_Fun */ , _funnyValue * OverallOperatingScale);

                _skinnedMeshRenderer.SetBlendShapeWeight(28 /* Fcl_MTH_Angly */ , _anglyValue * OverallOperatingScale);

            }
        }
    }
}// namespace Mediapipe.Allocator
