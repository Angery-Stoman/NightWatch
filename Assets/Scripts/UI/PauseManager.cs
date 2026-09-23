using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel; 

    [Header("Settings")]
    public string mainMenuSceneName = "MainMenuScene";
    
    [Tooltip("Check this box only for games where you can look around (like the Ship Game)")]
    public bool lockCursorOnResume = false; 

    private bool isPaused = false;

    void Start()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel != null && settingsPanel.activeInHierarchy)
            {
                CloseSettings();
            }
            else if (isPaused)
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
        
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true); 
        if (settingsPanel != null) settingsPanel.SetActive(false); 

        AudioListener.pause = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        EventSystem.current.SetSelectedGameObject(null);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; 
        
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false); 
        if (settingsPanel != null) settingsPanel.SetActive(false); 

        AudioListener.pause = false;

        if (lockCursorOnResume)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }


    public void OpenSettings()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
    }


    public void RetryGame()
    {
        Time.timeScale = 1f; 
        AudioListener.pause = false; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f; 
        AudioListener.pause = false; 
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game from pause menu...");
        Application.Quit();
    }
}