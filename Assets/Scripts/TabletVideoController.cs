using UnityEngine;
using UnityEngine.Video;
using UnityEngine.InputSystem;
public class TabletVideoController : MonoBehaviour
{
   

    private VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.loopPointReached += EndReached;
 
    }

    void Update()
    {
        // Schalte die Wiedergabe mit der Leertaste um
       // if (Input.GetKeyDown(KeyCode.Space))
        {
            if (videoPlayer.isPlaying)
                videoPlayer.Pause();
            else
                videoPlayer.Play();
        }
    }

    // Aktion, wenn das Video zu Ende ist
    void EndReached(VideoPlayer vp)
    {
        vp.Stop(); // Stoppe das Video
    }
}
