// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;
using System.Collections.ObjectModel;

namespace Mediapipe.Allocator
{
    public class CatEarAdapter : EmotionAdapterBase
    {
        private readonly ReadOnlyCollection<float> _eyeControlValues;

        private readonly Transform _transformEarR;
        private readonly Transform _transformEarL;

        private readonly bool _haveCatEar = false;

        private readonly Vector3 _initialRotationCatEarR;
        private readonly Vector3 _initialRotationCatEarL;
        private readonly float _centerRotationZ;

        #region General Properties

        /// <summary>
        /// Whether the cat ears move spontaneously in response to facial expressions.
        /// There is still passive ear movement in response to body movement.
        /// </summary>
        public bool CanMoveResponseToExpression { get; set; } = true;

        /// <summary>
        /// Amount of ear growth when looking surprised (Greater than 1)
        /// </summary>
        public float GrowthAmount { get; set; } = 1.10f;

        /// <summary>
        /// Settings for whether or not to make minute movements automatically.
        /// This movement is not affected by the detection results of MediaPipe.
        /// When this is set to true, the ears move on their own and look cute.
        /// </summary>
        public bool MoveMinuteMovementsAutomatically { get; set; } = true;

        /// <summary>
        /// Frequency of automatic updates.
        /// This is defined by the number of times ForwardApply() is called, 
        /// so the interval (in seconds) varies depending on the execution environment.
        /// </summary>
        public int FrequencyOfAutomaticUpdates { get; set; } = 6;

        /// <summary>
        /// Specify the range of angles [deg] that can be changed.
        /// It is recommended not to set this value too large.
        /// </summary>
        public float AutomaticOperationSize { get; set; } = 2.0f;

        #endregion

        public CatEarAdapter(GameObject earObjectR, GameObject earObjectL, LandmarksPacket landmarksPacket, ReadOnlyCollection<float> eyeControlValues)
            : base(new GameObject(), landmarksPacket)
        {
            _eyeControlValues = eyeControlValues;

            bool haveCatEarR = !IsEmptyGameObject(earObjectR);
            bool haveCatEarL = !IsEmptyGameObject(earObjectL);

            if (haveCatEarR && haveCatEarL)
            {
                _haveCatEar = true;
                _transformEarR = earObjectR.GetComponent<Transform>();
                _transformEarL = earObjectL.GetComponent<Transform>();

                _initialRotationCatEarR = _transformEarR.rotation.eulerAngles;
                _initialRotationCatEarL = _transformEarL.rotation.eulerAngles;

                if (_initialRotationCatEarR.z > 180.0f) _initialRotationCatEarR.z -= 360.0f;
                if (_initialRotationCatEarL.z > 180.0f) _initialRotationCatEarL.z -= 360.0f;

                _centerRotationZ = (Mathf.Abs(_initialRotationCatEarR.z) + Mathf.Abs(_initialRotationCatEarL.z)) * 0.5f;
            }
        }

        int _calledCounter = 0;

        public override void ForwardApply()
        {
            if (!_haveCatEar) return;

            if (!CanMoveResponseToExpression) return;

            float surpriseValue = Sigmoid((_eyeControlValues[4] + _eyeControlValues[5]) * 0.5f, 0.3f) * 0.01f; // Average & [0,100] -> [0,1]

            float growthValue = surpriseValue * GrowthAmount > 1.0f ? surpriseValue * GrowthAmount : 1.0f;

            _transformEarR.localScale = new Vector3(1.0f, growthValue, 1.0f);
            _transformEarL.localScale = new Vector3(1.0f, growthValue, 1.0f);

            if (!MoveMinuteMovementsAutomatically) return;

            if(_calledCounter++ > FrequencyOfAutomaticUpdates)
            {
                float _rotationZ = Random.Range(_centerRotationZ - AutomaticOperationSize, _centerRotationZ + AutomaticOperationSize);

                _transformEarR.localRotation = Quaternion.Euler( new Vector3(0.0f, 0.0f, -_rotationZ) );
                _transformEarL.localRotation = Quaternion.Euler( new Vector3(0.0f, 0.0f, _rotationZ) );

                _calledCounter = 0;
            }
        }
    }
}
