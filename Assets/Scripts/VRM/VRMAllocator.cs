// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Mediapipe.Allocator;
using Mediapipe.Tasks.Vision.FaceLandmarker;
using Mediapipe.Tasks.Vision.HandLandmarker;
using Mediapipe.Tasks.Vision.PoseLandmarker;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace VRMController
{
    public enum Sleeve
    {
        Long,
        Short,
    }

    // This class generates and manages the "AdaptationManager" and relays the delivery of MediaPipe results.

    public class VRMAllocator : MonoBehaviour
    {
        PoseAdaptationManager _poseAdaptationManager;
        FaceAdaptationManager _faceAdaptationManager;
        HandAdaptationManager _handAdaptationManager;

        [SerializeField] OperationDimension _operationDimension;
        [SerializeField] Sleeve _sleeve;

        public OperationDimension OperationDimension => _operationDimension;

        public void Allocate()
        {
            if (_poseAdaptationManager != null)
            {
                _poseAdaptationManager = null;
            }

            if (_faceAdaptationManager != null)
            {
                _faceAdaptationManager = null;
            }

            if (_handAdaptationManager != null)
            {
                _handAdaptationManager = null;
            }

            _poseAdaptationManager = ScriptableObject.CreateInstance<PoseAdaptationManager>();
            _faceAdaptationManager = ScriptableObject.CreateInstance<FaceAdaptationManager>();
            _handAdaptationManager = ScriptableObject.CreateInstance<HandAdaptationManager>();

            _poseAdaptationManager.Dimension = _operationDimension;
            _faceAdaptationManager.Dimension = _operationDimension;
            _handAdaptationManager.Dimension = _operationDimension;

            _poseAdaptationManager.Sleeve = _sleeve;
        }

        public void EntryPoseAdaptation(PoseLandmarkerResult recognitionResult)
        {
            if (_poseAdaptationManager != null)
            {
                _poseAdaptationManager.ApplyMediapipeResult(recognitionResult);
                _poseAdaptationManager.PalmVectors = _handAdaptationManager.CalculatePalmVectors();
            }
        }

        public void EntryFaceAdaptation(FaceLandmarkerResult recognitionResult)
        {
            if (_faceAdaptationManager != null)
            {
                _faceAdaptationManager.ApplyMediapipeResult(recognitionResult);
            }
        }

        public void EntryHandAdaptation(HandLandmarkerResult recognitionResult)
        {
            if (_handAdaptationManager != null)
            {
                _handAdaptationManager.ApplyMediapipeResult(recognitionResult);
            }
        }

        public void OnSleeveChanged(Sleeve sleeve)
        {
            _sleeve = sleeve;
            if (_poseAdaptationManager != null)
            {
                _poseAdaptationManager.Sleeve = _sleeve;
            }
        }

        public ReadOnlyCollection<Mediapipe.Tasks.Components.Containers.NormalizedLandmark> Landmarks()
        {
            if (_poseAdaptationManager == null)
            {
                return new ReadOnlyCollection<Mediapipe.Tasks.Components.Containers.NormalizedLandmark>
                    (new List<Mediapipe.Tasks.Components.Containers.NormalizedLandmark>());
            }

            return _poseAdaptationManager.Landmarks;
        }
    }
}// namespace VRMController
