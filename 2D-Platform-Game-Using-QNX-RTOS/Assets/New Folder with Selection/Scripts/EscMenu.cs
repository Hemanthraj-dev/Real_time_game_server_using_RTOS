using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class EscMenu : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private GameObject escMenu;

    [Header("Default Selected Button")]
    [SerializeField] private GameObject resumeButton;

    [Header("Music")]
    [SerializeField] private GameObject gameplayMusic;
    [SerializeField] private GameObject pauseMusic;

    private AudioSource[] gameplayAudioSources;

    private bool isPaused = false;

    private void Start()
    {
        // Pause menu hidden
        escMenu.SetActive(false);

        Time.timeScale = 1f;

        // Make sure GameplayMusic stays active
        gameplayMusic.SetActive(true);

        // Get all AudioSources inside GameplayMusic
        gameplayAudioSources =
            gameplayMusic.GetComponentsInChildren<AudioSource>(true);

        // Pause music disabled
        pauseMusic.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
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

        // Show pause menu
        escMenu.SetActive(true);

        // Pause gameplay
        Time.timeScale = 0f;

        // Pause existing gameplay music
        foreach (AudioSource source in gameplayAudioSources)
        {
            if (source != null)
            {
                source.Pause();
            }
        }

        // Start pause music
        pauseMusic.SetActive(true);

        // Select Resume button
        if (EventSystem.current != null && resumeButton != null)
        {
            EventSystem.current.SetSelectedGameObject(resumeButton);
        }
    }

    public void ResumeGame()
    {
        isPaused = false;

        // Hide pause menu
        escMenu.SetActive(false);

        // Resume gameplay
        Time.timeScale = 1f;

        // Stop pause music
        pauseMusic.SetActive(false);

        // Resume gameplay music
        foreach (AudioSource source in gameplayAudioSources)
        {
            if (source != null)
            {
                source.UnPause();
            }
        }

        // Clear selection
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;

        // Stop pause music
        pauseMusic.SetActive(false);

        // Resume gameplay music before leaving
        foreach (AudioSource source in gameplayAudioSources)
        {
            if (source != null)
            {
                source.UnPause();
            }
        }

        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

        Application.Quit();

#if UNITY_EDITOR
        Debug.Log("Quit Game pressed");
#endif
    }
}