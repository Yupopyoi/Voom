using UnityEngine;

namespace Mediapipe.UnityRunner
{
    public class Pauser : MonoBehaviour
    {
        [SerializeField] bool _pauseAll = false;
        bool _prevPauseAll = false;

        [SerializeField] FaceLandmarkDetection.FaceLandmarkerRunner _faceLandmarkerRunner;
        [SerializeField] HandLandmarkDetection.HandLandmarkerRunner _handLandmarkerRunner;
        [SerializeField] PoseLandmarkDetection.PoseLandmarkerRunner _poseLandmarkerRunner;

        void Update()
        {
            if (_pauseAll == _prevPauseAll) return;

            if (_pauseAll)
            {
                _faceLandmarkerRunner.Pause();
                _handLandmarkerRunner.Pause();
                _poseLandmarkerRunner.Pause();
            }
            else
            {
                _faceLandmarkerRunner.Resume();
                _handLandmarkerRunner.Resume();
                _poseLandmarkerRunner.Resume();
            }

            _prevPauseAll = _pauseAll;
        }
    }
}// Mediapipe.UnityRunner
