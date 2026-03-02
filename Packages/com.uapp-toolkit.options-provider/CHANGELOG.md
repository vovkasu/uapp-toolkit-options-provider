Changelog
=========

[1.0.1] - 2026.03.02
* **Added** -
* **Changed** 
    * Changed namespace.
* **Fixed** - 

[1.0.0] - 2026.03.02
--------------------
* **Added** 
    * OptionsProviderBase - class serves as an abstract foundation for managing typed application settings via Unity’s PlayerPrefs, handling save/load, type tracking, and change notification.
    * DeferredPlayerPrefsSaver - component batches and delays calls to PlayerPrefs.Save() by marking data as dirty and periodically flushing it (or on pause/quit), reducing frequent disk writes.
    * PlayerPrefsEditor - The scripts in the Editor/PlayerPrefsEditor folder provide a Unity editor window and backing store for viewing and editing PlayerPrefs entries during development.
* **Changed** -
* **Fixed** - 