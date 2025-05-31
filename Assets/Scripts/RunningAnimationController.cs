using UnityEngine;

public class RunningAnimationController : MonoBehaviour
{
    Animator animator;
    Rigidbody rb;
    private void Awake()
    {
        
        animator = GetComponent<Animator>();
        rb = transform.root.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (rb.linearVelocity != Vector3.zero)
            animator.SetBool("IsMoving", true);

        else
            animator.SetBool("IsMoving", false);

    }

}
