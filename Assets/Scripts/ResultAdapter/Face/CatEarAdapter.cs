// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;
using System.Collections.ObjectModel;

namespace Mediapipe.Allocator
{
    [CreateAssetMenu(menuName = "Emotion/CatEarParams", fileName = "CatEarParams")]
    public class CatEarParams : ScriptableObject, IAdapterParams
    {
        [Header("Overall")]
        [Tooltip("Whether the cat ears move spontaneously in response to facial expressions.\n" +
                 "There is still passive ear movement in response to body movement.")]
        public bool CanMoveResponseToExpression = true;

        [Header("Growth")]
        [Tooltip("Amount of ear growth when looking surprised (Greater than 1)")]
        [Range(1f, 2f)] public float GrowthAmount = 1.10f;

        [Header("Automatically Movements")]
        [Tooltip("Settings for whether or not to make minute movements automatically.\n" +
                 "This movement is not affected by the detection results of MediaPipe.\n" +
                 "When this is set to true, the ears move on their own and look cute.")]
        public bool MoveMinuteMovementsAutomatically = true;

        [Tooltip("Frequency of automatic updates.\n" +
                 "This is defined by the number of times ForwardApply() is called, " +
                 "so the interval (in seconds) varies depending on the execution environment.")]
        [Range(0, 30)] public int FrequencyOfAutomaticUpdates = 6;

        [Tooltip("Specify the range of angles [deg] that can be changed.It is recommended not to set this value too large.")]
        [Range(0f, 20f)] public float AutomaticOperationSize = 3.0f;

        public void ResetToDefaults()
        {
            CanMoveResponseToExpression = true;

            GrowthAmount = 1.10f;

            MoveMinuteMovementsAutomatically = true;

            FrequencyOfAutomaticUpdates = 6;
            AutomaticOperationSize = 3.0f;
        }
    }

    public class CatEarAdapter : EmotionAdapterBase
    {
        private readonly ReadOnlyCollection<float> _eyeControlValues;

        private readonly Transform _transformEarR;
        private readonly Transform _transformEarL;

        private readonly bool _haveCatEar = false;

        private readonly Vector3 _initialRotationCatEarR;
        private readonly Vector3 _initialRotationCatEarL;
        private readonly float _centerRotationZ;

        private CatEarParams _prms;

        public CatEarAdapter(GameObject earObjectR, GameObject earObjectL, LandmarksPacket landmarksPacket, ReadOnlyCollection<float> eyeControlValues, CatEarParams catEarParams = null)
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

                if (catEarParams == null)
                {
                    _prms = ScriptableObject.CreateInstance<CatEarParams>();
                }
                else
                {
                    _prms = catEarParams;
                }
            }
        }

        public override void SetParameter(IAdapterParams catEarParams)
        {
            _prms = (CatEarParams)catEarParams;
        }

        int _calledCounter = 0;

        public override void ForwardApply()
        {
            if (!_haveCatEar) return;

            if (!_prms.CanMoveResponseToExpression) return;

            float surpriseValue = Sigmoid((_eyeControlValues[4] + _eyeControlValues[5]) * 0.5f, 0.3f) * 0.01f; // Average & [0,100] -> [0,1]

            float growthValue = surpriseValue * _prms.GrowthAmount > 1.0f ? surpriseValue * _prms.GrowthAmount : 1.0f;

            _transformEarR.localScale = new Vector3(1.0f, growthValue, 1.0f);
            _transformEarL.localScale = new Vector3(1.0f, growthValue, 1.0f);

            if (!_prms.MoveMinuteMovementsAutomatically) return;

            if(_calledCounter++ > _prms.FrequencyOfAutomaticUpdates)
            {
                float _rotationZ = Random.Range(_centerRotationZ - _prms.AutomaticOperationSize, _centerRotationZ + _prms.AutomaticOperationSize);

                _transformEarR.localRotation = Quaternion.Euler( new Vector3(0.0f, 0.0f, -_rotationZ) );
                _transformEarL.localRotation = Quaternion.Euler( new Vector3(0.0f, 0.0f, _rotationZ) );

                _calledCounter = 0;
            }
        }
    }
}// namespace Mediapipe.Allocator
