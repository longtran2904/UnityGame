using System.Collections;
using UnityEngine;

public enum ParticleType
{
    None,
    Explosion,
}

public class ParticleEffect : MonoBehaviour
{
    private struct ParticleCircle
    {
        public float radiusTime;
        public float thicknessTime;
        public float fadeOutTime;
        public float speed;
        public float timer;

        public ParticleCircle(float radiusTime, float thicknessTime, float fadeOutTime, float speed)
        {
            this.radiusTime = radiusTime;
            this.thicknessTime = thicknessTime;
            this.fadeOutTime = fadeOutTime;
            this.speed = speed;
            timer = 1;
        }
    }

    private enum CircleType
    {
        Static_Filled,
        Scale_Radius_Filled,
        Scale_Radius_Outline,
        Scale_Thickness,
    }

    private const int MAX_PARTICLES = 64;
    private ParticleCircle[] particles = new ParticleCircle[MAX_PARTICLES];
    private int particleCount;

    public float[] radiusScales = new float[]
    {
        1f,
        .5f,
        .75f,
        1f
    };

    public float[] radiusTimes = new float[]
    {
        0f,
        1f,
        -.5f,
        0f
    };

    public float[] thicknessValues = new float[]
    {
        1f,
        1f,
        .075f,
        .75f
    };

    public float[] thicknessTimes = new float[]
    {
        .0f,
        .0f,
        .0f,
        .5f
    };

    [ColorUsage(true, true)] public Color baseColor;
    [ColorUsage(true, true)] public Color ringColor;
    [ColorUsage(true, true)] public Color lightColor;
    [Range(0, 1)] public float centerAlpha;
    [Range(0, 1)] public float secondaryAlpha;
    [Range(0, 3)] public float speedModifier = 1f;

    public SpriteRenderer prefab;
    public GameObject explodeEffect;

    private SpriteRenderer[] renderers = new SpriteRenderer[MAX_PARTICLES];
    private MaterialPropertyBlock block;
    private int radiusID;
    private int radiusTimerID;
    private int thicknessID;
    private int thicknessTimerID;
    private int fadeOutTimerID;
    private int colorID;

    float FrameToTime(int frame)
    {
        return 1f / 60f * frame;
    }

    float LifeTimeToSpeed(float time)
    {
        return 1f / time;
    }

    // Start is called before the first frame update
    void Start()
    {
        explodeEffect = Instantiate(explodeEffect, Vector3.zero, Quaternion.identity);
        explodeEffect.SetActive(false);

        for (int i = 0; i < MAX_PARTICLES; i++)
        {
            renderers[i] = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            renderers[i].gameObject.SetActive(false);
        }
        block = new MaterialPropertyBlock();

        radiusID = Shader.PropertyToID("_Radius");
        radiusTimerID = Shader.PropertyToID("_RadiusTimer");
        thicknessID = Shader.PropertyToID("_Thickness");
        thicknessTimerID = Shader.PropertyToID("_ThicknessTimer");
        fadeOutTimerID = Shader.PropertyToID("_FadeOutTimer");
        colorID = Shader.PropertyToID("_Color");
    }

    // Update is called once per frame
    void Update()
    {
        if (GameInput.GetInput(InputType.Debug_X))
            SpawnParticle(ParticleType.Explosion, Vector2.zero, 1);

        for (int i = 0; i < particleCount; ++i)
        {
            bool empty = false;
            while (particles[i].timer < 0)
            {
                renderers[i].gameObject.SetActive(false);

                if (particleCount == i)
                {
                    empty = true;
                    break;
                }
                particles[i] = particles[--particleCount];
                MathUtils.Swap(ref renderers[i], ref renderers[particleCount]);
            }

            if (empty)
                break;

            renderers[i].GetPropertyBlock(block);

            {
                if (particles[i].radiusTime < 0)
                {
                    if (particles[i].timer >= 1 + particles[i].radiusTime)
                        block.SetFloat(radiusTimerID, (1 - particles[i].timer) / -particles[i].radiusTime);
                }
                else if (particles[i].timer <= particles[i].radiusTime)
                    block.SetFloat(radiusTimerID, (particles[i].radiusTime - particles[i].timer) / particles[i].radiusTime);
            }
            if (particles[i].timer <= particles[i].thicknessTime)
                block.SetFloat(thicknessTimerID, (particles[i].thicknessTime - particles[i].timer) / particles[i].thicknessTime);
            if (particles[i].timer <= particles[i].fadeOutTime)
                block.SetFloat(fadeOutTimerID, (particles[i].fadeOutTime - particles[i].timer) / particles[i].fadeOutTime);

            renderers[i].SetPropertyBlock(block);
            particles[i].timer -= Time.deltaTime * particles[i].speed * speedModifier;
        }
    }

    public void SpawnParticle(ParticleType type, Vector2 pos, float range)
    {
        if (particleCount >= MAX_PARTICLES)
        {
            Debug.LogError("Particle count is bigger than MAX_PARTICLES");
            return;
        }

        switch (type)
        {
            case ParticleType.Explosion:
                {
                    StartCoroutine(SpawnExplosion(pos, range));
                } return;
        }
    }

    IEnumerator SpawnExplosion(Vector2 pos, float range)
    {
        explodeEffect.SetActive(true);
        explodeEffect.transform.localScale = new Vector3(range, range, 1);
        explodeEffect.transform.position = new Vector3(pos.x, pos.y, -10);

        Color color = baseColor;

        // Step 1
        CreateCircle(pos, range, .5f, color, CircleType.Static_Filled, 1, LifeTimeToSpeed(FrameToTime(16))); // 8 frames (solid) - 8 frames (fade)
        CreateCircle(pos, range, .5f, ringColor, CircleType.Scale_Radius_Outline, 0, LifeTimeToSpeed(FrameToTime(16))); // 8 frames (zoom) - 8 frames (stop and fade)
        //DrawSprite(explodeVFX, FrameToTime(2));
        //PlayChargingParticle(FrameToTime(2 + 4 + 2 + 4 + 18);

        yield return new WaitForSeconds(FrameToTime(2));

        // Step 2
        // TODO: Replace this with a point light
        CreateCircle(pos, 1, 0, lightColor, CircleType.Static_Filled, 3, LifeTimeToSpeed(FrameToTime(6)));

        yield return new WaitForSeconds(FrameToTime(4));

        // Step 3
        Vector2 randomOffset = MathUtils.RandomPointInRange(range / 2);
        color.a = secondaryAlpha;
        CreateCircle(pos + randomOffset, range / 2, .8f, color, CircleType.Scale_Radius_Filled, 2, LifeTimeToSpeed(FrameToTime(12))); // 4 frames - 8 frames

        yield return new WaitForSeconds(FrameToTime(6));


        // Step 4
        color.a = centerAlpha;
        CreateCircle(pos, range / 3f, 0f, color, CircleType.Scale_Thickness, 0, LifeTimeToSpeed(FrameToTime(18))); // 18 frames

        yield return new WaitForSeconds(FrameToTime(8));

        explodeEffect.transform.localScale = Vector3.one;
        explodeEffect.SetActive(false);
    }

    void CreateCircle(Vector2 pos, float radius, float fadeOutTime, Color color, CircleType type, int sortOrder = 0, float speed = 1f)
    {
        int index = particleCount++;

        SpriteRenderer sr = renderers[index];
        sr.transform.position = pos;
        sr.sortingOrder = 5 + sortOrder;
        sr.gameObject.SetActive(true);
        sr.transform.localScale = Vector2.one * radius / radiusScales[(int)type];

        particles[index] = new ParticleCircle(radiusTimes[(int)type], thicknessTimes[(int)type], fadeOutTime, speed);

        block.Clear();
        block.SetFloat(radiusID, radiusScales[(int)type]);
        block.SetFloat(radiusTimerID, 0);
        block.SetFloat(thicknessID, thicknessValues[(int)type]);
        block.SetFloat(thicknessTimerID, 0);
        block.SetFloat(fadeOutTimerID, 0);
        block.SetColor(colorID, color);
        sr.SetPropertyBlock(block);
    }
}
