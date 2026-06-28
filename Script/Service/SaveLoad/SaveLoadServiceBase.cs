using System;
using System.Threading.Tasks;
using UnityEngine;

namespace ETServerSystem
{
    /// <summary>
    /// Generic abstract base class that implements the core flow and lifetime management for saving and loading.
    /// </summary>
    /// <typeparam name="TState">The type representing the game state.</typeparam>
    public abstract class SaveLoadServiceBase<TState> : ISaveLoadService<TState> where TState : class, new()
    {
        protected float autoSaveInterval = 30f;
        protected float timeSinceLastSave = 0f;
        protected bool isSaving = false;

        protected const float SERVER_SAVE_COOLDOWN = 10f;
        protected float lastServerSaveTime = float.NegativeInfinity;

        protected TState gameStateData;

        /// <inheritdoc />
        public bool IsBanned { get; protected set; }

        /// <inheritdoc />
        public TState GameState => gameStateData;

        protected SaveLoadServiceBase(TState gameStateData)
        {
            this.gameStateData = gameStateData ?? new TState();
        }

        /// <inheritdoc />
        public virtual async Task SaveGameStateAsync()
        {
            isSaving = true;
            try
            {
                var dataToSave = gameStateData;

                // Save local gameplay state to device
                SaveLocalState(dataToSave);

                // Save server data to server (subject to cooldown)
                float now = Time.realtimeSinceStartup;
                if (now - lastServerSaveTime >= SERVER_SAVE_COOLDOWN)
                {
                    lastServerSaveTime = now;
                    await SaveServerStateAsync(dataToSave);
                }
                else
                {
                    Debug.Log($"[{GetType().Name}] Server save skipped (cooldown), local saved");
                }
            }
            finally
            {
                isSaving = false;
            }
        }

        /// <inheritdoc />
        public virtual void SaveLocalStateOnly()
        {
            try
            {
                SaveLocalState(gameStateData);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{GetType().Name}] SaveLocalStateOnly failed: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public virtual void SaveSessionTimestampOnQuit()
        {
            try
            {
                UpdateSessionTimestamp();
                SaveSessionTimestampToServer();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{GetType().Name}] SaveSessionTimestampOnQuit failed: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public virtual async Task LoadGameStateAsync()
        {
            // Load server data
            TState serverState = await LoadServerStateAsync();

            // Check server-side ban
            if (serverState != null && IsStateBanned(serverState))
            {
                IsBanned = true;
                Debug.LogWarning($"[{GetType().Name}] Server hard-banned this device. Aborting game data load.");
                return;
            }
            // Load local data
            TState localState = LoadLocalState();

            // Merge local data into server data and update active in-memory instance
            MergeStates(serverState, localState);
        }

        /// <inheritdoc />
        public abstract Task ClearSaveAsync();

        /// <inheritdoc />
        public abstract Task<bool> SaveExistsAsync();

        /// <inheritdoc />
        public abstract string GetSaveFilePath();

        /// <inheritdoc />
        public virtual void AutoSaveUpdate()
        {
            timeSinceLastSave += Time.deltaTime;
            if (timeSinceLastSave >= autoSaveInterval && !isSaving)
            {
                _ = SaveGameStateAsync();
                timeSinceLastSave = 0f;
            }
        }

        // --- Abstract hooks to be implemented by subclass ---

        protected abstract void SaveLocalState(TState state);
        protected abstract TState LoadLocalState();
        protected abstract Task SaveServerStateAsync(TState state);
        protected abstract Task<TState> LoadServerStateAsync();
        protected abstract void UpdateSessionTimestamp();
        protected abstract void SaveSessionTimestampToServer();
        protected abstract void MergeStates(TState serverState, TState localState);
        protected abstract bool IsStateBanned(TState state);
    }
}
