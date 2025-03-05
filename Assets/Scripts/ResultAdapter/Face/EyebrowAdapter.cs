// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.ObjectModel;
using UnityEngine;

namespace Mediapipe.Allocator
{
    public class EyebrowAdapter : EmotionAdapterBase
    {
        private readonly ReadOnlyCollection<float> _eyeControlValues;

        public EyebrowAdapter(GameObject faceObject, LandmarksPacket landmarksPacket, ReadOnlyCollection<float> eyeControlValues)
            : base(faceObject, landmarksPacket) 
        {
            _eyeControlValues = eyeControlValues;
        }

        public float SensitivityOfBrowAngly { get; set; } = 0.8f;
        public float SensitivityOfBrowSurprised { get; set; } = 1.2f;

        /* ### ReadOnlyCollection<float> _eyeControlValues
                
            | List Index |  Parameter's Name  |                      Description                      |
            |:----------:|:------------------:|:-----------------------------------------------------:|
            |      0     |  Fcl_EYE_Angly     |  Expressing "angly" by opening both eyes              |
            |      1     |  Fcl_EYE_Joy_R     |  General-purpose eye closing control (Right eye)      |
            |      2     |  Fcl_EYE_Joy_L     |  General-purpose eye closing control (Left  eye)      |
            |      3     |  Fcl_EYE_Sorrow    |  General-purpose eye closing control (Both  eyes)     |
            |      4     |  Fcl_EYE_Surprised |  Expressing "surprise" by opening both eyes           |
            |      5     |  Fcl_EYE_Spread    |  General-purpose eye more-opening control (Both eyes) |

         */

        /* ### Controlling Parameters

            | Index |  Parameter's Name  |
            |:-----:|:------------------:|
            |   6   |  Fcl_BRW_Angle     |
            |  10   |  Fcl_BRW_Surprised |

        */

        public override void ForwardApply()
        {
            float anglyValue = Sigmoid(_eyeControlValues[0], 0.08f);
            float surprised = Sigmoid(_eyeControlValues[4], 0.08f);

            _skinnedMeshRenderer.SetBlendShapeWeight(6, anglyValue * SensitivityOfBrowAngly);
            _skinnedMeshRenderer.SetBlendShapeWeight(10, surprised * SensitivityOfBrowSurprised);
        }
    }
}// namespace Mediapipe.Allocator
