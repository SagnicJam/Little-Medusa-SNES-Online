using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class EnterNameScreen : MonoBehaviour
{
    public TMP_InputField nameField;
    
    public void EstablishConnection()
    {
        MultiplayerManager.instance.EstablishConnection(nameField.text);
    }
}
