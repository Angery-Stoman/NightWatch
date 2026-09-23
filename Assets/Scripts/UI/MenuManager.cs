using UnityEngine;
using UnityEngine.SceneManagement; 

public class MenuManager : MonoBehaviour
{
    public enum GameMode { Learning, VisualChallenge, AudioChallenge }
    public enum Difficulty { Easy, Medium, Hard }

    public static GameMode selectedMode; 
    public static Difficulty selectedDifficulty = Difficulty.Easy; 
    private GameObject lastActivePanel;

    [Header("Main Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;
    public GameObject settingsPanel;
    public GameObject difficultySelectPanel; 

    [Header("Level Select Sub-Panels")]
    public GameObject levelsList;
    public GameObject[] allModePanels;

    private void Start()
    {
        ShowMainMenu(); 
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        levelSelectPanel.SetActive(false);
        settingsPanel.SetActive(false);
        if (difficultySelectPanel != null) difficultySelectPanel.SetActive(false);
    }

    public void ShowLevelSelect()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
        settingsPanel.SetActive(false);
        if (difficultySelectPanel != null) difficultySelectPanel.SetActive(false);
        
        foreach (GameObject panel in allModePanels)
        {
            panel.SetActive(false);
        }

        levelsList.SetActive(true);
    }

    public void ShowSettings()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
        settingsPanel.SetActive(true);
        if (difficultySelectPanel != null) difficultySelectPanel.SetActive(false);
    }

    public void ShowDifficultySelect()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
        settingsPanel.SetActive(false);
        difficultySelectPanel.SetActive(true);
    }

    public void OpenSpecificModeMenu(GameObject menuToOpen)
    {
        levelsList.SetActive(false); 
        menuToOpen.SetActive(true);  
        lastActivePanel = menuToOpen;
    }

    public void CloseModeMenu(GameObject menuToClose)
    {
        menuToClose.SetActive(false); 
        levelsList.SetActive(true);   
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game..."); 
        Application.Quit(); 
    }


    public void StartLevel1_Learning() 
    { 
        selectedMode = GameMode.Learning;
        ShowDifficultySelect(); 
    }
    
    public void StartLevel1_Visual() 
    { 
        selectedMode = GameMode.VisualChallenge;
        selectedDifficulty = Difficulty.Hard; 
        SceneManager.LoadScene("FirstGameScene"); 
    }
    
    public void StartLevel1_Audio() 
    { 
        selectedMode = GameMode.AudioChallenge;
        ShowDifficultySelect(); 
    }


    public void SelectEasyAndStart()
    {
        selectedDifficulty = Difficulty.Easy;
        SceneManager.LoadScene("FirstGameScene");
    }

    public void SelectMediumAndStart()
    {
        selectedDifficulty = Difficulty.Medium;
        SceneManager.LoadScene("FirstGameScene");
    }

    public void SelectHardAndStart()
    {
        selectedDifficulty = Difficulty.Hard;
        SceneManager.LoadScene("FirstGameScene");
    }

    public void CancelDifficultySelection()
    {
        difficultySelectPanel.SetActive(false); 
        levelSelectPanel.SetActive(true); 
        
        if (lastActivePanel != null)
        {
            lastActivePanel.SetActive(true);
        }
        else
        {
            levelsList.SetActive(true);
        }
    }

    public void StartShipChallenge()
    {
        SceneManager.LoadScene("ShipChallengeScene"); 
    }
        public void StartRadarChallenge()
    {
        SceneManager.LoadScene("RadarGameScene"); 
    }
    public void StartRadioChallenge()
    {
        SceneManager.LoadScene("RadioGameScene"); 
    }
}