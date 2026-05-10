using System.Collections.Generic;

namespace UAppToolKit.Options.Editor.PlayerPrefsTool
{
    /// <summary>
    /// Reads all PlayerPrefs entries that belong to the current Unity project.
    /// A separate implementation is provided for each target platform.
    /// </summary>
    public interface IPlayerPrefsReader
    {
        /// <summary>
        /// Returns all stored PlayerPref entries, or an empty list when none
        /// exist or the backing store cannot be found.
        /// </summary>
        List<PlayerPrefStore> ReadAll();
    }
}

