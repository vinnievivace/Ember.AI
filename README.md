# Readyverse Launcher SDK

## Overview

A Unity implementation of the Readyverse External SDK, implemented as a custom package.
Include this package in any Unity project by adding the following to the package manifest:

`Packages/manifest.json`

`"com.readyverse.launcher": "https://github.com/Readyverse/Unity-Launcher-SDK.git"`

if you wish to target a particular tag/branch/commit, add the appropriate hash

`"com.readyverse.launcher": "https://github.com/Readyverse/Unity-Launcher-SDK.git#develop"`

**NOTE:** Currently the custom package repository is private, so you need to setup GIT credential manager as described in Unity documentation

https://docs.unity3d.com/6000.0/Documentation/Manual/upm-config-https-git.html

Once the package is installed, you need to ensure the RV Launcher is installed on your machine, and then add an instance of `RVClient` to your scene.

--------

## General Notes

1. Currently the RV Launcher is Windows only, as such full functionality has only been tested on that platform.
2. This SDK relies on 2 external files, a settings.json, and the Launcher itself. The paths to these 2 files do not follow the default data paths implemented by Unity, as such the code that defines them is pretty fragile and could be improved.

----------------

## RVLauncherSettings

Deserialized representation of the settings.json loaded in the Launcher's data directory.
Dev/Staging/Production environments each have separate settings file, with the expected path:

`ReadyVerse_Launcher_{environment}/settings.json`

Settings include the websocket address and port to connect to (The Launcher acts as the Websocket server) and the path the the RV Launcher:

`{local user data path}/Ready-verse_launcher/ReadyVerse_Launcher.exe`

**NOTE:** this Settings JSON is part of the RV Launcher installation. For development / testing, you can use 
`RVLauncherSettings.Save()` to maintain a local instance.

----------------

## IRVClientEvents

Interface that defines the expected callbacks supported by RVClient. These fall in to 2 categories.

### Websocket callbacks

`public void OnConnected();`

`public void OnDisconnected();`

`public void OnConnectError(string error);`

`public void OnSendMessage(string message);`

`public void OnReceiveMessage(string message);`


### Log/Error callbacks

`public void OnErrorMessage(string message)`

`public void OnLogMessage(string message)`

----------------

## BaseMessage

Abstract classes that each custom Message inherits from. There are 3 basic Message Types

1. Request - Message sent from the RVClient.
2. Response - Message received from RV Launcher in response to Request (not all Request messages have responses)
3. Other - Message received from RV Launcher outside of Requests, for example Errors.

---------------

## RVClient

This is the main Class (implements IRVClientEvents), providing core Readyverse Launcher functionality.

- RV Launcher (web socket server) Connection handling
- RV Launcher custom messaging and event handling.

### Properties

`LauncherEnvironment environment` - Sets the active environment (Development, Staging, Production)

`RVLauncherSettings settings` - Deserialized Settings loaded from FV Launcher installation path.

`string GameId` - Unique identifier for the game implementing this SDK. This ID must exist in the Readyverse game registry.


### Core Methods

`Initialize()`
- Calls `OpenLauncher`, then `Connect`, to Initialize the RV Launcher and Websocket connection.

`OpenLauncher()` 
- Checks the RV Launcher is running, and if not, starts it. 
- If launcher cannot be found/started the `OnConnectError` is dispatched.

`Connect()`
- Connects to the websocket server using defined settings.
- If connection settings are invalid, or any connection issues occur, the `OnConnectError` is dispatched.
- On successful connection, `OnConnected` is dispatched.

`Disconnect()` 
- Disconnects from the websocket and tidies up connection handling, then `OnClientDisconnected` is dispatched.

`SendMessage(BaseMessage message)`
- Sends custom messages to the RV Launcher, then dispatch `OnSendMessage`.
- Responses are handled by `ReceiveMessages()` and then dispatch the appropriate events.

### Message Methods

`GetLoginTokens()`
- Response contains user information and required token data.

`SendLog()`
- Logs message to FV Launcher logs.
- No response expected.

`OpenURL()`
- Trigger FV Launcher to open the supplied URL.
- No response expected.

`GetGameVersionInfo()`
- Response contains information about the current game, including if a newer version exists.

`SendAnalytics()`
- Logs Analytics data that is used by 3rd party Analytics tools (currently Mixpanel).
- No response expected.


### Response Methods

`OnLoginTokensReceived(LoginTokensResponse response)`
- Called when `GetLoginTokens` response is received.
- If user is not logged into RV Launcher, `OnUserNotLoggedIn` is called.
- Method marked protected, allowing custom handling via override.

`OnUserNotLoggedIn()`
- Error is logged
- Method marked protected, allowing custom handling via override.

`OnGameVersionReceived(GameVersionResponse response)`
- Called when `GetGameVersionInfo` response is received.
- response is logged, it includes installed version (empty string if not installed) and latest version, allowing logic for prompting or enforcing upgrade.
- Method marked protected, allowing custom handling via override.

`OnErrorResponse(ErrorResponse response)`
- Error logged to console
- Method marked protected, allowing custom handling via override.



### Callbacks

`OnClientConnected()`

`OnClientConnectError(string error)`

`OnClientDisconnected()`

`OnClientMessage(string message)`