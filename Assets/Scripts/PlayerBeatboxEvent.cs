#define DebugBuild //Comment out when Game is finished

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
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

    private bool useManualControl = false;

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

    private float ChangeBeatboxValueTimeCurrent = 0.0f;

    [SerializeField, Tooltip("The time it takes for the Beatbox Value to change in Seconds")]
    private float ChangeBeatboxValueTime = 3.0f;

    private float BeatboxToFollow = 0.0f;

    [SerializeField, Tooltip("The Index to take for the Microphone Array (0 = Default System Mic)")]
    private int MicrophoneIndex = 0;
    private int InternalMicrophoneIndex = 0;

    private AudioSource MicrophoneSource = null;
    private AudioRecordingState audioRecordingState = AudioRecordingState.none;

    [SerializeField]
    private AudioMixerGroup silentMixerAsset = null;

    private string AudioErrorMessage = "";

    private float[] spectrumData = new float[256];
    private float dataProcessed = 0.0f; // value between 0 and 1

    void Start()
    {
        this.MicrophoneSource = GetComponent<AudioSource>();

#if DebugBuild
        if (this.targetRenderImage == null)
        {
            DebugLog("No Target Render Image Attached to the Beatbox Component!");
        }

        if (this.MicrophoneSource == null)
        {
            DebugLog("No Audio Source Component attached to the Beatbox Component");
        }
#endif

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

        ActivateRecording();
    }

    private void OnDestroy()
    {
        DeactivateEvent();
    }

    private static void DebugLog(object Message)
    {
#if DebugBuild
        Debug.Log(Message);
#endif
    }

    private void ActivateRecording()
    {
        if (audioRecordingState == AudioRecordingState.none) // TODO: Add Logic to take first Mic and Start Recording to Audio Clip
        {
            if (Microphone.devices.Length > 0)
            {
                InternalMicrophoneIndex = MicrophoneIndex;

                if (MicrophoneIndex >= Microphone.devices.Length || MicrophoneIndex < 0)
                {
                    DebugLog("Entered Microphone Index was Invalid, switching to Default System Index");
                    InternalMicrophoneIndex = 0;
                }

                this.MicrophoneSource.clip = Microphone.Start(Microphone.devices[InternalMicrophoneIndex], true, 1, 44100);


                if (!this.silentMixerAsset)
                {
                    DebugLog("this.silentMixerAsset was null, leaving output to normal Deivce Output");
                }
                else
                    this.MicrophoneSource.outputAudioMixerGroup = this.silentMixerAsset;
            }
            else
            {
                var lastError = "'No Input Devices detected!'";

                this.AudioErrorMessage = lastError;

                audioRecordingState = AudioRecordingState.failed;

                DebugLog("Failed to find Default Mic because: " + lastError);

                //switch to Manual Control
            }

        }
        else
        {
            switch (audioRecordingState)
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

    private void DeactivateRecording()
    {
        switch (audioRecordingState)
        {
            case AudioRecordingState.recording:
                this.MicrophoneSource.Stop();

                Microphone.End(Microphone.devices[InternalMicrophoneIndex]);

                this.MicrophoneSource.clip = null;

                audioRecordingState = AudioRecordingState.none;
                break;


            default:
                audioRecordingState = AudioRecordingState.none;

                DebugLog("Clean up not needed, Manual Controll was Active!");
                break;
        }
    }

    public void ActivateEvent(EnemyAI joinklerAi)
    {
        this.joinklerAI = joinklerAi;

        this.joinklerAI.SetTriggerColliderState(false);

        isBeatboxActive = true;

        CurrentTimeToBeatbox = 0;
        PlayerBeatboxAccuracy = 0;
    }

    public void DeactivateEvent()
    {
        this.joinklerAI.SetTriggerColliderState(true);

        this.joinklerAI.beatboxEvent = false;

        this.joinklerAI = null;

        isBeatboxActive = false;
        CurrentTimeToBeatbox = 0;

        ResetCanvas();
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

    private void UpdateProcessedData()
    {
        this.MicrophoneSource.Play();

        this.dataProcessed = 0.0f;

        this.MicrophoneSource.GetSpectrumData(this.spectrumData, 0, FFTWindow.Rectangular);

        float bassLevel = 0.0f;

        for (int i = 0; i < 5; i++)
        {
            bassLevel += spectrumData[i];
        }

        this.dataProcessed =  bassLevel;
    }

    void Update()
    {
        if (!isBeatboxActive)
        {
            return;
        }

#if DebugBuild
        if (joinklerAI == null)
        {
            Debug.LogError("this.joinklerAI is Null, something went wrong and i dont even know cuh");
            return;
        }
#endif
        
        if(useManualControl)
        {

        }
        else
        {
            UpdateProcessedData();

            DebugLog("Current Bass Level is: " + this.dataProcessed.ToString());
        }

        if (ChangeBeatboxValueTimeCurrent >= ChangeBeatboxValueTime)
        {
            ChangeBeatboxValueTimeCurrent = 0;

            //this.PlayerBeatboxAccuracy = 

            this.BeatboxToFollow = Random.Range(1.0f, 2.0f);
        }
        else
            ChangeBeatboxValueTimeCurrent += Time.deltaTime;

        if (CurrentTimeToBeatbox >= MaxTimeToBeatbox)
        {
            if (PlayerBeatboxAccuracy >= MinBeatboxAccuracy)
            {
                StunJoinkler(false);
            }
            else
            {
                KillPlayer();
            }

            DeactivateEvent();

            return;
        }
        else
        {
            CurrentTimeToBeatbox += Time.deltaTime;

            if (PlayerBeatboxAccuracy >= MaxBeatboxAccuracy) //if player reaches Maximum Accuracy, end early
            {
                StunJoinkler(true);

                DeactivateEvent();

                return;
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
