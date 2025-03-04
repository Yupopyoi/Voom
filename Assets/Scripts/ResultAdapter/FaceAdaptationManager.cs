// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;
using Mediapipe.Tasks.Vision.FaceLandmarker;

namespace Mediapipe.Allocator
{
    public class FaceAdaptationManager : AdaptationManagerBase<FaceLandmarkerResult>
    {
        GameObject _faceObject; 

        LandmarksPacket _mouthPacket;
        MouthAdapter _mouthAdapter;

        LandmarksPacket _eyePacket;
        EyeAdapter _eyeAdapter;

        protected override void Start()
        {
            base.Start();

            GenerateLandmarksList(478);

            _faceObject = _vrmObject.transform.Find("Face").gameObject;

            _mouthPacket = new(_landmarks, new int[13] { 13, 14, 306, 78, 311, 81, 402, 178, 17, 473, 468, 334, 105 });
            _mouthAdapter = new(_faceObject, _mouthPacket);

            _eyePacket = new(_landmarks, new int[16] { 473, 468, 386, 374, 159, 145, 263, 362, 132, 33, 475, 477, 470, 472, 334, 105 });
            _eyeAdapter = new(_faceObject, _eyePacket);
        }

        public override void ApplyMediapipeResult(FaceLandmarkerResult recognitionResult)
        {
            for (int i = 0; i < _landmarks.Count; i++)
            {
                _landmarks[i] = recognitionResult.faceLandmarks[0].landmarks[i];
            }

            _mouthAdapter.ForwardApply();
            _eyeAdapter.ForwardApply();
        }
    }

} // namespace Mediapipe.Allocator
