using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;


/// <summary>
/// GameManager for this project.
/// </summary>
public class AsterX : MonoBehaviour
{
    [SerializeField] private GameObject _playerPF;
    [SerializeField] private Collider screenBounds;
    public Collider deathBounds;
    [SerializeField] private ParticleSystem respawnFX;
    [SerializeField] private AudioSource level;
    [SerializeField] private AudioSource start;

    public AudioMixerSnapshot introSnap;
    public AudioMixerSnapshot gameSnap;

    private static AsterX _asterX;
    public static AsterX Instance
    {
        get { return _asterX; }
        private set
        {
            if (value != null)
            {
                Debug.LogError("[AsterX] There is already an instance of the static feild AsterX");
                return;
            }
            else
            {
                _asterX = value;
            }
        }
    }

    public List<Asteroid> pooledAsteroids;
    public int amountToPool;

    public AsteroidScriptableObject asteroid_SO;

    public int Score;
    public int Lives;
    public float asteroidRespawnRate;
    [SerializeField] float playerRespawnClearDistance = 20f;

    public string playerName;

    public Text scoreText;
    [SerializeField] Text livesText;
    [SerializeField] Slider ammoSlider;
    public Animator animator;

    bool isPlayerAlive = true;

    [System.Flags]
    public enum State
    {
        PREGAME,
        PAUSED,
        PLAY,
        GAME_OVER
    }

    [SerializeField]
    private State gameState;

    public State GAME_STATE
    {
        get { return gameState; }
        private set { gameState = value; }
    }

    public delegate void GameStateChanged(State newState);
    public static GameStateChanged GAME_STATE_CHANGED;

    public delegate void ScoreChanged(int score);
    public static ScoreChanged SCORE_CHANGED;

    public delegate void AsteroidCount(int rootAsteroids);
    public static AsteroidCount ASTEROID_COUNT;

    private void Awake()
    {
        _asterX = this;
    }

    private void Start()
    {
        if (SaveGameManager.playerName != null || SaveGameManager.playerName != "")
        {
            GetComponentInChildren<InputField>().text = SaveGameManager.playerName;
        }

        pooledAsteroids = new List<Asteroid>();
        Asteroid tmp;
        for(int i = 0; i < amountToPool; i++)
        {
            tmp = Instantiate(asteroid_SO.PickRandomModel(), Vector3.zero, Quaternion.identity).GetComponent<Asteroid>();
            tmp.gameObject.SetActive(false);
            pooledAsteroids.Add(tmp);
        }

        // Spawn 3 asteroids for background during start screen.
        SpawnAsteroids();
        SpawnAsteroids();
        SpawnAsteroids();

        SaveGameManager.Load();
        introSnap.TransitionTo(0.5f);
    }

    public Asteroid GetPooledAsteroid()
    {
        foreach(Asteroid a in pooledAsteroids)
        {
            if (!a.gameObject.activeInHierarchy)
            {
                return a;
            }
        }
        Debug.Log("[AsterX] Adding to pool");
        Asteroid tmp = Instantiate(asteroid_SO.PickRandomModel(), Vector3.zero, Quaternion.identity).GetComponent<Asteroid>();
        tmp.gameObject.SetActive(false);
        pooledAsteroids.Add(tmp);
        return tmp;
    }

    private void PlayPause()
    {
        if (gameState == State.PLAY)
        {
            ChangeGameState(State.PAUSED);
        }
        else if (gameState == State.PAUSED)
        {
            ChangeGameState(State.PLAY);
        }

    }

    public void DeleteSaveFile()
    {
        SaveGameManager.DeleteSave();
    }


    public void SpawnAsteroids()
    {
        Asteroid ast = GetPooledAsteroid();
        ast.gameObject.SetActive(true);
        ast.Initialize();    
    }

    void ChangeGameState(State newState)
    {
        State oldState = gameState;
        gameState = newState;

        switch (gameState)
        {
            case State.PREGAME:
                Time.timeScale = 1;
                break;
            case State.PLAY:
                Time.timeScale = 1;
                break;
            case State.PAUSED:
                Time.timeScale = 0;
                break;
            case State.GAME_OVER:
                Time.timeScale = 0;
                break;
        }
        GAME_STATE_CHANGED.Invoke(newState);
    }

    public void StartButton()
    {
        animator.SetBool("start", true);
        level.Play();
        start.Play();
        gameSnap.TransitionTo(2);
    }

    public void PauseButton()
    {
        if (gameState == State.PLAY)
        {
            ChangeGameState(State.PAUSED);
        }
        else if (gameState == State.PAUSED)
        {
            ChangeGameState(State.PLAY);
        }
    }

    public void StartGame()
    {
        DisableAllAsteroids();
        StartCoroutine(RespawnPlayer(Vector3.zero));
        InvokeRepeating("SpawnAsteroids", 0.5f, asteroidRespawnRate);
        ChangeGameState(State.PLAY);
    }

    void DisableAllAsteroids()
    {
        foreach (Asteroid ast in pooledAsteroids)
        {
            ast.transform.SetParent(null);
            ast.gameObject.SetActive(false);
        }
    }

    //public void AddAsteroid(Asteroid ast)
    //{
    //    pooledAsteroids.Add(ast);

    //    int count = 0;
    //    foreach (Asteroid a in pooledAsteroids)
    //    {
    //        if (a.transform.root == a.transform && a != null)
    //        {
    //            count++;
    //        }
    //    }
    //    ASTEROID_COUNT.Invoke(count);
    //}

    public void RemoveAsteroid(Asteroid ast)
    {
        int count = 0;
        foreach (Asteroid a in pooledAsteroids)
        {
            if (a.transform.root == a.transform && a != null)
            {
                count++;
            }
        }
        ASTEROID_COUNT.Invoke(count);

        pooledAsteroids.Remove(ast);
    }

    public void PlayerDied()
    {
        Bullet.hitInARowCounter = 0;

        SaveGameManager.Save(true);

        if (isPlayerAlive)
        {
            isPlayerAlive = false;
            Lives--;
            livesText.text = "Lives: " + Lives;
            if (Lives == 0)
            {
                //game over
                animator.SetBool("gameOver", true);
                level.Stop();
            }
            else
            {
                StartCoroutine(RespawnPlayer());
            }
        }

    }

    public IEnumerator RespawnPlayer()
    {
        Debug.Log("Respawn Player");

        yield return new WaitForSeconds(1.5f);
        Vector3 loc = FindSafeLocation();
        Instantiate(respawnFX, loc, Quaternion.identity);
        respawnFX.Play();

        yield return new WaitForSeconds(.5f);
        Instantiate(_playerPF, loc, Quaternion.identity);
        isPlayerAlive = true;
    }

    public IEnumerator RespawnPlayer(Vector3 location)
    {
        Debug.Log("Respawn Player");

        Instantiate(respawnFX, location, Quaternion.identity);
        respawnFX.Play();

        yield return new WaitForSeconds(.5f);
        Instantiate(_playerPF, location, Quaternion.identity);
    }

    /// <summary>
    /// Picks a random point to respawn. Picks a new location if it is too close to an asteroid.
    /// </summary>
    /// <returns></returns>
    private Vector3 FindSafeLocation()
    {
        bool notSafe = true;
        Vector3 location = FindRandomLocation();

        while (notSafe)
        {
        Start:
            foreach (Asteroid g in pooledAsteroids)
            {
                if (g != null)
                {
                    if (Mathf.Abs(Vector3.SqrMagnitude(location - g.transform.position)) < playerRespawnClearDistance)
                    {
                        Debug.Log("Need to check again");
                        location = FindRandomLocation();
                        goto Start;
                    }
                    else
                    {
                        notSafe = false;
                    }
                }

            }
        }
        return location;
    }

    private Vector3 FindRandomLocation()
    {
        return new Vector3(Random.Range(-screenBounds.bounds.extents.x + 5, screenBounds.bounds.extents.x - 5), Random.Range(-screenBounds.bounds.extents.y + 3, screenBounds.bounds.extents.y - 3));
    }

    public int RandomNeg()
    {
        int num = Random.Range(0, 2);
        if (num == 0)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }

    public IEnumerator GameOver()
    {
        AnalyticsEvent.GameOver(null, new Dictionary<string, object>
        {
            {"User Info", "Player: " +playerName+ ", Final score: "  +Score+ ", New high score? " +SaveGameManager.CheckHighScore(Score)+ ", Time: " +System.DateTime.Now}
        });
        AnalyticsEvent.Custom("Deaths", new Dictionary<string, object> {
            {"User Info", "Player: " +playerName+ ", Total Deaths: "  +SaveGameManager.deaths+ ", Time: " +System.DateTime.Now}
        });
        Analytics.FlushEvents();

        
        

        SaveGameManager.CheckHighScore(Score);
        SaveGameManager.Save(false);
        ChangeGameState(State.GAME_OVER);
        yield return new WaitForSecondsRealtime(5f);
        Time.timeScale = 1;
        SceneManager.LoadScene("_Scene_0");
    }

    public void AddScore(int size)
    {
        if (size == 3)
        {
            Score += asteroid_SO.size3Score;
        }
        else if (size == 2)
        {
            Score += asteroid_SO.size2Score;
        }
        else if (size == 1)
        {
            Score += asteroid_SO.size1Score;
        }

        scoreText.text = Score.ToString();

        if (SCORE_CHANGED != null)
        {
            SCORE_CHANGED.Invoke(Score);
        }

    }

    public void PopupDone()
    {
        GetComponentInChildren<AchievementManager>().PopupDone();
    }

    private void OnApplicationQuit()
    {
        SaveGameManager.CheckHighScore(Score);
        SaveGameManager.Save(false);

        //AnalyticsEvent.Custom("Deaths", new Dictionary<string, object> {
        //    {"User Info", "Player: " +playerName+ ", Total Deaths: "  +SaveGameManager.deaths+ ", Time: " +System.DateTime.Now}
        //});
        Analytics.FlushEvents();
    }

    public void GetPlayerName(string name)
    {
        playerName = name;
    }

    public void UpdateAmmoSlider(float ammo)
    {
        ammoSlider.value = ammo;
    }


}
