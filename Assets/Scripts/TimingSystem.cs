using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class TimingSystem : MonoBehaviour
{
    [Header("Panels")]
    public GameObject start_panel;
    public GameObject win_panel;
    public GameObject lose_panel;
    public GameObject pause_panel;

    [Header("References")]
    public PlayerStats player;

    [Header("UI")]
    public TextMeshProUGUI waveLabel;      // "Wave X"
    public TextMeshProUGUI killLabel;      // "Wave X: killed/needed"
    public TextMeshProUGUI loseStatsLabel; // "Kills: N\nWave: X"

    [Header("Wave Settings")]
    public bool useWaveSystem = true;
    public int baseEnemiesPerWave = 10;    // wave1=10, wave2=20, etc.
    public int maxWaves = 6;               // win after this wave in limited mode

    // wave state
    private bool endlessMode = false;
    private bool wavesRunning = false;
    private int currentWave = 0;
    private int enemiesToSpawnThisWave = 0;
    private int enemiesSpawnedThisWave = 0;
    private int enemiesKilledThisWave = 0;

    // general state
    private int totalKills = 0;
    private bool gamePaused = false;
    private bool gameEnded = false;

    void Awake()
    {
        Time.timeScale = 1f;
    }

    void Start()
    {
        if (!player) player = FindFirstObjectByType<PlayerStats>();

        if (player != null)
        {
            player.OnHPChanged += (hp, max) =>
            {
                if (!gameEnded && hp <= 0)
                    ShowLose();
            };
        }

        ShowAll(false);

        if (useWaveSystem)
        {
            if (start_panel) start_panel.SetActive(true);
            Time.timeScale = 0f; // wait for player to choose mode
        }
    }

    void Update()
    {
        bool pausePressed =
            (Keyboard.current?.pKey.wasPressedThisFrame ?? false) ||
            (Keyboard.current?.escapeKey.wasPressedThisFrame ?? false) ||
            (Gamepad.current?.startButton.wasPressedThisFrame ?? false);

        if (pausePressed && !gameEnded && (start_panel == null || !start_panel.activeSelf))
        {
            if (gamePaused) ResumeGame();
            else PauseGame();
        }
    }

    // ========== SPAWNPOINT API ==========

    public bool CanSpawnEnemy()
    {
        if (!useWaveSystem || !wavesRunning || gameEnded) return false;
        if (!endlessMode && currentWave > maxWaves) return false;
        if (enemiesSpawnedThisWave >= enemiesToSpawnThisWave) return false;
        return true;
    }

    public void RegisterEnemySpawned()
    {
        if (!useWaveSystem || !wavesRunning) return;
        enemiesSpawnedThisWave++;
    }

    // called from EnemyBase.Die()
    public void ReportEnemyKilled()
    {
        if (gameEnded) return;

        totalKills++;

        if (useWaveSystem && wavesRunning)
        {
            enemiesKilledThisWave++;

            if (killLabel)
            {
                killLabel.gameObject.SetActive(true);
                killLabel.text = $"Wave {currentWave}: {enemiesKilledThisWave}/{enemiesToSpawnThisWave}";
            }

            if (enemiesKilledThisWave >= enemiesToSpawnThisWave)
            {
                OnWaveCompleted();
            }
        }
        else
        {
            if (killLabel)
            {
                killLabel.gameObject.SetActive(true);
                killLabel.text = $"Kills: {totalKills}";
            }
        }
    }

    // ========== WAVE FLOW ==========

    public void OnStartEndless()
    {
        endlessMode = true;
        BeginWaveGame();
    }

    public void OnStartLimited()
    {
        endlessMode = false;
        BeginWaveGame();
    }

    private void BeginWaveGame()
    {
        if (!useWaveSystem) return;

        if (start_panel) start_panel.SetActive(false);
        Time.timeScale = 1f;

        gameEnded = false;
        wavesRunning = true;
        currentWave = 0;
        totalKills = 0;

        StartNextWave();
    }

    private void StartNextWave()
    {
        currentWave++;

        enemiesToSpawnThisWave = baseEnemiesPerWave * currentWave;
        enemiesSpawnedThisWave = 0;
        enemiesKilledThisWave = 0;

        if (waveLabel)
        {
            waveLabel.gameObject.SetActive(true);
            waveLabel.text = $"Wave {currentWave}";
        }

        if (killLabel)
        {
            killLabel.gameObject.SetActive(true);
            killLabel.text = $"Wave {currentWave}: 0/{enemiesToSpawnThisWave}";
        }
    }

    private void OnWaveCompleted()
    {
        if (!useWaveSystem || !wavesRunning || gameEnded) return;

        if (!endlessMode && currentWave >= maxWaves)
        {
            ShowWin();
            wavesRunning = false;
            return;
        }

        StartNextWave();
    }

    // ========== PANELS / END GAME ==========

    void ShowWin()
    {
        gameEnded = true;
        Time.timeScale = 0f;
        ShowAll(false);
        if (win_panel) win_panel.SetActive(true);
    }

    void ShowLose()
    {
        gameEnded = true;
        Time.timeScale = 0f;
        ShowAll(false);
        if (lose_panel) lose_panel.SetActive(true);

        if (loseStatsLabel)
        {
            loseStatsLabel.text = $"Kills: {totalKills}\nWave reached: {currentWave}";
        }
    }

    void ShowAll(bool on)
    {
        if (start_panel) start_panel.SetActive(on);
        if (pause_panel) pause_panel.SetActive(on);
        if (lose_panel) lose_panel.SetActive(on);
        if (win_panel) win_panel.SetActive(on);
    }

    // ========== PAUSE / BUTTONS ==========

    public void ResumeGame()
    {
        gamePaused = false;
        Time.timeScale = 1f;
        if (pause_panel) pause_panel.SetActive(false);
    }

    public void PauseGame()
    {
        if (gameEnded) return;
        gamePaused = true;
        Time.timeScale = 0f;
        if (pause_panel) pause_panel.SetActive(true);
    }

    public void OnResumeButton() => ResumeGame();

    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    public void OnMainMenuButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }
}
