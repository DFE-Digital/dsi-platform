using System.Runtime.CompilerServices;
using DiffEngine;

namespace Dfe.SignIn.DocfxPlugin.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        BuildServerDetector.Detected = Environment.GetEnvironmentVariable("BUILD_SERVER") == "true";
        VerifyDiffPlex.Initialize(VerifyTests.DiffPlex.OutputType.Compact);
    }
}
