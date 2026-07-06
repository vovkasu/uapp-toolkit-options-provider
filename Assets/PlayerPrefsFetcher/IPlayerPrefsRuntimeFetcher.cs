#if PLAYER_PREFS_RUNTIME_TOOL
using System.Collections.Generic;

namespace YummyDev.PlayerPrefsEditor
{
    /// <summary>
    /// Interface for platform-specific PlayerPrefs fetcher implementations.
    /// </summary>
    public interface IPlayerPrefsRuntimeFetcher
    {
        /// <summary>
        /// Retrieves all PlayerPrefs as a dictionary.
        /// </summary>
        /// <returns>A dictionary containing all PlayerPrefs keys and values.</returns>
        Dictionary<string, object> GetAllPlayerPrefs();
    }
}
#endif