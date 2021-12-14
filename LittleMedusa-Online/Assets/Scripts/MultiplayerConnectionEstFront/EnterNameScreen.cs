using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace MedusaMultiplayer
{
    public class EnterNameScreen : MonoBehaviour
    {
        public TMP_InputField nameField;

        public void EstablishConnection()
        {
            if (!string.IsNullOrEmpty(nameField.text))
            {
                MultiplayerManager.instance.EstablishConnection(nameField.text);
            }
        }
    }
}