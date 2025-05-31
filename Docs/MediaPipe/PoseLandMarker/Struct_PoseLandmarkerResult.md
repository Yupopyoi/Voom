# Struct PoseLandmarkerResult

## public readonly List\<NormalizedLandmarks> poseLandmarks

> Detected pose landmarks in normalized image coordinates.

検出された身体のランドマークの座標（正規化済み）です。  

The length of the List depends on the number of people detected, or null if no people are detected.  
リストの長さは検出された人の数によって変わります。もし手が検出されなかった場合は、このリストはnullになります。  

This list holds the normalized xyz coordinates of hands landmarks.  
このリストは（複数の）人のランドマークのxyz座標を正規化した上で保持しています。  

There are 33 hand landmarks, each as follows.  
手のランドマークは 33 か所あり、それぞれ次の通りです。

Google has published this information with images on [this page](https://chuoling.github.io/mediapipe/solutions/pose.html).  
この情報については、Googleが [このページ](https://chuoling.github.io/mediapipe/solutions/pose.html) で画像付きで公開しています。

| Index |    Description    |     説明　 　|     VRM Name     |
|:------|------------------:|:-----------:|:----------------:|
|    0  | nose              |　　 鼻　　　 |                  |
|    1  | left  eye inner   | 左目　内　　 |                  |
|    2  | left  eye         | 左目　　　　 |                  |
|    3  | left  eye outer   | 左目　外　　 |                  |
|    4  | right eye inner   | 右目　内　　 |                  |
|    5  | right eye         | 右目　　　　 |                  |
|    6  | right eye outer   | 右目　外　　 |                  |
|    7  | left  ear         | 左耳 　　　　|                  |
|    8  | right ear         | 右耳　　　　 |                  |
|    9  | mouth left        | 口　左端　　 |                  |
|   10  | mouth right       | 口　右端　　 |                  |
|   11  | left  shoulder    | 左肩　　　　 | J_Bip_L_Shoulder |
|   12  | right shoulder    | 右肩　　　　 | J_Bip_R_Shoulder |
|   13  | left  elbow       | 左肘　　　　 | J_Bip_L_LowerArm |
|   14  | right elbow       | 右肘　　　　 | J_Bip_R_LowerArm |
|   15  | left  wrist       | 左手首　　　 | J_Bip_L_Hand     |
|   16  | right wrist       | 右手首　　　 | J_Bip_R_Hand     |
|   17  | left  pinky       | 左手小指　　 |  *- Not used -*  |
|   18  | right pinky       | 右手小指　　 |  *- Not used -*  |
|   19  | left  index       | 左手人差し指 |  *- Not used -*  |
|   20  | right index       | 右手人差し指 |  *- Not used -*  |
|   21  | left  thumb       | 左手親指 　　|  *- Not used -*  |
|   22  | right thumb       | 右手親指 　　|  *- Not used -*  |
|   23  | left  hip         | 左尻 　　　　| J_Bip_L_UpperLeg |
|   24  | right hip         | 右尻 　　　　| J_Bip_R_UpperLeg |
|   25  | left  knee        | 左膝 　　　　| J_Bip_L_LowerLeg |
|   26  | right knee        | 右膝 　　　　| J_Bip_R_LowerLeg |
|   27  | left  ankle       | 左足首　　　 | J_Bip_L_Foot     |
|   28  | right ankle       | 右足首　　　 | J_Bip_R_Foot     |
|   29  | left  heel        | 左踵 　　　　|  *- Not used -*  |
|   30  | right heel        | 右踵 　　　　|  *- Not used -*  |
|   31  | left  foot index  | 左足指 　　　| J_Bip_L_ToeBase  |
|   32  | right foot index  | 右足指 　　　| J_Bip_R_ToeBase  |

For example, if you want to check the coordinates of the your nose, you can write like this.  
例えば、鼻の座標を確認したい場合はこのように書けます。

```csharp
if(_currentTarget.poseLandmarks?.Count > 0)
{
　Debug.Log(_currentTarget.poseLandmarks[0].landmarks[0]); // landmarks[0] = nose
}
```

If each landmark is considered to be outside the screen, the value will be outside the interval [0,1].  
もし各ランドマークが画面の外にあると考えられる場合、 [0,1]の区間の外の値となります。

## public readonly List\<Landmarks> poseWorldLandmarks

> Detected pose landmarks in world coordinates.

検出された身体のランドマークの座標（ワールド座標）です。  

The explanation is omitted because it is almost the same as for poseLandmarks.  
poseLandmarksとほぼ同じであるため、解説は省略します。　　
