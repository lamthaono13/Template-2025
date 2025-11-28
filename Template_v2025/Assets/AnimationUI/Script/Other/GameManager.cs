using UnityEngine;
using UnityEngine.EventSystems;

namespace UiTransition
{
    public class GameManager : MonoBehaviour
 {
     public void SetActiveAllInput(bool isActive)
     {
         transform.GetChild(0).gameObject.SetActive(!isActive);
     }
 }
}


