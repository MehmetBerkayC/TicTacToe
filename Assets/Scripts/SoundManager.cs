using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private Transform placeSFXPrefab;

    private void Start()
    {
        GameManager.Instance.OnObjectPlaced += GameManager_OnObjectPlaced;
    }

    private void GameManager_OnObjectPlaced(object sender, System.EventArgs e)
    {
        Transform sfxTransform = Instantiate(placeSFXPrefab);
        Destroy(sfxTransform.gameObject, 5f);
    }
}
