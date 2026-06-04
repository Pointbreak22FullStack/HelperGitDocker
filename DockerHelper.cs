namespace HelperGitDocker;

internal sealed class DockerHelper
{
    private readonly AppConfig _config;
    private readonly string _rootPath;

    public DockerHelper(AppConfig config, string rootPath)
    {
        _config = config;
        _rootPath = rootPath;
    }

    public void BuildAll()
    {
        if (_config.UseDockerCompose && TryGetComposePath(out var composeFilePath))
        {
            Console.WriteLine($"Using Docker Compose file: {composeFilePath}");
            CommandRunner.Run("docker", "compose build", Path.GetDirectoryName(composeFilePath), GetComposeEnvironment());
            return;
        }

        var repos = GetServiceFolders();
        if (!repos.Any())
        {
            Console.WriteLine("No Docker services found to build.");
            return;
        }

        foreach (var repo in repos)
        {
            var dockerfile = Path.Combine(repo, "Dockerfile");
            if (!File.Exists(dockerfile))
            {
                Console.WriteLine($"Dockerfile not found in {repo}. Skipping.");
                continue;
            }

            var imageName = GenerateImageName(repo);
            Console.WriteLine($"Building Docker image {imageName} from {repo}...");
            CommandRunner.Run("docker", $"build -t {imageName} .", repo);
        }
    }

    public void UpAll()
    {
        if (!TryGetComposePath(out var composeFilePath))
        {
            Console.WriteLine("Docker Compose file not configured or not found. Create a docker-compose.yml and set DockerComposeFile in config.json.");
            return;
        }

        var composeDir = Path.GetDirectoryName(composeFilePath);
        Console.WriteLine($"Starting Docker Compose in {composeDir}...");
        CommandRunner.Run("docker", "compose up -d", composeDir, GetComposeEnvironment());
    }

    public void DownAll()
    {
        if (!TryGetComposePath(out var composeFilePath))
        {
            Console.WriteLine("Docker Compose file not configured or not found. Create a docker-compose.yml and set DockerComposeFile in config.json.");
            return;
        }

        var composeDir = Path.GetDirectoryName(composeFilePath);
        Console.WriteLine($"Stopping Docker Compose in {composeDir}...");
        CommandRunner.Run("docker", "compose down", composeDir, GetComposeEnvironment());
    }

    public void LogsAll()
    {
        if (!TryGetComposePath(out var composeFilePath))
        {
            Console.WriteLine("Docker Compose file not configured or not found. Create a docker-compose.yml and set DockerComposeFile in config.json.");
            return;
        }

        var composeDir = Path.GetDirectoryName(composeFilePath);
        Console.WriteLine($"Showing Docker Compose logs in {composeDir}...");
        CommandRunner.Run("docker", "compose logs -f --tail 100", composeDir, GetComposeEnvironment());
    }

    private IReadOnlyDictionary<string, string> GetComposeEnvironment()
    {
        var env = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!string.IsNullOrWhiteSpace(_config.DockerBuildContextApi))
        {
            env["API_BUILD_CONTEXT"] = _config.DockerBuildContextApi;
        }

        if (!string.IsNullOrWhiteSpace(_config.DockerfileApi))
        {
            env["API_DOCKERFILE"] = _config.DockerfileApi;
        }

        if (!string.IsNullOrWhiteSpace(_config.DockerBuildContextClient))
        {
            env["CLIENT_BUILD_CONTEXT"] = _config.DockerBuildContextClient;
        }

        if (!string.IsNullOrWhiteSpace(_config.DockerfileClient))
        {
            env["CLIENT_DOCKERFILE"] = _config.DockerfileClient;
        }

        return env;
    }

    private IEnumerable<string> GetServiceFolders()
    {
        var root = Path.GetFullPath(Path.Combine(_rootPath, _config.GitRootPath));
        if (!Directory.Exists(root))
        {
            return Array.Empty<string>();
        }

        return Directory.GetDirectories(root)
            .Where(directory => File.Exists(Path.Combine(directory, "Dockerfile")))
            .ToArray();
    }

    private bool TryGetComposePath(out string composeFilePath)
    {
        composeFilePath = Path.GetFullPath(Path.Combine(_rootPath, _config.DockerComposeFile));
        return _config.UseDockerCompose && File.Exists(composeFilePath);
    }

    private static string GenerateImageName(string servicePath)
    {
        var name = Path.GetFileName(servicePath) ?? "service";
        var safeName = name.ToLowerInvariant().Replace(" ", "-").Replace("_", "-");
        return $"{safeName}-service";
    }
}
