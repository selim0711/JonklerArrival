using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBeatbox : MonoBehaviour
{
    [SerializeField]
    public RawImage rawImage = null;

    private bool isBeatboxActive = false;

    public float startingValueY = 0;
    public int curveWidth = 10;

    private Texture2D canvas = null;
    private Color32[] clearColors;

    public Color32 BeatboxPlayerColor = new Color32(255, 0, 0, 255);
    private Color32 transparentColor = new Color32(0, 0, 0, 0);

    [SerializeField, Header("The Maximum Time the player has time to Beatbox")]
    private float MaxTimeToBeatbox = 10.0f;
    private float CurrentTimeToBeatbox = 0.0f;

    private float LoudnessToFollow = 0.0f;
    private float PlayerLoudness = 0.0f;

    [SerializeField, Header("The Maximum Accuracy the player can reach")]
    private float MaxBeatboxAccuracy = 1.0f;

    [SerializeField, Header("The Minimum Percantage Accuracy the player has to reach"), MinMaxRangeSlider(0, 1)]
    private float MinBeatboxAccuracy = 1.0f;

    public float PlayerBeatboxAccuracy = 0.0f;

    [SerializeField, Header("The Time the Joinkler gets stunned for after Winning (In Seconds)")]
    private float EnemyStunTime = 2.0f;

    public float intensity = 1.0f;

    private float DrawTime = 0.0f;
    private Color32[] pixelBuffer;

    void Start()
    {
        int canvasWidth = (int)rawImage.rectTransform.rect.width;
        int canvasHeight = (int)rawImage.rectTransform.rect.height;

        canvas = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);
        rawImage.texture = canvas;

        clearColors = new Color32[canvasWidth * canvasHeight];
        for (int i = 0; i < clearColors.Length; i++)
        {
            clearColors[i] = transparentColor;
        }

        pixelBuffer = new Color32[canvasWidth * canvasHeight];
        ResetCanvas();
        rawImage.enabled = true;
    }

    public void ActivateEvent()
    {
        isBeatboxActive = true;
        CurrentTimeToBeatbox = 0;
    }

    private void StunJoinkler(bool earlyBonus) //Implement Stunning Logic: Add State to Joinkler Class to Stun him, with time
    {
        float stunTime = this.EnemyStunTime * (earlyBonus ? 1.5f : 1.0f);
    }

    private void KillPlayer() //Implement Killing Logic: Animation and States
    {

    }

    void Update()
    {
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

        if (DrawTime >= 0.0f)
        {
            DrawTime = 0;
            DrawSinusCurve();
        }
        else
        {
            DrawTime += Time.deltaTime;
        }
    }

    private void DrawSinusCurve()
    {
        int canvasHeight = canvas.height;
        int canvasWidth = canvas.width;
        float frequency = 2.0f * Mathf.PI * intensity;

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
