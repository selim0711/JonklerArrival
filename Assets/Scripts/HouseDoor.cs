using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseDoor : MonoBehaviour
{
    public Animator openandclose1;
    public bool open;
    public Transform Player;

   


    // Audio clips for opening and closing sounds
    public AudioClip openSound;
    public AudioClip closeSound;

    private AudioSource audioSource;



    void Start()
    {
        open = false;
        Player = GameObject.FindWithTag("Player").transform;

        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

       


    }

    void OnMouseOver()
    {
        if (Player)
        {
            float dist = Vector3.Distance(Player.position, transform.position);
            if (dist < 4)
            {
                

                if (!open && Input.GetMouseButtonDown(0))
                {
                    StartCoroutine(opening());
                }
                else if (open && Input.GetMouseButtonDown(0))
                {
                    StartCoroutine(closing());
                }
            }
            
        }
    }
   

    IEnumerator opening()
    {
        print("you are opening the door");
        openandclose1.Play("Opening 1");
        audioSource.clip = openSound;  // Set the clip to open sound
        audioSource.Play();  // Play the open sound
        open = true;
        yield return new WaitForSeconds(.5f);
    }

    IEnumerator closing()
    {
        print("you are closing the door");
        openandclose1.Play("Closing 1");
        audioSource.clip = closeSound;  // Set the clip to close sound
        audioSource.Play();  // Play the close sound
        open = false;
        yield return new WaitForSeconds(.5f);
    }
}

