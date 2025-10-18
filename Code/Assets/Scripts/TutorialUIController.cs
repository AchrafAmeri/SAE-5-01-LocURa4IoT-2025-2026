using UnityEngine;

public class TutorialUIController : MonoBehaviour
{
    [SerializeField] private GameObject tutorialCanvas;

    // Appelé quand le joueur se connecte
    public void ShowTutorial()
    {
        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(true);
    }

    // Appelé si le joueur clique sur "Skip"
    public void HideTutorial()
    {
        if (tutorialCanvas != null) tutorialCanvas.SetActive(false);
            tutorialCanvas.SetActive(false);
    }
}
