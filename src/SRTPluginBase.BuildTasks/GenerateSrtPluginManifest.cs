using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace SRTPluginBase.BuildTasks;

/// <summary>
/// Emits <c>srtplugin.json</c> for a compiled plugin assembly.
/// </summary>
/// <remarks>
/// Invoked from <c>buildTransitive/SRTPluginBase.targets</c> in the SRTPluginBase package, so plugin
/// authors get it by referencing the package they already reference and never hand-write a manifest.
/// </remarks>
public sealed class GenerateSrtPluginManifest : Microsoft.Build.Utilities.Task
{
    /// <summary>The compiled plugin assembly to describe. Usually <c>@(IntermediateAssembly)</c>.</summary>
    [Required]
    public string AssemblyPath { get; set; } = string.Empty;

    /// <summary>
    /// The assembly's compile-time references, used to resolve base types and interfaces.
    /// Usually <c>@(ReferencePathWithRefAssemblies)</c>.
    /// </summary>
    /// <remarks>
    /// Without these the entry type's interface list cannot be walked, and producer-versus-consumer
    /// is exactly what that walk decides.
    /// </remarks>
    public ITaskItem[] ReferencePaths { get; set; } = [];

    /// <summary>Where to write the manifest.</summary>
    [Required]
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// The contract generation this build expects. When set and the plugin declares something else,
    /// the build fails rather than shipping a plugin no host will load.
    /// </summary>
    public int ExpectedContractGeneration { get; set; }

    /// <summary>The manifest that was written, for the caller to add to the output group.</summary>
    [Output]
    public string? ManifestPath { get; private set; }

    /// <inheritdoc />
    public override bool Execute()
    {
        try
        {
            PluginManifest manifest = ManifestReader.Read(
                AssemblyPath,
                ReferencePaths.Select(static i => i.ItemSpec));

            if (ExpectedContractGeneration > 0 && manifest.ContractGeneration != ExpectedContractGeneration)
            {
                Log.LogError(
                    subcategory: null,
                    errorCode: "SRT1006",
                    helpKeyword: null,
                    file: AssemblyPath,
                    lineNumber: 0, columnNumber: 0, endLineNumber: 0, endColumnNumber: 0,
                    message: "Plugin '{0}' declares contract generation {1}, but this build targets "
                        + "generation {2}. A host only loads plugins of its own generation, so remove the "
                        + "Generation override on [SrtPluginAssembly] and let it default to the version "
                        + "you compiled against.",
                    manifest.Id, manifest.ContractGeneration, ExpectedContractGeneration);

                return false;
            }

            WriteIfChanged(OutputPath, manifest.ToJson());
            ManifestPath = OutputPath;

            Log.LogMessage(
                MessageImportance.Low,
                "Wrote {0} for {1} ({2}, {3}).",
                OutputPath, manifest.Id, manifest.Kind, manifest.Architecture);

            return true;
        }
        catch (ManifestException ex)
        {
            Log.LogError(
                subcategory: null,
                errorCode: ex.Code,
                helpKeyword: null,
                file: AssemblyPath,
                lineNumber: 0, columnNumber: 0, endLineNumber: 0, endColumnNumber: 0,
                message: "{0}",
                ex.Message);

            return false;
        }
        catch (Exception ex) when (ex is IOException or BadImageFormatException or UnauthorizedAccessException)
        {
            Log.LogErrorFromException(ex, showStackTrace: false);
            return false;
        }
    }

    /// <summary>
    /// Leaves the file alone when the content already matches.
    /// </summary>
    /// <remarks>
    /// The manifest is copied to the output directory on <c>PreserveNewest</c>, and it feeds an
    /// incremental up-to-date check; rewriting an identical file would re-trigger the copy on every
    /// build and make the project perpetually out of date in Visual Studio.
    /// </remarks>
    private static void WriteIfChanged(string path, string content)
    {
        if (File.Exists(path) && File.ReadAllText(path) == content)
            return;

        string? directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(path, content, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }
}
