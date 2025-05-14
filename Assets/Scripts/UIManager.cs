using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject startPanel;
    public GameObject hole;
    public GameObject gameOverPanel;

    void Awake()
    {
        ShowStart();
    }

    public void ShowStart()
    {
        startPanel.SetActive(true);
        hole.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void ShowGame()
    {
        startPanel.SetActive(false);
        hole.SetActive(true);
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        startPanel.SetActive(false);
        hole.SetActive(false);
        gameOverPanel.SetActive(true);
    }

    public void OnMainMenuButton()
    {
        GameManager.Instance.EnterStartState();
    }


}
