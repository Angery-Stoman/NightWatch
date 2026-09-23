using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI Panel")]
    public GameObject pauseMenuPanel;

    [Header("Settings")]
    [Tooltip("Check this if the game requires the mouse cursor to be hidden while playing (e.g., Ship Game).")]
    public bool lockCursorOnResume = false; 

    private bool isPaused = false;

    void Start()
    {
        Time.timeScale = 1f; 
        AudioListener.pause = false;
        
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        
        Time.timeScale = 0f;         
        AudioListener.pause = true;  
        
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        
        Time.timeScale = 1f;          
        AudioListener.pause = false; 
        
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);

        if (lockCursorOnResume)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f; 
        AudioListener.pause = false;
        
        SceneManager.LoadScene("MainMenuScene"); 
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}