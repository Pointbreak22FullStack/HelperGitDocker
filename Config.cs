using System.Text.Json;

namespace HelperGitDocker;

internal sealed class AppConfig
{
    public string Organization { get; set; } = string.Empty;
    public string GitHost { get; set; } = "github.com";
    public string GitRootPath { get; set; } = "..";
    public string DockerComposeFile { get; set; } = "../docker-compose.yml";
    public bool UseDockerCompose { get; set; } = true;
    public string DockerBuildContextApi { get; set; } = "../Api/WebParserApi/WebApi";
    public string DockerfileApi { get; set; } = "Dockerfile";
    public string DockerBuildContextClient { get; set; } = "../Client/web-parser-client";
    public string DockerfileClient { get; set; } = "Dockerfile";
    public string DockerBuildContextIdentity { get; set; } = "../Api/WebParserApi";
    public string DockerfileIdentity { get; set; } = "Identity/Dockerfile";
    public List<RepoDefinition> Repositories { get; set; } = new();

    public static AppConfig Load(string configFilePath)
    {
        if (!File.Exists(configFilePath))
        {
            Console.WriteLine($"Config file not found: {configFilePath}. Using default discovery mode.");
            return new AppConfig();
        }

        var json = File.ReadAllText(configFilePath);
        var config = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        });

        return config ?? new AppConfig();
    }
}

internal sealed class RepoDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string CloneUrl { get; set; } = string.Empty;
}
