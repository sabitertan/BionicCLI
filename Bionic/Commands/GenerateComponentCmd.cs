using System.ComponentModel.DataAnnotations;
using System.IO;
using BionicCore;
using BionicCore.Project;
using BionicPlugin;
using McMaster.Extensions.CommandLineUtils;
using static BionicCore.DirectoryUtils;

namespace BionicCLI.Commands {
  [Command("component", Description = "Generate Blazor component")]
  public class GenerateComponentCmd : CommandBase, ICommand {
    [Argument(0, Description = "Artifact Name"), Required]
    private string Artifact { get; set; }

    [Option("-n|--no-styles", Description = "Do not generate component style file")]
    private bool noStyles { get; set; } = false;

    public GenerateCommand Parent { get; }

    public GenerateComponentCmd() { }

    public GenerateComponentCmd(string artifact) => Artifact = artifact;

    protected override int OnExecute(CommandLineApplication app) => GenerateComponent();

    public int Execute() => GenerateComponent();

    private int GenerateComponent() {
      Logger.Success($"Generating a component named {Artifact}");
      var dest = ToOSPath("./Components");
      int dotnetRun = DotNetHelper.RunDotNet($"new bionic.component -n {Artifact} -o {dest}");
      if (noStyles) File.Delete($"{dest}/{Artifact}.scss");
      int ccssResult = noStyles ? 0 : GenerateCommand.IntroduceAppCssImport("Components", Artifact);

      string projectName = ProjectHelper.GetProjectName();

      string usingString = $"@using {projectName}.Components\n";
      if (FileHelper.FileContains("_imports.razor", usingString)) return dotnetRun | ccssResult; 
      int importResult = FileHelper.InsertLast("_imports.razor", usingString);

      return dotnetRun | ccssResult | importResult;
    }
  }
}