using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public GameObject menuPanel;

    void Start()
    {
        menuPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        bool isActive = !menuPanel.activeSelf;
        menuPanel.SetActive(isActive);
        Time.timeScale = isActive ? 0 : 1;
    }

    public void OnSaveClicked()
    {
        SaveManager.Instance.SaveGame();
    }

    public void OnLoadClicked()
    {
        SaveManager.Instance.LoadGame();
        ToggleMenu(); // Menüyü kapat
    }
    
    public void OnQuitClicked()
    {
        Application.Quit();
    }
}