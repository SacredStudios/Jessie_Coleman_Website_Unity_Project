using UnityEngine;

public class Trigger : MonoBehaviour
{
    [SerializeField] GameObject UI;
    [SerializeField] TriggerHandler th;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
            th.ChangeUI(UI);
    }
}
