#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
namespace YummyDev.PlayerPrefsEditor
{
    public sealed class PlayerPrefsRuntimeFetcherMacOS : PlayerPrefsRuntimeFetcherMacOSFileSystem
    {
    }
}
#endif
