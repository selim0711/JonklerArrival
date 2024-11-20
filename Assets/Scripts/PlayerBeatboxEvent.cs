#define DebugBuild //Comment out when Game is finished

using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBeatbox : MonoBehaviour // TODO: Record Microphone, Get Data from Microphone and either use Bass Channel or use all Channels Loudness
{
    enum AudioRecordingState
    {
        none,
        recording,
        finished,
        failed
    }

    [HideInInspector]
    public EnemyAI joinklerAI = null; //Set this to the ai which the Player beatboxes against

    [SerializeField]
    public RawImage targetRenderImage = null;

    private bool isBeatboxActive = false;

    private Texture2D canvas = null;
    private Color32[] clearColors;

    private Color32[] pixelBuffer;

    [SerializeField, Tooltip("Width of the Sinus Curve")]
    private int curveWidth = 10;
    private float startingValueY = 0;

    private float SinusCurveintensity = 1.0f; //Set this to the accuracy of the Beatboxing

    public Color32 BeatboxPlayerColor = new Color32(255, 0, 0, 255);
    private Color32 transparentColor = new Color32(0, 0, 0, 0);

    [SerializeField, Tooltip("The Maximum Time the player has time to Beatbox")]
    private float MaxTimeToBeatbox = 10.0f;
    private float CurrentTimeToBeatbox = 0.0f;

    [SerializeField, Tooltip("The Maximum Accuracy the player can reach, if he does he gets an bonus")]
    private float MaxBeatboxAccuracy = 1.0f;

    [SerializeField, Tooltip("The Minimum Accuracy % the player has to reach to not die"), Range(0.0f, 100.0f)]
    private float MinBeatboxAccuracy = 1.0f;

    private float PlayerBeatboxAccuracy = 0.0f;

    [SerializeField, Tooltip("The Time the Joinkler gets stunned for after Winning (In Seconds)")]
    private float EnemyStunTime = 2.0f;

    private float LoudnessToFollow = 0.0f;
    private float PlayerLoudness = 0.0f;

    private AudioClip MicrophoneClip = null;
    private Microphone MicrophoneControll = null;

    private AudioRecordingState audioRecordingState = AudioRecordingState.none;
    private Thread audioRecordingThread = null;

    private bool stopRecording = false;

    private string AudioErrorMessage = "";

    void Start()
    {
        int canvasWidth = (int)targetRenderImage.rectTransform.rect.width;
        int canvasHeight = (int)targetRenderImage.rectTransform.rect.height;

        canvas = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);
        targetRenderImage.texture = canvas;

        clearColors = new Color32[canvasWidth * canvasHeight];
        for (int i = 0; i < clearColors.Length; i++)
        {
            clearColors[i] = transparentColor;
        }

        pixelBuffer = new Color32[canvasWidth * canvasHeight];
        ResetCanvas();

        targetRenderImage.enabled = true;
    }

    private void OnDestroy()
    {
        
    }

    private static void DebugLog(object Message)
    {
#if DebugBuild
        Debug.Log(Message);
#endif
    }

    public void ActivateRecording()
    {
        if(audioRecordingState == AudioRecordingState.none) // TODO: Add Logic to take first Mic and Start Recording to Audio Clip
        {

        }
        else
        {
            switch(audioRecordingState)
            {
                case AudioRecordingState.recording:
                    DebugLog("Audio is already recording!");
                    break;

                case AudioRecordingState.finished:
                    DebugLog("Audio clean up failed, Cleaning up now!");
                    break;

                case AudioRecordingState.failed:
                    DebugLog("Previous Audio recording failed before because: " + AudioErrorMessage);
                    DebugLog("Switching to Keyboard Comtrolled Beatboxing");
                    break;
            }
        }
    }

    public void ActivateEvent()
    {
        isBeatboxActive = true;
        CurrentTimeToBeatbox = 0;
    }

    private void StunJoinkler(bool earlyBonus)
    {
        float stunTime = this.EnemyStunTime * (earlyBonus ? 1.5f : 1.0f);

        joinklerAI.StunJoinkler(stunTime);
    }

    private void KillPlayer()
    {
        joinklerAI.KillPlayer();
    }

    void Update()
    {

#if DebugBuild
        if(joinklerAI == null)
        {
            Debug.LogError("this.joinklerAI is Null, set the Joinkler AI before Starting Event");
        }
#endif
        if (!isBeatboxActive)
        {
            return;
        }
            

        if(CurrentTimeToBeatbox >= MaxTimeToBeatbox)
        {
            if (PlayerBeatboxAccuracy >= MinBeatboxAccuracy)
            {
                StunJoinkler(false);
            }
            else
            {
                KillPlayer();
            }
        }
        else
        {
            CurrentTimeToBeatbox += Time.deltaTime;

            if (PlayerBeatboxAccuracy >= MaxBeatboxAccuracy) //if player reaches Maximum Accuracy, end early
            {
                StunJoinkler(true);
            }
        }

        DrawSinusCurve();
    }

    private void DrawSinusCurve()
    {
        int canvasHeight = canvas.height;
        int canvasWidth = canvas.width;
        float frequency = 2.0f * Mathf.PI * SinusCurveintensity;

        System.Array.Copy(clearColors, pixelBuffer, pixelBuffer.Length);

        int halfWidth = curveWidth / 2;
        float offset = (startingValueY * 100);

        for (int x = 0; x < canvasWidth; x++)
        {
            float radians = (x / (float)canvasWidth) * frequency;
            int y = Mathf.RoundToInt(Mathf.Sin(radians + offset) * (canvasHeight / 4) + (canvasHeight / 2));

            int baseIndex = y * canvasWidth + x;
            for (int i = -halfWidth; i <= halfWidth; i++)
            {
                int newY = y + i;
                if (newY >= 0 && newY < canvasHeight)
                {
                    pixelBuffer[newY * canvasWidth + x] = BeatboxPlayerColor;
                }
            }
        }

        startingValueY += 0.1f * Time.deltaTime;
        if (startingValueY > 1) startingValueY = -1;

        canvas.SetPixels32(pixelBuffer);
        canvas.Apply();
    }

    private void ResetCanvas()
    {
        canvas.SetPixels32(clearColors);
        canvas.Apply();
    }


}
