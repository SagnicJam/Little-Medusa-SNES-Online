using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
namespace MedusaMultiplayer
{
    public interface IMatchMake
    {
        Task JoinQueue();
        Task OnMatchMakingCompleted(int matchId);
        void OnClientMatchStarted(int matchId);
        void OnMatchFound();
        Task LeaveQueue();
        Task AbortMatch(int matchId);
        Task OnMatchEnded(int matchId);
        Task OnMatchAborted(int matchId);
    }
}