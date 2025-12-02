using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private PopupLose popupLose;

    private void Start()
    {
        EventBus.AddListener<EventEndGame>(OnEndGame);
    }

    public void Init()
    {

    }

    private void OnDestroy()
    {
        EventBus.RemoveListener<EventEndGame>(OnEndGame);
    }

    private void OnEndGame(EventEndGame eventEndGame)
    {
        popupLose.Show(true);
    }
}
