using UnityEngine;

public class SecondOrder : MonoBehaviour
{
    public bool useSmoothDamp;
    public float f, z, r;
    public Transform target;

    private Vector2 prevX;
    private Vector2 y, dy;
    private float k1, k2, k3;

    // y [n+1] = y [n] + T * y'[n]
    // y'[n+1] = y'[n] + T * (x[n+1] + k3*x'[n+1] - y[n+1] - k1*y'[n]) / k2
    // x'[n+1] = (x[n+1] - x[n]) / T

    public void Init(float f, float z, float r, Vector2 x0)
    {
        k1 = z / (Mathf.PI * f);
        k2 = 1 / ((2 * Mathf.PI * f) * (2 * Mathf.PI * f));
        k3 = r * z / (2 * Mathf.PI * f);

        prevX = x0;
        y = x0;
        dy = Vector2.zero;
    }

    // Start is called before the first frame update
    void Start()
    {
        Init(f, z, r, target.position);
    }

    public void UpdateStep(float t, Vector2 x)
    {
        Vector2 dx = (x - prevX) / t;
        prevX = x;
        UpdateStep(t, x, dx);
    }

    public void UpdateStep(float t, Vector2 x, Vector2 dx)
    {
        k1 = z / (Mathf.PI * f);
        k2 = 1 / ((2 * Mathf.PI * f) * (2 * Mathf.PI * f));
        k3 = r * z / (2 * Mathf.PI * f);

        /*
         * The second order system works like this:
         * | y [n+1] | = |   1              T          | * | y  |   +   |  0       0    | * | x [n+1] |
         * | y'[n+1] |   | -T/k2  (k2 - T*T - T*k1)/k2 |   | y' |       | T/k2  T*k3/k2 |   | x'[n+1] |
         * Call this 2x2 matrix A:
         * |   1              T          |
         * | -T/k2  (k2 - T*T - T*k1)/k2 |
         * Then we have: y[n+1] = A*y[n]
         * The system will be stabilize over time if A < 1
         * The Eigenvalues z1, z2 of A is:
         * (1 -z) * ((k2 - T*T - T*k1)/k2 -z) - (-T/k2) * (T) = 0
         * k2*z*z + (T*T + T*k1 - 2k2)z + (k2 - T*k1) = 0
         * Our system will be stable if:
         * T < sqrt(4*k2 + k1*k1) - k1
         * or:
         * T_crit = sqrt(4*k2 + k1*k1) - k1
         * When T is greater than T_crit we just need to run multiple steps in that frame
         * Or slowing down the dynamic (k2 in this case) instead:
         * k2 > T*T/4 + T*k1/2
         * This isn't physically correct but it's faster so who care
         * Also, the frame-to-frame jittering behaviour, appear when f (frequency) is high, is caused by negative Eigenvalues
         * So change the code from Mathf.Max(k2, 1.1f * (t*t/4 + t*k1/2)) to Mathf.Max(k2, t*t/2 + t*k1/2, t*k1) will fix the problem
        */
        float k2_stable = Mathf.Max(k2, t*t/2 + t*k1/2, t*k1);

        y = y + t * dy;
        dy = dy + t * (x + k3*dx - y - k1*dy) / k2_stable;

        transform.position = y;
    }

    // Update is called once per frame
    void Update()
    {
        if (useSmoothDamp)
            transform.position = Vector2.SmoothDamp(transform.position, target.position, ref dy, 1f / (Mathf.PI * f));
        else
            UpdateStep(Time.deltaTime, target.position);
    }
}
