// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;

namespace Mediapipe.Allocator
{
    [CreateAssetMenu(menuName = "Emotion/MouthParams", fileName = "MouthParams")]
    public class MouthParams : ScriptableObject, IAdapterParams
    {
        [Header("Overall")]
        [Range(0f, 2f)] public float OverallOperatingGain = 1.0f;

        [Header("Offset")]
        [Range(0f, 5f)] public float MouthSizeOffset = 1.0f;
        [Range(0f, 5f)] public float SurpriseEyebrowOffset = 1.0f;
        [Range(-1f, 1f)] public float MouthPositionOffset = 0.0f;

        [Header("Gain")]
        [Range(0f, 2f)] public float VerticalOpenGain = 1.0f;
        [Range(0f, 2f)] public float HorizontalMouthSizeGain = 1.0f;
        [Range(0f, 2f)] public float SorrowGain = 1.0f;
        [Range(0f, 2f)] public float FunnyGain = 1.0f;
        [Range(0f, 2f)] public float AngryGain = 1.0f;
        [Range(0f, 2f)] public float SurpriseEyebrowGain = 1.0f;
        [Range(0f, 2f)] public float MouthPositionGain = 1.0f;

        [Header("Max / Min")]
        [Range(0f, 100f)] public float OverallOpenMax = 100.0f;
        [Range(0f, 100f)] public float VerticalOpenMax = 70.0f;
        [Range(0f, 100f)] public float SorrowMax = 100.0f;
        [Range(0f, 100f)] public float FunnyMax = 80.0f;
        [Range(0f, 100f)] public float AngryMax = 100.0f;
        [Range(0f, 100f)] public float MouthPositionMin = 20.0f;

        public void ResetToDefaults()
        {
            OverallOperatingGain = 1.0f;

            MouthSizeOffset = 1.0f;
            SurpriseEyebrowOffset = 1.0f;
            MouthPositionOffset = 0.0f;

            VerticalOpenGain = 1.0f;
            HorizontalMouthSizeGain = 1.0f;
            SorrowGain = 1.0f;
            FunnyGain = 1.0f;
            AngryGain = 1.0f;
            SurpriseEyebrowGain = 1.0f;
            MouthPositionGain = 1.0f;

            OverallOpenMax = 100.0f;
            VerticalOpenMax = 70.0f;
            SorrowMax = 100.0f;
            FunnyMax = 80.0f;
            AngryMax = 100.0f;
            MouthPositionMin = 20.0f;
        }
    }

    public class MouthAdapter : EmotionAdapterBase
    {
        private MouthParams _prms;

        public MouthAdapter(GameObject faceObject, LandmarksPacket landmarksPacket, MouthParams mouthParams = null)
            : base(faceObject, landmarksPacket)
        {
            if (mouthParams == null)
            {
                _prms = ScriptableObject.CreateInstance<MouthParams>();
            }
            else
            {
                _prms = mouthParams;
            }
        }

        public override void SetParameter(IAdapterParams mouthParams)
        {
            _prms = (MouthParams)mouthParams;
        }

        /* Landmark Index

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
            |  12   |   105    |    Center of Right eyebrow    |
        |
         */

        /* Controlling Parameters
                
            | Index |  Parameter's Name  |                     Description                    |
            |:-----:|:------------------:|:--------------------------------------------------:|
            |   33  |  Fcl_MTH_Joy       |  General-purpose mouth opening control (Vertical)  |
            |   35  |  Fcl_MTH_Surprised |  General-purpose mouth opening control (Vertical)  |
            |   34  |  Fcl_MTH_Sorrow    |  General-purpose mouth opening control (Horizontal)|
            |   32  |  Fcl_MTH_Fun       |  Raise the corner of mouth                         |
            |   28  |  Fcl_MTH_Angly     |  Droop the corner of mouth                         |
            |   26  |  Fcl_MTH_Up        |  Position of mouth                                 |
            |   27  |  Fcl_MTH_Down      |  Position of mouth                                 |

         */

        private float _binocularDistance;
        private float _verticalOpening;
        private float _horizontalLength;
        private float _funnyValue;
        private float _anglyValue;
        private float _surpriseValue;
        private float _downValue;

        public override void ForwardApply()
        {
            Vector3 binocularVector = Landmark(9) - Landmark(10);
            _binocularDistance = PlaneDistance(binocularVector);

            // Calculate _verticalOpening, _horizontalLength
            CalculateGeneralOpeningAmount();

            // Calculate _funnyValue, _anglyValue 
            CalculateRaisingCornersAmount();

            CalculateSurpriseAmount();

            CalculateMouthDownAmount();

            Adapt();

            #region Local functions

            // Numeric literals defined within these local functions are intended
            // to make the properties "clean numbers", such as 1.0f

            void CalculateGeneralOpeningAmount()
            {

                float VerticalOpening()
                {
                    Vector3 verticalMouthVector = Landmark(0) - Landmark(1);
                    float verticalMouthLength = PlaneDistance(verticalMouthVector);

                    return BindControlValue(verticalMouthLength / _binocularDistance, _prms.VerticalOpenGain, _prms.VerticalOpenMax);
                }

                float Sorrow()
                {
                    Vector3 horizontalMouthVector = Landmark(2) - Landmark(3);
                    float horizontalMouthLength = PlaneDistance(horizontalMouthVector);

                    // 1.6f is multiplied to set the property's default value to 1.0.
                    float adjustedLength = (_binocularDistance - horizontalMouthLength * _prms.HorizontalMouthSizeGain * 1.6f) / horizontalMouthLength;

                    return BindControlValue(adjustedLength, _prms.SorrowGain * 0.5f, _prms.SorrowMax);
                }

                float v = VerticalOpening();
                float h = Sorrow();

                // Adjustment of vertical/horizontal opening amount
                // This prevents the mouth from opening too wide, which would be unnatural.
                float sum = v + h;
                float adjustedRatio = sum > _prms.OverallOpenMax ? (_prms.OverallOpenMax / sum) : 1.0f;

                _verticalOpening = v * adjustedRatio;
                _horizontalLength = h * adjustedRatio;
            }

            void CalculateRaisingCornersAmount()
            {
                Vector3 leftRaisingCorner = Landmark(0) - Landmark(2);
                Vector3 rightRaisingCorner = Landmark(0) - Landmark(3);

                float raisingCornerLengthAverage = (PlaneDistance(leftRaisingCorner) + PlaneDistance(rightRaisingCorner)) * 0.5f; // Average
                float raisingCornerLengthRatio = raisingCornerLengthAverage / _binocularDistance - _prms.MouthSizeOffset * 0.2f;

                float correctionValueOfCorners = ((100.0f - _verticalOpening) / 100.0f) * ((100.0f - _horizontalLength) / 100.0f);

                if (raisingCornerLengthRatio > 0.0f /* Funny */)
                {
                    // 17.5f is multiplied to set the property's default value to 1.0.
                    _funnyValue = BindControlValue(raisingCornerLengthRatio, _prms.FunnyGain * 17.5f, _prms.FunnyMax) * correctionValueOfCorners;
                    _anglyValue = 0.0f;
                }
                else /* Angly */
                {
                    // 22.5f is multiplied to set the property's default value to 1.0.
                    _anglyValue = BindControlValue(- raisingCornerLengthRatio - 0.05f, _prms.AngryGain * 22.5f, _prms.AngryMax) * correctionValueOfCorners;
                    _funnyValue = 0.0f;
                }
            }

            void CalculateSurpriseAmount()
            {
                Vector3 leftEyebrowVector = Landmark(9) - Landmark(11);
                Vector3 rightEyebrowVector = Landmark(10) - Landmark(12);

                float eyebrowToEyeLengthAverage = (PlaneDistance(leftEyebrowVector) + PlaneDistance(rightEyebrowVector)) * 0.5f;
                float eyebrowToEyeLengthRatio = eyebrowToEyeLengthAverage / _binocularDistance - _prms.SurpriseEyebrowOffset * 0.7f;

                // 3.0f is multiplied to set the property's default value to 1.0.
                _surpriseValue = BindControlValue(eyebrowToEyeLengthRatio, _prms.SurpriseEyebrowGain * 3.0f, 1.0f);
            }

            void CalculateMouthDownAmount()
            {
                // This function expresses for example the gmunchingh when eating something.
                // This is defined by the RATIO of the distance from the eyes to the mouth
                // relative to the distance between the left and right eyes (=_binocularDistance).

                Vector3 positionBetweenEyes = (Landmark(9) + Landmark(10)).Mul(0.5f);

                Vector3 lowerMouthCentral = Landmark(1);

                Vector3 vectorMouthToEyes = positionBetweenEyes - lowerMouthCentral;

                // This number is normally around 4.0, and during chewing movements , it is around 5.0.
                float distanceRatio = PlaneDistance(vectorMouthToEyes) / _binocularDistance;

                float standardizedDistanceRatio = distanceRatio / 4.0f + _prms.MouthPositionOffset;

                // 80.0f is multiplied to set the property's default value to 1.0.
                _downValue = (standardizedDistanceRatio - 1.0f) * _prms.MouthPositionGain * 80.0f;
            }

            void Adapt()
            {

                _skinnedMeshRenderer.SetBlendShapeWeight(33 /* Fcl_MTH_Joy */ , _verticalOpening * (1.0f - _surpriseValue) * 1.4f * _prms.OverallOperatingGain);

                _skinnedMeshRenderer.SetBlendShapeWeight(35 /* Fcl_MTH_Surprised */, _verticalOpening * _surpriseValue * 0.5f * _prms.OverallOperatingGain);

                _skinnedMeshRenderer.SetBlendShapeWeight(34 /* Fcl_MTH_Sorrow */ , _horizontalLength * _prms.OverallOperatingGain);

                _skinnedMeshRenderer.SetBlendShapeWeight(32 /* Fcl_MTH_Fun */ , _funnyValue * _prms.OverallOperatingGain);

                _skinnedMeshRenderer.SetBlendShapeWeight(28 /* Fcl_MTH_Angry */ , _anglyValue * _prms.OverallOperatingGain);

                if(_downValue > 0.0f)
                {
                    _skinnedMeshRenderer.SetBlendShapeWeight(26 /* Fcl_MTH_Up   */ , 0.0f);
                    _skinnedMeshRenderer.SetBlendShapeWeight(27 /* Fcl_MTH_Down */ , 
                                                            (_downValue > _prms.MouthPositionMin) ? _prms.MouthPositionMin : _downValue);
                }
                else
                {
                    _skinnedMeshRenderer.SetBlendShapeWeight(26 /* Fcl_MTH_Up   */ , _downValue);
                    _skinnedMeshRenderer.SetBlendShapeWeight(27 /* Fcl_MTH_Down */ , 0.0f);
                }

            }

            #endregion
        }
    }
}// namespace Mediapipe.Allocator
