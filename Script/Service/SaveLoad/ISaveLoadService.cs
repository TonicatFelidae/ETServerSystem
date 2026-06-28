using System.Threading.Tasks;

namespace ETServerSystem
{
    /// <summary>
    /// Generic interface for saving and loading player game state.
    /// </summary>
    /// <typeparam name="TState">The type representing the game state.</typeparam>
    public interface ISaveLoadService<TState>
    {
        /// <summary>
        /// Gets whether the current session is banned by the server.
        /// </summary>
        bool IsBanned { get; }

        /// <summary>
        /// Gets the current in-memory game state.
        /// </summary>
        TState GameState { get; }

        /// <summary>
        /// Saves both local (device) and server state with cooldown management.
        /// </summary>
        Task SaveGameStateAsync();

        /// <summary>
        /// Saves the local (device) state immediately. Useful on quit/destroy.
        /// </summary>
        void SaveLocalStateOnly();

        /// <summary>
        /// Saves the session timestamp and days elapsed to the server. Useful on quit/destroy.
        /// </summary>
        void SaveSessionTimestampOnQuit();

        /// <summary>
        /// Loads the complete game state, merging server and local state.
        /// </summary>
        Task LoadGameStateAsync();

        /// <summary>
        /// Clears all saved data from both local storage and the server.
        /// </summary>
        Task ClearSaveAsync();

        /// <summary>
        /// Checks if there is any saved data on the server.
        /// </summary>
        Task<bool> SaveExistsAsync();

        /// <summary>
        /// Returns the file path for the local backup save file.
        /// </summary>
        string GetSaveFilePath();

        /// <summary>
        /// Updates the auto-save timer and triggers a save when the interval elapsed.
        /// </summary>
        void AutoSaveUpdate();
    }
}
