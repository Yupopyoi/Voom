// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UniVRM10;

using Mediapipe.Allocator;

namespace VRMController
{
    [System.Serializable]
    public struct RotationFreezeSettings
    {
        public bool x;
        public bool y;
        public bool z;

        public static RotationFreezeSettings AllTrue => new() { x = true, y = true, z = true };

        public readonly override string ToString()
        {
            return $"Rotation Freeze | x : {x} , y : {y} , z : {z}";
        }
    }

    public class VRMLoader : MonoBehaviour
    {
        private static GameObject _currentModel;
        private VRMAllocator _vrmAllocator;

        // After attaching the RigidBody dynamically, these values are applied.
        // The collider is also automatically set to a child object namedÅgBodyÅh.
        // This is only valid in 3D mode.
        [Header("Physical parameters (RigidBody)")]
        [Tooltip("Mass of model [kg]")]
        [SerializeField] private float _mass = 50.0f;

        [SerializeField] private float _drag = 0.0f;

        [SerializeField] private float _angularDrag = 0.05f;

        [SerializeField] private RigidbodyInterpolation _interpolation = RigidbodyInterpolation.None;

        [SerializeField] private RotationFreezeSettings _freezeRotation = RotationFreezeSettings.AllTrue;

        [Header("Body Collider (Capsule Collider)")]
        [Tooltip("This value varies by model. " +
                 "If necessary, it is better to change it while looking at the Scene view. " +
                 "However, for most models, a value around 0.2 is fine.")]
        [SerializeField] private float _radius = 0.2f;

        [Header("Other Settings")]
        [Tooltip("Specifies the height of spawn. " +
                 "This does not particularly affect the functionality, " +
                 "but it is visually enjoyable because hair and other swinging objects will sway when spawned.")]
        [SerializeField, Range(0.0f, 1.0f)] float _spawnHeight = 0.0f;

        private void Start()
        {
            _vrmAllocator = GetComponent<VRMAllocator>();
        }

        public async Task LoadVRM(string path, string modelName = "VRM1")
        {
            // Limit simultaneous output of models to one person.
            if (_currentModel != null)
            {
                _currentModel.SetActive(false);

                Destroy(_currentModel, 0.2f);
                _currentModel = null;
            }

            byte[] vrmBytes = File.ReadAllBytes(path);

            var instance = await Vrm10.LoadBytesAsync(vrmBytes);

            _currentModel = instance.gameObject;
            _currentModel.transform.position = Vector3.zero;

            _currentModel.name = modelName;
             
            AttachComponents();

            _vrmAllocator.Allocate();
        }

        // Attach Collider and Rigidbody
        private void AttachComponents()
        {
            OperationDimension operationDimension = _vrmAllocator.OperationDimension;

            if (operationDimension == OperationDimension.ThreeDimension)
            {
                bool isColliderAttached = AttachCollider();

                if (isColliderAttached)
                {
                    AttachRigidbody();
                }

                _currentModel.transform.position = new Vector3(0.0f, _spawnHeight, 0.0f);
            }
            else /* operationDimension == OperationDimension.TwoDimension */
            {
                // Nothing to do for now.
            }
        }

        private bool AttachCollider()
        {
            Transform bodyTransform = _currentModel.transform.Find("Body");

            if (bodyTransform != null)
            {
                GameObject body = bodyTransform.gameObject;
                if (body.GetComponent<CapsuleCollider>() == null)
                {
                    body.AddComponent<CapsuleCollider>();
                }

                CapsuleCollider capsuleCollider = body.GetComponent<CapsuleCollider>();

                capsuleCollider.radius = _radius;

                return true;
            }
            else
            {
                Debug.LogWarning("Body object not found under this VRM Model.");
                return false;
            }
        }

        private void AttachRigidbody()
        {
            if (_currentModel.GetComponent<Rigidbody>() == null)
            {
                _currentModel.AddComponent<Rigidbody>();
            }

            Rigidbody rb = _currentModel.GetComponent<Rigidbody>();

            rb.mass = _mass;
            rb.linearDamping = _drag;
            rb.angularDamping = _angularDrag;
            rb.interpolation = _interpolation;

            #region FreezeRotation

            RigidbodyConstraints constraints = RigidbodyConstraints.None;

            if (_freezeRotation.x) constraints |= RigidbodyConstraints.FreezeRotationX;
            if (_freezeRotation.y) constraints |= RigidbodyConstraints.FreezeRotationY;
            if (_freezeRotation.z) constraints |= RigidbodyConstraints.FreezeRotationZ;

            rb.constraints = constraints;

            #endregion
        }
    }
}// namespace VRMController
