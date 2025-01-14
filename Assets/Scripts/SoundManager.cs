using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private Transform placeSFXPrefab;
    [SerializeField] private Transform winSFXPrefab;
    [SerializeField] private Transform loseSFXPrefab;

    private void Start()
    {
        GameManager.Instance.OnObjectPlaced += GameManager_OnObjectPlaced;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin; ;

    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == e.winnerPlayerType)
        {
            Transform sfxTransform = Instantiate(winSFXPrefab);
            Destroy(sfxTransform.gameObject, 5f);
        }
        else
        {
            Transform sfxTransform = Instantiate(loseSFXPrefab);
            Destroy(sfxTransform.gameObject, 5f);
        }
    }

    private void GameManager_OnObjectPlaced(object sender, System.EventArgs e)
    {
        Transform sfxTransform = Instantiate(placeSFXPrefab);
        Destroy(sfxTransform.gameObject, 5f);
    }
}
