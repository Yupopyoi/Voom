// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UniVRM10;

namespace Mediapipe.Allocator
{
    public struct Rotation
    {
        public float x;
        public float y;
        public float z;

        public Rotation(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public readonly Vector3 ToVector3 => new(x, y, z);

        public override readonly string ToString()
        {
            return $"Rotation (x: {x}, y: {y}, z: {z})";
        }
    }

    public struct LandmarksPacket
    {
        public List<Tasks.Components.Containers.NormalizedLandmark> Landmarks { get; private set; }
        public int Capacity { get; private set; }
        private readonly int[] Indexes;

        public LandmarksPacket(List<Tasks.Components.Containers.NormalizedLandmark> landmarks, int[] indexes)
        {
            Landmarks = landmarks;
            Indexes = indexes;
            Capacity = Indexes.Length;
        }

        public readonly Tasks.Components.Containers.NormalizedLandmark GetLandmark(int index)
        {
            return Landmarks[Indexes[index]];
        }
    }

    public abstract class AdaptationManagerBase<T> : MonoBehaviour
    {
        protected static GameObject _vrmObject;

        protected List<Tasks.Components.Containers.NormalizedLandmark> _landmarks;

        protected virtual void Start()
        {
            if(_vrmObject.IsUnityNull())
            {
                FindVRM();
            }
        }

        private void FindVRM()
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
        }

        protected void GenerateLandmarksList(int length)
        {
            _landmarks = Enumerable.Repeat(new Tasks.Components.Containers.NormalizedLandmark(), length).ToList();
        }

        protected GameObject FindChildByName(string name)
        {
            List<GameObject> _allChildren = new();
            GetAllChildren(_vrmObject.transform, _allChildren);

            return _allChildren.FirstOrDefault(obj => obj.name.Contains(name));
        }

        private void GetAllChildren(Transform parent, List<GameObject> list)
        {
            foreach (Transform child in parent)
            {
                list.Add(child.gameObject);
                GetAllChildren(child, list);
            }
        }

        public abstract void ApplyMediapipeResult(T recognitionResult);
    }

} // namespace Mediapipe.Allocator
