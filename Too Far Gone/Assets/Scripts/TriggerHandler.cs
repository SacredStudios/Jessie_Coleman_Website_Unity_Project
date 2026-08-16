using UnityEngine;

public class TriggerHandler : MonoBehaviour
{
    [SerializeField] GameObject currUI;
    public void ChangeUI(GameObject newUI)
    {
        if (currUI != newUI)
        {
            currUI.SetActive(false);
            newUI.SetActive(true);
            currUI = newUI;
        }
    }
}
