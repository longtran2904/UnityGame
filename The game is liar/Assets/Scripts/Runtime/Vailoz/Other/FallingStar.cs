using System.Collections;
using System.Collections.Generic;
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
    public Rigidbody2D meteor;
    [MinMax(1, 20)] public RangedFloat speed;
    [MinMax(0, 30)] public RangedFloat timeBtwMeteors;
    public float lifetime;

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
            image.texture = renderTex;
            image.canvas.worldCamera = camera;

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
            Vector3 pos = MathUtils.RandomVector2(sr.bounds.extents, sr.bounds.extents * 2);
            Vector2 dir = -MathUtils.MakeVector2(Random.Range(30, 60));
            if (MathUtils.RandomBool())
            {
                pos.x = -pos.x;
                dir.x = -dir.x;
            }
            Rigidbody2D meteorRb = Instantiate(meteor, pos + sr.bounds.center, Quaternion.LookRotation(dir, Vector3.up));
            TrailRenderer trail = meteorRb.GetComponent<TrailRenderer>();
            float meteorWidth = transform.lossyScale.x / sr.sharedMaterial.GetFloat("_PixelateAmount") * 2;
            trail.startWidth = meteorWidth;
            meteorRb.velocity = speed.randomValue * dir;
            yield return new WaitForSeconds(lifetime);
            Destroy(meteorRb.gameObject);
        }
    }
}