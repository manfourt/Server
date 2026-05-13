using UnityEngine;

public class ServerBoxInteract : MonoBehaviour
{
    public Animator animator;

    private bool opened = false;

    public void OnSelect()
    {
        opened = !opened;

        animator.SetBool("Open", opened);
    }
}