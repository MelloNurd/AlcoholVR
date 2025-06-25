using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Arcade : MonoBehaviour
{
    public static Arcade Instance { get; private set; }

    public float GameSpeed { get; private set; } = 0.5f; // Default game speed
    private float _maxGameSpeed = 2.5f; // Maximum game speed
    public bool IsGameRunning { get; private set; } = true;

    public Camera arcadeGameCamera;
    public GameObject arcadeBackgroundObj;

    [SerializeField] private GameObject _obstaclePrefab;
    private Vector3 _obstacleSpawnPosition;

    private MaterialScroll _bgScroll;
    [SerializeField] private ArcadePlayer _arcadePlayer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }

        _bgScroll = arcadeBackgroundObj.GetComponent<MaterialScroll>();

        _obstacleSpawnPosition = arcadeGameCamera.transform.position
            .WithZ(_arcadePlayer.transform.position.z)
            .AddX(arcadeGameCamera.orthographicSize + 1);

        _bgScroll.SetScrollSpeed(0, 0);

        _arcadePlayer.OnPlayerDeath.AddListener(() =>
        {
            _bgScroll.ScrollingActive = false;
            IsGameRunning = false;
            GameSpeed = 0f;
        });
    }

    private void Start()
    {
        StartSpawningObstacles();
    }

    private void Update()
    {
        if (!IsGameRunning) return;
        
        if(GameSpeed < _maxGameSpeed)
        {
            GameSpeed += Time.deltaTime * 0.01f; // Gradually increase game speed
            _bgScroll.SetScrollSpeedX(-GameSpeed);
        }

        // Temporary
        if(Input.GetKeyDown(KeyCode.Space))
        {
            ChangePlayerDirection();
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            SpawnObstacle();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Restart the game
        }
    }

    private async void StartSpawningObstacles()
    {
        while (IsGameRunning)
        {
            SpawnObstacle();
            await UniTask.Delay(System.TimeSpan.FromSeconds(1f / GameSpeed * Random.Range(0.95f, 1.05f)));
        }
    }

    public void ChangePlayerDirection()
    {
        _arcadePlayer.ChangeDirection();
    }

    public void SpawnObstacle()
    {
        ArcadeObstacle obstacle = Instantiate(_obstaclePrefab, _obstacleSpawnPosition.AddY(Random.Range(-4f, 4f)), Quaternion.identity).GetComponent<ArcadeObstacle>();
    }
}
