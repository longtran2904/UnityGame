using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class FallingStar : MonoBehaviour
{
    [Header("Shader")]
    [MinMax(50, 1000)] public RangedFloat noiseScale;
    [MinMax(100, 800)] public RangedFloat minTimeNoiseScale;
    [MinMax(100, 800)] public RangedFloat maxTimeNoiseScale;

    [Header("Meteor")]
    public bool disableMeteor;
    [MinMax(1, 20)] public RangedFloat speed;
    [MinMax(0, 30)] public RangedFloat timeBtwMeteors;

    [Header("Render Texture")]
    public Vector2Int size;

    private SpriteRenderer sr;
    private RenderTexture renderTex;
    private RawImage image;
    private RectTransform canvas;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        canvas = GetComponentInChildren<RectTransform>();

        // NOTE: The flickering bloom bug happens because Unity URP's bloom effect works on a half resolution
        // Increasing it to full-res and the render scale to 1.5 fix the problem
        GameUtils.SetMaterialBlock(sr, block =>
        {
            block.SetFloat("_NoiseScale", noiseScale.randomValue);
            block.SetFloat("_MinTimeNoiseScale", minTimeNoiseScale.randomValue);
            block.SetFloat("_MaxTimeNoiseScale", maxTimeNoiseScale.randomValue);
            block.SetFloat("_Ratio", sr.size.x / sr.size.y);
        });


        if (Application.isPlaying)
        {
            // NOTE: DefaultHDR is chosen based on the platform, so it may not always have bloom effect. If it turn out to be the case then use RGB111110Float.
            // https://forum.unity.com/threads/camera-render-texture-with-post-processing.817674/#post-6777809
            renderTex = new RenderTexture((int)(size.x * sr.size.x), (int)(size.y * sr.size.y), 0, RenderTextureFormat.DefaultHDR);
            renderTex.Create();

            Camera camera = new GameObject("Window Camera", typeof(Camera)).GetComponent<Camera>();
            camera.transform.parent = transform;
            camera.transform.localPosition = new Vector3(0, 0, -10);
            camera.orthographic = true;
            camera.aspect = (sr.size.x * transform.localScale.x) / (sr.size.y * transform.localScale.y);
            camera.orthographicSize = (sr.size.y * transform.localScale.y) / 2;
            camera.cullingMask = LayerMask.GetMask("Meteor");
            camera.targetTexture = renderTex;

            image = GetComponentInChildren<RawImage>();
            if (image)
            {
                image.texture = renderTex;
                image.canvas.worldCamera = camera;
            }

            if (!disableMeteor)
                StartCoroutine(SpawnMeteor());
        }
    }

#if UNITY_EDITOR
    private void Update()
    {
        canvas.sizeDelta = sr.size;
        GameUtils.SetMaterialBlock(sr, block => block.SetFloat("_Ratio", sr.size.x / sr.size.y));
    }
#endif

    IEnumerator SpawnMeteor()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBtwMeteors.randomValue);
            Vector3 pos = (Vector3)MathUtils.RandomPoint(new Vector2(-sr.bounds.size.x, sr.bounds.extents.y), sr.bounds.size) + sr.bounds.center;
            RangedFloat rot = new RangedFloat(pos.x < sr.bounds.min.x ? 280f : 210f, pos.x > sr.bounds.max.x ? 260f : 330f);

            Entity meteor = ObjectPooler.Spawn<Entity>(PoolType.Meteor, pos);
            meteor.transform.right = MathUtils.MakeVector2(rot.randomValue);
            meteor.speed = speed.randomValue;
            meteor.GetComponent<TrailRenderer>().startWidth = transform.lossyScale.x / sr.sharedMaterial.GetFloat("_PixelateAmount") * 2;
        }
    }
}