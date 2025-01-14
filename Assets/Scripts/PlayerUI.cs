using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject crossArrowGameObject;
    [SerializeField] private GameObject crossYouTextGameObject;
    [SerializeField] private TextMeshProUGUI crossScoreTextGameObject;

    [SerializeField] private GameObject circleArrowGameObject;
    [SerializeField] private GameObject circleYouTextGameObject;
    [SerializeField] private TextMeshProUGUI circleScoreTextGameObject;

    private void Awake()
    {
        crossArrowGameObject.SetActive(false);
        crossYouTextGameObject.SetActive(false);
        crossScoreTextGameObject.text = "";

        circleArrowGameObject.SetActive(false);
        circleYouTextGameObject.SetActive(false);
        circleScoreTextGameObject.text = "";
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChanged += GameManager_OnCurrentPlayablePlayerTypeChanged;
        GameManager.Instance.OnScoreChanged += GameManager_OnScoreChanged;
    }

    private void GameManager_OnScoreChanged(object sender, System.EventArgs e)
    {
        // Nice to remember functionality -> python like value/tuple data fetching 
        GameManager.Instance.GetScores(out int playerCrossScore, out int playerCircleScore);

        crossScoreTextGameObject.text = playerCrossScore.ToString();
        circleScoreTextGameObject.text = playerCircleScore.ToString();
    }

    private void GameManager_OnCurrentPlayablePlayerTypeChanged(object sender, System.EventArgs e)
    {
        UpdateCurrentArrow();
    }

    // First time start - not rematches
    private void GameManager_OnGameStarted(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.Cross)
        {
            crossYouTextGameObject.SetActive(true);
        }
        else
        {
            circleYouTextGameObject.SetActive(true);
        }

        crossScoreTextGameObject.text = "0";
        circleScoreTextGameObject.text = "0";

        UpdateCurrentArrow();
    }

    private void UpdateCurrentArrow()
    {
        if (GameManager.Instance.GetCurrentPlayablePlayerType() == GameManager.PlayerType.Cross)
        {
            crossArrowGameObject.SetActive(true);
            circleArrowGameObject.SetActive(false);
        }
        else
        {
            crossArrowGameObject.SetActive(false);
            circleArrowGameObject.SetActive(true);
        }
    }
}
