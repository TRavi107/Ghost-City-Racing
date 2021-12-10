using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class messageListingManager : MonoBehaviour
{
    public messageListing messagePrefab;
    public GameObject messageContainer;
    private messageListing tempoMessage;
    private List<messageListing> _messageListings = new List<messageListing>();

    public static messageListingManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void SpawnMessage(string message)
    {
        tempoMessage = (Instantiate(messagePrefab, messageContainer.transform));
        if(tempoMessage != null)
        {
            tempoMessage.SetMessage(message);
            _messageListings.Add(tempoMessage);

            if (_messageListings.Count > 6)
            {
                Destroy(_messageListings[0].gameObject);
                _messageListings.RemoveAt(0);
            }
        }
        
    }

    public void DeletePlayerList()
    {
        foreach (messageListing message in _messageListings)
        {
            DeleteMessageListing(message);
        }
        _messageListings.Clear();

    }

    private void DeleteMessageListing(messageListing _message)
    {
        int index = _messageListings.FindIndex(x => x == _message);
        if (index != -1)
        {
            Destroy(_messageListings[index].gameObject);
            
        }
    }
}
