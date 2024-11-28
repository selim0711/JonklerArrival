using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class KillRoomBehaviour : MonoBehaviour
{
    [SerializeField]
    private Animator  joinklerAnimator = null, cameraAnimator = null;

    [SerializeField]
    private Volume volume = null;
    private Vignette vignetteRef = null;

    void Start()
    {
        var ai = GameObject.FindAnyObjectByType<EnemyAI>();
        var jonklerEndScene = GetComponentInChildren<JonklerEndSceneAnim>();
        var camEndScene = GetComponentInChildren<EndSceneCam>();

        ai.killRoom = gameObject;
        jonklerEndScene.killRoomBehaviour = this;
        camEndScene.killRoomBehaviour = this;

        volume.profile.TryGet<Vignette>(out vignetteRef);

        camEndScene.updateSpeedCurrent = vignetteRef.intensity.value;
    }

    private void OnDestroy()
    {
        var jonklerEndScene = GetComponentInChildren<JonklerEndSceneAnim>();
        var camEndScene = GetComponentInChildren<EndSceneCam>();

        jonklerEndScene.killRoomBehaviour = null;
        camEndScene.killRoomBehaviour = null;
    }

    private void Awake()
    {
        PlayKillScene();
    }

    public void ActivateJoinkler()
    {
        joinklerAnimator.SetBool("active", true);
        joinklerAnimator.SetTrigger("jump");
    }
    public void PlayKillScene()
    {
        cameraAnimator.SetTrigger("plead");
    }

    public void PlayCameraKicked()
    {
        cameraAnimator.SetTrigger("knockOut");
    }

    public void ActivateVignette()
    {
        vignetteRef.active = true;
    }

    public void UpdateVignette(float vignette)
    {
        vignetteRef.intensity.value = vignette;
    }
}
