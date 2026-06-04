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

    public void UpProd()
    {
        if (!TryGetComposePath(out var composeFilePath))
        {
            Console.WriteLine("docker-compose.yml not found.");
            return;
        }

        var prodComposePath = Path.GetFullPath(Path.Combine(_rootPath, "docker-compose.prod.yml"));
        if (!File.Exists(prodComposePath))
        {
            Console.WriteLine($"docker-compose.prod.yml not found at {prodComposePath}.");
            return;
        }

        var composeDir = Path.GetDirectoryName(composeFilePath);
        Console.WriteLine($"Starting Production Docker Compose in {composeDir}...");
        CommandRunner.Run("docker",
            $"compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build",
            composeDir, GetComposeEnvironment());
    }

    public void RebuildService(string service, bool isProd)
    {
        if (!TryGetComposePath(out var composeFilePath))
        {
            Console.WriteLine("docker-compose.yml not found.");
            return;
        }

        var composeDir = Path.GetDirectoryName(composeFilePath);
        var composeArgs = isProd ? BuildProdArgs(composeDir) : "";

        if (composeArgs == null) return;

        Console.WriteLine($"[{(isProd ? "prod" : "dev")}] Building {service}...");
        var buildExit = CommandRunner.Run("docker",
            $"compose {composeArgs}build {service}",
            composeDir, GetComposeEnvironment());

        if (buildExit != 0)
        {
            Console.WriteLine($"Build failed for {service}. Aborting restart.");
            return;
        }

        Console.WriteLine($"[{(isProd ? "prod" : "dev")}] Restarting {service} (without touching other containers)...");
        CommandRunner.Run("docker",
            $"compose {composeArgs}up -d --no-deps --force-recreate {service}",
            composeDir, GetComposeEnvironment());
    }

    private string? BuildProdArgs(string? composeDir)
    {
        var prodComposePath = Path.GetFullPath(Path.Combine(_rootPath, "docker-compose.prod.yml"));
        if (!File.Exists(prodComposePath))
        {
            Console.WriteLine($"docker-compose.prod.yml not found at {prodComposePath}.");
            return null;
        }
        return "-f docker-compose.yml -f docker-compose.prod.yml ";
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

        if (!string.IsNullOrWhiteSpace(_config.DockerBuildContextIdentity))
        {
            env["IDENTITY_BUILD_CONTEXT"] = _config.DockerBuildContextIdentity;
        }

        if (!string.IsNullOrWhiteSpace(_config.DockerfileIdentity))
        {
            env["IDENTITY_DOCKERFILE"] = _config.DockerfileIdentity;
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
