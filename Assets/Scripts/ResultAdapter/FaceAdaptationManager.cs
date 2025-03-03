// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Mediapipe.Tasks.Vision.FaceLandmarker;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniVRM10;

namespace Mediapipe.Allocator
{
    public class FaceAdaptationManager : MonoBehaviour
    {
        GameObject _vrmObject;

        List<Tasks.Components.Containers.NormalizedLandmark> _landmarks;

        LandmarksPacket _mouthPacket;
        MouthAdapter _mouthAdapter;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            Vrm10Instance vrmInstance = FindFirstObjectByType<Vrm10Instance>();
            if (vrmInstance != null)
            {
                _vrmObject = vrmInstance.gameObject;
                Debug.Log($"VRM Model found: {_vrmObject.name}");
            }
            else
            {
                Debug.Log("No VRM Model found.");
                return;
            }

            _landmarks = Enumerable.Repeat(new Tasks.Components.Containers.NormalizedLandmark(), 478).ToList();

            _mouthPacket = new(_landmarks, new int[13] { 13, 14, 306, 78, 311, 81, 402, 178, 17, 473, 468, 334, 105 });
            _mouthAdapter = new(_vrmObject.transform.Find("Face")?.gameObject, _mouthPacket);
        }

        // Update is called once per frame
        public void ApplyMediapipeResult(FaceLandmarkerResult recognitionResult)
        {
            for (int i = 0; i < _landmarks.Count; i++)
            {
                _landmarks[i] = recognitionResult.faceLandmarks[0].landmarks[i];
            }

            _mouthAdapter.ForwardApply();
        }
    }
} // namespace Mediapipe.Allocator
