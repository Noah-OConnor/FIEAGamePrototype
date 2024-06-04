using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class AIDead : NetworkBehaviour
{
    protected AIMain aiMain;
    protected Animator animator;
    protected NavMeshAgent agent;

    protected virtual void OnEnable()
    {
        aiMain = GetComponent<AIMain>();
        animator = aiMain.GetAnimator();
        agent = aiMain.GetAgent();
        agent.enabled = false;
        aiMain.TriggerOnEnemyDeath();

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
            if (IsServer) animator.SetFloat("Forward", newSpeed);
            yield return null;
        }

        animator.SetFloat("Forward", 0); // Ensure speed is set to 0 at the end

        yield return new WaitForSeconds(5f);

        // lerp the object downwards
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos - Vector3.up * 2;
        float lerpDuration = 2f;
        for (float t = 0; t < lerpDuration; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(startPos, endPos, t / lerpDuration);
            yield return null;
        }
        if (IsServer) gameObject.SetActive(false);
    }

    protected virtual void OnDisable()
    {
        agent.enabled = true;
        if (IsServer) Destroy(gameObject);
    }
}