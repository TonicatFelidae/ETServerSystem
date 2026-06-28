using UnityEngine;
namespace ETServerSystem
{
    /// <summary>
    /// Interface for server state data.
    /// </summary>
    public interface IServerStateData
    {
        // Server hard-ban flag (country/IP/user). Server returns banned=true with an
        // empty default state when the request is hard-banned at loadFullGameState.
        bool Banned { get; set; }
        // Client-only transient flag (never serialized to/from the server): set true when
        // the game-state GET itself failed (offline / server down), so we can distinguish
        // "network failure returned an empty state" from "a real empty account" and refuse
        // to auto-save that empty state back over the server's real data.
        bool LoadFailed { get; set; }
        int? Platform { get; set; }
        string Idfv { get; set; }
        string AppSetId { get; set; }
        string AdjustAdId { get; set; }

        //EXAMPLE FOR GAMESTATEDATA
        /*
          public GameStateData()
        {

        }
        public void InitData(GameData gameData)
        {


        }
        public void ImportFrom(GameStateData source)
        {
            platform = source.platform;
            idfv = source.idfv;
            appSetId = source.appSetId;
            adjustAdId = source.adjustAdId;
        }
        */

    }
}
