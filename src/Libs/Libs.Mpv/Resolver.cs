// Copyright (c) Bili Copilot. All rights reserved.

using System.Reflection;
using System.Runtime.InteropServices;

namespace Bili.Copilot.Libs.Mpv;

/// <summary>
/// Libraries import resolver.
/// </summary>
public static class Resolver
{
    /// <summary>
    /// Dictionary containing pointers to native libraries.
    /// </summary>
    private static readonly Dictionary<string, nint> _libraries = new Dictionary<string, nint>();
    private static string _libmpvPath = string.Empty;

    /// <summary>
    /// Set import resolver if needed.
    /// </summary>
    public static void SetResolver(string libmpvPath)
    {
        _libmpvPath = libmpvPath;
        if (_libraries.Count == 0)
        {
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), LibraryImportResolver);
        }
    }

    /// <summary>
    /// Resolve native libraries.
    /// </summary>
    /// <param name="libraryName">The string representing a library.</param>
    /// <param name="assembly">The assembly loading a native library.</param>
    /// <param name="searchPath">The search path.</param>
    private static nint LibraryImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (_libraries.TryGetValue(libraryName, out var lib))
        {
            return lib;
        }

        var filename = libraryName switch
        {
            "mpv" => _libmpvPath,
            "gl" => "opengl32.dll",
            _ => libraryName
        };

        _libraries[libraryName] = NativeLibrary.Load(filename, assembly, searchPath);
        return _libraries[libraryName];
    }
}
