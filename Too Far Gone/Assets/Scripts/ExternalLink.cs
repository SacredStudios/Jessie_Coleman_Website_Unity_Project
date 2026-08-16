using UnityEngine;

public class ExternalLink : MonoBehaviour
{
    [SerializeField] string url;
    public void GoToLink()
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            Debug.LogWarning("No URL assigned.");
            return;
        }

        string finalUrl = url.Trim();

        if (!finalUrl.StartsWith("http://") && !finalUrl.StartsWith("https://"))
        {
            finalUrl = "https://" + finalUrl;
        }

        Application.OpenURL(finalUrl);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
