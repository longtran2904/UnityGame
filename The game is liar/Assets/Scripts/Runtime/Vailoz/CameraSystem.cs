using UnityEngine;

public delegate float SmoothFunc(float t);

public struct CameraShakeData
{
    public float trauma;
    public Vector3 force;
    public float speed;
    public int seed;
    public Vector2 dir;
    public SmoothFunc smoothFunc;

    public bool isValid => trauma > 0f;

    public Vector3 UpdateShake(float delta) // return posX, posY, rotZ
    {
        Vector3 result = new Vector3(
            Mathf.PerlinNoise(seed + 0, Time.time * speed),
            Mathf.PerlinNoise(seed + 1, Time.time * speed),
            Mathf.PerlinNoise(seed + 2, Time.time * speed)
        );

        if (dir == Vector2.zero)
            result += (Vector3)((Vector2)result - Vector2.one);
        else
            result.Scale(new Vector3(dir.x, dir.y, 1));
        result.z += result.z - 1;
        return Vector3.Scale(result *= smoothFunc(trauma = Mathf.Clamp01(trauma - delta)), force);
    }
}

public enum ShakeMode
{
    None,
    Small,
    Medium,
    Strong,
    GunKnockback,
}

public class CameraSystem : MonoBehaviour
{
    public static CameraSystem instance;
    public SpriteRenderer flashScreen;
    public Vector3[] shakeForces =
    {
        new Vector3(0f , 0f , 0f ),
        new Vector3(2f , 2f , 8f ),
        new Vector3(5f , 5f , 15f),
        new Vector3(10f, 10f, 20f),
        new Vector3(5f , 5f , 4f ),
    };
    public float[] shakeSpeeds =
    {
         0f,
        10f,
        15f,
        20f,
        12f,
    };

    private Vector3 basePosition;
    private int shakeDataCount;
    private CameraShakeData[] shakeData = new CameraShakeData[256];

    public Material shockMat;
    private bool useShock;
    private float maxTime;
    private float timer;
    private int timeID;
    private int speedID;
    private int sizeID;

    private void Awake()
    {
        if (!instance)
            instance = this;
        basePosition = transform.localPosition;

        timeID = Shader.PropertyToID("_Timer");
        speedID = Shader.PropertyToID("_Speed");
        sizeID = Shader.PropertyToID("_Size");
        ResetShockwave();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (GameInput.GetInput(InputType.Debug_LeftAlt))
            Shake(ShakeMode.Small, MathUtils.SmoothStart3);
        if (GameInput.GetInput(InputType.Debug_LeftCtrl))
            Shake(ShakeMode.Medium, MathUtils.SmoothStart3);
        if (GameInput.GetInput(InputType.Debug_LeftShift))
            Shake(ShakeMode.Strong, MathUtils.SmoothStart3);
        if (GameInput.GetInput(InputType.Debug_T))
            Shock(2, .1f);
#endif

        float delta = Time.deltaTime;
        transform.localPosition = basePosition;
        transform.localRotation = Quaternion.identity;

        Vector3 shakePos = Vector3.zero;
        float shakeRot = 0;
        for (int i = 0; i < shakeDataCount; i++)
        {
            while (!shakeData[i].isValid)
            {
                if (shakeDataCount == i)
                    goto END;
                shakeData[i] = shakeData[--shakeDataCount];
            }
            Vector3 shake = shakeData[i].UpdateShake(delta);
            shakePos += (Vector3)(Vector2)shake;
            shakeRot += shake.z;
        }

        END:
        transform.localPosition = shakePos;
        transform.localRotation = Quaternion.Euler(0, 0, shakeRot);

        if (useShock)
        {
            if (timer <= maxTime)
                shockMat.SetFloat(timeID, timer += Time.unscaledDeltaTime);
            else
                ResetShockwave();
        }
    }

    public System.Collections.IEnumerator Flash(float time, float alpha)
    {
        flashScreen.enabled = true;
        float t = time;
        float a = alpha;
        while (t > 0)
        {
            flashScreen.color = new Color(1, 1, 1, a);
            t -= Time.unscaledDeltaTime;
            a = Mathf.Lerp(0, alpha, t / time);
            yield return null;
        }
        flashScreen.enabled = false;
    }

    public void Shake(ShakeMode mode, SmoothFunc smoothFunc = null, float trauma = 1f) => Shake(mode, Vector2.zero, smoothFunc, trauma);

    public void Shake(ShakeMode mode, Vector2 dir, SmoothFunc smoothFunc = null, float trauma = 1f)
    {
        int seed = Random.Range(-100, 100);
        CameraShakeData data = new CameraShakeData
        {
            trauma = trauma,
            force = shakeForces[(int)mode],
            speed = shakeSpeeds[(int)mode],
            seed = seed,
            dir = dir,
            smoothFunc = smoothFunc ?? MathUtils.SmoothStart2
        };
        shakeData[shakeDataCount++] = data;
        Debug.Assert(shakeDataCount <= shakeData.Length, "Shake data is full");
    }

    public void Shock(float speed = 1, float size = .1f)
    {
        if (!useShock)
        {
            shockMat.SetFloat(speedID, speed);
            shockMat.SetFloat(sizeID, size);
            maxTime = 1f / speed;
            useShock = true;
        }
    }

    private void ResetShockwave()
    {
        shockMat.SetFloat(timeID, 0f);
        shockMat.SetFloat(sizeID, 0f);
        timer = 0;
        useShock = false;
    }
}
