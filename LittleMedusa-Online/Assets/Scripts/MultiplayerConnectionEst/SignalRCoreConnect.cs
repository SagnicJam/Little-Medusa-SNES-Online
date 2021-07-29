using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BestHTTP.Examples;
using BestHTTP.ServerSentEvents;
using BestHTTP.SignalRCore;
using BestHTTP.SignalRCore.Encoders;
using UnityEngine;
public class SignalRCoreConnect : MonoBehaviour
{
    public HubConnection _connection;

    public static SignalRCoreConnect instance;

    private void Awake()
    {
        instance = this;
    }
    
    private async void OnDisable()
    {
        if (_connection != null) await _connection.CloseAsync();
    }


    void InitialiseHubConnection(string username)
    {
        //public ip here//private ip in server
        _connection = new HubConnection(new Uri("https://localhost:5001/gamehub?user=" + username)
            , new JsonProtocol(new LitJsonEncoder()), new HubOptions());

        _connection.OnError += Hub_OnError;
        _connection.OnConnected += Hub_OnConnected;
        _connection.OnReconnected += Hub_OnReconnected;
        _connection.OnReconnecting += Hub_OnReconnecting;
        _connection.OnClosed += Hub_OnClosed;
        _connection.OnRedirected += Hub_OnRedirected;
        _connection.OnTransportEvent += Hub_OnTransportEvent;
        if(!MultiplayerManager.instance.isServer)
        {
            _connection.On<Match>(nameof(OnMatchStartedOnClients), OnMatchStartedOnClients);
        }
    }


    public async Task ServerConnectSignalR(string username,int port, OnWorkDone<int> onCompleted)
    {
        InitialiseHubConnection(username);
        Debug.Log("Connecting for server!");
        await _connection.ConnectAsync();
        Debug.Log("Connection Complete...");
        onCompleted?.Invoke(port);
    }

    public async Task ClientConnectSignalR(string username,OnWorkDone onCompleted)
    {
        InitialiseHubConnection(username);
        Debug.Log("Connecting for client!");
        Loader loader=null;
        if (!MultiplayerManager.instance.isServer)
        {
            loader = Instantiate(MultiplayerManager.instance.loader, MultiplayerManager.instance.Canvas, false);
            loader.StartLoading();
        }
        await _connection.ConnectAsync();
        Debug.Log("Connection Complete...");
        onCompleted?.Invoke();
        if (!MultiplayerManager.instance.isServer)
        {
            if (loader != null)
            {
                loader.SetMessage("User succssfully connected");
                loader.transform.SetAsLastSibling();
            }
        }
    }

    public async Task StartMatch(int port)
    {
        OnWorkDone<Match> OnMatchStartedOnServerResponse = OnMatchStartedOnServer;
        Debug.Log("On Match started on server at port "+ port);
        await SendAsyncData("OnMatchStarted", port, OnMatchStartedOnServerResponse);
    }

    void OnMatchStartedOnServer(Match match)
    {
        Debug.Log("Match has successfully begun in the server at port"+ match.MatchID);
    }

    public void OnMatchStartedOnClients(Match match)
    {
        MultiplayerManager.instance.OnMatchBegun(match);
    }

    private void Hub_OnTransportEvent(HubConnection hubConnection, ITransport transport, TransportEvents transportEvents)
    {
        Debug.Log("MessageHub_Hub_OnTransportEvent: " +
                   $"Transport(<color=green>{transport.TransportType}</color>) " +
                   $"event: <color=green>{transportEvents}</color>"
        );
    }
    private void Hub_OnRedirected(HubConnection hubConnection, Uri uri1, Uri uri2)
    {
        Debug.Log("MessageHub_Hub_OnRedirected: " +
                   $"Hub_OnRedirected Called. Uri1 = {uri1}, Uri2 = {uri2}");
    }
    private bool Hub_OnMessage(HubConnection hubConnection, Message message)
    {
        Debug.Log("MessageHub_Hub_OnMessage: " +
                   $"Hub's OnMessage Called: Message received: {message}");

        // When returning false, no further processing is done by the plugin..
        return false;
    }
    private void Hub_OnClosed(HubConnection hubConnection)
    {
        Debug.Log("MessageHub_Hub_OnClosed: " +
                   "HubConnection is Closed!");
    }
    private void Hub_OnReconnecting(HubConnection hub, string message)
    {
        Debug.Log("MessageHub_Hub_OnReconnecting: " +
                   "Hub is Reconnecting!.. Message: " + message);
    }
    private void Hub_OnReconnected(HubConnection hubConnection)
    {
        Debug.Log("MessageHub_Hub_OnReconnected: " +
                   "Hub Reconnected!");
    }
    private void Hub_OnConnected(HubConnection hubConnection)
    {
        Debug.Log("MessageHub_Hub_OnConnected: " +
                   "Hub Connected! ");
    }
    public void Hub_OnError(HubConnection hubConnection, string error)
    {
        Debug.Log("MessageHub_Hub_OnError: " +
                   $"The HubConnection has an Error: {error}");
    }

    public async Task SendAsyncData<V>(string method,OnWorkDone<V> onComplete)
    {
        Debug.Log("SendAsync "+method);
        Loader loader=null;
        if (!MultiplayerManager.instance.isServer)
        {
            loader = Instantiate(MultiplayerManager.instance.loader, MultiplayerManager.instance.Canvas, false);
            loader.StartLoading();
        }
        
        try
        {
            Response<V> response = await _connection.InvokeAsync<Response<V>>(method);
            Debug.Log("Json data is " + JsonUtility.ToJson(response));
            if (response.Success)
            {
                Debug.Log("Done Success");
                onComplete?.Invoke(response.data);
            }
            if (!MultiplayerManager.instance.isServer)
            {
                if (loader != null)
                {
                    loader.SetMessage(response.Message);
                    loader.transform.SetAsLastSibling();
                }
            }

        }
        catch (TaskCanceledException ex)
        {
            Debug.LogError("ex" + ex);
        }
    }

    public async Task SendAsyncData<T,V>(string method, T data,OnWorkDone<V> onComplete)
    {
        Debug.Log("SendAsync " + method);
        Loader loader = null;
        if (!MultiplayerManager.instance.isServer)
        {
            loader = Instantiate(MultiplayerManager.instance.loader, MultiplayerManager.instance.Canvas, false);
            loader.StartLoading();
        }

        try
        {
            Debug.Log("Getting reposne now!! for data : "+data);
            Response<V> response = await _connection.InvokeAsync<Response<V>>(method, data);
            Debug.Log("Json data is " + JsonUtility.ToJson(response));
            if (response.Success)
            {
                Debug.Log("Done Success");
                onComplete?.Invoke(response.data);
            }
            if (!MultiplayerManager.instance.isServer)
            {
                if (loader != null)
                {
                    loader.SetMessage(response.Message);
                    loader.transform.SetAsLastSibling();
                }
            }
        }
        catch (TaskCanceledException ex)
        {
            Debug.LogError("ex" + ex);
        }
    }
}

public struct Response<V>
{
    public bool Success;
    public string Message;
    public V data;
}