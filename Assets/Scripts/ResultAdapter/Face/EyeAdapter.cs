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
    public class EyeAdapter : EmotionAdapterBase
    {
        public EyeAdapter(GameObject faceObject, LandmarksPacket landmarksPacket)
            : base(faceObject, landmarksPacket) { }

        #region General Properties

        // True makes the right eye close the same amount as the left eye.
        // This makes VRM Model's eyes less unnatural, but VRM Model won't be able to wink.Å@
        public bool KeepBothEyesSameMovement { get; set; } = true;

        public bool AlwaysDisplayEyeHighlight { get; set; } = true;

        public bool CanModifyEyeHighlight { get; set; } = true;

        // The larger this number, the larger the program will recognize your eyes.
        // This variable is used to adjust for eye size, which varies by person.
        public float EyeSizeScale { get; set; } = 1.0f;

        // This number indicates the ease of closing the eye.
        // When you want to be able to close eyes, this value should be greater than 1.
        // This makes VRM Model closer to actual human movement.
        // On the other hand, if you do not want to close eyes completely, this value should be less than 1.
        public float EaseOfClosingEyes = 1.6f;

        // The larger the number, the easier it is to express surprise.
        public float SurprisedEyeSizeClampFactor { get; set; } = 1.0f;

        // The larger this value is, the more emphasis the expression of surprise is given.
        public float ExpressionSurpriseMax { get; set; } = 100.0f;

        public float AnglyEyebrowOffset { get; set; } = 0.5f;

        // Variables for the impression of half-open eyes.
        // around 0.0 : Cute impression
        // around 3.0 : Easy to make disgusted eyes, pity impression
        public float AnglyEyebrowScale { get; set; } = 3.0f;

        #endregion

        #region Advanced Properties

        public float BorderBetweenJoyAndSorrow { get; set; } = 80.0f;
        public float MaxAmountOfEyeClosuresUsingSorrow { get; set; } = 90.0f;
        public float GradientOfSurpriseAmountSigmoid { get; set; } = 0.12f;
        public float GradientOfOpenCloseAmountSigmoid { get; set; } = 0.08f;

        #endregion

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

        readonly float[] _openedAmount = new float[2];
        readonly float[] _closedAmount = new float[2];
        readonly float[] _joyValue = new float[2];
        readonly float[] _sorrowValue = new float[2];
        float _spreadValue;
        float _surprisedValue;
        float _anglyValue;

        readonly float[] _eyeControlValues = new float[6];
        public ReadOnlyCollection<float> GetEyeControlValues()
        {
            return Array.AsReadOnly(_eyeControlValues);
        }

        public override void ForwardApply()
        {
            if (AlwaysDisplayEyeHighlight)
            {
                _skinnedMeshRenderer.SetBlendShapeWeight(24, 0.0f);
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

            #region Close / Open

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

                return verticalEyeLength / binocularDistance * EyeSizeScale * 20.0f;
            }

            for (int eyeIndex = 0; eyeIndex < 2; eyeIndex++) /* 0 : Right Eye, 1 : Left Eye */
            {
                _openedAmount[eyeIndex] = OpenedAmount(eyeIndex);
                _closedAmount[eyeIndex] = BindControlValue(1.0f - _openedAmount[eyeIndex]);

                // Joy
                _joyValue[eyeIndex] = Sigmoid(_closedAmount[eyeIndex], BorderBetweenJoyAndSorrow, MeshInputMax, GradientOfOpenCloseAmountSigmoid) * EaseOfClosingEyes;

                // Sorrow
                if(_closedAmount[eyeIndex] < BorderBetweenJoyAndSorrow)
                {
                    _sorrowValue[eyeIndex] = Sigmoid(_closedAmount[eyeIndex], MeshInputMin, BorderBetweenJoyAndSorrow, GradientOfOpenCloseAmountSigmoid);
                }
                else
                {
                    _sorrowValue[eyeIndex] = MeshInputMax - Sigmoid(_closedAmount[eyeIndex], BorderBetweenJoyAndSorrow, MaxAmountOfEyeClosuresUsingSorrow, GradientOfOpenCloseAmountSigmoid);
                }
            }

            #endregion

            #region Spread/Surprised

            // As the name indicates,ÅgSpreadÅhwidens the upper part of the eye.
            //ÅgSurprisedÅhalso widens the upper part of the eye, but is accompanied with a reduction of the iris area.

            // Defines the size of the eyes.
            // Multiplying by 50 is to make "eyeSize" roughly between 0 - 100 when SurprisedEyeSizeClampFactor == 1.0f
            float eyeSize = Mathf.Max(_openedAmount[0] , _openedAmount[1]) * SurprisedEyeSizeClampFactor * 50.0f;

            if (eyeSize > 100.0f /* Surprised enough */)
            {
                _spreadValue = MeshInputMax;
                _surprisedValue = ExpressionSurpriseMax;
            }
            else if (eyeSize > 50.0f /* A little surprised */)
            {
                _spreadValue = Sigmoid(eyeSize, 50.0f, 100.0f, GradientOfSurpriseAmountSigmoid);
                _surprisedValue = Math.Clamp(_spreadValue, MeshInputMin, ExpressionSurpriseMax);
            }
            else /* Not surprised */
            {
                _spreadValue = MeshInputMin;
                _surprisedValue = MeshInputMin;
            }

            #endregion

            #region Angly

            Vector3 leftEyebrowVector = Landmark(0) - Landmark(14);
            Vector3 rightEyebrowVector = Landmark(1) - Landmark(15);

            float eyebrowToEyeLengthAverage = (PlaneDistance(leftEyebrowVector) + PlaneDistance(rightEyebrowVector)) * 0.5f;
            float eyebrowToEyeLengthRatio = eyebrowToEyeLengthAverage / binocularDistance - AnglyEyebrowOffset;

            _anglyValue = BindControlValue(-eyebrowToEyeLengthRatio, AnglyEyebrowScale * 3.0f /* Make the SurpriseEyebrowScale roughly 0 - 3 */);

            #endregion

            #region Others

            if(!AlwaysDisplayEyeHighlight && CanModifyEyeHighlight)
            {
                _skinnedMeshRenderer.SetBlendShapeWeight(24, Sigmoid(_closedAmount.Max()));
            }

            if (KeepBothEyesSameMovement)
            {
                _joyValue[0] = _joyValue.Min();
                _joyValue[1] = _joyValue[0];
            }

            #endregion

            #region Adaptation

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

            #endregion
        }
    }
}// namespace Mediapipe.Allocator
