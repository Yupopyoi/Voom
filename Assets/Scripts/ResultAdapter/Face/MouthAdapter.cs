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

        public float OverallOperatingScale { get; set; } = 0.9f;
        public float VerticalOpenScale { get; set; } = 1.4f;
        public float HorizontalMouthSizeScale { get; set; } = 1.6f;
        public float HorizontalOpenScale { get; set; } = 0.5f;
        public float MouthSizeOffset { get; set; } = 0.2f;
        public float AnglyScale { get; set; } = 3.0f;
        public float SurpriseEyebrowOffset { get; set; } = 0.7f;
        public float SurpriseEyebrowScale { get; set; } = 1.0f;

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
            |   8   |    17    | Lowest point of the lower lip |
            |:-----:|:--------:|:-----------------------------:|
            |   9   |   473    |           Left  eye           |
            |  10   |   468    |           Right eye           |
            |:-----:|:--------:|:-----------------------------:|
            |  11   |   334    |    Center of Left  eyebrow    |
            |  12   |   105    |    Center of Right eyebrow    |
         */

        /* ### Controlling Parameters
                
            | Index |  Parameter's Name  |                     Description                    |
            |:-----:|:------------------:|:--------------------------------------------------:|
            |   33  |  Fcl_MTH_Joy       |  General-purpose mouth opening control (Vertical)  |
            |   34  |  Fcl_MTH_Sorrow    |  General-purpose mouth opening control (Horizontal)|
            |   35  |  Fcl_MTH_Surprised |  General-purpose mouth opening control (Vertical)  |
            |   32  |  Fcl_MTH_Fun       |  Raise the corner of mouth                         |
            |   28  |  Fcl_MTH_Angly     |  Droop the corner of mouth                         |

         */

        public override void ForwardApply()
        {
            Vector3 binocularVector = Landmark(9) - Landmark(10);
            float binocularDistance = PlaneDistance(binocularVector);

            #region General Opening Amount

            // Vertical mouth opening amount

            Vector3 verticalMouthVector = Landmark(0) - Landmark(1);
            float verticalMouthLength = PlaneDistance(verticalMouthVector);

            float verticalMouthOpening = BindControlValue(verticalMouthLength / binocularDistance, VerticalOpenScale, 70.0f);

            // Horizontal mouth opening amount

            Vector3 horizontalMouthVector = Landmark(2) - Landmark(3);
            float horizontalMouthLength = PlaneDistance(horizontalMouthVector);

            float horizontalMouthOpening = BindControlValue((binocularDistance - horizontalMouthLength * HorizontalMouthSizeScale) / horizontalMouthLength, HorizontalOpenScale);

            // Adjustment of vertical/horizontal opening amount
            // This prevents the mouth from opening too wide, which would be unnatural.

            float generalOpeningValue = verticalMouthOpening + horizontalMouthOpening;

            float adjustedRatioPreventingOverOpening = generalOpeningValue > 100.0f ? (100.0f / generalOpeningValue) : 1.0f;

            verticalMouthOpening *= adjustedRatioPreventingOverOpening;
            horizontalMouthOpening *= adjustedRatioPreventingOverOpening;

            #endregion

            #region Raising corners of mouth

            Vector3 leftRaisingCornerVector  = Landmark(0) - Landmark(2);
            Vector3 rightRaisingCornerVector = Landmark(0) - Landmark(3);

            float raisingCornerLengthAverage = (PlaneDistance(leftRaisingCornerVector) + PlaneDistance(rightRaisingCornerVector)) * 0.5f;
            float raisingCornerLengthRatio = raisingCornerLengthAverage / binocularDistance - MouthSizeOffset;

            float correctionValueOfCorners = ((100.0f - verticalMouthOpening) / 100.0f) * ((100.0f - horizontalMouthOpening) / 100.0f);
            float funnyValue = 0.0f;
            float anglyValue = 0.0f;

            if (raisingCornerLengthRatio > 0.0f /* Funny */)
            {
                funnyValue = BindControlValue( raisingCornerLengthRatio, 15.0f, 80.0f) * correctionValueOfCorners;
            }
            else /* Angly */
            {
                anglyValue = BindControlValue(-raisingCornerLengthRatio - 0.05f, 15.0f, 80.0f) * correctionValueOfCorners;
            }

            #endregion

            #region Surprise

            Vector3 leftEyebrowVector  = Landmark( 9) - Landmark(11);
            Vector3 rightEyebrowVector = Landmark(10) - Landmark(12);

            float eyebrowToEyeLengthAverage = (PlaneDistance(leftEyebrowVector) + PlaneDistance(rightEyebrowVector)) * 0.5f;
            float eyebrowToEyeLengthRatio = eyebrowToEyeLengthAverage / binocularDistance - SurpriseEyebrowOffset;

            float surpriseValue = BindControlValue(eyebrowToEyeLengthRatio, SurpriseEyebrowScale * 3.0f /* Make the SurpriseEyebrowScale roughly 1.5 */, 1.0f);

            #endregion

            #region Adaptation

            _skinnedMeshRenderer.SetBlendShapeWeight(33, verticalMouthOpening * (1.0f - surpriseValue) * OverallOperatingScale);

            _skinnedMeshRenderer.SetBlendShapeWeight(34, horizontalMouthOpening * OverallOperatingScale);

            _skinnedMeshRenderer.SetBlendShapeWeight(35, verticalMouthOpening * surpriseValue * OverallOperatingScale);

            _skinnedMeshRenderer.SetBlendShapeWeight(32, funnyValue * OverallOperatingScale);

            _skinnedMeshRenderer.SetBlendShapeWeight(28, anglyValue * OverallOperatingScale * AnglyScale);

            #endregion
        }
    }
}// namespace Mediapipe.Allocator
