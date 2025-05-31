# Struct HandLandmarkerResult

## public readonly List\<Classifications> handedness

> Classification of handedness.  

右手か左手かを分類します。

Example of output by Debug.Log()  
Debug.Log() による出力例  

```txt
{ "categories": [{ "index": 0, "score": 0.9289777, "categoryName": "Right", "displayName": "Right" }], "headIndex": 0, "headName": null }
```

The length of the List depends on the number of moves detected, or null if no hands are detected.  
リストの長さは検出された手の数によって変わります。もし手が検出されなかった場合は、このリストはnullになります。  

For example, to check whether the detected hand is right or left, you can write the following to HandLandmarkerResultAnnotationController.SyncNow()  
例えば、検出された手が右手か左手かを確認したい場合は、次の文をHandLandmarkerResultAnnotationController.SyncNow() に書き加えることで実現できます。  

```csharp
if(_currentTarget.handedness?.Count > 0)
{
    Debug.Log(_currentTarget.handedness[0].categories[0].categoryName);
}
```

### public readonly struct Classifications

#### List\<Category> categories

> The array of predicted categories, usually sorted by descending scores

##### public readonly int index

> The index of the category in the classification model output.

##### public readonly float score

> The score for this category, e.g. (but not necessarily) a probability in [0,1].

##### public readonly string categoryName

> The optional ID for the category, read from the label map packed in the TFLite Model Metadata if present.

## public readonly List\<NormalizedLandmarks> handLandmarks

> Detected hand landmarks in normalized image coordinates.

検出された手のランドマークの座標（正規化済み）です。  

The length of the List depends on the number of moves detected, or null if no hands are detected.  
リストの長さは検出された手の数によって変わります。もし手が検出されなかった場合は、このリストはnullになります。  

This list holds the normalized xyz coordinates of hands landmarks.  
このリストは（複数の）手のランドマークのxyz座標を正規化した上で保持しています。  

There are 21 hand landmarks, each as follows.  
手のランドマークは 21 か所あり、それぞれ次の通りです。

Google has published this information with images on [this page](https://chuoling.github.io/mediapipe/solutions/hands.html).  
この情報については、Googleが [このページ](https://chuoling.github.io/mediapipe/solutions/hands.html) で画像付きで公開しています。

| Index |    Description    |       説明　　 　|
|:------|------------------:|:---------------:|
|    0  | Wrist             |　　 手首    　　 |
|    1  | Thumb CMC         | 　　親指  CM関節 |
|    2  | Thumb MCP         | 　　親指 MCP関節 |
|    3  | Thumb  IP         | 　　親指  IP関節 |
|    4  | Thumb TIP         | 　　親指    先端 |
|    5  | Index Finger MCP  | 人差し指  MP関節 |
|    6  | Index Finger PIP  | 人差し指 PIP関節 |
|    7  | Index Finger DIP  | 人差し指 DIP関節 |
|    8  | Index Finger TIP  | 人差し指    先端 |
|    9  | Middle Finger MCP | 　　中指  MP関節 |
|   10  | Middle Finger PIP | 　　中指 PIP関節 |
|   11  | Middle Finger DIP | 　　中指 DIP関節 |
|   12  | Middle Finger TIP | 　　中指    先端 |
|   13  | Ring Finger MCP   | 　　薬指  MP関節 |
|   14  | Ring Finger PIP   | 　　薬指 PIP関節 |
|   15  | Ring Finger DIP   | 　　薬指 DIP関節 |
|   16  | Ring Finger TIP   | 　　薬指    先端 |
|   17  | Pinky MCP         | 　　子指  MP関節 |
|   18  | Pinky PIP         | 　　子指 PIP関節 |
|   19  | Pinky DIP         | 　　子指 DIP関節 |
|   20  | Pinky TIP         | 　　子指    先端 |

For example, if you want to check the coordinates of the wrist and the tip of the index finger, you can write like this.  
例えば、手首と人差し指の先端の座標を確認したい場合はこのように書けます。

```csharp
if(_currentTarget.handLandmarks?.Count > 0)
{
    Debug.Log(_currentTarget.handLandmarks[0].landmarks[0]); // landmarks[0] = Wrist
    Debug.Log(_currentTarget.handLandmarks[0].landmarks[8]); // landmarks[8] = Index Finger TIP
}
```

If each landmark is considered to be outside the screen, the value will be outside the interval [0,1].  
もし各ランドマークが画面の外にあると考えられる場合、 [0,1]の区間の外の値となります。

## public readonly List\<Landmarks> handWorldLandmarks

> Detected hand landmarks in world coordinates.

検出された手のランドマークの座標（ワールド座標）です。  

The explanation is omitted because it is almost the same as for handLandmarks.  
handLandmarksとほぼ同じであるため、解説は省略します。　　
