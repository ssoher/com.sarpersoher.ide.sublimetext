using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Unity.CodeEditor;

namespace SublimeTextCodeEditor {
    [InitializeOnLoad]
    public class SublimeTextScriptEditor : IExternalCodeEditor {
        const string sublimetext_argument = "sublimetext_arguments";
        const string sublimetext_extension = "sublimetext_userExtensions";
        static readonly GUIContent k_ResetArguments = EditorGUIUtility.TrTextContent("Reset argument");
        string m_Arguments;

        IDiscovery m_Discoverability;
        IGenerator m_ProjectGeneration;

        static readonly string[] k_SupportedFileNames = { "sublime_text.exe", "sublimetext.app" };

        static bool IsOSX => Application.platform == RuntimePlatform.OSXEditor;

        static string DefaultApp => EditorPrefs.GetString("kScriptsDefaultApp");

        static string DefaultArgument { get; } = "$(File):$(Line):$(Column)";

        string Arguments {
            get => m_Arguments ?? (m_Arguments = EditorPrefs.GetString(sublimetext_argument, DefaultArgument));
            set {
                m_Arguments = value;
                EditorPrefs.SetString(sublimetext_argument, value);
            }
        }

        static string[] defaultExtensions {
            get {
                var customExtensions = new[] { "json", "asmdef", "log" };
                return EditorSettings.projectGenerationBuiltinExtensions
                    .Concat(EditorSettings.projectGenerationUserExtensions)
                    .Concat(customExtensions)
                    .Distinct().ToArray();
            }
        }

        static string[] HandledExtensions {
            get {
                return HandledExtensionsString
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.TrimStart('.', '*'))
                    .ToArray();
            }
        }

        static string HandledExtensionsString {
            get => EditorPrefs.GetString(sublimetext_extension, string.Join(";", defaultExtensions));
            set => EditorPrefs.SetString(sublimetext_extension, value);
        }

        public bool TryGetInstallationForPath(string editorPath, out CodeEditor.Installation installation) {
            var lowerCasePath = editorPath.ToLower();
            var filename = Path.GetFileName(lowerCasePath).Replace(" ", "");
            var installations = Installations;
            if(!k_SupportedFileNames.Contains(filename)) {
                installation = default;
                return false;
            }

            if(!installations.Any()) {
                installation = new CodeEditor.Installation {
                    Name = "Sublime Text",
                    Path = editorPath
                };
            } else {
                try {
                    installation = installations.First(inst => inst.Path == editorPath);
                } catch(InvalidOperationException) {
                    installation = new CodeEditor.Installation {
                        Name = "Sublime Text",
                        Path = editorPath
                    };
                }
            }

            return true;
        }

        public void OnGUI() {
            Arguments = EditorGUILayout.TextField("External Script Editor Args", Arguments);
            if(GUILayout.Button(k_ResetArguments, GUILayout.Width(120))) {
                Arguments = DefaultArgument;
            }

            EditorGUILayout.LabelField("Generate .csproj files for:");
            EditorGUI.indentLevel++;
            SettingsButton(ProjectGenerationFlag.Embedded, "Embedded packages", "");
            SettingsButton(ProjectGenerationFlag.Local, "Local packages", "");
            SettingsButton(ProjectGenerationFlag.Registry, "Registry packages", "");
            SettingsButton(ProjectGenerationFlag.Git, "Git packages", "");
            SettingsButton(ProjectGenerationFlag.BuiltIn, "Built-in packages", "");
#if UNITY_2019_3_OR_NEWER
            SettingsButton(ProjectGenerationFlag.LocalTarBall, "Local tarball", "");
#endif
            SettingsButton(ProjectGenerationFlag.Unknown, "Packages from unknown sources", "");
            RegenerateProjectFiles();
            EditorGUI.indentLevel--;

            HandledExtensionsString = EditorGUILayout.TextField(new GUIContent("Extensions handled: "), HandledExtensionsString);
        }

        void RegenerateProjectFiles() {
            var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(new GUILayoutOption[] { }));
            rect.width = 252;
            if(GUI.Button(rect, "Regenerate project files")) {
                m_ProjectGeneration.Sync();
            }
        }

        void SettingsButton(ProjectGenerationFlag preference, string guiMessage, string toolTip) {
            var prevValue = m_ProjectGeneration.AssemblyNameProvider.ProjectGenerationFlag.HasFlag(preference);
            var newValue = EditorGUILayout.Toggle(new GUIContent(guiMessage, toolTip), prevValue);
            if(newValue != prevValue) {
                m_ProjectGeneration.AssemblyNameProvider.ToggleProjectGeneration(preference);
            }
        }

        public void CreateIfDoesntExist() {
            if(!m_ProjectGeneration.SolutionExists()) {
                m_ProjectGeneration.Sync();
            }
        }

        public void SyncIfNeeded(string[] addedFiles, string[] deletedFiles, string[] movedFiles, string[] movedFromFiles, string[] importedFiles) {
            (m_ProjectGeneration.AssemblyNameProvider as IPackageInfoCache)?.ResetPackageInfoCache();
            m_ProjectGeneration.SyncIfNeeded(addedFiles.Union(deletedFiles).Union(movedFiles).Union(movedFromFiles).ToList(), importedFiles);
        }

        public void SyncAll() {
            (m_ProjectGeneration.AssemblyNameProvider as IPackageInfoCache)?.ResetPackageInfoCache();
            AssetDatabase.Refresh();
            m_ProjectGeneration.Sync();
        }

        public bool OpenProject(string path, int line, int column) {
            if (path != "" && (!SupportsExtension(path) || !File.Exists(path))) {
                return false;
            }

            if(line == -1)
                line = 1;
            if(column == -1)
                column = 0;

            string arguments;

            if(path == string.Empty) {
                string projectFileName = $"{Path.GetFileName(m_ProjectGeneration.ProjectDirectory)}.sublime-project";
                arguments = $@"--project ""{projectFileName}""";

            } else {
                arguments = $@"""{path}"":{line}:{column}";
            }

            if(IsOSX) {
                return OpenOSX(arguments);
            }

            var app = DefaultApp;
            var process = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = app,
                    Arguments = arguments,
                    WindowStyle = app.EndsWith(".cmd", StringComparison.OrdinalIgnoreCase) ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                    CreateNoWindow = true,
                    UseShellExecute = true,
                }
            };

            process.Start();
            return true;
        }

        static bool OpenOSX(string arguments) {
            Process.Start($"{DefaultApp}/Contents/SharedSupport/bin/subl", arguments);
            return true;
        }

        static bool SupportsExtension(string path) {
            var extension = Path.GetExtension(path);
            if(string.IsNullOrEmpty(extension))
                return false;
            return HandledExtensions.Contains(extension.TrimStart('.'));
        }

        public CodeEditor.Installation[] Installations => m_Discoverability.PathCallback();

        public SublimeTextScriptEditor(IDiscovery discovery, IGenerator projectGeneration) {
            m_Discoverability = discovery;
            m_ProjectGeneration = projectGeneration;
        }

        static SublimeTextScriptEditor() {
            var editor = new SublimeTextScriptEditor(new SublimeTextDiscovery(), new ProjectGeneration(Directory.GetParent(Application.dataPath).FullName));
            CodeEditor.Register(editor);

            if(IsSublimeTextInstallation(CodeEditor.CurrentEditorInstallation)) {
                editor.CreateIfDoesntExist();
            }
        }

        static bool IsSublimeTextInstallation(string path) {
            if(string.IsNullOrEmpty(path)) {
                return false;
            }

            var lowerCasePath = path.ToLower();
            var filename = Path
                .GetFileName(lowerCasePath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar))
                .Replace(" ", "");

            return k_SupportedFileNames.Contains(filename);
        }

        public void Initialize(string editorInstallationPath) { }
    }
}
