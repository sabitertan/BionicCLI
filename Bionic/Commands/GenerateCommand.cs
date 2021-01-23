using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BionicCLI.Factories;
using BionicCore;
using BionicCore.Project;
using BionicPlugin;
using McMaster.Extensions.CommandLineUtils;

namespace BionicCLI.Commands {
  [Command("generate", Description = "Generate pages, layouts, components or services/providers")]
  [Subcommand(typeof(GeneratePageCmd))]
  [Subcommand(typeof(GenerateComponentCmd))]
  [Subcommand(typeof(GenerateServiceCmd))]
  [Subcommand(typeof(GenerateServiceCmd))]
  [Subcommand(typeof(GenerateLayoutCmd))]
  public class GenerateCommand : CommandBase, ICommand {
    private static readonly List<string> GenerateOptions = new List<string>
      {"component", "layout", "page", "provider", "service"};

    private string _option;
    private string _artifact;

    public BionicCommandFactory Parent { get; }

    public GenerateCommand() {}

    public GenerateCommand(string option, string artifact) {
      _option = option;
      _artifact = artifact;
    }

    protected override int OnExecute(CommandLineApplication app) {
      app.ShowHelp();
      return 0;
    }

    public int Execute() => IsGenerateCommandComplete() ? GenerateArtifact() : 1;

    private bool IsGenerateCommandComplete() {
      if (_option != null && !GenerateOptions.Contains(_option)) {
        Logger.Error($"Can't generate \"{_option}\"");
        Logger.Log($"   You can only generate: {string.Join(", ", GenerateOptions)}");
        return false;
      }

      while (!GenerateOptions.Contains(_option)) {
        _option = Prompt.GetString("What would you like to generate?\n (component, layout, page or service/provider): ",
          promptColor: ConsoleColor.DarkGreen);
      }

      while (_artifact == null) {
        _artifact = Prompt.GetString($"How would you like to name your {_option}?",
          promptColor: ConsoleColor.DarkGreen);
      }

      return true;
    }

    private int GenerateArtifact() {
      if (_option == "page") return new GeneratePageCmd(_artifact).Execute();
      if (_option == "layout") return new GenerateLayoutCmd(_artifact).Execute();
      if (_option == "component") return new GenerateComponentCmd(_artifact).Execute();
      if (_option == "provider" || _option == "service") return new GenerateServiceCmd(_artifact).Execute();

      return 1;
    }

    public static int IntroduceAppCssImport(string type, string artifactName) {
      return FileHelper.SeekForLineStartingWithAndInsert(ProjectHelper.AppCssPath, $"// {type}",
        $"@import \"{type}/{artifactName}.scss\";");
    }

    public static string ToPageName(string artifact) {
      var rx = new Regex("[pP]age");
      var name = rx.Replace(artifact, "").ToLower();
      return string.IsNullOrEmpty(name) ? artifact.ToLower() : name;
    }

    public static string ToCamelCase(string str) {
      return string.IsNullOrEmpty(str) || str.Length < 1 ? "" : char.ToUpperInvariant(str[0]) + str.Substring(1);
    }
  }
}