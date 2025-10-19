using UnityEngine;

public class DestroyAfterAnimation : MonoBehaviour
{
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
        Destroy(gameObject, anim.GetCurrentAnimatorStateInfo(0).length);
    }
}
