// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;

namespace Mediapipe.Allocator
{
    [CreateAssetMenu(menuName = "Emotion/EyeGazeParams", fileName = "EyeGazeParams")]
    public class EyeGazeParams : ScriptableObject, IAdapterParams
    {
        [Header("General")]
        public bool CanDrawIrisMovement = true;
        public bool CanDrawIrisUpDownMovement = false;
        [Tooltip("It is more natural to set this variable to true.")]
        public bool KeepBothEyesSameUpDownMovement = true;

        [Header("X-Axis Max")]
        [Range(0f, 0.1f)] public float InnerMostValue = 0.012f;
        [Range(0f, 0.1f)] public float OuterMostValue = 0.021f;

        [Header("Y-Axis Max")]
        [Range(0f, 0.1f)] public float UpperMostValue = 0.06f;
        [Range(0f, 0.1f)] public float LowerMostValue = 0.05f;

        [Header("Gains")]
        [Range(0f, 2f)] public float LeftRightMovementGain = 1.0f;
        [Range(0f, 2f)] public float UpDownMovementGain = 1.0f;

        [Header("Center Position")]
        [Range(0f, 1f)] public float CenterOfLeftRightMovement = 0.55f;
        [Range(0f, 1f)] public float CenterOfUpDownMovement = 0.1f;

        public void ResetToDefaults()
        {
            CanDrawIrisMovement = true;
            CanDrawIrisUpDownMovement = false;
            KeepBothEyesSameUpDownMovement = true;

            InnerMostValue = 0.012f;
            OuterMostValue = 0.021f;
            UpperMostValue = 0.06f;
            LowerMostValue = 0.05f;
            LeftRightMovementGain = 1.0f;
            UpDownMovementGain = 1.0f;
            CenterOfLeftRightMovement = 0.55f;
            CenterOfUpDownMovement = 0.1f;
        }
    }

    public class EyeGazeAdapter : EmotionAdapterBase
    {
        readonly Transform _rightIris;
        readonly Transform _leftIris;

        Vector3 _rightEyeInitPosition;
        Vector3 _leftEyeInitPosition;

        Vector3 _rightEyePosition;
        Vector3 _leftEyePosition;

        private EyeGazeParams _prms;

        public EyeGazeAdapter(GameObject faceObject, LandmarksPacket landmarksPacket, GameObject rightIris, GameObject leftIris, EyeGazeParams eyeGazeParams = null)
            : base(faceObject, landmarksPacket) 
        { 
            _rightIris = rightIris.transform;
            _leftIris = leftIris.transform;

            _rightEyeInitPosition = _rightIris.localPosition;
            _leftEyeInitPosition = _leftIris.localPosition;

            _rightEyePosition = _rightIris.localPosition;
            _leftEyePosition = _leftIris.localPosition;

            if (eyeGazeParams == null)
            {
                _prms = ScriptableObject.CreateInstance<EyeGazeParams>();
            }
            else
            {
                _prms = eyeGazeParams;
            }
        }

        public override void SetParameter(IAdapterParams eyeGazeParams)
        {
            _prms = (EyeGazeParams)eyeGazeParams;
        }

        /* Landmark Index

        | Index | MP Index |              Part             |
        |:-----:|:--------:|:-----------------------------:|
        |   0   |   473    |       Right eye central       |
        |   1   |   468    |       Left  eye central       |
        |   2   |   386    |   Right eye (Upper central)   |
        |   3   |   374    |   Right eye (Lower central)   |
        |   4   |   159    |   Left  eye (Upper central)   |
        |   5   |   145    |   Left  eye (Lower central)   |            
        |   6   |   263    |     Right eye (Left  edge)    |
        |   7   |   362    |     Right eye (Right edge)    |
        |   8   |   133    |     Left eye (Left  edge)     |
        |   9   |    33    |     Left eye (Right edge)     |
        |  10   |   475    |     Iris of Right eye UC      |
        |  11   |   477    |     Iris of Right eye LC      |
        |  12   |   470    |     Iris of Left eye UC       |
        |  13   |   472    |     Iris of Left eye LC       |

        */

        public override void ForwardApply()
        {
            if (!_prms.CanDrawIrisMovement)
            {
                _rightIris.localPosition = _rightEyeInitPosition;
                _leftIris.localPosition = _leftEyeInitPosition;
                return;
            }

            float LeftRightPosition(int eyeIndex) /* 0 : Right Eye, 1 : Left Eye */
            {
                Vector3 horizontalEyeVector;
                Vector3 horizontalIrisEyeVector;

                if (eyeIndex == 0 /* Right Eye */)
                {
                    horizontalEyeVector = Landmark(7) - Landmark(6);
                    horizontalIrisEyeVector = Landmark(0) - Landmark(6);
                }
                else if (eyeIndex == 1 /* Left Eye */)
                {
                    horizontalEyeVector = Landmark(9) - Landmark(8);
                    horizontalIrisEyeVector = Landmark(9) - Landmark(1);
                }
                else /* Undefined */
                {
                    return float.NaN;
                }

                float flip = (eyeIndex == 0 /* Right Eye */) ? 1.0f : -1.0f;

                // Stretching the value to the left and right around 0.5
                float leftRightPositionRatio = Mathf.Clamp01((horizontalIrisEyeVector.x / horizontalEyeVector.x - _prms.CenterOfLeftRightMovement) * _prms.LeftRightMovementGain * 3.0f + 0.5f);

                float absLocalPosition = (_prms.OuterMostValue - _prms.InnerMostValue) * leftRightPositionRatio + _prms.InnerMostValue;

                return absLocalPosition * flip;
            }

            _rightEyePosition.x = LeftRightPosition(0);
            _leftEyePosition.x = LeftRightPosition(1);

            float UpDownPosition(int eyeIndex)
            {
                Vector3 vertivalEyeVector;
                Vector3 vertivalIrisEyeVector;

                if (eyeIndex == 0 /* Right Eye */)
                {
                    vertivalEyeVector = Landmark(3) - Landmark(2);
                    vertivalIrisEyeVector = Landmark(11) - Landmark(3);
                }
                else if (eyeIndex == 1 /* Left Eye */)
                {
                    vertivalEyeVector = Landmark(5) - Landmark(4);
                    vertivalIrisEyeVector = Landmark(13) - Landmark(5);
                }
                else /* Undefined */
                {
                    return float.NaN;
                }

                // Stretching the value to the left and right around 0.5
                float leftRightPositionRatio = Mathf.Clamp01((vertivalIrisEyeVector.y / vertivalEyeVector.y - _prms.CenterOfUpDownMovement) * _prms.UpDownMovementGain * 7.0f + 0.5f);

                float localPosition = (_prms.UpperMostValue - _prms.LowerMostValue) * (1.0f - leftRightPositionRatio) + _prms.LowerMostValue;

                return localPosition;
            }

            if(_prms.CanDrawIrisUpDownMovement)
            {
                _rightEyePosition.y = UpDownPosition(0);
                _leftEyePosition.y = UpDownPosition(1);
            }
            else
            {
                _rightEyeInitPosition.y = _rightIris.localPosition.y;
                _leftEyeInitPosition.y = _leftIris.localPosition.y;
            }

            if(_prms.KeepBothEyesSameUpDownMovement)
            {
                float max = Mathf.Max(_rightEyePosition.y, _leftEyePosition.y);
                _rightEyePosition.y = max;
                _leftEyePosition.y = max;
            }

            _rightIris.localPosition = _rightEyePosition;
            _leftIris.localPosition = _leftEyePosition;

        }
    }
}// namespace Mediapipe.Allocator
