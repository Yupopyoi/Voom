// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;

namespace Mediapipe.Allocator
{
    public class MouthAdapter : EmotionAdapterBase
    {
        public MouthAdapter(GameObject faceObject, LandmarksPacket landmarksPacket)
            : base(faceObject, landmarksPacket) { }

        public float VerticalOpenScale { get; set; } = 1.5f;

        /*  [Landmark Index]

            | Index | MP Index |              Part             |
            |:-----:|:--------:|:-----------------------------:|
            |   0   |    13    |         Upper Central         |
            |   1   |    14    |         Lower Central         |
            |   2   |   306    |           Left  edge          |
            |   3   |    78    |           Right edge          |
            |   4   |   311    |       Upper Left  Middle      |
            |   5   |    81    |       Upper Right Middle      |
            |   6   |   402    |       Lower Left  Middle      |
            |   7   |   178    |       Lower Right Middle      |
            |:-----:|:--------:|:-----------------------------:|
            |   8   |   468    |           Right Eye           |
            |   9   |   473    |           Left  Eye           |
         */

        public override void ForwardApply()
        {
            Vector3 binocularVector = Landmark(8) - Landmark(9);
            float binocularDistance = PlaneDistance(binocularVector);

            Vector3 verticalMouthVector = Landmark(0) - Landmark(1);
            float verticalMouthLength = PlaneDistance(verticalMouthVector);

            _skinnedMeshRenderer.SetBlendShapeWeight(33, BindControlValue(verticalMouthLength / binocularDistance, VerticalOpenScale));
        }

    }
}// namespace Mediapipe.Allocator
