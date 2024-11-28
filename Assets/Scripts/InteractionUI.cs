using UnityEngine;
using TMPro;
public class InteractionUI : MonoBehaviour
{
    [SerializeField] private float interactionRange = 5f; // Die Reichweite des Raycasts
    [SerializeField] private TextMeshProUGUI doorText; // Referenz zu deinem "Door"-UI-Text
    [SerializeField] private TextMeshProUGUI itemText; // Referenz zu deinem "Item"-UI-Text

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;

        // Sicherstellen, dass UI-Texte zu Beginn deaktiviert sind
        if (doorText != null) doorText.gameObject.SetActive(false);
        if (itemText != null) itemText.gameObject.SetActive(false);
    }

    private void Update()
    {
        CheckForInteractables();
    }

    private void CheckForInteractables()
    {
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        Debug.DrawRay(ray.origin, ray.direction * interactionRange, Color.red); // Debug Ray anzeigen

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            Debug.Log($"Raycast hit: {hit.collider.name} with tag: {hit.collider.tag}"); // Logge Trefferinformationen

            if (hit.collider.CompareTag("Door"))
            {
                ShowUI(doorText, "[E]");
                HideUI(itemText);
            }
            else if (hit.collider.CompareTag("Item"))
            {
                ShowUI(itemText, "[E] Pick Up");
                HideUI(doorText);
            }
            else
            {
                HideUI(doorText);
                HideUI(itemText);
            }
        }
        else
        {
            HideUI(doorText);
            HideUI(itemText);
        }
    }

    private void ShowUI(TextMeshProUGUI uiElement, string message)
    {
        if (uiElement != null)
        {
            uiElement.text = message;
            uiElement.gameObject.SetActive(true);
        }
    }

    private void HideUI(TextMeshProUGUI uiElement)
    {
        if (uiElement != null)
        {
            uiElement.gameObject.SetActive(false);
        }
    }
}
