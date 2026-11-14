using UnityEngine;
using UnityEditor;
using System.IO;

public class CubemapExporter : EditorWindow
{
    private int imageSize = 2048; // 書き出し解像度
    private Transform targetPosition; // 撮影基準となるオブジェクト
    private float simulationTime = 2.0f; // パーティクルを馴染ませるためのシミュレーション時間（秒）

    [MenuItem("Tools/Cosmic Dome/Cubemap Exporter")]
    public static void ShowWindow()
    {
        GetWindow<CubemapExporter>("Cubemap Exporter");
    }

    private void OnGUI()
    {
        GUILayout.Label("360度 Cubemap (6面PNG) 書き出しツール", EditorStyles.boldLabel);

        imageSize = EditorGUILayout.IntField("Image Size (px)", imageSize);
        targetPosition = (Transform)EditorGUILayout.ObjectField("Target Transform", targetPosition, typeof(Transform), true);
        simulationTime = EditorGUILayout.FloatField("Simulation Time (sec)", simulationTime);

        GUILayout.Space(10);

        if (GUILayout.Button("6面PNGを書き出す！"))
        {
            ExportCubemapFaces();
        }
    }

    private void ExportCubemapFaces()
    {
        // シーン内のすべてのParticleSystemを事前にシミュレーションして進めておく
        ParticleSystem[] particleSystems = FindObjectsByType<ParticleSystem>(FindObjectsInactive.Include);
        foreach (var ps in particleSystems)
        {
            ps.Simulate(simulationTime, true, true);
            ps.Pause();
        }

        // 一時的なカメラ用GameObjectを作成
        GameObject camObj = new GameObject("TempCubemapCamera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.fieldOfView = 90f;
        cam.aspect = 1f;

        // 背景を「黒の単色」に指定してスカイボックスの写り込みを防止
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;

        // 撮影位置の設定
        Vector3 pos = targetPosition != null ? targetPosition.position : Vector3.zero;
        camObj.transform.position = pos;

        string[] faces = { "Front", "Back", "Left", "Right", "Up", "Down" };
        Quaternion[] rotations = {
            Quaternion.Euler(0, 0, 0),     // Front (+Z)
            Quaternion.Euler(0, 180, 0),   // Back (-Z)
            Quaternion.Euler(0, -90, 0),   // Left (-X)
            Quaternion.Euler(0, 90, 0),    // Right (+X)
            Quaternion.Euler(-90, 0, 0),   // Up (+Y)
            Quaternion.Euler(90, 0, 0)     // Down (-Y)
        };

        RenderTexture rt = new RenderTexture(imageSize, imageSize, 24);
        rt.format = RenderTextureFormat.ARGB32;
        cam.targetTexture = rt;

        string folderPath = "Assets/ExportedCubemaps";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        for (int i = 0; i < 6; i++)
        {
            camObj.transform.rotation = rotations[i];
            cam.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(imageSize, imageSize, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, imageSize, imageSize), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            byte[] bytes = tex.EncodeToPNG();
            string fileName = $"{folderPath}/Skybox_{faces[i]}.png";
            File.WriteAllBytes(fileName, bytes);

            Object.DestroyImmediate(tex);
            Debug.Log($"[Exported] {fileName}");
        }

        rt.Release();
        Object.DestroyImmediate(camObj);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("完了だ、相棒！", "背景が黒く、パーティクルもしっかり展開された6面テクスチャが書き出されたぜ！", "OK");
    }
}