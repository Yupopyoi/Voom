// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Mediapipe.Allocator
{
    [CreateAssetMenu(menuName = "Emotion/EyeParams", fileName = "EyeParams")]
    public class EyeParams : ScriptableObject, IAdapterParams
    {
        [Header("General")]
        [Tooltip("True makes the right eye close the same amount as the left eye.\n" +
                 "This makes VRM Model's eyes less unnatural, but VRM Model won't be able to wink.")]Å@
        public bool KeepBothEyesSameMovement = true;

        public bool AlwaysDisplayEyeHighlight = true;

        public bool CanModifyEyeHighlight = true;

        [Tooltip("True  : It is easier to express feelings of surprise.\n"+
                 "False : It is easier to express looking up.")]
        public bool CanChangeSizeIris = true;

        [Header("Gain")]
        [Tooltip("The larger this number, the larger the program will recognize your eyes.\n"+
                 "This variable is used to adjust for eye size, which varies by person.")]
        [Range(0f, 2f)] public float EyeSizeGain = 1.0f;

        [Tooltip("This number indicates the ease of closing the eye.\n"+
                 "When you want to be able to close eyes, this value should be greater than 1.\n" +
                 "This makes VRM Model closer to actual human movement.\n" +
                 "On the other hand, if you do not want to close eyes completely, this value should be less than 1.")]
        [Range(0f, 2f)] public float ClosingEyeGain = 1.2f;

        [Tooltip("The larger the number, the easier it is to express surprise.")]
        [Range(0f, 2f)] public float SurprisedEyeSizeGain = 1.0f;

        [Tooltip("Variables for the impression of half-open eyes.\n" + 
                 "around 0.0 : Cute impression\n" + 
                 "around 3.0 : Easy to make disgusted eyes, pity impression")]
        [Range(0f, 4f)] public float AnglyEyebrowGain = 1.0f;

        [Header("Others")]
        [Tooltip("The larger this value is, the more emphasis the expression of surprise is given.")]
        [Range(0f, 100f)] public float ExpressionSurpriseMax = 100.0f;

        [Tooltip("If the face is too far from the camera, eyes will move unnaturally.\n" + 
                 "When the distance is less than this variable, do not move VRM Model's eyes.")]
        [Range(0f, 2f)] public float RecognitionLowerLimitDistance = 1.0f;

        [Range(0f, 2f)] public float AnglyEyebrowOffset = 0.5f;

        [Header("Advanced Properties")]
        [Range(0f, 100f)] public float BorderBetweenJoyAndSorrow = 80.0f;
        [Range(0f, 100f)] public float MaxAmountOfEyeClosuresUsingSorrow = 90.0f;
        [Range(0f, 0.2f)] public float GradientOfSurpriseAmountSigmoid = 0.12f;
        [Range(0f, 0.2f)] public float GradientOfOpenCloseAmountSigmoid = 0.08f;

        public void ResetToDefaults()
        {
            KeepBothEyesSameMovement = true;
            AlwaysDisplayEyeHighlight = true;
            CanModifyEyeHighlight = true;
            CanChangeSizeIris  = true;

            EyeSizeGain = 1.0f;
            ClosingEyeGain = 1.2f;
            SurprisedEyeSizeGain  = 1.0f;
            AnglyEyebrowGain = 1.0f;
            ExpressionSurpriseMax = 100.0f;
            RecognitionLowerLimitDistance = 1.0f;
            AnglyEyebrowOffset = 0.5f;

            BorderBetweenJoyAndSorrow = 80.0f;
            MaxAmountOfEyeClosuresUsingSorrow = 90.0f;
            GradientOfSurpriseAmountSigmoid = 0.12f;
            GradientOfOpenCloseAmountSigmoid = 0.08f;
        }
    }

    public class EyeAdapter : EmotionAdapterBase
    {
        private EyeParams _prms;

        public EyeAdapter(GameObject faceObject, LandmarksPacket landmarksPacket, EyeParams eyeParams = null)
            : base(faceObject, landmarksPacket) 
        {
            if (eyeParams == null)
            {
                _prms = ScriptableObject.CreateInstance<EyeParams>();
            }
            else
            {
                _prms = eyeParams;
            }
        }

        public override void SetParameter(IAdapterParams eyeParams)
        {
            _prms = (EyeParams)eyeParams;
        }

        public ReadOnlyCollection<float> GetEyeControlValues()
        {
            return Array.AsReadOnly(_eyeControlValues);
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
            |  (6)  |   263    |     Right eye (Left  edge)    |
            |  (7)  |   362    |     Right eye (Right edge)    |
            |  (8)  |   133    |     Left eye (Left  edge)     |
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

        /* Controlling Parameters
                
            | Index |  Parameter's Name  |                      Description                      |
            |:-----:|:------------------:|:-----------------------------------------------------:|
            |   12  |  Fcl_EYE_Angly     |  Expressing "angly" by opening both eyes              |
            |   18  |  Fcl_EYE_Joy_R     |  General-purpose eye closing control (Right eye)      |
            |   19  |  Fcl_EYE_Joy_L     |  General-purpose eye closing control (Left  eye)      |
            |   20  |  Fcl_EYE_Sorrow    |  General-purpose eye closing control (Both  eyes)     |
            |   21  |  Fcl_EYE_Surprised |  Expressing "surprise" by opening both eyes           |
            |   22  |  Fcl_EYE_Spread    |  General-purpose eye more-opening control (Both eyes) |

         */

        #region Private Member Variables

        private float _binocularDistance;
        private readonly float[] _openedAmount = new float[2];
        private readonly float[] _closedAmount = new float[2];
        private readonly float[] _joyValue = new float[2];
        private readonly float[] _sorrowValue = new float[2];
        private float _spreadValue;
        private float _surprisedValue;
        private float _anglyValue;

        private readonly float[] _eyeControlValues = new float[6];

        #endregion

        public override void ForwardApply()
        {
            if (_prms.AlwaysDisplayEyeHighlight)
            {
                _skinnedMeshRenderer.SetBlendShapeWeight(24, 0.0f);
            }

            _binocularDistance = PlaneDistance(Landmark(0) - Landmark(1));

            // If the face is too far from the camera, eyes will move unnaturally;
            // In this case, do not move the eyes.
            if (_binocularDistance < _prms.RecognitionLowerLimitDistance)
            {
                _skinnedMeshRenderer.SetBlendShapeWeight(18, 0.0f);
                _skinnedMeshRenderer.SetBlendShapeWeight(19, 0.0f);
                _skinnedMeshRenderer.SetBlendShapeWeight(20, 0.0f);
                _skinnedMeshRenderer.SetBlendShapeWeight(21, 0.0f);
                _skinnedMeshRenderer.SetBlendShapeWeight(22, 0.0f);
                return;
            }
            
            ControlOpenClose();

            ControlSpreadSurprised();

            ControlAngly();

            if(!_prms.AlwaysDisplayEyeHighlight && _prms.CanModifyEyeHighlight)
            {
                _skinnedMeshRenderer.SetBlendShapeWeight(24, Sigmoid(_closedAmount.Max()));
            }

            if (_prms.KeepBothEyesSameMovement)
            {
                _joyValue[0] = _joyValue.Min();
                _joyValue[1] = _joyValue[0];
            }

            Adapt();
        }

        private void ControlOpenClose()
        {
            float OpenedAmount(int eyeIndex)
            {
                Vector3 VerticalEyeVector;

                if (eyeIndex == 0) /* Right Eye */
                {
                    VerticalEyeVector = Landmark(3) - Landmark(2);
                }
                else /* Left Eye */
                {
                    VerticalEyeVector = Landmark(5) - Landmark(4);
                }

                float verticalEyeLength = PlaneDistance(VerticalEyeVector);

                return verticalEyeLength / _binocularDistance * _prms.EyeSizeGain * 20.0f;
            }

            for (int eyeIndex = 0; eyeIndex < 2; eyeIndex++) /* 0 : Right Eye, 1 : Left Eye */
            {
                _openedAmount[eyeIndex] = OpenedAmount(eyeIndex);
                _closedAmount[eyeIndex] = BindControlValue(1.0f - _openedAmount[eyeIndex]);

                // Joy
                _joyValue[eyeIndex] = Sigmoid(_closedAmount[eyeIndex] * _prms.ClosingEyeGain, _prms.BorderBetweenJoyAndSorrow, MeshInputMax, _prms.GradientOfOpenCloseAmountSigmoid);

                // Sorrow
                if (_closedAmount[eyeIndex] < _prms.BorderBetweenJoyAndSorrow)
                {
                    _sorrowValue[eyeIndex] = Sigmoid(_closedAmount[eyeIndex], MeshInputMin, _prms.BorderBetweenJoyAndSorrow, _prms.GradientOfOpenCloseAmountSigmoid);
                }
                else
                {
                    _sorrowValue[eyeIndex]
                        = MeshInputMax - Sigmoid(_closedAmount[eyeIndex], _prms.BorderBetweenJoyAndSorrow, _prms.MaxAmountOfEyeClosuresUsingSorrow, _prms.GradientOfOpenCloseAmountSigmoid);
                }
            }
        }
    
        private void ControlSpreadSurprised()
        {
            // As the name indicates,ÅgSpreadÅhwidens the upper part of the eye.
            //ÅgSurprisedÅhalso widens the upper part of the eye, but is accompanied with a reduction of the iris area.

            // Defines the size of the eyes.
            // Multiplying by 50 is to make "eyeSize" roughly between 0 - 100 when SurprisedEyeSizeClampFactor == 1.0f
            float eyeSize = Mathf.Max(_openedAmount[0], _openedAmount[1]) * _prms.SurprisedEyeSizeGain * 50.0f;

            if (eyeSize > 100.0f /* Surprised enough */)
            {
                _spreadValue = MeshInputMax;
                _surprisedValue = _prms.ExpressionSurpriseMax;
            }
            else if (eyeSize > 50.0f /* A little surprised */)
            {
                _spreadValue = Sigmoid(eyeSize, 50.0f, 100.0f, _prms.GradientOfSurpriseAmountSigmoid);
                _surprisedValue = Math.Clamp(_spreadValue, MeshInputMin, _prms.ExpressionSurpriseMax);
            }
            else /* Not surprised */
            {
                _spreadValue = MeshInputMin;
                _surprisedValue = MeshInputMin;
            }

            if (!_prms.CanChangeSizeIris)
            {
                _surprisedValue = MeshInputMin;
            }
        }
    
        private void ControlAngly()
        {
            Vector3 leftEyebrowVector = Landmark(0) - Landmark(14);
            Vector3 rightEyebrowVector = Landmark(1) - Landmark(15);

            float eyebrowToEyeLengthAverage = (PlaneDistance(leftEyebrowVector) + PlaneDistance(rightEyebrowVector)) * 0.5f;
            float eyebrowToEyeLengthRatio = eyebrowToEyeLengthAverage / _binocularDistance - _prms.AnglyEyebrowOffset;

            _anglyValue = BindControlValue(- eyebrowToEyeLengthRatio, _prms.AnglyEyebrowGain * 3.0f /* Make the SurpriseEyebrowScale roughly 0 - 3 */);
        }

        private void Adapt()
        {
            _skinnedMeshRenderer.SetBlendShapeWeight(12, _anglyValue);
            _skinnedMeshRenderer.SetBlendShapeWeight(18, _joyValue[0]);
            _skinnedMeshRenderer.SetBlendShapeWeight(19, _joyValue[1]);
            _skinnedMeshRenderer.SetBlendShapeWeight(20, _sorrowValue.Max());
            _skinnedMeshRenderer.SetBlendShapeWeight(21, _surprisedValue);
            _skinnedMeshRenderer.SetBlendShapeWeight(22, _spreadValue);

            _eyeControlValues[0] = _anglyValue;
            _eyeControlValues[1] = _joyValue[0];
            _eyeControlValues[2] = _joyValue[1];
            _eyeControlValues[3] = _sorrowValue.Max();
            _eyeControlValues[4] = _surprisedValue;
            _eyeControlValues[5] = _spreadValue;
        }
    }
}// namespace Mediapipe.Allocator
