using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class CharacterSelectionScreen : MonoBehaviour
{
    public static CharacterSelectionScreen instance;

    public CharacterSelectButton[] characterSelectButtons;

    public GameObject characterJoinOrderPrefab;
    public Hero clientlocalActor;

    public Dictionary<int, GameObject> idToCharacterDic = new Dictionary<int, GameObject>();

    private void Awake()
    {
        instance = this;
    }

    public void AssignCharacterToId(int hero,int id)
    {
        GameObject g;
        if (idToCharacterDic.TryGetValue(id, out g))
        {
            g.transform.parent = characterSelectButtons[hero].joinOrderTextParent;
            g.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
        else
        {
            Debug.LogError("Could not find the object at id : "+id);
        }
    }

    public void PlayerConnected(int id)
    {
        GameObject g = Instantiate(characterJoinOrderPrefab);
        JoinOrderText joinOrderText = g.GetComponent<JoinOrderText>();
        joinOrderText.characterPlayerText.text = id+"p";

        GameObject g1;
        if (idToCharacterDic.TryGetValue(id, out g1))
        {
            Debug.LogError("Object is already added to the dictionary");
        }
        else
        {
            idToCharacterDic.Add(id, g);
        }
    }

    public void PlayerDisconnected(int id)
    {
        GameObject g;
        if(idToCharacterDic.TryGetValue(id,out g))
        {
            Destroy(g);
            idToCharacterDic.Remove(id);
        }
        else
        {
            Debug.LogError("Could not find the object disconnected");
        }
    }

    public void ChangeCharacter(string hero)
    {
        EnumData.Heroes heroEnum = (EnumData.Heroes)Enum.Parse(typeof(EnumData.Heroes), hero);

        if(clientlocalActor!=null)
        {
            if (heroEnum != (EnumData.Heroes)clientlocalActor.hero)
            {
                CharacterChangeCommand characterChangeCommand = new CharacterChangeCommand(clientlocalActor.GetLocalSequenceNo(), (int)heroEnum);
                ClientSend.ChangeCharacter(characterChangeCommand);
            }
            else
            {
                Debug.Log("Cant change character");
            }
        }
    }
}
