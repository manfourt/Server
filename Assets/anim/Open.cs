using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class door : MonoBehaviour
{
    private Animator anim;
    public float openDistance = 5f;
    private Transform player;

    void Start()
    {
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        anim.SetBool("Open", true);
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < openDistance && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2, 0));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, openDistance))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    bool isOpen = anim.GetBool("Open");
                    anim.SetBool("Open", !isOpen);
                }
            }
        }
    }
}