using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupLose : MonoBehaviour
{
    private bool isShow = false;

    [SerializeField] private Button btnReplay;

    // Start is called before the first frame update
    void Start()
    {
        btnReplay.onClick.AddListener(OnClickBtnReplay);
    }

    public void Show(bool _isShow)
    {
        gameObject.SetActive(_isShow);

        isShow = _isShow;
    }

    private void OnClickBtnReplay()
    {
        EventBus.Raise(new EventReplay());
        Show(false);
    }
}