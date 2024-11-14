using UnityEngine;
using UnityEngine.UI;

public class PlayerBeatbox : MonoBehaviour
{
    [SerializeField]
    public RawImage rawImage = null;

    public float startingValueY = 0;
    public int curveWidth = 10;

    private Texture2D canvas = null;
    private Color32[] clearColors;

    public Color32 BeatboxPlayerColor = new Color32(255, 0, 0, 255);
    private Color32 transparentColor = new Color32(0, 0, 0, 0);

    private float TimeToBeatbox = 0.0f;
    private float CurrentTimeToBeatbox = 0.0f;

    private float LoudnessToFollow = 0.0f;
    private float PlayerLoudness = 0.0f;

    private float BeatboxAccuracy = 1.0f;
    public float PlayerBeatboxAccuracy = 0.0f;

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

    void Update()
    {
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
