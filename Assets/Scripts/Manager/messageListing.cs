using TMPro;
using UnityEngine;

public class messageListing : MonoBehaviour
{
    public TMP_Text text;

    public void SetMessage(string message)
    {
        text.text = message;
    }
}
