using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerBeatbox : MonoBehaviour
{
    [SerializeField]
    public RawImage rawImage = null;

    public float startingValueY = 0;
    public int curveWidth = 10;

    private Texture2D canvas = null;

    public Color BeatboxPlayerColor = Color.red;
    private Color[] clearColors;

    private float TimeToBeatbox = 0.0f;
    private float CurrentTimeToBeatbox = 0.0f;

    private float LoudnessToFollow = 0.0f;
    private float PlayerLoudness = 0.0f;

    private float BeatboxAccuracy = 1.0f;
    public float PlayerBeatboxAccuracy = 0.0f;

    public float intensity = 1.0f;

    private float DrawTime = 0.0f;

    void Start()
    {
        Color transparentColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        canvas = new Texture2D((int)rawImage.rectTransform.rect.width, (int)rawImage.rectTransform.rect.height);

        clearColors = new Color[canvas.width * canvas.height];

        for (int i = 0; i < clearColors.Length; i++)
        {
            clearColors[i] = transparentColor;
        }

        canvas.SetPixels(clearColors);
        canvas.Apply();
    }

    void Update()
    {
        if (this.DrawTime >= 0.0)
        {
            this.DrawTime = 0;
            DrawSinusCurve();
        }
        else
            this.DrawTime += Time.deltaTime;
    }

    private void DrawSinusCurve()
    { 
        float CanvasHeight = canvas.height;
        float CanvasWidth = canvas.width;

        const float base_frequency = 2.0f * Mathf.PI;
        float frequency = base_frequency * intensity;

        canvas.SetPixels(clearColors);
        canvas.Apply();

        Color[] pixels = canvas.GetPixels();

        int halfWidth = curveWidth / 2;

        float valueFF = (startingValueY * 100);

        for (int x = 0; x < CanvasWidth; x++)
        {
            float radians = (x / (float)CanvasWidth) * frequency * 4;
            int y = Mathf.RoundToInt(Mathf.Sin(radians + valueFF) * (CanvasHeight / 4) + (CanvasHeight / 2));

            for (int i = -halfWidth; i <= halfWidth; i++)
            {
                int newY = y + i;

                if (newY >= 0 && newY < CanvasHeight)
                {
                    pixels[newY * Mathf.RoundToInt(CanvasWidth) + x] = this.BeatboxPlayerColor;
                }
            }
        }

        this.startingValueY += 0.1f * (Time.deltaTime);

        if (this.startingValueY > 1)
            this.startingValueY = -1;

        canvas.SetPixels(pixels);
        canvas.Apply();

        rawImage.texture = canvas;
    }


}
