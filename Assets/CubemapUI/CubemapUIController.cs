using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

[ExecuteAlways]
public class CubemapUIController : MonoBehaviour
{
    public RenderTexture frontRT;

    private void Awake()
    {
        GetComponent<PanelRenderer>()
            .RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDestroy()
    {
        GetComponent<PanelRenderer>()
            .UnregisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(
        PanelRenderer panelRenderer,
        VisualElement root,
        int version)
    {
        // RenderTexture割り当て
        root.Q<Image>("cam-front").image = frontRT;
        Debug.Log("OnUIReload");
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        EditorApplication.delayCall += ForceInit;
    }

    private void OnDisable()
    {
        EditorApplication.delayCall -= ForceInit;
    }

    private void ForceInit()
    {
        var pr = GetComponent<PanelRenderer>();

        pr.UnregisterUIReloadCallback(OnUIReload);
        pr.RegisterUIReloadCallback(OnUIReload);
    }
#endif
}