using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class OutlineWhenOccluded : MonoBehaviour
{
    [Header("Outline material")]
    public Material outlineMaterial;
    public Material maskMaterial;
    [ColorUsage(false, true)] public Color outlineColor = new Color(0.2f, 0.9f, 1f, 1f);
    [Range(0.001f, 0.1f)] public float outlineWidth = 0.02f;

    [Tooltip("AfterForwardAlpha(Forward) / AfterEverything(Deferred)")]
    public CameraEvent forwardEvent = CameraEvent.AfterForwardOpaque;
    public CameraEvent deferredEvent = CameraEvent.AfterGBuffer;

    Camera cam;
    CommandBuffer cb;
    readonly List<Renderer> renderers = new List<Renderer>();

    bool IsSRP => GraphicsSettings.currentRenderPipeline != null;

    void Awake()
    {
        cam = Camera.main;
        GetComponentsInChildren<Renderer>(true, renderers);
        renderers.RemoveAll(r => r is TrailRenderer || r is LineRenderer || r is ParticleSystemRenderer);
    }

    void OnEnable()
    {
        if (!cam)
        {
            Debug.LogWarning("[Outline] Camera.main을 찾지 못했습니다. 스크립트의 _cam을 직접 할당하세요.");
            return;
        }
        if (outlineMaterial == null)
        {
            Debug.LogWarning("[Outline] outlineMaterial이 비었습니다.");
            return;
        }

        cb = new CommandBuffer { name = "[Outline] Occluded Outline" };
        cam.AddCommandBuffer(GetCameraEventFor(cam), cb);

        // SRP면 SRP 콜백, Built-in이면 카메라 이벤트로 갱신
        if (IsSRP)
            RenderPipelineManager.beginCameraRendering += OnBeginCameraRenderingSRP;
        else
            Camera.onPreRender += OnPreRenderBuiltin; // Built-in에서 매 프레임 커맨드버퍼 채움
    }

    void OnDisable()
    {
        if (cb != null && cam != null)
        {
            cam.RemoveCommandBuffer(GetCameraEventFor(cam), cb);
            cb.Release();
            cb = null;
        }

        if (IsSRP)
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRenderingSRP;
        else
            Camera.onPreRender -= OnPreRenderBuiltin;
    }

    CameraEvent GetCameraEventFor(Camera cam)
    {
        var path = cam.actualRenderingPath;
        if (path == RenderingPath.DeferredShading)
            return deferredEvent;            // 기본 AfterGBuffer
        // Forward (또는 UseGraphicsSettings로 Forward가 되는 경우)
        return forwardEvent;                 // 기본 AfterForwardOpaque
    }

    // --- SRP용 갱신 ---
    void OnBeginCameraRenderingSRP(ScriptableRenderContext ctx, Camera ca)
    {
        if (ca != cam || cb == null) return;
        FillCommandBuffer();
    }

    // --- Built-in용 갱신 ---
    void OnPreRenderBuiltin(Camera ca)
    {
        if (ca != cam || cb == null) return;
        FillCommandBuffer();
    }

    void FillCommandBuffer()
    {
        cb.Clear();

        foreach (var r in renderers)
            if (r && r.enabled) cb.DrawRenderer(r, maskMaterial);

    
        outlineMaterial.SetColor("_OutlineColor", outlineColor);
        outlineMaterial.SetFloat("_OutlineWidth", outlineWidth);

        foreach (var r in renderers)
            if (r && r.enabled) cb.DrawRenderer(r, outlineMaterial);
    }
}