// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;
using System.Collections.Generic;
using System.IO;

using VRMController;

public class VRMSelectDropdownHandler : TMPDropdownHandlerBase
{
    readonly List<string> _modelNames = new();

    [Header("Avoke")]
    [SerializeField] VRMLoader _vrmLoader;

    string _folderPath;

    protected override void Awake()
    {
        base.Awake();

        _modelNames.Clear();
        _modelNames.Add("Select VRM Model");

        _folderPath = Path.Combine(Application.streamingAssetsPath, "VRMModel");
        string[] files = Directory.GetFiles(_folderPath, "*.vrm");

        foreach (var file in files)
        {
            string filename = Path.GetFileNameWithoutExtension(file);
            _modelNames.Add(filename);
        }

        SetOptions(_modelNames, _initialIndex);
        SetInitialIndex();
    }

    async protected override void OnChangedValue(int index)
    {
        if (index <= 0) return;

        await _vrmLoader.LoadVRM(Path.Combine(_folderPath, _modelNames[index] + ".vrm"), _modelNames[index]);
    }
}
