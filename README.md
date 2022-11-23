Sublime Text integration for the Unity game engine.

![image](https://user-images.githubusercontent.com/4283979/200619168-3132de72-7844-436f-974b-7d6017e1c3e4.png)


# Features
- Unity "Preferences/External Tools" menu integration.
- Automatic generation of the `sln` file and `csproj` files per assembly.
- Automatic generation of the `sublime-project` with default excluded file/folders for meta files, library folder etc.
- Opens correct line/column.
- Context menu "Open C# Project" works as intended with Sublime Text now (opens the auto-generated .sublime-project file).
- Syncs correctly.

# How to Install
1) Open Unity package manager.
2) Click the `+` button on the top left of the package manager window.
3) Select the option "Add package from git URL".
4) Copy and paste the url of this repository `https://github.com/ssoher/com.sarpersoher.ide.sublimetext.git` and click the Add button.
5) Once the package is downloaded and added to your project, open the Edit->Preferences->External Tools area. From the External Script Editor dropdown, select Browse and find your `sublime_text.exe` (`Sublime Text.app` if you are on macOS). Your sublime-project file, csproj files and the sln file will be created automatically. There is also a `Regenerate project files` button to refresh solution related files at will.

# How to Use
1) `Assets->Open C# Project` opens the asssociated Sublime Text projects (sublime-project file created automatically at the root of your Unity project).
2) Double clicking any script file opens the script in the active Sublime Text instance.
3) Console messages and stack traces open the associated script at the correct line & column.

# Requirements
It is recommended that you install the LSP and LSP-Omnisharp package for your installation of Sublime Text for ide-like support for C# projects. You should also install the "LSP-file-watcher-chokidar" package so that LSP-OmniSharp (or any other language server) receives file system change events and index the newly created files. Without this, the new files you create won't be indexed unless you restart the Omnisharp server.
+ https://github.com/sublimelsp/LSP
+ https://github.com/sublimelsp/LSP-OmniSharp
+ https://github.com/sublimelsp/LSP-file-watcher-chokidar
