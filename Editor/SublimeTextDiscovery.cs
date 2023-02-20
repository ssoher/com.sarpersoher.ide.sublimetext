using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.CodeEditor;

namespace SublimeTextCodeEditor {
    public interface IDiscovery {
        CodeEditor.Installation[] PathCallback();
    }

    public class SublimeTextDiscovery : IDiscovery {
        List<CodeEditor.Installation> m_Installations;

        public CodeEditor.Installation[] PathCallback() {
            if(m_Installations == null) {
                m_Installations = new List<CodeEditor.Installation>();
                FindInstallationPaths();
            }

            return m_Installations.ToArray();
        }

        void FindInstallationPaths() {
            string[] possiblePaths =
#if UNITY_EDITOR_OSX
            {
                "/Applications/Sublime Text.app"
            };
#elif UNITY_EDITOR_WIN
            {
                GetProgramFiles() + @"/Sublime Text/sublime_text.exe",
            };
#else
            {
                "/usr/bin/sublime_text",
                "/bin/sublime_text",
                "/usr/local/bin/sublime_text",
                "/snap/current/bin/sublime_text",
                "/snap/bin/sublime_text",
                "/snap/sublime_tex/current/opt/_sublime_text",
                "/usr/bin/subl",
                "/opt/sublime_text/sublime_text"
            };
#endif
            var existingPaths = possiblePaths.Where(SublimeTextExists).ToList();
            if(!existingPaths.Any()) {
                return;
            }

            var lcp = GetLongestCommonPrefix(existingPaths);
            switch(existingPaths.Count) {
                case 1: {
                        var path = existingPaths.First();
                        m_Installations = new List<CodeEditor.Installation>
                        {
                        new CodeEditor.Installation
                        {
                            Path = path,
                            Name = "Sublime Text"
                        }
                    };
                        break;
                    }
                case 2 when existingPaths.Any(path => !(path.Substring(lcp.Length).Contains("/") || path.Substring(lcp.Length).Contains("\\"))): {
                        goto case 1;
                    }
                default: {
                        m_Installations = existingPaths.Select(path => new CodeEditor.Installation {
                            Name = $"Sublime Text ({path.Substring(lcp.Length)})",
                            Path = path
                        }).ToList();

                        break;
                    }
            }
        }

#if UNITY_EDITOR_WIN
        static string GetProgramFiles()
        {
            return Environment.GetEnvironmentVariable("ProgramFiles")?.Replace("\\", "/");
        }

        static string GetLocalAppData()
        {
            return Environment.GetEnvironmentVariable("LOCALAPPDATA")?.Replace("\\", "/");
        }
#endif

        static string GetLongestCommonPrefix(List<string> paths) {
            var baseLength = paths.First().Length;
            for(var pathIndex = 1; pathIndex < paths.Count; pathIndex++) {
                baseLength = Math.Min(baseLength, paths[pathIndex].Length);
                for(var i = 0; i < baseLength; i++) {
                    if(paths[pathIndex][i] == paths[0][i]) continue;

                    baseLength = i;
                    break;
                }
            }

            return paths[0].Substring(0, baseLength);
        }

        static bool SublimeTextExists(string path) {
#if UNITY_EDITOR_OSX
            return System.IO.Directory.Exists(path);
#else
            return new FileInfo(path).Exists;
#endif
        }
    }
}
