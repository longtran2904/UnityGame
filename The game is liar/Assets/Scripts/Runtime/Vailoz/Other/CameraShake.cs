using UnityEngine;

public delegate float SmoothFunc(float t);

public struct CameraShakeData
{
    public ShakeMode mode;
    public float trauma;
    public float force;
    public float speed;
    public int seed;
    public Vector2 dir;
    public SmoothFunc smoothFunc;

    public bool isValid => trauma > 0f;    

    public CameraShakeData(Vector2 dir, float speed, int seed, SmoothFunc smoothFunc, float trauma) : this(0f, speed, seed, smoothFunc, trauma)
    {
        this.dir = dir;
    }

    public CameraShakeData(ShakeMode mode, float speed, int seed, SmoothFunc smoothFunc, float trauma) : this(0f, speed, seed, smoothFunc, trauma)
    {
        this.mode = mode;
    }

    public CameraShakeData(float force, float speed, int seed, SmoothFunc smoothFunc, float trauma)
    {
        this.trauma = Mathf.Clamp01(trauma);
        this.speed = speed;
        this.seed = seed;
        this.smoothFunc = smoothFunc ?? MathUtils.SmoothStart2;

        this.force = force;
        this.dir = Vector2.zero;
        mode = ShakeMode.None;
    }

    public Vector3 UpdateShake(float delta) // return posX, posY, rotZ
    {
        trauma = Mathf.Clamp01(trauma - delta);
        float shake = smoothFunc(trauma);

        float angle = mode != ShakeMode.None ? shake * GetRandom(seed, true) : 0;
        float offsetX = shake * GetRandom(seed + 1, mode != ShakeMode.None);
        float offsetY = shake * GetRandom(seed + 2, mode != ShakeMode.None);

        Vector3 result = new Vector3(offsetX, offsetY, angle);
        return result;
    }

    private float GetRandom(int seed, bool negOneToOne)
    {
        float result = Mathf.PerlinNoise(seed, Time.time * speed);
        if (negOneToOne)
            result = result * 2f - 1f;
        return result;
    }
}

public enum ShakeMode
{
    None,
    Small,
    Medium,
    Strong
}

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;
    private int shakeDataCount;
    private CameraShakeData[] shakeData = new CameraShakeData[256];

    public float maxAngle;
    public float maxOffsetX;
    public float maxOffsetY;

    private Vector3[] shakeForces =
    {
        new Vector3(2f, 2f, 8f),
        new Vector3(5f, 5f, 15f),
        new Vector3(10f, 10f, 20f)
    };
    private float[] shakeSpeeds =
    {
        10f,
        15f,
        20f
    };
    private Vector3 basePosition;

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
            if (shakeData[i].mode != ShakeMode.None)
                shake.Scale(shakeForces[(int)shakeData[i].mode - 1]);
            else
                shake.Scale(new Vector3(maxOffsetX * shakeData[i].dir.x, maxOffsetY * shakeData[i].dir.y, maxAngle));
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

    public void Knockback(Vector2 dir, float speed, SmoothFunc smoothFunc = null, float trauma = 1f)
    {
        Shake(seed => new CameraShakeData(dir, speed, seed, smoothFunc, trauma));
    }

    public void Shake(ShakeMode mode, SmoothFunc smoothFunc = null, float trauma = 1f)
    {
        Shake(seed => new CameraShakeData(mode, mode == ShakeMode.None ? 0 : shakeSpeeds[(int)mode - 1], seed, smoothFunc, trauma));
    }

    private void Shake(System.Func<int, CameraShakeData> shake)
    {
        int seed = Random.Range(-100, 100);
        CameraShakeData data = shake(seed);
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
