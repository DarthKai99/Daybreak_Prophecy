using UnityEngine;
using UnityEngine.InputSystem;   // NEW Input System
using UnityEngine.SceneManagement;
using TMPro;                    // for TextMeshProUGUI


public class TimingSystem : MonoBehaviour
{
    //game objects panels
    [Header("Panels")]
    public GameObject win_panel;
    public GameObject lose_panel;
    public GameObject victory_panel;
    public GameObject pause_panel;


    [Header("Refs")]
    public PlayerStats player;

    [Header("Side Quest")]
    public int sideQuestEnemyGoal = 5;
    public int sideQuestXpReward = 10;
    private int sideQuestKills = 0;
    private bool sideQuestCompleted = false;

    [Header("Kill Indicator UI")]
    public TextMeshProUGUI killLabel;
    public float killShowDuration = 2f;

    [Header("Optional Kill Goal (for quests/tasks)")]
    public bool killGoalEnabled = false;
    public string killGoalName = "Enemies";
    public int killGoalTarget = 10;
    private int killGoalProgress = 0;

    private int totalKills = 0;
    private float killHideTimer = 0f;

    private bool gamePaused = false;
    private bool gameEnded = false;
    private float victoryHideTimer = -1f; // timer for hiding the victory panel

    void Awake()
    {
        Time.timeScale = 1f;
    }

    void Start()
    {
        if (!player) player = FindFirstObjectByType<PlayerStats>();
        ShowAll(false);

        if (killLabel) killLabel.gameObject.SetActive(false);

        if (!sideQuestCompleted && sideQuestEnemyGoal > 0)
            StartKillGoal(sideQuestEnemyGoal, "Enemies");

        if (player != null)
        {
            player.OnHPChanged += (hp, max) =>
            {
                if (!gameEnded && hp <= 0) ShowLose();
            };
        }
    }

    void Update()
    {
        // Pause handling
        bool pausePressed =
            (Keyboard.current?.pKey.wasPressedThisFrame ?? false) ||
            (Keyboard.current?.escapeKey.wasPressedThisFrame ?? false) ||
            (Gamepad.current?.startButton.wasPressedThisFrame ?? false);

        if (pausePressed && !gameEnded)
        {
            if (gamePaused) ResumeGame();
            else PauseGame();
        }

        // Auto-hide kill indicator
        if (killLabel && killLabel.gameObject.activeSelf && !killGoalEnabled)
        {
            killHideTimer -= Time.unscaledDeltaTime;
            if (killHideTimer <= 0f) killLabel.gameObject.SetActive(false);
        }

        // Auto-hide victory panel after 3 seconds
        if (victory_panel && victory_panel.activeSelf && victoryHideTimer >= 0f)
        {
            victoryHideTimer -= Time.unscaledDeltaTime;
            if (victoryHideTimer <= 0f)
            {
                victory_panel.SetActive(false);
                victoryHideTimer = -1f; // reset timer
            }
        }
    }

    // ========== PUBLIC HOOKS ==========
    public void ReportEnemyKilled()
    {
        if (gameEnded) return;

        totalKills++;

        // ---- 1) Update generic kill goal ONCE (if active) ----
        if (killGoalEnabled)
        {
            killGoalProgress = Mathf.Clamp(killGoalProgress + 1, 0, killGoalTarget);
            UpdateKillLabelForGoal(); // stays visible while goal active

            if (killGoalProgress >= killGoalTarget)
            {
                ShowVictory();
                ClearKillGoal(); // hides the indicator
            }
        }
        else
        {
            // No active goal: show transient "Kills: N"
            if (killLabel)
            {
                killLabel.text = $"Kills: {totalKills}";
                killLabel.gameObject.SetActive(true);
                killHideTimer = killShowDuration;
            }
        }

        // ---- 2) Side quest progress (separate from the generic goal) ----
        if (!sideQuestCompleted && sideQuestEnemyGoal > 0)
        {
            sideQuestKills++;
            if (sideQuestKills >= sideQuestEnemyGoal)
            {
                sideQuestCompleted = true;
                if (player) player.AddXP(sideQuestXpReward);
                ShowVictory();
                ClearKillGoal(); // if you were showing a goal for this, hide it
            }
        }

    }

    public void ReportBossDefeated()
    {
        if (gameEnded) return;
        ShowWin();
        ClearKillGoal();
    }

    public void StartKillGoal(int target, string displayName = "Enemies")
    {
        killGoalEnabled = true;
        killGoalTarget = Mathf.Max(1, target);
        killGoalProgress = 0;
        killGoalName = string.IsNullOrWhiteSpace(displayName) ? "Enemies" : displayName;

        UpdateKillLabelForGoal();
        if (killLabel) killLabel.gameObject.SetActive(true);
    }

    public void ClearKillGoal()
    {
        killGoalEnabled = false;
        if (killLabel) killLabel.gameObject.SetActive(false);
    }

    // ========== PANEL LOGIC ==========
    void ShowWin()
    {
        gameEnded = true;
        Time.timeScale = 0f;
        ShowOnly(win_panel);
    }

    void ShowLose()
    {
        gameEnded = true;
        Time.timeScale = 0f;
        ShowOnly(lose_panel);
    }

    void ShowVictory()
    {
        if (victory_panel)
        {
            victory_panel.SetActive(true);
            victoryHideTimer = 2f; // hides after 3 seconds
        }
    }

    void ShowOnly(GameObject panel)
    {
        ShowAll(false);
        if (panel) panel.SetActive(true);
    }

    void ShowAll(bool on)
    {
        if (pause_panel) pause_panel.SetActive(on);
        if (lose_panel) lose_panel.SetActive(on);
        if (win_panel) win_panel.SetActive(on);
        if (victory_panel) victory_panel.SetActive(on);
    }

    // ========== PAUSE ==========
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

    // ========== UI BUTTONS ==========
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
        SceneManager.LoadScene("MainMenu"); // replace with your menu scene name
    }

    public void OnQuitButton()
    {
        Application.Quit();
    }

    // ========== HELPERS ==========
    void UpdateKillLabelForGoal()
    {
        if (!killLabel) return;
        killLabel.text = $"{killGoalName}: {killGoalProgress}/{killGoalTarget}";
    }
}
