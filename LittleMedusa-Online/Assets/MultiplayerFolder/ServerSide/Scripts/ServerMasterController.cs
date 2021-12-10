using System.Collections.Generic;
using UnityEngine;

public class ServerMasterController : MonoBehaviour
{
    [Header("Scene references")]
    public Hero serverInstanceHero;

    [Header("Tweak params")]
    public int packetHistorySize;
    public int snapShotsInOnePacket;
    public int idealSnapshotBufferCount;
    public int maxedOutSnapshotBufferCount;
    public int reliabilityCheckBufferCount;
    //public int playerStateSaveBufferCount;
    
    //private float moveSpeed = 5f / Constants.TICKS_PER_SECONDS;

    private Dictionary<int, InputCommands> inputUpdatesFromClientToServerDic = new Dictionary<int, InputCommands>();
    private Dictionary<int, PushCommand> pushCommandFromClientToServerDic = new Dictionary<int, PushCommand>();
    private Dictionary<int, PlaceBoulderCommand> placeBoulderCommandFromClientToServerDic = new Dictionary<int, PlaceBoulderCommand>();
    private Dictionary<int, RemoveBoulderCommand> removeBoulderCommandFromClientToServerDic = new Dictionary<int, RemoveBoulderCommand>();
    private Dictionary<int, SpawnItemCommand> spawnItemCommandFromClientToServerDic = new Dictionary<int, SpawnItemCommand>();
    private Dictionary<int, PetrificationCommand> petrificationRequestReceivedFromClientToServerDic = new Dictionary<int, PetrificationCommand>();
    private Dictionary<int, FireTidalWaveCommand> tidalWaveFireRequestReceivedFromClientToServerDic = new Dictionary<int, FireTidalWaveCommand>();
    private Dictionary<int, FireMightyWindCommand> mightyWindFireRequestReceivedFromClientToServerDic = new Dictionary<int, FireMightyWindCommand>();
    private Dictionary<int, CastBubbleShieldCommand> castBubbleShieldRequestReceivedFromClientToServerDic = new Dictionary<int, CastBubbleShieldCommand>();
    private Dictionary<int, CastFlamePillar> castFlamePillarRequestReceivedFromClientToServerDic = new Dictionary<int, CastFlamePillar>();
    private Dictionary<int, CastPitfallCommand> castPitfallRequestReceivedFromClientToServerDic = new Dictionary<int, CastPitfallCommand>();
    private Dictionary<int, CastEarthQuakeCommand> castEarthQuakeRequestReceivedFromClientToServerDic = new Dictionary<int, CastEarthQuakeCommand>();
    private Dictionary<int, PlaceTornadoCommand> placeTornadoRequestReceivedFromClientToServerDic = new Dictionary<int, PlaceTornadoCommand>();
    private Dictionary<int, CharacterChangeCommand> changeCharacterRequestReceivedFromClientToServerDic = new Dictionary<int, CharacterChangeCommand>();
    private Dictionary<int, RespawnPlayerCommand> respawnCommandRequestReceivedFromClientToServerDic = new Dictionary<int, RespawnPlayerCommand>();
    private Dictionary<int, LandPlayerCommand> landplayerCommandRequestReceivedFromClientToServerDic = new Dictionary<int, LandPlayerCommand>();
    private List<PlayerStateServerUpdates> playerStateListOnServer = new List<PlayerStateServerUpdates>();
    private List<PreviousPlayerUpdatedStatePacks> previousPlayerUpdatedStatePacks = new List<PreviousPlayerUpdatedStatePacks>();

    [Header("Live Units")]
    public int id;
    public string username;
    public string connectionID;
    public ProcessMode currentInputProcessingModeOnServer;
    public int snapShotBufferSize;
    public int serverLocalSequenceNumber = 0;
    public int playerSequenceNumberProcessed = 0;
    public int lastSequenceNumberProcessed;
    public InputCommands latestPlayerInputPackage;

    

    public void Initialise(int id,string connectionID, string username, Vector3 position)
    {
        this.id = id;
        this.connectionID = connectionID;
        this.username = username;
        serverInstanceHero.actorTransform.position = position;
        serverInstanceHero.movePoint.position = position;
        serverInstanceHero.Facing = serverInstanceHero.faceDirectionInit;
        serverInstanceHero.InitialiseHP();
        serverInstanceHero.InitialiseStockLives();
        serverInstanceHero.InitialiseServerActor(this,id);
        serverLocalSequenceNumber = 1;
    }

    #region ReliableDataCheckForImplementation
    public void CheckForChangeCharacterRequestForPlayer(int sequenceNoToCheck)
    {
        List<int> toDiscardSequences = new List<int>();
        foreach (KeyValuePair<int, CharacterChangeCommand> kvp in changeCharacterRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck - reliabilityCheckBufferCount;
            if (kvp.Key <= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<int, CharacterChangeCommand> kvp in changeCharacterRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck + reliabilityCheckBufferCount;
            if (kvp.Key >= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (int i in toDiscardSequences)
        {
            if (changeCharacterRequestReceivedFromClientToServerDic.ContainsKey(i))
            {
                //Debug.Log("<color=red>discarding seq </color>" + i);
                changeCharacterRequestReceivedFromClientToServerDic.Remove(i);
            }
            else
            {
                Debug.LogError("Could not find the key: " + i);
            }
        }

        for (int i = (reliabilityCheckBufferCount - 1); i >= 0; i--)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck - i;
            CharacterChangeCommand characterChangeCommand;
            if (changeCharacterRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out characterChangeCommand))
            {
                int hero = characterChangeCommand.characterHero;
                changeCharacterRequestReceivedFromClientToServerDic.Remove(characterChangeCommand.sequenceNoCharacterChangeCommand);
                //do server rollback here to check to check if damage actually occured on server
                ChangeCharacterCommandForPlayerImplementation(hero);
            }
        }

        for (int i = 0; i <= (reliabilityCheckBufferCount - 1); i++)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck + i;
            CharacterChangeCommand characterChangeCommand;
            if (changeCharacterRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out characterChangeCommand))
            {
                int hero = characterChangeCommand.characterHero;
                changeCharacterRequestReceivedFromClientToServerDic.Remove(characterChangeCommand.sequenceNoCharacterChangeCommand);
                //do server rollback here to check to check if damage actually occured on server
                ChangeCharacterCommandForPlayerImplementation(hero);
            }
        }
    }

    public void CheckForFlamePillarRequestForPlayer(int sequenceNoToCheck)
    {
        List<int> toDiscardSequences = new List<int>();
        foreach (KeyValuePair<int, CastFlamePillar> kvp in castFlamePillarRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck - reliabilityCheckBufferCount;
            if (kvp.Key <= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<int, CastFlamePillar> kvp in castFlamePillarRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck + reliabilityCheckBufferCount;
            if (kvp.Key >= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (int i in toDiscardSequences)
        {
            if (castFlamePillarRequestReceivedFromClientToServerDic.ContainsKey(i))
            {
                //Debug.Log("<color=red>discarding seq </color>" + i);
                castFlamePillarRequestReceivedFromClientToServerDic.Remove(i);
            }
            else
            {
                Debug.LogError("Could not find the key: " + i);
            }
        }

        for (int i = (reliabilityCheckBufferCount - 1); i >= 0; i--)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck - i;
            CastFlamePillar castFlamePillar;
            if (castFlamePillarRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out castFlamePillar))
            {
                int direction = castFlamePillar.direction;
                Vector3Int cellPredicted = castFlamePillar.predictedCell;
                castFlamePillarRequestReceivedFromClientToServerDic.Remove(castFlamePillar.sequenceNoCastingFlamePillarCommand);
                //do server rollback here to check to check if damage actually occured on server
                CastFlamePillarForPlayerImplementation(direction, cellPredicted,null);
            }
        }

        for (int i = 0; i <= (reliabilityCheckBufferCount - 1); i++)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck + i;
            CastFlamePillar castFlamePillar;
            if (castFlamePillarRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out castFlamePillar))
            {
                int direction = castFlamePillar.direction;
                Vector3Int cellPredicted = castFlamePillar.predictedCell;
                castFlamePillarRequestReceivedFromClientToServerDic.Remove(castFlamePillar.sequenceNoCastingFlamePillarCommand);
                //do server rollback here to check to check if damage actually occured on server
                CastFlamePillarForPlayerImplementation(direction, cellPredicted,null);
            }
        }
    }

    public void CheckForEarthQuakeRequestForPlayer(int sequenceNoToCheck)
    {
        List<int> toDiscardSequences = new List<int>();
        foreach (KeyValuePair<int, CastEarthQuakeCommand> kvp in castEarthQuakeRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck - reliabilityCheckBufferCount;
            if (kvp.Key <= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<int, CastEarthQuakeCommand> kvp in castEarthQuakeRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck + reliabilityCheckBufferCount;
            if (kvp.Key >= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (int i in toDiscardSequences)
        {
            if (castEarthQuakeRequestReceivedFromClientToServerDic.ContainsKey(i))
            {
                //Debug.Log("<color=red>discarding seq </color>" + i);
                castEarthQuakeRequestReceivedFromClientToServerDic.Remove(i);
            }
            else
            {
                Debug.LogError("Could not find the key: " + i);
            }
        }

        for (int i = (reliabilityCheckBufferCount - 1); i >= 0; i--)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck - i;
            CastEarthQuakeCommand castEarthQuakeCommand;
            if (castEarthQuakeRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out castEarthQuakeCommand))
            {
                castEarthQuakeRequestReceivedFromClientToServerDic.Remove(castEarthQuakeCommand.sequenceNoForCastingEarthQuakeCommand);
                //do server rollback here to check to check if damage actually occured on server
                CastEarthQuakeImplementation(null);
            }
        }

        for (int i = 0; i <= (reliabilityCheckBufferCount - 1); i++)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck + i;
            CastEarthQuakeCommand castEarthQuakeCommand;
            if (castEarthQuakeRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out castEarthQuakeCommand))
            {
                castEarthQuakeRequestReceivedFromClientToServerDic.Remove(castEarthQuakeCommand.sequenceNoForCastingEarthQuakeCommand);
                //do server rollback here to check to check if damage actually occured on server
                CastEarthQuakeImplementation(null);
            }
        }
    }

    public void CheckForPitfallRequestForPlayer(int sequenceNoToCheck)
    {
        List<int> toDiscardSequences = new List<int>();
        foreach (KeyValuePair<int, CastPitfallCommand> kvp in castPitfallRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck - reliabilityCheckBufferCount;
            if (kvp.Key <= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<int, CastPitfallCommand> kvp in castPitfallRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck + reliabilityCheckBufferCount;
            if (kvp.Key >= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (int i in toDiscardSequences)
        {
            if (castPitfallRequestReceivedFromClientToServerDic.ContainsKey(i))
            {
                //Debug.Log("<color=red>discarding seq </color>" + i);
                castPitfallRequestReceivedFromClientToServerDic.Remove(i);
            }
            else
            {
                Debug.LogError("Could not find the key: " + i);
            }
        }

        for (int i = (reliabilityCheckBufferCount - 1); i >= 0; i--)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck - i;
            CastPitfallCommand castPitfallCommand;
            if (castPitfallRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out castPitfallCommand))
            {
                int direction = castPitfallCommand.direction;
                castPitfallRequestReceivedFromClientToServerDic.Remove(castPitfallCommand.sequenceNoForCastingPitfallCommand);
                //do server rollback here to check to check if damage actually occured on server
                CastPitfallImplementation(direction,null);
            }
        }

        for (int i = 0; i <= (reliabilityCheckBufferCount - 1); i++)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck + i;
            CastPitfallCommand castPitfallCommand;
            if (castPitfallRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out castPitfallCommand))
            {
                int direction = castPitfallCommand.direction;
                castPitfallRequestReceivedFromClientToServerDic.Remove(castPitfallCommand.sequenceNoForCastingPitfallCommand);
                //do server rollback here to check to check if damage actually occured on server
                CastPitfallImplementation(direction,null);
            }
        }
    }

    public void CheckForTornadoRequestForPlayer(int sequenceNoToCheck)
    {
        List<int> toDiscardSequences = new List<int>();
        foreach (KeyValuePair<int, PlaceTornadoCommand> kvp in placeTornadoRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck - reliabilityCheckBufferCount;
            if (kvp.Key <= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<int, PlaceTornadoCommand> kvp in placeTornadoRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck + reliabilityCheckBufferCount;
            if (kvp.Key >= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (int i in toDiscardSequences)
        {
            if (placeTornadoRequestReceivedFromClientToServerDic.ContainsKey(i))
            {
                //Debug.Log("<color=red>discarding seq </color>" + i);
                placeTornadoRequestReceivedFromClientToServerDic.Remove(i);
            }
            else
            {
                Debug.LogError("Could not find the key: " + i);
            }
        }

        for (int i = (reliabilityCheckBufferCount - 1); i >= 0; i--)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck - i;
            PlaceTornadoCommand placeTornadoCommand;
            if (placeTornadoRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out placeTornadoCommand))
            {
                int direction = placeTornadoCommand.direction;
                placeTornadoRequestReceivedFromClientToServerDic.Remove(placeTornadoCommand.sequenceForPlaceTornadoCommand);
                //do server rollback here to check to check if damage actually occured on server
                CastTornadoForPlayerImplementation(direction,null);
            }
        }

        for (int i = 0; i <= (reliabilityCheckBufferCount - 1); i++)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck + i;
            PlaceTornadoCommand placeTornadoCommand;
            if (placeTornadoRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out placeTornadoCommand))
            {
                int direction = placeTornadoCommand.direction;
                placeTornadoRequestReceivedFromClientToServerDic.Remove(placeTornadoCommand.sequenceForPlaceTornadoCommand);
                //do server rollback here to check to check if damage actually occured on server
                CastTornadoForPlayerImplementation(direction,null);
            }
        }
    }

    public void CheckForBubbleShieldRequestForPlayer(int sequenceNoToCheck)
    {
        List<int> toDiscardSequences = new List<int>();
        foreach (KeyValuePair<int, CastBubbleShieldCommand> kvp in castBubbleShieldRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck - reliabilityCheckBufferCount;
            if (kvp.Key <= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<int, CastBubbleShieldCommand> kvp in castBubbleShieldRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck + reliabilityCheckBufferCount;
            if (kvp.Key >= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (int i in toDiscardSequences)
        {
            if (castBubbleShieldRequestReceivedFromClientToServerDic.ContainsKey(i))
            {
                //Debug.Log("<color=red>discarding seq </color>" + i);
                castBubbleShieldRequestReceivedFromClientToServerDic.Remove(i);
            }
            else
            {
                Debug.LogError("Could not find the key: " + i);
            }
        }

        for (int i = (reliabilityCheckBufferCount - 1); i >= 0; i--)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck - i;
            CastBubbleShieldCommand castBubbleShieldCommand;
            if (castBubbleShieldRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out castBubbleShieldCommand))
            {
                Vector3Int cellPredicted = castBubbleShieldCommand.predictedCell;
                castBubbleShieldRequestReceivedFromClientToServerDic.Remove(castBubbleShieldCommand.sequenceNoForCastingBubbleShield);
                //do server rollback here to check to check if damage actually occured on server
                CastBubbleShieldForPlayerImplementation(cellPredicted,null);
            }
        }

        for (int i = 0; i <= (reliabilityCheckBufferCount - 1); i++)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck + i;
            CastBubbleShieldCommand castBubbleShieldCommand;
            if (castBubbleShieldRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out castBubbleShieldCommand))
            {
                Vector3Int cellPredicted = castBubbleShieldCommand.predictedCell;
                castBubbleShieldRequestReceivedFromClientToServerDic.Remove(castBubbleShieldCommand.sequenceNoForCastingBubbleShield);
                //do server rollback here to check to check if damage actually occured on server
                CastBubbleShieldForPlayerImplementation(cellPredicted,null);
            }
        }
    }

    public void CheckForMightyWindFireRequestOnPlayer(int sequenceNoToCheck)
    {
        List<int> toDiscardSequences = new List<int>();
        foreach (KeyValuePair<int, FireMightyWindCommand> kvp in mightyWindFireRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck - reliabilityCheckBufferCount;
            if (kvp.Key <= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<int, FireMightyWindCommand> kvp in mightyWindFireRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck + reliabilityCheckBufferCount;
            if (kvp.Key >= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (int i in toDiscardSequences)
        {
            if (mightyWindFireRequestReceivedFromClientToServerDic.ContainsKey(i))
            {
                //Debug.Log("<color=red>discarding seq </color>" + i);
                mightyWindFireRequestReceivedFromClientToServerDic.Remove(i);
            }
            else
            {
                Debug.LogError("Could not find the key: " + i);
            }
        }

        for (int i = (reliabilityCheckBufferCount - 1); i >= 0; i--)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck - i;
            FireMightyWindCommand fireMightyWindCommand;
            if (mightyWindFireRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out fireMightyWindCommand))
            {
                int direction = fireMightyWindCommand.direction;
                Vector3Int cellPredicted = fireMightyWindCommand.cellPredicted;
                mightyWindFireRequestReceivedFromClientToServerDic.Remove(fireMightyWindCommand.sequenceNoForFiringMightyWindCommand);
                //do server rollback here to check to check if damage actually occured on server
                MightyWindFirePlayerRequestImplementation(direction, cellPredicted,null);
            }
        }

        for (int i = 0; i <= (reliabilityCheckBufferCount - 1); i++)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck + i;
            FireMightyWindCommand fireMightyWindCommand;
            if (mightyWindFireRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out fireMightyWindCommand))
            {
                int direction = fireMightyWindCommand.direction;
                Vector3Int cellPredicted = fireMightyWindCommand.cellPredicted;
                mightyWindFireRequestReceivedFromClientToServerDic.Remove(fireMightyWindCommand.sequenceNoForFiringMightyWindCommand);
                //do server rollback here to check to check if damage actually occured on server
                MightyWindFirePlayerRequestImplementation(direction, cellPredicted,null);
            }
        }
    }

    public void CheckForTidalWaveFireRequestOnPlayer(int sequenceNoToCheck)
    {
        List<int> toDiscardSequences = new List<int>();
        foreach (KeyValuePair<int, FireTidalWaveCommand> kvp in tidalWaveFireRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck - reliabilityCheckBufferCount;
            if (kvp.Key <= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<int, FireTidalWaveCommand> kvp in tidalWaveFireRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck + reliabilityCheckBufferCount;
            if (kvp.Key >= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (int i in toDiscardSequences)
        {
            if (tidalWaveFireRequestReceivedFromClientToServerDic.ContainsKey(i))
            {
                //Debug.Log("<color=red>discarding seq </color>" + i);
                tidalWaveFireRequestReceivedFromClientToServerDic.Remove(i);
            }
            else
            {
                Debug.LogError("Could not find the key: " + i);
            }
        }

        for (int i = (reliabilityCheckBufferCount - 1); i >= 0; i--)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck - i;
            FireTidalWaveCommand tidalWaveFireCommand;
            if (tidalWaveFireRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out tidalWaveFireCommand))
            {
                Vector3Int cellPredicted = tidalWaveFireCommand.predictedCell;
                tidalWaveFireRequestReceivedFromClientToServerDic.Remove(tidalWaveFireCommand.sequenceNoForFiringTidalWaveCommand);
                //do server rollback here to check to check if damage actually occured on server
                TidalWaveFirePlayerRequestImplementation(cellPredicted,null);
            }
        }

        for (int i = 0; i <= (reliabilityCheckBufferCount - 1); i++)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck + i;
            FireTidalWaveCommand tidalWaveFireCommand;
            if (tidalWaveFireRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out tidalWaveFireCommand))
            {
                Vector3Int cellPredicted = tidalWaveFireCommand.predictedCell;
                tidalWaveFireRequestReceivedFromClientToServerDic.Remove(tidalWaveFireCommand.sequenceNoForFiringTidalWaveCommand);
                //do server rollback here to check to check if damage actually occured on server
                TidalWaveFirePlayerRequestImplementation(cellPredicted,null);
            }
        }
    }

    public void CheckForPetrificationRequestOnPlayer(int sequenceNoToCheck)
    {
        List<int> toDiscardSequences = new List<int>();
        foreach (KeyValuePair<int, PetrificationCommand> kvp in petrificationRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck - reliabilityCheckBufferCount;
            if (kvp.Key <= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<int, PetrificationCommand> kvp in petrificationRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck + reliabilityCheckBufferCount;
            if (kvp.Key >= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (int i in toDiscardSequences)
        {
            if (petrificationRequestReceivedFromClientToServerDic.ContainsKey(i))
            {
                //Debug.Log("<color=red>discarding seq </color>" + i);
                petrificationRequestReceivedFromClientToServerDic.Remove(i);
            }
            else
            {
                Debug.LogError("Could not find the key: " + i);
            }
        }

        for (int i = (reliabilityCheckBufferCount - 1); i >= 0; i--)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck - i;
            PetrificationCommand petrificationCommand;
            if (petrificationRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out petrificationCommand))
            {
                petrificationRequestReceivedFromClientToServerDic.Remove(petrificationCommand.sequenceNoForPetrificationCommand);
                //do server rollback here to check to check if damage actually occured on server
                int playerIdPetrified = petrificationCommand.playerIdPetrified;
                PetrifyPlayerRequestImplementation(playerIdPetrified);
            }
        }

        for (int i = 0; i <= (reliabilityCheckBufferCount - 1); i++)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck + i;
            PetrificationCommand petrificationCommand;
            if (petrificationRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out petrificationCommand))
            {
                petrificationRequestReceivedFromClientToServerDic.Remove(petrificationCommand.sequenceNoForPetrificationCommand);
                //do server rollback here to check to check if damage actually occured on server
                int playerIdPetrified = petrificationCommand.playerIdPetrified;
                PetrifyPlayerRequestImplementation(playerIdPetrified);
            }
        }
    }

    

    public void CheckForPushRequestOnServer(int sequenceNoToCheck)
    {
        List<int> toDiscardSequences = new List<int>();
        foreach (KeyValuePair<int, PushCommand> kvp in pushCommandFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck - reliabilityCheckBufferCount;
            if (kvp.Key <= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<int, PushCommand> kvp in pushCommandFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck + reliabilityCheckBufferCount;
            if (kvp.Key >= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (int i in toDiscardSequences)
        {
            if (pushCommandFromClientToServerDic.ContainsKey(i))
            {
                //Debug.Log("<color=red>discarding seq </color>" + i);
                pushCommandFromClientToServerDic.Remove(i);
            }
            else
            {
                Debug.LogError("Could not find the key: " + i);
            }
        }

        for (int i = (reliabilityCheckBufferCount - 1); i >= 0; i--)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck - i;
            PushCommand pushCommand;
            if (pushCommandFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out pushCommand))
            {
                pushCommandFromClientToServerDic.Remove(pushCommand.sequenceNumber);
                //do server rollback here to check to check if damage actually occured on server
                int playerIdToPush = pushCommand.playerIdToPush;
                int directionOfPush = pushCommand.directionOfPush;

                PushPlayerRequestImplementation(playerIdToPush, directionOfPush);

            }
        }

        for (int i = 0; i <= (reliabilityCheckBufferCount - 1); i++)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck + i;
            PushCommand pushCommand;
            if (pushCommandFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out pushCommand))
            {
                pushCommandFromClientToServerDic.Remove(pushCommand.sequenceNumber);
                //do server rollback here to check to check if damage actually occured on server
                int playerIdToPush = pushCommand.playerIdToPush;
                int directionOfPush = pushCommand.directionOfPush;

                PushPlayerRequestImplementation(playerIdToPush, directionOfPush);

            }
        }
    }

    public void CheckForPlaceBoulderRequestOnServer(int sequenceNoToCheck)
    {
        List<int> toDiscardSequences = new List<int>();
        foreach (KeyValuePair<int, PlaceBoulderCommand> kvp in placeBoulderCommandFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck - reliabilityCheckBufferCount;
            if (kvp.Key <= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<int, PlaceBoulderCommand> kvp in placeBoulderCommandFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck + reliabilityCheckBufferCount;
            if (kvp.Key >= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (int i in toDiscardSequences)
        {
            if (placeBoulderCommandFromClientToServerDic.ContainsKey(i))
            {
                //Debug.Log("<color=red>discarding seq </color>" + i);
                placeBoulderCommandFromClientToServerDic.Remove(i);
            }
            else
            {
                Debug.LogError("Could not find the key: " + i);
            }
        }

        for (int i = (reliabilityCheckBufferCount - 1); i >= 0; i--)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck - i;
            PlaceBoulderCommand placeBoulderCommand;
            if (placeBoulderCommandFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out placeBoulderCommand))
            {
                placeBoulderCommandFromClientToServerDic.Remove(placeBoulderCommand.sequenceNumber);
                //do server rollback here to check to check if damage actually occured on server
                Vector3Int cellPointToPlaceBoulder = placeBoulderCommand.boulderCellPos;
                PlaceBoulderRequestImplementation(cellPointToPlaceBoulder,null);
            }
        }

        for (int i = 0; i <= (reliabilityCheckBufferCount - 1); i++)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck + i;
            PlaceBoulderCommand placeBoulderCommand;
            if (placeBoulderCommandFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out placeBoulderCommand))
            {
                placeBoulderCommandFromClientToServerDic.Remove(placeBoulderCommand.sequenceNumber);
                //do server rollback here to check to check if damage actually occured on server
                Vector3Int cellPointToPlaceBoulder = placeBoulderCommand.boulderCellPos;
                PlaceBoulderRequestImplementation(cellPointToPlaceBoulder,null);
            }
        }
    }

    public void CheckForRemovingBoulderRequestOnServer(int sequenceNoToCheck)
    {
        List<int> toDiscardSequences = new List<int>();
        foreach (KeyValuePair<int, RemoveBoulderCommand> kvp in removeBoulderCommandFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck - reliabilityCheckBufferCount;
            if (kvp.Key <= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<int, RemoveBoulderCommand> kvp in removeBoulderCommandFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck + reliabilityCheckBufferCount;
            if (kvp.Key >= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (int i in toDiscardSequences)
        {
            if (removeBoulderCommandFromClientToServerDic.ContainsKey(i))
            {
                //Debug.Log("<color=red>discarding seq </color>" + i);
                removeBoulderCommandFromClientToServerDic.Remove(i);
            }
            else
            {
                Debug.LogError("Could not find the key: " + i);
            }
        }

        for (int i = (reliabilityCheckBufferCount - 1); i >= 0; i--)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck - i;
            RemoveBoulderCommand removeBoulderCommand;
            if (removeBoulderCommandFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out removeBoulderCommand))
            {
                removeBoulderCommandFromClientToServerDic.Remove(removeBoulderCommand.sequenceNumber);
                //do server rollback here to check to check if damage actually occured on server
                Vector3Int cellPointToRemoveBoulder = removeBoulderCommand.removalCellPos;
                RemoveBoulderRequestImplementation(cellPointToRemoveBoulder);
            }
        }

        for (int i = 0; i <= (reliabilityCheckBufferCount - 1); i++)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck + i;
            RemoveBoulderCommand removeBoulderCommand;
            if (removeBoulderCommandFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out removeBoulderCommand))
            {
                removeBoulderCommandFromClientToServerDic.Remove(removeBoulderCommand.sequenceNumber);
                //do server rollback here to check to check if damage actually occured on server
                Vector3Int cellPointToRemoveBoulder = removeBoulderCommand.removalCellPos;
                RemoveBoulderRequestImplementation(cellPointToRemoveBoulder);
            }
        }
    }

    public void CheckForSpawnItemRequestOnServer(int sequenceNoToCheck)
    {
        List<int> toDiscardSequences = new List<int>();
        foreach (KeyValuePair<int, SpawnItemCommand> kvp in spawnItemCommandFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck - reliabilityCheckBufferCount;
            if (kvp.Key <= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<int, SpawnItemCommand> kvp in spawnItemCommandFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck + reliabilityCheckBufferCount;
            if (kvp.Key >= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (int i in toDiscardSequences)
        {
            if (spawnItemCommandFromClientToServerDic.ContainsKey(i))
            {
                //Debug.Log("<color=red>discarding seq </color>" + i);
                spawnItemCommandFromClientToServerDic.Remove(i);
            }
            else
            {
                Debug.LogError("Could not find the key: " + i);
            }
        }

        for (int i = (reliabilityCheckBufferCount - 1); i >= 0; i--)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck - i;
            SpawnItemCommand placeMinionCommand;
            if (spawnItemCommandFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out placeMinionCommand))
            {
                spawnItemCommandFromClientToServerDic.Remove(placeMinionCommand.sequenceNumber);
                //do server rollback here to check to check if damage actually occured on server
                Vector3Int spawnCell = placeMinionCommand.spawnCell;
                int spawnItemType = placeMinionCommand.spawnItemType;
                int direction = placeMinionCommand.direction;
                SpawnItemImplementation(direction, spawnItemType, spawnCell);
            }
        }

        for (int i = 0; i <= (reliabilityCheckBufferCount - 1); i++)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck + i;
            SpawnItemCommand placeMinionCommand;
            if (spawnItemCommandFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out placeMinionCommand))
            {
                spawnItemCommandFromClientToServerDic.Remove(placeMinionCommand.sequenceNumber);
                //do server rollback here to check to check if damage actually occured on server
                Vector3Int spawnCell = placeMinionCommand.spawnCell;
                int spawnItemType = placeMinionCommand.spawnItemType;
                int direction = placeMinionCommand.direction;
                SpawnItemImplementation(direction, spawnItemType, spawnCell);
            }
        }
    }
    public void CheckForLandingPlayerRequestOnServer(int sequenceNoToCheck)
    {
        List<int> toDiscardSequences = new List<int>();
        foreach (KeyValuePair<int, LandPlayerCommand> kvp in landplayerCommandRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck - reliabilityCheckBufferCount;
            if (kvp.Key <= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<int, LandPlayerCommand> kvp in landplayerCommandRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck + reliabilityCheckBufferCount;
            if (kvp.Key >= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (int i in toDiscardSequences)
        {
            if (landplayerCommandRequestReceivedFromClientToServerDic.ContainsKey(i))
            {
                //Debug.Log("<color=red>discarding seq </color>" + i);
                landplayerCommandRequestReceivedFromClientToServerDic.Remove(i);
            }
            else
            {
                Debug.LogError("Could not find the key: " + i);
            }
        }

        for (int i = (reliabilityCheckBufferCount - 1); i >= 0; i--)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck - i;
            LandPlayerCommand landPlayerCommand;
            if (landplayerCommandRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out landPlayerCommand))
            {
                landplayerCommandRequestReceivedFromClientToServerDic.Remove(landPlayerCommand.sequenceNumber);
                //do server rollback here to check to check if damage actually occured on server
                LandPlayerImplementation();
            }
        }

        for (int i = 0; i <= (reliabilityCheckBufferCount - 1); i++)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck + i;
            LandPlayerCommand landPlayerCommand;
            if (landplayerCommandRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out landPlayerCommand))
            {
                landplayerCommandRequestReceivedFromClientToServerDic.Remove(landPlayerCommand.sequenceNumber);
                //do server rollback here to check to check if damage actually occured on server
                LandPlayerImplementation();
            }
        }
    }
    public void CheckForRespawnningRequestOnServer(int sequenceNoToCheck)
    {
        List<int> toDiscardSequences = new List<int>();
        foreach (KeyValuePair<int, RespawnPlayerCommand> kvp in respawnCommandRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck - reliabilityCheckBufferCount;
            if (kvp.Key <= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (KeyValuePair<int, RespawnPlayerCommand> kvp in respawnCommandRequestReceivedFromClientToServerDic)
        {
            int sequenceNoToCheckReliablilityEventFrom = sequenceNoToCheck + reliabilityCheckBufferCount;
            if (kvp.Key >= sequenceNoToCheckReliablilityEventFrom)
            {
                toDiscardSequences.Add(kvp.Key);
            }
        }

        foreach (int i in toDiscardSequences)
        {
            if (respawnCommandRequestReceivedFromClientToServerDic.ContainsKey(i))
            {
                //Debug.Log("<color=red>discarding seq </color>" + i);
                respawnCommandRequestReceivedFromClientToServerDic.Remove(i);
            }
            else
            {
                Debug.LogError("Could not find the key: " + i);
            }
        }

        for (int i = (reliabilityCheckBufferCount - 1); i >= 0; i--)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck - i;
            RespawnPlayerCommand respawnPlayerCommand;
            if (respawnCommandRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out respawnPlayerCommand))
            {
                respawnCommandRequestReceivedFromClientToServerDic.Remove(respawnPlayerCommand.sequenceNumber);
                //do server rollback here to check to check if damage actually occured on server
                RespawnPlayerRequestImplementation();
            }
        }

        for (int i = 0; i <= (reliabilityCheckBufferCount - 1); i++)
        {
            int sequenceNoToCheckReliablilityEventFor = sequenceNoToCheck + i;
            RespawnPlayerCommand respawnPlayerCommand;
            if (respawnCommandRequestReceivedFromClientToServerDic.TryGetValue(sequenceNoToCheckReliablilityEventFor, out respawnPlayerCommand))
            {
                respawnCommandRequestReceivedFromClientToServerDic.Remove(respawnPlayerCommand.sequenceNumber);
                //do server rollback here to check to check if damage actually occured on server
                RespawnPlayerRequestImplementation();
            }
        }
    }
    #endregion

    #region ReliableDataAccumulation

    public void AccumulateChangeCharacterCommandToBePlayedOnServerFromClient(CharacterChangeCommand characterChangeCommand)
    {
        if (characterChangeCommand.sequenceNoCharacterChangeCommand > playerSequenceNumberProcessed)
        {
            CharacterChangeCommand dataPackage;
            if (changeCharacterRequestReceivedFromClientToServerDic.TryGetValue(characterChangeCommand.sequenceNoCharacterChangeCommand, out dataPackage))
            {
                Debug.Log("<color=orange>AccumulateChangeCharacterCommandToBePlayedOnServerFromClient dataPackage already exists for sequence no. </color>" + dataPackage.sequenceNoCharacterChangeCommand);
            }
            else
            {
                Debug.Log("<color=green>AccumulateChangeCharacterCommandToBePlayedOnServerFromClient Added successfully to processing buffer dic </color>" + characterChangeCommand.sequenceNoCharacterChangeCommand);
                changeCharacterRequestReceivedFromClientToServerDic.Add(characterChangeCommand.sequenceNoCharacterChangeCommand, characterChangeCommand);
            }
        }
        else
        {
            Debug.Log("<color=red>AccumulateChangeCharacterCommandToBePlayedOnServerFromClient Already processed this sequence no </color>" + playerSequenceNumberProcessed + " got the sequence for : " + characterChangeCommand.sequenceNoCharacterChangeCommand);
        }
    }

    public void AccumulateCastingTornadoRequestToBePlayedOnServerFromClient(PlaceTornadoCommand placeTornadoCommand)
    {
        if (placeTornadoCommand.sequenceForPlaceTornadoCommand > playerSequenceNumberProcessed)
        {
            PlaceTornadoCommand dataPackage;
            if (placeTornadoRequestReceivedFromClientToServerDic.TryGetValue(placeTornadoCommand.sequenceForPlaceTornadoCommand, out dataPackage))
            {
                Debug.Log("<color=orange>AccumulateCastingTornadoRequestToBePlayedOnServerFromClient dataPackage already exists for sequence no. </color>" + dataPackage.sequenceForPlaceTornadoCommand);
            }
            else
            {
                Debug.Log("<color=green>AccumulateCastingTornadoRequestToBePlayedOnServerFromClient Added successfully to processing buffer dic </color>" + placeTornadoCommand.sequenceForPlaceTornadoCommand);
                placeTornadoRequestReceivedFromClientToServerDic.Add(placeTornadoCommand.sequenceForPlaceTornadoCommand, placeTornadoCommand);
            }
        }
        else
        {
            Debug.Log("<color=red>AccumulateCastingTornadoRequestToBePlayedOnServerFromClient Already processed this sequence no </color>" + playerSequenceNumberProcessed + " got the sequence for : " + placeTornadoCommand.sequenceForPlaceTornadoCommand);
        }
    }

    public void AccumulateCastingEarthQuakeRequestToBePlayedOnServerFromClient(CastEarthQuakeCommand castEarthQuakeCommand)
    {
        if (castEarthQuakeCommand.sequenceNoForCastingEarthQuakeCommand > playerSequenceNumberProcessed)
        {
            CastEarthQuakeCommand dataPackage;
            if (castEarthQuakeRequestReceivedFromClientToServerDic.TryGetValue(castEarthQuakeCommand.sequenceNoForCastingEarthQuakeCommand, out dataPackage))
            {
                Debug.Log("<color=orange>AccumulateCastingEarthQuakeRequestToBePlayedOnServerFromClient dataPackage already exists for sequence no. </color>" + dataPackage.sequenceNoForCastingEarthQuakeCommand);
            }
            else
            {
                Debug.Log("<color=green>AccumulateCastingEarthQuakeRequestToBePlayedOnServerFromClient Added successfully to processing buffer dic </color>" + castEarthQuakeCommand.sequenceNoForCastingEarthQuakeCommand);
                castEarthQuakeRequestReceivedFromClientToServerDic.Add(castEarthQuakeCommand.sequenceNoForCastingEarthQuakeCommand, castEarthQuakeCommand);
            }
        }
        else
        {
            Debug.Log("<color=red>AccumulateCastingEarthQuakeRequestToBePlayedOnServerFromClient Already processed this sequence no </color>" + playerSequenceNumberProcessed + " got the sequence for : " + castEarthQuakeCommand.sequenceNoForCastingEarthQuakeCommand);
        }
    }

    public void AccumulateCastingPitfallRequestToBePlayedOnServerFromClient(CastPitfallCommand castPitfallCommand)
    {
        if (castPitfallCommand.sequenceNoForCastingPitfallCommand > playerSequenceNumberProcessed)
        {
            CastPitfallCommand dataPackage;
            if (castPitfallRequestReceivedFromClientToServerDic.TryGetValue(castPitfallCommand.sequenceNoForCastingPitfallCommand, out dataPackage))
            {
                Debug.Log("<color=orange>AccumulateCastingPitfallRequestToBePlayedOnServerFromClient dataPackage already exists for sequence no. </color>" + dataPackage.sequenceNoForCastingPitfallCommand);
            }
            else
            {
                Debug.Log("<color=green>AccumulateCastingPitfallRequestToBePlayedOnServerFromClient Added successfully to processing buffer dic </color>" + castPitfallCommand.sequenceNoForCastingPitfallCommand);
                castPitfallRequestReceivedFromClientToServerDic.Add(castPitfallCommand.sequenceNoForCastingPitfallCommand, castPitfallCommand);
            }
        }
        else
        {
            Debug.Log("<color=red>AccumulateCastingPitfallRequestToBePlayedOnServerFromClient Already processed this sequence no </color>" + playerSequenceNumberProcessed + " got the sequence for : " + castPitfallCommand.sequenceNoForCastingPitfallCommand);
        }
    }

    public void AccumulateCastingFlamePillarRequestToBePlayedOnServerFromClient(CastFlamePillar castFlamePillar)
    {
        if (castFlamePillar.sequenceNoCastingFlamePillarCommand > playerSequenceNumberProcessed)
        {
            CastFlamePillar dataPackage;
            if (castFlamePillarRequestReceivedFromClientToServerDic.TryGetValue(castFlamePillar.sequenceNoCastingFlamePillarCommand, out dataPackage))
            {
                Debug.Log("<color=orange>AccumulateCastingFlamePillarRequestToBePlayedOnServerFromClient dataPackage already exists for sequence no. </color>" + dataPackage.sequenceNoCastingFlamePillarCommand);
            }
            else
            {
                Debug.Log("<color=green>AccumulateCastingFlamePillarRequestToBePlayedOnServerFromClient Added successfully to processing buffer dic </color>" + castFlamePillar.sequenceNoCastingFlamePillarCommand);
                castFlamePillarRequestReceivedFromClientToServerDic.Add(castFlamePillar.sequenceNoCastingFlamePillarCommand, castFlamePillar);
            }
        }
        else
        {
            Debug.Log("<color=red>AccumulateCastingFlamePillarRequestToBePlayedOnServerFromClient Already processed this sequence no </color>" + playerSequenceNumberProcessed + " got the sequence for : " + castFlamePillar.sequenceNoCastingFlamePillarCommand);
        }
    }
    public void AccumulateCastingBubbleShieldRequestToBePlayedOnServerFromClient(CastBubbleShieldCommand castBubbleShieldCommand)
    {
        if (castBubbleShieldCommand.sequenceNoForCastingBubbleShield > playerSequenceNumberProcessed)
        {
            CastBubbleShieldCommand dataPackage;
            if (castBubbleShieldRequestReceivedFromClientToServerDic.TryGetValue(castBubbleShieldCommand.sequenceNoForCastingBubbleShield, out dataPackage))
            {
                Debug.Log("<color=orange>AccumulateCastingBubbleShieldRequestToBePlayedOnServerFromClient dataPackage already exists for sequence no. </color>" + dataPackage.sequenceNoForCastingBubbleShield);
            }
            else
            {
                Debug.Log("<color=green>AccumulateCastingBubbleShieldRequestToBePlayedOnServerFromClient Added successfully to processing buffer dic </color>" + castBubbleShieldCommand.sequenceNoForCastingBubbleShield);
                castBubbleShieldRequestReceivedFromClientToServerDic.Add(castBubbleShieldCommand.sequenceNoForCastingBubbleShield, castBubbleShieldCommand);
            }
        }
        else
        {
            Debug.Log("<color=red>AccumulateCastingBubbleShieldRequestToBePlayedOnServerFromClient Already processed this sequence no </color>" + playerSequenceNumberProcessed + " got the sequence for : " + castBubbleShieldCommand.sequenceNoForCastingBubbleShield);
        }
    }

    public void AccumulateFiringMightyWindRequestToBePlayedOnServerFromClient(FireMightyWindCommand fireMightyWindCommand)
    {
        if (fireMightyWindCommand.sequenceNoForFiringMightyWindCommand > playerSequenceNumberProcessed)
        {
            FireMightyWindCommand dataPackage;
            if (mightyWindFireRequestReceivedFromClientToServerDic.TryGetValue(fireMightyWindCommand.sequenceNoForFiringMightyWindCommand, out dataPackage))
            {
                Debug.Log("<color=orange>AccumulateFiringMightyWindRequestToBePlayedOnServerFromClient dataPackage already exists for sequence no. </color>" + dataPackage.sequenceNoForFiringMightyWindCommand);
            }
            else
            {
                Debug.Log("<color=green>AccumulateFiringMightyWindRequestToBePlayedOnServerFromClient Added successfully to processing buffer dic </color>" + fireMightyWindCommand.sequenceNoForFiringMightyWindCommand);
                mightyWindFireRequestReceivedFromClientToServerDic.Add(fireMightyWindCommand.sequenceNoForFiringMightyWindCommand, fireMightyWindCommand);
            }
        }
        else
        {
            Debug.Log("<color=red>AccumulateFiringMightyWindRequestToBePlayedOnServerFromClient Already processed this sequence no </color>" + playerSequenceNumberProcessed + " got the sequence for : " + fireMightyWindCommand.sequenceNoForFiringMightyWindCommand);
        }
    }

    public void AccumulateFiringTidalWaveRequestToBePlayedOnServerFromClient(FireTidalWaveCommand fireTidalWaveCommand)
    {
        if (fireTidalWaveCommand.sequenceNoForFiringTidalWaveCommand > playerSequenceNumberProcessed)
        {
            FireTidalWaveCommand dataPackage;
            if (tidalWaveFireRequestReceivedFromClientToServerDic.TryGetValue(fireTidalWaveCommand.sequenceNoForFiringTidalWaveCommand, out dataPackage))
            {
                Debug.Log("<color=orange>AccumulateFiringTidalWaveRequestToBePlayedOnServerFromClient dataPackage already exists for sequence no. </color>" + dataPackage.sequenceNoForFiringTidalWaveCommand);
            }
            else
            {
                Debug.Log("<color=green>AccumulateFiringTidalWaveRequestToBePlayedOnServerFromClient Added successfully to processing buffer dic </color>" + fireTidalWaveCommand.sequenceNoForFiringTidalWaveCommand);
                tidalWaveFireRequestReceivedFromClientToServerDic.Add(fireTidalWaveCommand.sequenceNoForFiringTidalWaveCommand, fireTidalWaveCommand);
            }
        }
        else
        {
            Debug.Log("<color=red>AccumulateFiringTidalWaveRequestToBePlayedOnServerFromClient Already processed this sequence no </color>" + playerSequenceNumberProcessed + " got the sequence for : " + fireTidalWaveCommand.sequenceNoForFiringTidalWaveCommand);
        }
    }

    public void AccumulatePetrificationRequestToBePlayedOnServerFromClient(PetrificationCommand petrificationCommand)
    {
        if (petrificationCommand.sequenceNoForPetrificationCommand > playerSequenceNumberProcessed)
        {
            PetrificationCommand dataPackage;
            if (petrificationRequestReceivedFromClientToServerDic.TryGetValue(petrificationCommand.sequenceNoForPetrificationCommand, out dataPackage))
            {
                Debug.Log("<color=orange>AccumulatePetrificationRequestToBePlayedOnServerFromClient dataPackage already exists for sequence no. </color>" + dataPackage.sequenceNoForPetrificationCommand);
            }
            else
            {
                Debug.Log("<color=green>AccumulatePetrificationRequestToBePlayedOnServerFromClient Added successfully to processing buffer dic </color>" + petrificationCommand.sequenceNoForPetrificationCommand);
                petrificationRequestReceivedFromClientToServerDic.Add(petrificationCommand.sequenceNoForPetrificationCommand, petrificationCommand);
            }
        }
        else
        {
            Debug.Log("<color=red>AccumulatePetrificationRequestToBePlayedOnServerFromClient Already processed this sequence no </color>" + playerSequenceNumberProcessed + " got the sequence for : " + petrificationCommand.sequenceNoForPetrificationCommand);
        }
    }

    public void AccumulateRemoveBoulderCommandsToBePlayedOnServerFromClient(RemoveBoulderCommand removeBoulderCommand)
    {
        if (removeBoulderCommand.sequenceNumber > playerSequenceNumberProcessed)
        {
            RemoveBoulderCommand dataPackage;
            if (removeBoulderCommandFromClientToServerDic.TryGetValue(removeBoulderCommand.sequenceNumber, out dataPackage))
            {
                Debug.Log("<color=orange>AccumulateRemoveBoulderCommandsToBePlayedOnServerFromClient dataPackage already exists for sequence no. </color>" + removeBoulderCommand.sequenceNumber);
            }
            else
            {
                Debug.Log("<color=green>AccumulateRemoveBoulderCommandsToBePlayedOnServerFromClient Added successfully to processing buffer dic </color>" + removeBoulderCommand.sequenceNumber);
                removeBoulderCommandFromClientToServerDic.Add(removeBoulderCommand.sequenceNumber, removeBoulderCommand);
            }
        }
        else
        {
            Debug.Log("<color=red>AccumulateRemoveBoulderCommandsToBePlayedOnServerFromClient Already processed this sequence no </color>" + removeBoulderCommand.sequenceNumber);
        }
    }

    public void AccumulateSpawnItemCommandsToBePlayedOnServerFromClient(SpawnItemCommand spawnItemCommand)
    {
        if (spawnItemCommand.sequenceNumber > playerSequenceNumberProcessed)
        {
            SpawnItemCommand dataPackage;
            if (spawnItemCommandFromClientToServerDic.TryGetValue(spawnItemCommand.sequenceNumber, out dataPackage))
            {
                Debug.Log("<color=orange>AccumulateSpawnItemCommandsToBePlayedOnServerFromClient dataPackage already exists for sequence no. </color>" + spawnItemCommand.sequenceNumber);
            }
            else
            {
                Debug.Log("<color=green>AccumulateSpawnItemCommandsToBePlayedOnServerFromClient Added successfully to processing buffer dic </color>" + spawnItemCommand.sequenceNumber);
                spawnItemCommandFromClientToServerDic.Add(spawnItemCommand.sequenceNumber, spawnItemCommand);
            }
        }
        else
        {
            Debug.Log("<color=red>AccumulateSpawnItemCommandsToBePlayedOnServerFromClient Already processed this sequence no </color>" + spawnItemCommand.sequenceNumber);
        }
    }

    public void AccumulatePlaceBoulderCommandsToBePlayedOnServerFromClient(PlaceBoulderCommand placeBoulderCommand)
    {
        if (placeBoulderCommand.sequenceNumber > playerSequenceNumberProcessed)
        {
            PlaceBoulderCommand dataPackage;
            if (placeBoulderCommandFromClientToServerDic.TryGetValue(placeBoulderCommand.sequenceNumber, out dataPackage))
            {
                Debug.Log("<color=orange>AccumulatePlaceBoulderCommandsToBePlayedOnServerFromClient dataPackage already exists for sequence no. </color>" + placeBoulderCommand.sequenceNumber);
            }
            else
            {
                Debug.Log("<color=green>AccumulatePlaceBoulderCommandsToBePlayedOnServerFromClient Added successfully to processing buffer dic </color>" + placeBoulderCommand.sequenceNumber);
                placeBoulderCommandFromClientToServerDic.Add(placeBoulderCommand.sequenceNumber, placeBoulderCommand);
            }
        }
        else
        {
            Debug.Log("<color=red>AccumulatePlaceBoulderCommandsToBePlayedOnServerFromClient Already processed this sequence no </color>" + placeBoulderCommand.sequenceNumber);
        }
    }

    public void AccumulatePushCommandsToBePlayedOnServerFromClient(PushCommand pushCommand)
    {
        if (pushCommand.sequenceNumber > playerSequenceNumberProcessed)
        {
            PushCommand dataPackage;
            if (pushCommandFromClientToServerDic.TryGetValue(pushCommand.sequenceNumber, out dataPackage))
            {
                Debug.Log("<color=orange>AccumulatePushCommandsToBePlayedOnServerFromClient dataPackage already exists for sequence no. </color>" + dataPackage.sequenceNumber);
            }
            else
            {
                Debug.Log("<color=green>AccumulatePushCommandsToBePlayedOnServerFromClient Added successfully to processing buffer dic </color>" + pushCommand.sequenceNumber);
                pushCommandFromClientToServerDic.Add(pushCommand.sequenceNumber, pushCommand);
            }
        }
        else
        {
            Debug.Log("<color=red>Already processed this sequence no </color>" + pushCommand.sequenceNumber);
        }
    }

    public void AccumulateRespawnningRequestFromClientToServer(RespawnPlayerCommand respawnPlayerCommand)
    {
        if (respawnPlayerCommand.sequenceNumber > playerSequenceNumberProcessed)
        {
            RespawnPlayerCommand dataPackage;
            if (respawnCommandRequestReceivedFromClientToServerDic.TryGetValue(respawnPlayerCommand.sequenceNumber, out dataPackage))
            {
                Debug.Log("<color=orange>AccumulateRespawnningRequestFromClientToServer dataPackage already exists for sequence no. </color>" + dataPackage.sequenceNumber);
            }
            else
            {
                Debug.Log("<color=green>AccumulateRespawnningRequestFromClientToServer Added successfully to processing buffer dic </color>" + respawnPlayerCommand.sequenceNumber);
                respawnCommandRequestReceivedFromClientToServerDic.Add(respawnPlayerCommand.sequenceNumber, respawnPlayerCommand);
            }
        }
        else
        {
            Debug.Log("<color=red>AccumulateRespawnningRequestFromClientToServer Already processed this sequence no </color>" + playerSequenceNumberProcessed + " got the sequence for : " + respawnPlayerCommand.sequenceNumber);
        }
    }

    public void AccumulateLandingPlayerRequestFromClientToServer(LandPlayerCommand landPlayerCommand)
    {
        if (landPlayerCommand.sequenceNumber > playerSequenceNumberProcessed)
        {
            LandPlayerCommand dataPackage;
            if (landplayerCommandRequestReceivedFromClientToServerDic.TryGetValue(landPlayerCommand.sequenceNumber, out dataPackage))
            {
                Debug.Log("<color=orange>AccumulateLandingPlayerRequestFromClientToServer dataPackage already exists for sequence no. </color>" + dataPackage.sequenceNumber);
            }
            else
            {
                Debug.Log("<color=green>AccumulateLandingPlayerRequestFromClientToServer Added successfully to processing buffer dic </color>" + landPlayerCommand.sequenceNumber);
                landplayerCommandRequestReceivedFromClientToServerDic.Add(landPlayerCommand.sequenceNumber, landPlayerCommand);
            }
        }
        else
        {
            Debug.Log("<color=red>AccumulateLandingPlayerRequestFromClientToServer Already processed this sequence no </color>" + playerSequenceNumberProcessed + " got the sequence for : " + landPlayerCommand.sequenceNumber);
        }
    }
    #endregion

    #region UnReliableDataAccumulation
    public void AccumulateInputsToBePlayedOnServerFromClient(InputCommands inputCommands)
    {
        if (inputCommands.sequenceNumber > playerSequenceNumberProcessed)
        {
            InputCommands dataPackage;
            if (inputUpdatesFromClientToServerDic.TryGetValue(inputCommands.sequenceNumber, out dataPackage))
            {
                //Debug.Log("<color=orange>dataPackage already exists for sequence no. </color>" + dataPackage.sequenceNumber);
            }
            else
            {
                //Debug.Log("<color=green>Added successfully to processing buffer dic </color>" + inputCommands.sequenceNumber+"  processed sequence number: "+playerSequenceNumberProcessed);
                inputUpdatesFromClientToServerDic.Add(inputCommands.sequenceNumber, inputCommands);
            }
        }
        else
        {
            //Debug.Log("<color=red>Already processed this sequence no </color>" + playerSequenceNumberProcessed);
        }
    }
    #endregion

    #region SantiyCheck
    bool CanDoAction(string function)
    {
        if (serverInstanceHero.isRespawnningPlayer)
        {
            Debug.LogError(function + " Is respawnning");
            return false;
        }
        if (serverInstanceHero.isPhysicsControlled)
        {
            Debug.LogError(function + " server player is phyhsics controlled hence request failed");
            return false;
        }
        if (serverInstanceHero.isPetrified)
        {
            Debug.LogError(function + " server player is petrified hence request failed");
            return false;
        }
        if (serverInstanceHero.isPushed)
        {
            Debug.LogError(function + " server player is pushed hence request failed");
            return false;
        }
        if (serverInstanceHero.isInFlyingState)
        {
            Debug.LogError(function + " server player is isInFlyingState hence request failed");
            return false;
        }
        if (serverInstanceHero.isInvincible)
        {
            Debug.LogError(function + " server player is isInvincible hence request failed");
            return false;
        }
        if (serverInstanceHero.isInputFreezed)
        {
            Debug.LogError(function + " server player is isinputfreezed hence request failed");
            return false;
        }
        return true;
    }
    #endregion

    #region ServerRequestsImplementation
    void PushPlayerRequestImplementation(int playerIdToPush, int directionOfPush)
    {
        if(!CanDoAction("PushPlayerRequestImplementation"))
        {
            return;
        }
        if (!serverInstanceHero.completedMotionToMovePoint)
        {
            Debug.LogError("havent completed motion");
            return;
        }
        if (Server.clients.ContainsKey(playerIdToPush))
        {
            Actor actorToPush = Server.clients[playerIdToPush].serverMasterController.serverInstanceHero;

            if (actorToPush != null)
            {
                if (serverInstanceHero.IsActorAbleToPush((FaceDirection)directionOfPush))
                {
                    if (serverInstanceHero.IsActorPushableInDirection(actorToPush, (FaceDirection)directionOfPush))
                    {
                        Debug.Log("Pushing id: "+playerIdToPush+" direction: "+(FaceDirection)directionOfPush);
                        serverInstanceHero.InitialiseHeroPush(playerIdToPush, directionOfPush);
                    }
                    else
                    {
                        Debug.LogError(playerIdToPush + " IsActorPushableInDirection " + directionOfPush + " Not possible");
                    }
                }
                else
                {
                    Debug.LogError("IsActorAbleToPush " + directionOfPush + " Not possible");
                }
            }
            else
            {
                Debug.LogError("Actor to push : " + playerIdToPush + " Does not exists!");
            }
        }
        else if(Enemy.enemies.ContainsKey(playerIdToPush))
        {
            Enemy enemyToPush = Enemy.enemies[playerIdToPush];
            if(enemyToPush!=null)
            {
                if (serverInstanceHero.IsActorAbleToPush((FaceDirection)directionOfPush))
                {
                    if (serverInstanceHero.IsActorPushableInDirection(enemyToPush, (FaceDirection)directionOfPush))
                    {
                        Debug.Log("Pushing id: " + playerIdToPush + " direction: " + (FaceDirection)directionOfPush);
                        serverInstanceHero.InitialiseEnemyPush(enemyToPush, directionOfPush);
                    }
                    else
                    {
                        Debug.LogError(playerIdToPush + " IsActorPushableInDirection " + directionOfPush + " Not possible");
                    }
                }
                else
                {
                    Debug.LogError("IsActorAbleToPush " + directionOfPush + " Not possible");
                }
            }
            else
            {
                Debug.LogError("Enemy to push : " + playerIdToPush + " Does not exists!");
            }
        }
        else
        {
            Debug.LogError("Does not contain the key in the cdictionary");
        }
    }

    void PlaceGorgonGlassRequestImplementation(OnWorkDone onSuccessfullyItemPlaced)
    {
        if (!CanDoAction("PlaceGorgonGlassRequestImplementation"))
        {
            return;
        }

        //get all floor tiles

        //remove floor tiles
        //convert floor tiles to mirror tiles
        GridManager.instance.gorgonGlassTracker.PlaceGorgonTile(OnSuccessfullyUsedItem);
    }

    void PlaceMirrorRequestImplementation(Vector3Int cellPositionToPlaceDirectionalMirror, OnWorkDone onSuccess)
    {
        if (!CanDoAction("PlaceMirrorRequestImplementation"))
        {
            return;
        }
        if (!GridManager.instance.IsCellBlockedForSpawnObjectPlacementAtPos(cellPositionToPlaceDirectionalMirror))
        {
            Debug.Log("Setting tile mirror on " + cellPositionToPlaceDirectionalMirror);
            GridManager.instance.SetTile(cellPositionToPlaceDirectionalMirror, EnumData.TileType.Mirror, true, false);
            List<TileData> tdList = GridManager.instance.GetAllTileDataInCellPos(cellPositionToPlaceDirectionalMirror);
            foreach (TileData item in tdList)
            {
                GridManager.instance.SetTile(cellPositionToPlaceDirectionalMirror, item.tileType, false, false);
            }
            onSuccess?.Invoke();
        }
        else
        {
            Debug.LogError("Cell is blocked for mirror placement : " + cellPositionToPlaceDirectionalMirror);
        }
    }

    void PlaceArrowDirectionalRequestImplementation(int direction,Vector3Int cellPositionToPlaceDirectionalArrow, OnWorkDone onSuccess)
    {
        if (!CanDoAction("PlaceArrowDirectionalRequestImplementation"))
        {
            return;
        }
        if (!GridManager.instance.IsCellBlockedForSpawnObjectPlacementAtPos(cellPositionToPlaceDirectionalArrow))
        {
            Debug.Log("Setting tile arrow on " + cellPositionToPlaceDirectionalArrow);
            switch((FaceDirection)direction)
            {
                case FaceDirection.Up:
                    GridManager.instance.SetTile(cellPositionToPlaceDirectionalArrow, EnumData.TileType.UpArrow, true, false);
                    break;
                case FaceDirection.Down:
                    GridManager.instance.SetTile(cellPositionToPlaceDirectionalArrow, EnumData.TileType.DownArrow, true, false);
                    break;
                case FaceDirection.Left:
                    GridManager.instance.SetTile(cellPositionToPlaceDirectionalArrow, EnumData.TileType.LeftArrow, true, false);
                    break;
                case FaceDirection.Right:
                    GridManager.instance.SetTile(cellPositionToPlaceDirectionalArrow, EnumData.TileType.RightArrow, true, false);
                    break;
            }
            List<TileData> tdList = GridManager.instance.GetAllTileDataInCellPos(cellPositionToPlaceDirectionalArrow);
            foreach (TileData item in tdList)
            {
                GridManager.instance.SetTile(cellPositionToPlaceDirectionalArrow, item.tileType, false, false);
            }
            onSuccess?.Invoke();
        }
        else
        {
            Debug.LogError("Cell is blocked for arrow placement : " + cellPositionToPlaceDirectionalArrow);
        }
    }

    void PlaceBoulderRequestImplementation(Vector3Int cellPositionToPlaceBoulder,OnWorkDone onItemSuccessfullyUsed)
    {
        if (!CanDoAction("PlaceBoulderRequestImplementation"))
        {
            return;
        }
        if (serverInstanceHero.boulderUsedCount > 0)
        {
            GridManager.instance.boulderTracker.PlaceMedusaBoulderObject(serverInstanceHero, cellPositionToPlaceBoulder, onItemSuccessfullyUsed);
        }
        else
        {
            Debug.LogError("boulders exhausted!!");
        }
    }

    void RemoveBoulderRequestImplementation(Vector3Int cellPositionToRemoveBoulder)
    {
        if (!CanDoAction("RemoveBoulderRequestImplementation"))
        {
            return;
        }
        GridManager.instance.boulderTracker.RemoveBoulder(cellPositionToRemoveBoulder);
    }

    void SpawnItemImplementation(int direction,int spawnItemType, Vector3Int spawnCell)
    {
        if(serverInstanceHero.itemToCast.itemCount>0)
        {
            OnWorkDone onSuccess = OnSuccessfullyUsedItem;
            switch ((EnumData.UsableItemTypes)spawnItemType)
            {
                case EnumData.UsableItemTypes.Boulder:
                    PlaceBoulderRequestImplementation(spawnCell, onSuccess);
                    break;
                case EnumData.UsableItemTypes.ArrowDirectional:
                    PlaceArrowDirectionalRequestImplementation(direction,spawnCell, onSuccess);
                    break;
                case EnumData.UsableItemTypes.MirrorItem:
                    PlaceMirrorRequestImplementation(spawnCell, onSuccess);
                    break;
                case EnumData.UsableItemTypes.GorgonGlass:
                    PlaceGorgonGlassRequestImplementation(onSuccess);
                    break;
                case EnumData.UsableItemTypes.Pitfall:
                    CastPitfallImplementation(direction, onSuccess);
                    break;
                case EnumData.UsableItemTypes.Earthquake:
                    CastEarthQuakeImplementation(onSuccess);
                    break;
                case EnumData.UsableItemTypes.Tornado:
                    CastTornadoForPlayerImplementation(direction, onSuccess);
                    break;
                case EnumData.UsableItemTypes.Portal:
                    CastPortalImplementation(direction, onSuccess);
                    break;
                case EnumData.UsableItemTypes.Minion:
                    PlaceMinionImplementation(direction, spawnCell, onSuccess);
                    break;
                case EnumData.UsableItemTypes.CereberausHead:
                    PlaceCereberausHeadImplementation(direction, spawnCell, onSuccess);
                    break;
                case EnumData.UsableItemTypes.TidalWave:
                    TidalWaveFirePlayerRequestImplementation(spawnCell, onSuccess);
                    break;
                case EnumData.UsableItemTypes.BubbleShield:
                    CastBubbleShieldForPlayerImplementation(spawnCell, onSuccess);
                    break;
                case EnumData.UsableItemTypes.MightyWind:
                    MightyWindFirePlayerRequestImplementation(direction,spawnCell, onSuccess);
                    break;
                case EnumData.UsableItemTypes.FlamePillar:
                    CastFlamePillarForPlayerImplementation(direction, spawnCell, onSuccess);
                    break;
                case EnumData.UsableItemTypes.AeloianMight:
                    CastAeloianMightForPlayerImplementation(onSuccess);
                    break;
                case EnumData.UsableItemTypes.QuickSand:
                    CastQuickSandForPlayerImplementation(onSuccess);
                    break;
                case EnumData.UsableItemTypes.PermamnentBlock:
                    CastPermamentBlock(direction, spawnCell, onSuccess);
                    break;
            }
        }
    }

    void OnSuccessfullyUsedItem()
    {
        serverInstanceHero.itemToCast.itemCount--;
    }

    void CastPermamentBlock(int direction, Vector3Int cellPositionToPlaceMinion, OnWorkDone onSuccess)
    {
        if (!CanDoAction("CastPermamentBlock"))
        {
            return;
        }
        if (!GridManager.instance.IsCellBlockedForSpawnObjectPlacementAtPos(cellPositionToPlaceMinion))
        {
            Debug.Log("Setting tile permewmnt block minion on " + cellPositionToPlaceMinion);
            GridManager.instance.enemySpawnner.InstantiatePetrifiedEnemy(cellPositionToPlaceMinion, direction,EnumData.MonsterBreed.Cyclops);
            onSuccess?.Invoke();
        }
    }

    void PlaceMinionImplementation(int direction, Vector3Int cellPositionToPlaceMinion, OnWorkDone onSuccess)
    {
        if (!CanDoAction("PlaceMinionImplementation"))
        {
            return;
        }
        if (!GridManager.instance.IsCellBlockedForSpawnObjectPlacementAtPos(cellPositionToPlaceMinion))
        {
            Debug.Log("Setting tile minion on " + cellPositionToPlaceMinion);
            int heroType=-1;
            if(serverInstanceHero is Medusa)
            {
                heroType = (int)EnumData.Heroes.Medusa;
            }
            else if(serverInstanceHero is Posidanna)
            {
                heroType = (int)EnumData.Heroes.Posidanna;
            }
            else if (serverInstanceHero is Heliemis)
            {
                heroType = (int)EnumData.Heroes.Heliemis;
            }
            else if (serverInstanceHero is Ermolai)
            {
                heroType = (int)EnumData.Heroes.Ermolai;
            }
            else if (serverInstanceHero is Averna)
            {
                heroType = (int)EnumData.Heroes.Averna;
            }

            GridManager.instance.enemySpawnner.InstantiateEnemy(cellPositionToPlaceMinion, direction, serverInstanceHero.ownerId, heroType);
            onSuccess?.Invoke();
        }
        else
        {
            Debug.LogError("Cell is blocked for minion placement : " + cellPositionToPlaceMinion);
        }
    }

    void PlaceCereberausHeadImplementation(int direction,Vector3Int cellPositionToPlaceCereberausHead, OnWorkDone onSuccess)
    {
        if (!CanDoAction("PlaceCereberausHeadImplementation"))
        {
            return;
        }
        if (!GridManager.instance.IsCellBlockedForSpawnObjectPlacementAtPos(cellPositionToPlaceCereberausHead))
        {
            Debug.Log("Setting tile cereberausHead on " + cellPositionToPlaceCereberausHead);
            GridManager.instance.SetTile(cellPositionToPlaceCereberausHead,GridManager.instance.GetCereberausHeadTypeFromDirection((FaceDirection)direction), true, false);
            onSuccess?.Invoke();
        }
        else
        {
            Debug.LogError("Cell is blocked for cereberausHead placement : " + cellPositionToPlaceCereberausHead);
        }
    }

    void ChangeCharacterCommandForPlayerImplementation(int characterHero)
    {
        if (characterHero != (int)serverInstanceHero.hero)
        {
            Debug.Log("ChangeCharacterCommandForPlayerImplementation " + ((EnumData.Heroes)characterHero).ToString());

            Hero previousHero = serverInstanceHero;

            PositionUpdates positionUpdate = new PositionUpdates(previousHero.actorTransform.position,previousHero.currentMovePointCellPosition,previousHero.previousMovePointCellPosition, (int)previousHero.Facing,(int)previousHero.PreviousFacingDirection);
            PlayerFlyData playerFlyData = new PlayerFlyData(previousHero.flyingTickCountTemp);

            SetCharacter(characterHero, playerFlyData, positionUpdate);

            Destroy(previousHero.transform.parent.gameObject);
        }
        else
        {
            Debug.LogError("Character cant be changed");
        }
    }

    void CastQuickSandForPlayerImplementation(OnWorkDone onSuccess)
    {
        if (!CanDoAction("CastQuickSandForPlayerImplementation"))
        {
            return;
        }
        GridManager.instance.CastQuickSand(serverInstanceHero);
        onSuccess?.Invoke();
    }

    void CastAeloianMightForPlayerImplementation(OnWorkDone onSuccess)
    {
        if (!CanDoAction("CastAeloianMightForPlayerImplementation"))
        {
            return;
        }
        GridManager.instance.CastAeloianMight(serverInstanceHero.ownerId);
        onSuccess?.Invoke();
    }

    void CastFlamePillarForPlayerImplementation(int direction,Vector3Int predictedCell,OnWorkDone onItemSuccessfullyUsed)
    {
        if (!CanDoAction("CastFlamePillarForPlayerImplementation"))
        {
            return;
        }
        if (serverInstanceHero.flamePillarUsedCount > 0)
        {
            if (serverInstanceHero.IsProjectilePlacable(predictedCell, (FaceDirection)direction))
            {
                serverInstanceHero.CastFlamePillar(new Attack(0, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.FlamePillar), predictedCell);
                onItemSuccessfullyUsed?.Invoke();
            }
        }
        else
        {
            Debug.Log("Exhausted flame pillar use count");
        }
    }

    void TidalWaveFirePlayerRequestImplementation(Vector3Int predictedCell, OnWorkDone onItemSuccessfullyUsed)
    {
        if (!CanDoAction("TidalWaveFirePlayerRequestImplementation"))
        {
            return;
        }
        if (serverInstanceHero.tidalWaveUsedCount > 0)
        {
            Debug.Log("TidalWaveFirePlayerRequestImplementation ");
            serverInstanceHero.FireProjectile(new Attack(0, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.TidalWave), predictedCell);
            onItemSuccessfullyUsed?.Invoke();
        }
        else
        {
            Debug.Log("Exhausted tidal wave use count");
        }
    }

    void CastBubbleShieldForPlayerImplementation(Vector3Int predictedCell,OnWorkDone onItemSuccessfullyUsed)
    {
        if (!CanDoAction("CastBubbleShieldForPlayerImplementation"))
        {
            return;
        }
        bool isProjectilePlacableUp = serverInstanceHero.IsProjectilePlacable(predictedCell, FaceDirection.Up);
        bool isProjectilePlacableDown = serverInstanceHero.IsProjectilePlacable(predictedCell, FaceDirection.Down);
        bool isProjectilePlacableLeft = serverInstanceHero.IsProjectilePlacable(predictedCell, FaceDirection.Left);
        bool isProjectilePlacableRight = serverInstanceHero.IsProjectilePlacable(predictedCell, FaceDirection.Right);
        if (isProjectilePlacableUp)
        {
            serverInstanceHero.CastBubbleShield(new Attack(0, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.BubbleShield),predictedCell, FaceDirection.Up);
        }
        else
        {
            Debug.Log("bubbleshield-Hero is not able to fire up projectiles");
        }

        if (isProjectilePlacableDown)
        {
            serverInstanceHero.CastBubbleShield(new Attack(0, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.BubbleShield), predictedCell, FaceDirection.Down);
        }
        else
        {
            Debug.Log("bubbleshield-Hero is not able to fire down projectiles");
        }


        if (isProjectilePlacableLeft)
        {
            serverInstanceHero.CastBubbleShield(new Attack(0, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.BubbleShield),predictedCell, FaceDirection.Left);
        }
        else
        {
            Debug.Log("bubbleshield-Hero is not able to fire left projectiles");
        }


        if (isProjectilePlacableRight)
        {
            serverInstanceHero.CastBubbleShield(new Attack(0, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.BubbleShield),predictedCell, FaceDirection.Right);
        }
        else
        {
            Debug.Log("bubbleshield-Hero is not able to fire right projectiles");
        }

        if(isProjectilePlacableUp|| isProjectilePlacableDown|| isProjectilePlacableLeft|| isProjectilePlacableRight)
        {
            onItemSuccessfullyUsed?.Invoke();
        }
    }

    

    void CastPortalImplementation(int direction, OnWorkDone onSuccess)
    {
        if (!CanDoAction("CastPortalImplementation"))
        {
            return;
        }
        Debug.Log("CastPortalImplementation ");
        Vector3Int cellToPlacePortalOn = GridManager.instance.grid.WorldToCell(serverInstanceHero.actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3((FaceDirection)direction));
        if (!GridManager.instance.IsCellBlockedForSpawnObjectPlacementAtPos(cellToPlacePortalOn))
        {
            GridManager.instance.portalTracker.PlacePortal(serverInstanceHero.ownerId, cellToPlacePortalOn, onSuccess);
        }
        else
        {
            Debug.LogError("Portal-Hero is not able to place portals");
        }
    }

    void CastTornadoForPlayerImplementation(int direction, OnWorkDone onItemSuccessfullyUsed)
    {
        if (!CanDoAction("CastTornadoForPlayerImplementation"))
        {
            return;
        }
        

        if (serverInstanceHero.tornadoPlacedUsedCount > 0)
        {
            Vector3Int cellToPlaceTornadoOn = GridManager.instance.grid.WorldToCell(serverInstanceHero.actorTransform.position + GridManager.instance.GetFacingDirectionOffsetVector3((FaceDirection)direction));
            GridManager.instance.tornadoTracker.PlaceTornadoObject(serverInstanceHero, direction, cellToPlaceTornadoOn, onItemSuccessfullyUsed);
        }
        else
        {
            Debug.Log("Tornado placement limit exceeded");
        }
    }

    void MightyWindFirePlayerRequestImplementation(int direction,Vector3Int cellPredicted,OnWorkDone onItemSuccessfullyUsed)
    {
        if (!CanDoAction("MightyWindFirePlayerRequestImplementation"))
        {
            return;
        }
        Debug.Log("MightyWindFirePlayerRequestImplementation ");
        if (serverInstanceHero.IsProjectilePlacable(cellPredicted, (FaceDirection)direction))
        {
            serverInstanceHero.FireProjectile(new Attack(0, EnumData.AttackTypes.ProjectileAttack, EnumData.Projectiles.MightyWind),
                cellPredicted);

            onItemSuccessfullyUsed?.Invoke();
        }
        else
        {
            Debug.LogError("MightyWind-Hero is not able to fire projectiles");
        }
    }

    void CastEarthQuakeImplementation(OnWorkDone onSuccess)
    {
        if (!CanDoAction("CastEarthQuakeImplementation"))
        {
            return;
        }
        if (!serverInstanceHero.completedMotionToMovePoint)
        {
            Debug.LogError("havent completed motion");
            return;
        }
        Debug.Log("CastEarthQuake ");
        GridManager.instance.EarthQuake(serverInstanceHero,GridManager.instance.grid.WorldToCell(serverInstanceHero.actorTransform.position));
        onSuccess?.Invoke();
    }

    void CastPitfallImplementation(int direction, OnWorkDone onSuccess)
    {
        if (!CanDoAction("CastPitfallImplementation"))
        {
            return;
        }
        if (!serverInstanceHero.completedMotionToMovePoint)
        {
            Debug.LogError("havent completed motion");
            return;
        }
        Debug.Log("CastPitfall ");
        Vector3Int cellToCheck = GridManager.instance.grid.WorldToCell(serverInstanceHero.actorTransform.position + 2 * GridManager.instance.GetFacingDirectionOffsetVector3((FaceDirection) direction));
        if (GridManager.instance.HasTileAtCellPoint(cellToCheck, EnumData.TileType.Normal))
        {
            serverInstanceHero.CastPitfall(cellToCheck);
            onSuccess?.Invoke();
        }
        else
        {
            Debug.LogError("No normal tile on placable position");
        }
    }
    void PetrifyPlayerRequestImplementation(int playerIdToPetrify)
    {
        if (serverInstanceHero.isPhysicsControlled)
        {
            Debug.LogError("PetrifyPlayerRequestImplementation server player is phyhsics controlled hence request failed");
            return;
        }
        if (!serverInstanceHero.isPushed)
        {
            Debug.Log("PetrifyPlayerRequestImplementation playerIdToPetrify " + playerIdToPetrify);
            Server.clients[playerIdToPetrify].serverMasterController.serverInstanceHero.Petrify();
        }
        else
        {
            Debug.LogError("I was pushed when i implemented the petrification command hence failed");
        }
    }

    void LandPlayerImplementation()
    {
        if (serverInstanceHero.isRespawnningPlayer)
        {
            Debug.LogError("LandPlayerImplementation Is respawnning");
            return;
        }
        if (serverInstanceHero.isPhysicsControlled)
        {
            Debug.LogError("LandPlayerImplementation server player is phyhsics controlled hence request failed");
            return;
        }
        if (serverInstanceHero.isPetrified)
        {
            Debug.LogError("LandPlayerImplementation server player is petrified hence request failed");
            return;
        }
        if (serverInstanceHero.isPushed)
        {
            Debug.LogError("LandPlayerImplementation server player is pushed hence request failed");
            return;
        }
        if (!serverInstanceHero.isInFlyingState)
        {
            Debug.LogError("LandPlayerImplementation server player is not isInFlyingState hence request failed");
            return;
        }
        //land here
        if (serverInstanceHero.isInFlyingState)
        {
            //land here
            serverInstanceHero.flyingTickCountTemp = 0;
        }
    }

    void RespawnPlayerRequestImplementation()
    {
        if (!serverInstanceHero.isRespawnningPlayer)
        {
            Debug.LogError("RespawnPlayerRequestImplementation Is not respawnning");
            return;
        }
        if (serverInstanceHero.isPhysicsControlled)
        {
            Debug.LogError("RespawnPlayerRequestImplementation server player is phyhsics controlled hence request failed");
            return;
        }
        if (serverInstanceHero.isPetrified)
        {
            Debug.LogError("RespawnPlayerRequestImplementation server player is petrified hence request failed");
            return;
        }
        if(serverInstanceHero.isPushed)
        {
            Debug.LogError("RespawnPlayerRequestImplementation server player is pushed hence request failed");
            return;
        }
        if (serverInstanceHero.isInFlyingState)
        {
            Debug.LogError("RespawnPlayerRequestImplementation server player is isInFlyingState hence request failed");
            return;
        }
        Vector3Int cell = GridManager.instance.grid.WorldToCell(serverInstanceHero.actorTransform.position);
        if (serverInstanceHero.IsPlayerSpawnable(cell))
        {
            //Respawn here
            Debug.Log("RespawnPlayerRequestImplementation " + cell);
            serverInstanceHero.SpawnPlayer();
        }
        else
        {
            Debug.LogError("Invalid location to spawn player");
        }
    }
    #endregion

    private void FixedUpdate()
    {
        for (int i = 0; i < (int)currentInputProcessingModeOnServer; i++)
        {
            CheckForChangeCharacterRequestForPlayer(playerSequenceNumberProcessed+1);
            CheckForPushRequestOnServer(playerSequenceNumberProcessed+1);
            CheckForPlaceBoulderRequestOnServer(playerSequenceNumberProcessed+1);
            CheckForRemovingBoulderRequestOnServer(playerSequenceNumberProcessed+1);
            CheckForSpawnItemRequestOnServer(playerSequenceNumberProcessed+1);
            CheckForTidalWaveFireRequestOnPlayer(playerSequenceNumberProcessed+1);
            CheckForMightyWindFireRequestOnPlayer(playerSequenceNumberProcessed+1);
            CheckForBubbleShieldRequestForPlayer(playerSequenceNumberProcessed+1);
            CheckForTornadoRequestForPlayer(playerSequenceNumberProcessed+1);
            CheckForPitfallRequestForPlayer(playerSequenceNumberProcessed+1);
            CheckForEarthQuakeRequestForPlayer(playerSequenceNumberProcessed+1);
            CheckForFlamePillarRequestForPlayer(playerSequenceNumberProcessed+1);
            CheckForPetrificationRequestOnPlayer(playerSequenceNumberProcessed+1);
            CheckForRespawnningRequestOnServer(playerSequenceNumberProcessed+1);
            CheckForLandingPlayerRequestOnServer(playerSequenceNumberProcessed+1);

            InputCommands inputPackageCorrespondingToSeq;

            if (inputUpdatesFromClientToServerDic.TryGetValue(playerSequenceNumberProcessed + 1, out inputPackageCorrespondingToSeq))
            {
                inputUpdatesFromClientToServerDic.Remove(inputPackageCorrespondingToSeq.sequenceNumber);
                latestPlayerInputPackage = inputPackageCorrespondingToSeq;
                playerSequenceNumberProcessed = inputPackageCorrespondingToSeq.sequenceNumber;


                serverInstanceHero.ProcessCollisionEnter();
                serverInstanceHero.ProcessMovementInputs(inputPackageCorrespondingToSeq.commands
                    , inputPackageCorrespondingToSeq.previousCommands);

                serverInstanceHero.ProcessEventsInputs(inputPackageCorrespondingToSeq.commands, inputPackageCorrespondingToSeq.previousCommands);
                serverInstanceHero.ProcessAnimationsInputs(inputPackageCorrespondingToSeq.commands, inputPackageCorrespondingToSeq.previousCommands);

                serverInstanceHero.ProcessInputMovementsControl();
                serverInstanceHero.ProcessFlyingControl();
                serverInstanceHero.ProcessInputEventControl();
                serverInstanceHero.ProcessInputAnimationControl();
                //Debug.Log(serverInstanceHero.movePoint.position + "Block pos "+serverInstanceHero.GetBlockPosition()+"<color=yellow>Processing seqence no </color>" + inputPackageCorrespondingToSeq.sequenceNumber + "<color=green>position </color>" + serverInstanceHero.actorTransform.position + "<color=green>inputs </color>" + inputPackageCorrespondingToSeq.commands[0] + inputPackageCorrespondingToSeq.commands[1] + inputPackageCorrespondingToSeq.commands[2] + inputPackageCorrespondingToSeq.commands[3]+" Previous Commands "+ inputPackageCorrespondingToSeq.previousCommands[0] + inputPackageCorrespondingToSeq.previousCommands[1] + inputPackageCorrespondingToSeq.previousCommands[2] + inputPackageCorrespondingToSeq.previousCommands[3]);
            }
            else
            {
                if (latestPlayerInputPackage.sequenceNumber != 0)
                {
                    //Debug.LogError("Could not find any inputToProcess for  seq: " + (sequenceNumberProcessed + 1));
                    playerSequenceNumberProcessed = playerSequenceNumberProcessed + 1;
                    
                    serverInstanceHero.ProcessCollisionEnter();
                    serverInstanceHero.ProcessMovementInputs(latestPlayerInputPackage.commands
                    , latestPlayerInputPackage.previousCommands);

                    serverInstanceHero.ProcessEventsInputs(latestPlayerInputPackage.commands, latestPlayerInputPackage.previousCommands);
                    serverInstanceHero.ProcessAnimationsInputs(latestPlayerInputPackage.commands, latestPlayerInputPackage.previousCommands);

                    serverInstanceHero.ProcessInputMovementsControl();
                    serverInstanceHero.ProcessFlyingControl();
                    serverInstanceHero.ProcessInputEventControl();
                    serverInstanceHero.ProcessInputAnimationControl();
                }
            }
        }

        serverLocalSequenceNumber++;

        //////Debug.Log("<color=blue>inputsequence </color>"+ playerMovingCommandSequenceNumber + "<color=blue>inputs </color> "+ inputs[0]+" "+inputs[1]+" "+inputs[2]+" "+inputs[3]);
        PlayerAuthoratativeStates playerAuthoratativeStates = new PlayerAuthoratativeStates(
              serverInstanceHero.isPetrified
            , serverInstanceHero.isPushed
            , serverInstanceHero.isPhysicsControlled
            , serverInstanceHero.isInputFreezed
            , serverInstanceHero.isMovementFreezed
            , serverInstanceHero.isInvincible
            , serverInstanceHero.isRespawnningPlayer
            , serverInstanceHero.inCharacterSelectionScreen
            , serverInstanceHero.inGame
            , serverInstanceHero.currentHP
            , serverInstanceHero.currentStockLives
            , serverInstanceHero.hero
            , new ItemToCast((int)serverInstanceHero.itemToCast.castableItemType, (int)serverInstanceHero.itemToCast.usableItemType, serverInstanceHero.itemToCast.itemCount));

        PositionUpdates positionUpdates = new PositionUpdates(serverInstanceHero.actorTransform.position, serverInstanceHero.currentMovePointCellPosition
            , serverInstanceHero.previousMovePointCellPosition,(int)serverInstanceHero.Facing,(int)serverInstanceHero.PreviousFacingDirection);
        PlayerEvents playerEvents = new PlayerEvents(serverInstanceHero.isFiringPrimaryProjectile,
            serverInstanceHero.isFiringItemEyeLaser
            , serverInstanceHero.isFiringItemFireball
            , serverInstanceHero.isFiringItemStarShower
            , serverInstanceHero.isFiringItemCentaurBow);
        PlayerAnimationEvents playerAnimationEvents = new PlayerAnimationEvents(serverInstanceHero.isWalking,serverInstanceHero.isFlying, serverInstanceHero.isUsingPrimaryMove);
        PlayerFlyData playerFlyData = new PlayerFlyData(serverInstanceHero.flyingTickCountTemp);

        PlayerStateUpdates playerStateUpdates = new PlayerStateUpdates(serverLocalSequenceNumber,playerSequenceNumberProcessed, playerAuthoratativeStates, positionUpdates, playerEvents, playerAnimationEvents, playerFlyData);
        PlayerStateServerUpdates playerStateServerUpdates = new PlayerStateServerUpdates(id,connectionID, playerStateUpdates);
        //Debug.LogError("Server sequence number : "+ serverLocalSequenceNumber+" player sequence number processeed: "+ playerSequenceNumberProcessed+" player position : "+serverInstanceHero.actorTransform.position);

        playerStateListOnServer.Add(playerStateServerUpdates);

        if (playerStateListOnServer.Count >= snapShotsInOnePacket)
        {
            if (previousPlayerUpdatedStatePacks.Count > packetHistorySize)
            {
                previousPlayerUpdatedStatePacks.RemoveAt(0);
            }
            ServerSend.PlayerStateSend(playerStateListOnServer, previousPlayerUpdatedStatePacks);

            previousPlayerUpdatedStatePacks.Add(new PreviousPlayerUpdatedStatePacks(playerStateListOnServer.ToArray()));

            playerStateListOnServer.Clear();

            //Debug.Log("<color=red>--------------------------------------------------------------------</color>");
        }
        serverInstanceHero.ProcessAuthoratativeEvents();

        UpdateClientInputsProcessMode();
        snapShotBufferSize = GetTheLastestSequenceNoInDic() - playerSequenceNumberProcessed;
        //Debug.Log("<color=cyan>Dic Count </color>"+inputUpdatesFromClientToServerDic.Count);
    }


    int GetTheLastestSequenceNoInDic()
    {
        int largestInt = 0;
        foreach (KeyValuePair<int, InputCommands> kvp in inputUpdatesFromClientToServerDic)
        {
            if (largestInt < kvp.Key)
            {
                largestInt = kvp.Key;
            }
        }
        return largestInt;
    }

    void UpdateClientInputsProcessMode()
    {
        if (inputUpdatesFromClientToServerDic.Count == 0)
        {
            currentInputProcessingModeOnServer = ProcessMode.Lazy;
        }
        else if (currentInputProcessingModeOnServer == ProcessMode.Lazy && inputUpdatesFromClientToServerDic.Count >= idealSnapshotBufferCount)
        {
            currentInputProcessingModeOnServer = ProcessMode.Ideal;
        }
        else if (currentInputProcessingModeOnServer == ProcessMode.Ideal && inputUpdatesFromClientToServerDic.Count > maxedOutSnapshotBufferCount)
        {
            currentInputProcessingModeOnServer = ProcessMode.Hyper;
        }
        else if (currentInputProcessingModeOnServer == ProcessMode.Hyper && inputUpdatesFromClientToServerDic.Count <= idealSnapshotBufferCount)
        {
            currentInputProcessingModeOnServer = ProcessMode.Ideal;
        }
    }

    public void SetCharacter(int characterHero,PlayerFlyData playerFlyData,PositionUpdates positionUpdate)
    {
        Hero serverInstanceHero = Instantiate(Resources.Load("Characters/" + ((EnumData.Heroes)characterHero).ToString() + "/ServerInstance-" + ((EnumData.Heroes)characterHero).ToString()) as GameObject, transform, false).GetComponentInChildren<Hero>();
        
        this.serverInstanceHero = serverInstanceHero;
        this.serverInstanceHero.hero = characterHero;
        
        this.serverInstanceHero.inCharacterSelectionScreen = (ServerSideGameManager.instance.currentGameState == EnumData.GameState.CharacterSelection);
        this.serverInstanceHero.inGame = (ServerSideGameManager.instance.currentGameState == EnumData.GameState.Gameplay);

        this.serverInstanceHero.SetActorPositionalState(positionUpdate);
        this.serverInstanceHero.SetFlyingTickCount(playerFlyData);
        this.serverInstanceHero.InitialiseHP();
        this.serverInstanceHero.InitialiseStockLives();
        this.serverInstanceHero.InitialiseServerActor(this, id);
    }
}


public struct PlayerStateServerUpdates
{
    public int playerId;
    public string connectionId;
    public PlayerStateUpdates playerStateUpdates;

    public PlayerStateServerUpdates(int playerId,string connectionId, PlayerStateUpdates playerStateUpdates)
    {
        this.playerId = playerId;
        this.connectionId = connectionId;
        this.playerStateUpdates = playerStateUpdates;
    }
}

public struct PreviousPlayerUpdatedStatePacks
{
    public PlayerStateServerUpdates[] previousUpdatedStates;

    public PreviousPlayerUpdatedStatePacks(PlayerStateServerUpdates[] previousUpdatedPositions)
    {
        this.previousUpdatedStates = previousUpdatedPositions;
    }
}

public enum ProcessMode
{
    Lazy=0,
    Ideal,
    Hyper
}