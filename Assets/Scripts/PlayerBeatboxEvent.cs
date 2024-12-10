#define DebugBuild //Comment out when Game is finished

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class PlayerBeatbox : MonoBehaviour //TODO: Add Manual Control for people who got no mic
{
    enum AudioRecordingState
    {
        none,
        recording,
        finished,
        failed
    }

    [HideInInspector]
    public EnemyAI joinklerAI = null;

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

    private float SinusCurveintensity = 1.0f;

    private float targetCurveIntensity = 0.0f;
    private float targetCurveChangeTime = 0.05f;
    private float targetCurveChangeTimeCurrent = 0.0f;

    private bool changeIntensity = false;


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
    private float ChangeBeatboxValueTime = 0.2f;

    private float BeatboxToFollow = 0.0f;

    private List<float> Accuracies = new List<float>();

    [SerializeField, Tooltip("The Index to take for the Microphone Array (0 = Default System Mic)")]
    private int MicrophoneIndex = 0;
    private int InternalMicrophoneIndex = 0;

    private AudioSource MicrophoneSource = null;
    private AudioRecordingState audioRecordingState = AudioRecordingState.none;

    [SerializeField]
    private AudioMixerGroup silentMixerAsset = null;

    private PlayerInput playerInput = null;

    private string AudioErrorMessage = "";

    private float[] spectrumData = new float[256];
    private float dataProcessed = 0.0f; // value between 0 and 1


    private int lifesLeft = 0;

    [SerializeField]
    private int lifesMax = 3;

    void Start()
    {
        this.lifesLeft = lifesMax;

        this.MicrophoneSource = GetComponent<AudioSource>();
        this.playerInput = GetComponent<PlayerInput>();

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
        if (audioRecordingState == AudioRecordingState.none)
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

    private void ActivateControls(bool value)
    {
        this.playerInput.enabled = value;
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

        this.joinklerAI.beatboxEvent = true;

        ActivateControls(false);

        isBeatboxActive = true;

        CurrentTimeToBeatbox = 0;
        PlayerBeatboxAccuracy = 0;
    }

    public void DeactivateEvent()
    {
        if(this.isBeatboxActive)
        {
            this.joinklerAI.SetTriggerColliderState(true);

            this.joinklerAI.beatboxEvent = false;

            this.joinklerAI = null;

            isBeatboxActive = false;
            CurrentTimeToBeatbox = 0;

            ActivateControls(true);

            ResetCanvas();
        }
    }

    private void StunJoinkler(bool earlyBonus)
    {
        float stunTime = this.EnemyStunTime * (earlyBonus ? 1.5f : 1.0f);

        joinklerAI.StunJoinkler(stunTime);
    }

    private void KillPlayer()
    {
        joinklerAI.OnFinishKillPlayer();
        //joinklerAI.KillPlayer(JoinklerFinishers.uppercut);
    }

    private void UpdateProcessedData()
    {
        this.MicrophoneSource.Play();

        this.dataProcessed = 0.0f;

        this.MicrophoneSource.GetSpectrumData(this.spectrumData, 0, FFTWindow.Rectangular);

        float bassLevel = 0.0f;

        for (int i = 0; i < 12; i++)
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
        }

        /*
        if(this.changeIntensity)
        {
            if (targetCurveChangeTimeCurrent >= targetCurveChangeTime)
            {
                this.SinusCurveintensity = targetCurveIntensity;
                this.changeIntensity = false;
            }
            else
            {
                this.SinusCurveintensity = targetCurveIntensity * (targetCurveChangeTimeCurrent / targetCurveChangeTime);
                targetCurveChangeTimeCurrent += Time.deltaTime;
            }
                
        }
        */

        if (ChangeBeatboxValueTimeCurrent >= ChangeBeatboxValueTime)
        {
            const float minBeatboxVal = 1.0f;
            const float maxBeatboxVal = 4.0f;

            ChangeBeatboxValueTimeCurrent = 0;

            this.BeatboxToFollow = Random.Range(minBeatboxVal, maxBeatboxVal);
        }
        else
        {
            ChangeBeatboxValueTimeCurrent += Time.deltaTime;

            float PlayerBeatboxAccStatic = this.dataProcessed / this.BeatboxToFollow;

            if(PlayerBeatboxAccStatic < 0)
                PlayerBeatboxAccStatic = 0.0f - PlayerBeatboxAccStatic;

            float PlayerBeatboxAccuracyCopy = PlayerBeatboxAccStatic;
            this.PlayerBeatboxAccuracy = Mathf.Clamp(PlayerBeatboxAccuracyCopy, 0, 1);

            Debug.Log("Current Accuracy: " + PlayerBeatboxAccuracyCopy);

            this.SinusCurveintensity = this.dataProcessed / 10.0f;
        }
           



        SinusCurveintensity = Mathf.Clamp(this.PlayerBeatboxAccuracy * 100, 1.0f, 10.0f);

        if (CurrentTimeToBeatbox >= MaxTimeToBeatbox)
        {
            var accuracy = PlayerBeatboxAccuracy * 100;

            if (accuracy >= MinBeatboxAccuracy)
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

            var accuracy = PlayerBeatboxAccuracy * 100;

            if (accuracy >= MaxBeatboxAccuracy) //if player reaches Maximum Accuracy, end early
            {
                StunJoinkler(true);

                DeactivateEvent();

                return;
            }
        }

        ClearCanvas();

        DrawSinusCurve(Mathf.Clamp((BeatboxToFollow / 10.0f), 1.0f, 10.0f), new Color(0.0f, 1.0f, 0.0f));
        DrawSinusCurve(SinusCurveintensity, BeatboxPlayerColor);

        ApplyCanvas();
    }

    private void ClearCanvas()
    {
        System.Array.Copy(clearColors, pixelBuffer, pixelBuffer.Length);
    }

    private void ApplyCanvas()
    {
        canvas.SetPixels32(pixelBuffer);
        canvas.Apply();
    }

    private void DrawSinusCurve(float SinusCurveInten, Color color)
    {
        int canvasHeight = canvas.height;
        int canvasWidth = canvas.width;
        float frequency = 2.0f * Mathf.PI * SinusCurveInten;

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
                    pixelBuffer[newY * canvasWidth + x] = color;
                }
            }
        }

        startingValueY += 0.1f * Time.deltaTime;
        if (startingValueY > 1) startingValueY = -1;

    }

    private void ResetCanvas()
    {
        canvas.SetPixels32(clearColors);
        canvas.Apply();
    }
}
