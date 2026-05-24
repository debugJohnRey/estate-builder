using UnityEngine;

public class IdleTimeout : StateMachineBehaviour
{
    public float timeToWave = 5f; // Seconds to wait before waving
    private float timer;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer = 0f;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer += Time.deltaTime;
        if (timer >= timeToWave)
        {
            animator.SetTrigger("Wave");
            timer = 0f; 
        }
    }
}