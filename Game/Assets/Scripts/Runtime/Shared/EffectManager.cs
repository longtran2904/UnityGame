using UnityEngine;

public class EffectManager : MonoBehaviour, IPooledObject
{
    //public System.Action done;
    private Animator anim;
    private ParticleSystem particle;
    
    public void OnObjectInit()
    {
        anim = GetComponentInChildren<Animator>();
        particle = GetComponentInChildren<ParticleSystem>();
    }
    
    public void OnObjectSpawn(GameObject defaultObject)
    {
        if (anim)
        {
            anim.gameObject.SetActive(true);
            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
            anim.Play(state.shortNameHash);
            this.InvokeAfter(state.length, () => anim.gameObject.SetActive(false));
        }
        
        if (particle)
        {
            particle?.gameObject.SetActive(true);
            particle?.Play();
            // NOTE: The particle always automatically disabling itself.
        }
    }
    
    void Update()
    {
        bool doneAnimation = !(anim && anim.gameObject.activeSelf);
        bool doneParticle = !(particle && particle.gameObject.activeSelf);
        if (doneAnimation && doneParticle)
        {
            gameObject.SetActive(false);
            //done?.Invoke();
        }
    }
}
