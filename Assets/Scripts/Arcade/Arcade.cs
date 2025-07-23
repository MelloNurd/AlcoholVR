using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using PrimeTween;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[SelectionBase]
public class Arcade : MonoBehaviour
{
    public enum GameState
    {
        Menu,
        Transition,
        Playing,
        GameOver,
        Restarting
    }

    public static Arcade Instance { get; private set; }

    public float GameSpeed { get; private set; } = 0.5f; // Default game speed
    private float _maxGameSpeed = 2.5f; // Maximum game speed

    public int Score { get; set; } // Player score

    public GameState State { get; private set; } = GameState.Menu;

    [Header("Assignments")]
    public Camera arcadeGameCamera;
    public GameObject arcadeBackgroundObj;
    public ArcadePlayer _arcadePlayer;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private Image _scoreBG;
    [SerializeField] private SpriteRenderer _titleSprite;
    [SerializeField] private SpriteRenderer _subtitleSprite;

    [Header("Obstacles")]
    [SerializeField] private GameObject _obstaclePrefab;
    private Vector3 _obstacleSpawnPosition;
    private Transform _obstacleHolder;
    [HideInInspector] public List<ArcadeObstacle> obstacles = new List<ArcadeObstacle>();

    [Header("Details")]
    [SerializeField] private GameObject _detailPrefab;
    private Transform _detailHolder;
    [SerializeField] private Sprite[] _cloudSprites;
    [HideInInspector] public List<ArcadeDetail> details = new List<ArcadeDetail>();

    private MaterialScroll _bgScroll;

    private CancellationTokenSource _cancelToken;

    private Vector3 _titleStartPos;
    private Vector3 _subtitleStartPos;

    private void Awake()
    {
        _bgScroll = arcadeBackgroundObj.GetComponent<MaterialScroll>();

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }

        _obstacleSpawnPosition = arcadeGameCamera.transform.position
            .WithZ(_arcadePlayer.transform.position.z)
            .AddX(arcadeGameCamera.orthographicSize + 1);

        _arcadePlayer.OnPlayerDeath.AddListener(() =>
        {
            EndGame();
        });

        var root = new GameObject("ArcadeObjects").transform;
        _obstacleHolder = new GameObject("Obstacles").transform;
        _obstacleHolder.SetParent(root);
        _detailHolder = new GameObject("Details").transform;
        _detailHolder.SetParent(root);

        _titleStartPos = _titleSprite.transform.position;
        _subtitleStartPos = _subtitleSprite.transform.position;
    }

    private void Start()
    {
        InitializeGame();
    }

    private void Update()
    {
        IncreaseGameSpeed();
    }

    [Button("Simulate Button Press")]
    public void OnButtonPress()
    {
        // Nothing for GameState.Transitions
        if (State == GameState.Menu)
        {
            StartGame();
        }
        else if (State == GameState.Playing)
        {
            ChangePlayerDirection();
        }
        else if (State == GameState.GameOver)
        {
            RestartGame();
        }
    }

    #region Game Sequence Logic
    public void InitializeGame(bool restarting = false)
    {
        State = restarting ? GameState.Restarting : GameState.Menu;
        _cancelToken?.Cancel();
        _cancelToken = null;

        _arcadePlayer.RestartPlayer(arcadeGameCamera.transform.position.WithZ(0));
        _arcadePlayer.SetDirection(0);

        GameSpeed = 0.5f; // Reset game speed
        _bgScroll.SetScrollSpeed(-GameSpeed, 0);
        _bgScroll.ScrollingActive = true;
        _arcadePlayer.shouldMove = false;
        _arcadePlayer.transform.position = _arcadePlayer.transform.position.WithY(arcadeGameCamera.transform.position.y);
        obstacles.DestroyAllAndClear();
        details.DestroyAllAndClear();
        _scoreText.text = "";
        _scoreText.alpha = 1;
        _scoreText.rectTransform.anchoredPosition = new Vector3(0, 135, 0);

        _titleSprite.transform.position = _titleStartPos;
        _subtitleSprite.transform.position = _subtitleStartPos;

        RunTitlePositionAnimations();
        RunTitleRotationAnimations();
        RunSubtitleScaleAnimations();
    }

    public async void StartGame()
    {
        State = GameState.Transition;

        StartSpawningClouds();
        Tween.StopAll(_titleSprite.transform);
        Tween.StopAll(_subtitleSprite.transform);
        _ = Tween.Position(_titleSprite.transform, _titleStartPos.AddY(7), 0.5f, Ease.InOutCubic);
        _ = Tween.Position(_subtitleSprite.transform, _subtitleStartPos.AddY(-7), 0.5f, Ease.InOutCubic);
        await Tween.PositionX(_arcadePlayer.transform, -2.87f, 1.5f);

        State = GameState.Playing;
        _arcadePlayer.shouldMove = true;
        UpdateScore(0);
        _ = Tween.UIAnchoredPosition(_scoreText.rectTransform, Vector2.zero, 0.5f, Ease.OutExpo);
        _arcadePlayer.SetDirection(1);
        StartSpawningObstacles();
    }

    // The "Update" of the game sequence logic
    private void IncreaseGameSpeed()
    {
        if(State != GameState.Playing) return;

        if (GameSpeed < _maxGameSpeed)
        {
            GameSpeed += Time.deltaTime * 0.01f; // Gradually increase game speed
            _bgScroll.SetScrollSpeed(-GameSpeed, 0);
        }
    }

    public async void EndGame()
    {
        _bgScroll.ScrollingActive = false;
        State = GameState.Transition;

        int highScore = PlayerPrefs.GetInt("Arcade_HighScore", 0);
        if( Score > highScore)
        {
            PlayerPrefs.SetInt("Arcade_HighScore", Score);
            GlobalStats.arcadeScore = Score;
            PlayerPrefs.Save();
        }

        _cancelToken?.Cancel();

        await Tween.UIAnchoredPosition(_scoreText.rectTransform, new Vector3(0, 135, 0), 0.5f, Ease.OutExpo);
        _scoreText.text = "";
        _scoreBG.transform.localScale = new Vector3(0, 0.4f, 1);
        await Tween.Scale(_scoreBG.transform, new Vector3(1, 0.4f, 1), 0.5f, Ease.OutExpo);
        await UniTask.Delay(250); // Wait for the score text to clear
        _scoreText.rectTransform.anchoredPosition = new Vector3(0, -315, 0);
        _scoreText.text = $"Score: {Score}";
        await UniTask.Delay(1000);
        _scoreText.text = $"Score: {Score}\n Highscore: {PlayerPrefs.GetInt("Arcade_HighScore", Score)}";
        await UniTask.Delay(100);
        State = GameState.GameOver;
    }

    public async void RestartGame()
    {
        State = GameState.Restarting;
        _ = Tween.Alpha(_scoreText, 0f, 0.25f);
        _ = Tween.Alpha(_scoreBG, 1f, 0.4f);
        await Tween.Scale(_scoreBG.transform, Vector3.one, 0.3f, Ease.InExpo);
        InitializeGame(true);
        await UniTask.Delay(250);
        await Tween.Scale(_scoreBG.transform, Vector3.right, 0.4f, Ease.OutExpo);
        _scoreBG.color = Color.black.WithAlpha(0.87f);
        State = GameState.Menu;
    }

    #endregion

    #region Dedicated Animation Methods
    private async void RunTitlePositionAnimations()
    {
        while (State == GameState.Menu)
        {
            await Tween.PositionY(_titleSprite.transform, _titleStartPos.y + 0.1f, 1.012f, Ease.InOutSine);
            if (State != GameState.Menu) return;
            await Tween.PositionY(_titleSprite.transform, _titleStartPos.y - 0.1f, 1.012f, Ease.InOutSine);
        }
    }

    private async void RunTitleRotationAnimations()
    {
        while (State == GameState.Menu)
        {
            await Tween.Rotation(_titleSprite.transform, Quaternion.Euler(0, 0, 2), 0.9157f, Ease.InOutSine);
            if (State != GameState.Menu) return;
            await Tween.Rotation(_titleSprite.transform, Quaternion.Euler(0, 0, -2), 0.9157f, Ease.InOutSine);
        }
    }

    private async void RunSubtitleScaleAnimations()
    {
        while (State == GameState.Menu)
        {
            await Tween.Scale(_subtitleSprite.transform, Vector3.one * 1.6f, 0.5f, Ease.InOutSine);
            if (State != GameState.Menu) return;
            await Tween.Scale(_subtitleSprite.transform, Vector3.one * 1.4f, 0.5f, Ease.InOutSine);
        }
    }
    #endregion

    private async void StartSpawningObstacles()
    {
        if (_cancelToken != null)
        {
            _cancelToken.Dispose();
        }
        _cancelToken = new CancellationTokenSource();

        while (State == GameState.Playing)
        {
            SpawnObstacle();
            await UniTask.Delay(System.TimeSpan.FromSeconds(1f / GameSpeed * Random.Range(0.95f, 1.05f)), cancellationToken: _cancelToken.Token).SuppressCancellationThrow();
            if (_cancelToken.Token.IsCancellationRequested)
            {
                return;
            }
        }
    }
    
    private async void StartSpawningClouds()
    {
        if (_cancelToken != null)
        {
            _cancelToken.Dispose();
        }
        while (State != GameState.Menu && State != GameState.Restarting)
        {
            if (_cancelToken == null || _cancelToken.IsCancellationRequested)
            {
                _cancelToken = new CancellationTokenSource();
            }

            float speed = GameSpeed * Random.Range(0.4f, 0.6f);
            Sprite sprite = _cloudSprites.GetRandom();
            Vector3 size = Vector3.one * Random.Range(1, 2.5f);

            ArcadeDetail detail = SpawnDetail(
                sprite, // sprite
                Random.Range(0.25f, 0.5f) * GameSpeed, // alpha
                5, // order in layer
                size, // scale
                speed // speed
            );

            float shadowOffset = Random.Range(1.2f, 1.6f);

            ArcadeDetail detailShadow = SpawnDetail(
                sprite, // sprite
                0.2f, // alpha
                4, // order in layer
                size * shadowOffset, // scale
                speed * shadowOffset // speed
            );
            detailShadow.transform.position = detail.transform.position.WithY(arcadeGameCamera.transform.position.y - (detail.transform.position.y - arcadeGameCamera.transform.position.y)*0.5f);
            detailShadow.spriteRenderer.color = Color.black.WithAlpha(0.2f);

            await UniTask.Delay(System.TimeSpan.FromSeconds(Random.Range(1f, 5f)), cancellationToken: _cancelToken.Token).SuppressCancellationThrow();
        }
    }

    public void ChangePlayerDirection()
    {
        _arcadePlayer.ChangeDirection();
    }

    public ArcadeObstacle SpawnObstacle()
    {
        ArcadeObstacle obstacle = Instantiate(_obstaclePrefab, _obstacleSpawnPosition.AddY(Random.Range(-2.5f, 2.5f)), Quaternion.identity, _obstacleHolder).GetComponent<ArcadeObstacle>();
        obstacles.Add(obstacle);
        return obstacle;
    }

    public ArcadeDetail SpawnDetail(Sprite sprite, float alpha, int order, Vector3 scale, float speed)
    {
        ArcadeDetail detail = Instantiate(_detailPrefab, _obstacleSpawnPosition.AddY(Random.Range(-4f, 4f)).AddX(3), Quaternion.identity, _detailHolder).GetComponent<ArcadeDetail>();
        detail.Initialize(sprite, alpha, order, scale, speed);
        details.Add(detail);
        return detail;
    }
    
    public void UpdateScore(int score)
    {
        Score = score;
        _scoreText.text = Score.ToString();
    }
}
