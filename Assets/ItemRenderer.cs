using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ItemRenderer : MonoBehaviour
{
    private RenderTexture camOutTexture = null;

    [SerializeField]
    Camera itemCamera = null;

    [SerializeField]
    RawImage targetOutput = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var display = Display.main;

        var rectTransform = targetOutput.rectTransform;

        //rectTransform..width = display.renderingWidth;
        //rectTransform.rectTransform.rect.height = display.renderingHeight;

        targetOutput.texture.width = display.renderingWidth;
        targetOutput.texture.height = display.renderingHeight;

        camOutTexture = new RenderTexture(display.renderingWidth, display.renderingHeight, 24);

        activate(true);
    }

    public void activate(bool activate)
    {
        if (activate)
        {
            itemCamera.targetTexture = camOutTexture;
            targetOutput.texture = camOutTexture;
        }
        else
        { 
            itemCamera.targetTexture = null;
            targetOutput.texture = null;
        }
    }
}
