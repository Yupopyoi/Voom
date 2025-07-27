// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;
using Mediapipe.Tasks.Vision.HandLandmarker;
using UniVRM10;
using System.Collections.Generic;

namespace Mediapipe.Allocator
{
    public readonly struct PalmVectors : INamedVector
    {
        public readonly Vector3 LeftPalmVector;
        public readonly Vector3 RightPalmVector;

        private readonly Vector3 LIP; // Index => Pinky
        private readonly Vector3 RIP;

        public PalmVectors(Vector3 left, Vector3 right, Vector3 lip, Vector3 rip)
        {
            LeftPalmVector = left.normalized;
            RightPalmVector = right.normalized;
            LIP = lip.normalized;
            RIP = rip.normalized;
        }

        public Dictionary<string, Vector3> GetNamedVectors()
        {
            return new()
            {
                { "LeftPalm", LeftPalmVector },
                { "RightPalm", RightPalmVector }
            };
        }

        public Vector3 ConvertToVerticalPalmVector(Vector3 lowerArm, bool isLeft)
        {
            Vector3 rawPalmVector;
            if (isLeft)
            {
                rawPalmVector = LeftPalmVector;
            }
            else
            {
                rawPalmVector = RightPalmVector;
            }

            float snap = Vector3.Dot(lowerArm.normalized, rawPalmVector);
            return new();
        }

        public override readonly string ToString()
        {
            return $"Left : ({LeftPalmVector.x:F2}, {LeftPalmVector.y:F2}, {LeftPalmVector.z:F2}) , " +
                   $"Right : ({RightPalmVector.x:F2}, {RightPalmVector.y:F2}, {RightPalmVector.z:F2})";
        }
    }

    // See, https://ai.google.dev/edge/mediapipe/solutions/vision/hand_landmarker

    public class HandAdaptationManager : AdaptationManagerBase<HandLandmarkerResult>
    {

        bool _useHandAdaptation = true;

        protected override void OnEnable()
        {
            base.OnEnable();

            var vrmInstance = _vrmObject.GetComponent<Vrm10Instance>();
            vrmInstance.UpdateType = Vrm10Instance.UpdateTypes.None;

            GenerateLandmarksList(21 * 2); // (21 Landmarks) x (Left/Right Hands) , 0-20 : Left, 21-41 : Right 

            // Definition of "Adapters" that apply the result of MediaPipe to each part
            // and "Packets" that convey information to Adapter.
            
        }

        public override void ApplyMediapipeResult(HandLandmarkerResult recognitionResult)
        {
            if (!_useHandAdaptation) return;

            switch (recognitionResult.handedness.Count)
            {
                case 0: /* No hands detected */
                    return;
                case 1: /* One hand detected */
                    int offset = 0;
                    if(recognitionResult.handedness[0].categories[0].categoryName == "Right")
                    {
                        offset = 21;
                    }

                    for (int i = 0; i < _landmarks.Count / 2 /* = 21 Landmarks */; i++)
                    {
                        _landmarks[i + offset] = recognitionResult.handLandmarks[0].landmarks[i];
                    }
                    break;
                case 2: /* Two hands detected */
                    for (int i = 0; i < _landmarks.Count / 2 /* = 21 Landmarks */; i++)
                    {
                        _landmarks[i] = recognitionResult.handLandmarks[1].landmarks[i];
                        _landmarks[i + 21] = recognitionResult.handLandmarks[0].landmarks[i];
                    }
                    break;
                default:
                    return;
            }

            if (_vrmObject == null)
            {
                return;
            }
        }
    
        public PalmVectors CalculatePalmVectors()
        {
            //  0 (21) : Wrist
            //  5 (26) : Index_MCP
            // 17 (38) : Pinky_MCP

            Vector3 leftWI = new(_landmarks[5].x - _landmarks[0].x,
                                 _landmarks[5].y - _landmarks[0].y,
                                 _landmarks[5].z - _landmarks[0].z);
            Vector3 leftWP = new(_landmarks[17].x - _landmarks[0].x,
                                 _landmarks[17].y - _landmarks[0].y,
                                 _landmarks[17].z - _landmarks[0].z);
            Vector3 leftIP = new(_landmarks[17].x - _landmarks[5].x,
                                 _landmarks[17].y - _landmarks[5].y,
                                 _landmarks[17].z - _landmarks[5].z);
            Vector3 leftPalmVector = Vector3.Cross(leftWI, leftWP);


            Vector3 rightWI = new(_landmarks[26].x - _landmarks[21].x,
                                  _landmarks[26].y - _landmarks[21].y,
                                  _landmarks[26].z - _landmarks[21].z);
            Vector3 rightWP = new(_landmarks[38].x - _landmarks[21].x,
                                  _landmarks[38].y - _landmarks[21].y,
                                  _landmarks[38].z - _landmarks[21].z);
            Vector3 rightIP = new(_landmarks[38].x - _landmarks[26].x,
                                  _landmarks[38].y - _landmarks[26].y,
                                  _landmarks[38].z - _landmarks[26].z);
            Vector3 rightPalmVector = Vector3.Cross(rightWI, rightWP);

            return new PalmVectors(leftPalmVector, rightPalmVector, leftIP, rightIP);
        }
    }

}// namespace Mediapipe.Allocator
