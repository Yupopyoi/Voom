// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;

using Mediapipe.Allocator;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe.Tasks.Vision.FaceLandmarker;

namespace VRMController
{
    // This class generates and manages the "AdaptationManager" and relays the delivery of MediaPipe results.

    public class VRMAllocator : MonoBehaviour
    {
        PoseAdaptationManager _poseAdaptationManager;
        FaceAdaptationManager _faceAdaptationManager;

        [SerializeField] OperationDimension _operationDimension;

        public OperationDimension OperationDimension => _operationDimension;

        public void Allocate()
        {
            if(_poseAdaptationManager != null)
            {
                _poseAdaptationManager = null;
            }

            if (_faceAdaptationManager != null)
            {
                _faceAdaptationManager = null;
            }

            _poseAdaptationManager = ScriptableObject.CreateInstance<PoseAdaptationManager>();
            _faceAdaptationManager = ScriptableObject.CreateInstance<FaceAdaptationManager>();

            _poseAdaptationManager.Dimension = _operationDimension;
            _faceAdaptationManager.Dimension = _operationDimension;
        }

        public void EntryPoseAdaptation(PoseLandmarkerResult recognitionResult)
        {
            if (_poseAdaptationManager != null)
            {
                _poseAdaptationManager.ApplyMediapipeResult(recognitionResult);
            }
        }

        public void EntryFaceAdaptation(FaceLandmarkerResult recognitionResult)
        {
            if (_faceAdaptationManager != null)
            {
                _faceAdaptationManager.ApplyMediapipeResult(recognitionResult);
            }
        }
    }
}// namespace VRMController
