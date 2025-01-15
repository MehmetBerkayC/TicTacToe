using UnityEngine;
using UnityEngine.UI;

public class MultiplayerMenuUI : MonoBehaviour
{
    public static MultiplayerMenuUI Instance { get; private set; }

    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button joinButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Setup Events
        createLobbyButton.onClick.AddListener(() =>
        {
            Hide();
        });

        quickJoinButton.onClick.AddListener(() =>
        {
            Hide();
        });

        joinButton.onClick.AddListener(() =>
        {
            Hide();
        });
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
