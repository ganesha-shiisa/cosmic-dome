using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
class EditorCameraMover : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float lookSpeed = 0.2f;

    private float yaw = 0f;
    private float pitch = 0f;

    void OnEnable()
    {
        // 現在のカメラの向きを初期値として取得
        Vector3 angles = Vector3.zero;
        transform.eulerAngles = angles;
        yaw = angles.y;
        pitch = angles.x;
    }

    void Update()
    {
        // Playモード中ではなく、エディタ編集中（Editモード）のときの処理
        if (!Application.isPlaying)
        {
            // キーボードが接続されていなければ何もしない
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            bool isUpdated = false;

            // マウスによる視点回転（右クリック押しっぱなし中）
            if (mouse != null && mouse.rightButton.isPressed)
            {
                Vector2 delta = mouse.delta.ReadValue();
                if (delta != Vector2.zero)
                {
                    yaw += delta.x * lookSpeed;
                    pitch -= delta.y * lookSpeed;
                    // 真上・真下を向きすぎて反転するのを防止
                    pitch = Mathf.Clamp(pitch, -89f, 89f);

                    transform.eulerAngles = new Vector3(pitch, yaw, 0f);
                    isUpdated = true;
                }
            }

            if (isUpdated)
            {
#if UNITY_EDITOR
                if (SceneView.lastActiveSceneView != null)
                {
                    SceneView.lastActiveSceneView.Repaint();
                }
                EditorApplication.QueuePlayerLoopUpdate();
#endif
            }
        }
    }
}