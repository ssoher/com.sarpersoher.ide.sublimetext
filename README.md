Sublime Text integration for the Unity game engine.

# Features
- Unity "Preferences/External Tools" menu integration.
- Automatic generation of the `sln` file and `csproj` files per assembly.
- Automatic generation of the `sublime-project` with default excluded file/folders for meta files, library folder etc.
- Opens correct line/column.
- Context menu "Open C# Project" works as intended with Sublime Text now.
- Syncs correctly.

# Requirements
You must install the LSP and LSP-Omnisharp package for your installation of Sublime Text to be able to work with C# projects. That's all.

# Known issues
- Not tested on MacOS or Linux yet, I'm 99% it will fail on trying to open the project due to badly formed command line arguments. I will test and fix it asap.


# Screenshot
![image](https://user-images.githubusercontent.com/4283979/200619168-3132de72-7844-436f-974b-7d6017e1c3e4.png)
