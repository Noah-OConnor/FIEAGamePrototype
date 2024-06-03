using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AIDead : MonoBehaviour
{
    protected NetcodeAIMain aiMain;
    protected Animator animator;
    protected NavMeshAgent agent;

    protected virtual void OnEnable()
    {
        aiMain = GetComponent<NetcodeAIMain>();
        animator = aiMain.GetAnimator();
        agent = aiMain.GetAgent();

        agent.enabled = false;

        animator.SetBool("Dead", true);
        StartCoroutine(LerpSpeedToZero());
    }

    protected virtual IEnumerator LerpSpeedToZero()
    {
        float duration = 1f; // Duration over which to reduce speed to 0
        float speed = animator.GetFloat("Forward"); // Assuming "ForwardSpeed" is the parameter controlling speed in your Animator

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float newSpeed = Mathf.Lerp(speed, 0, t / duration);
            animator.SetFloat("Forward", newSpeed);
            yield return null;
        }

        animator.SetFloat("Forward", 0); // Ensure speed is set to 0 at the end
    }
}