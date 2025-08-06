// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class GameLoggerTarget : MonoBehaviour
{
    private TextMeshProUGUI textComponent;

    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        textComponent.text = "";
        GameLogger.RegisterTarget(textComponent);
    }

    void OnDestroy()
    {
        GameLogger.UnregisterTarget(textComponent);
    }
}
