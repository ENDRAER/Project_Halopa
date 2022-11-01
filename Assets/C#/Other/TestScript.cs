using UnityEngine;

public class TestScript : MonoBehaviour
{
    public Animator animator;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            animator.SetTrigger("Z");
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            animator.SetTrigger("X");
        }
    }
}