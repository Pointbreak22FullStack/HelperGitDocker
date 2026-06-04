using System.Diagnostics;

namespace HelperGitDocker;

internal sealed class GitHelper
{
    private readonly AppConfig _config;
    private readonly string _rootPath;

    public GitHelper(AppConfig config, string rootPath)
    {
        _config = config;
        _rootPath = rootPath;
    }

    public IReadOnlyList<RepoInfo> ResolveRepos()
    {
        var repos = new List<RepoInfo>();
        var root = Path.GetFullPath(Path.Combine(_rootPath, _config.GitRootPath));

        if (_config.Repositories.Count > 0)
        {
            foreach (var repo in _config.Repositories)
            {
                var repoPath = Path.GetFullPath(Path.Combine(_rootPath, repo.Path));
                repos.Add(new RepoInfo(repo.Name, repoPath, repo.CloneUrl));
            }
        }
        else if (Directory.Exists(root))
        {
            foreach (var directory in Directory.GetDirectories(root))
            {
                if (string.Equals(Path.GetFileName(directory), Path.GetFileName(_rootPath), StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (Directory.Exists(Path.Combine(directory, ".git")))
                {
                    repos.Add(new RepoInfo(Path.GetFileName(directory) ?? directory, directory, string.Empty));
                }
            }
        }

        return repos;
    }

    public void CloneOrUpdateRepos()
    {
        var repos = ResolveRepos();
        if (repos.Count == 0)
        {
            Console.WriteLine("No repositories found to update. Add entries to config.json or place sibling repos next to HelperGitDocker.");
            return;
        }

        foreach (var repo in repos)
        {
            Console.WriteLine($"\n=== Repository: {repo.Name} ({repo.Path}) ===");

            if (!Directory.Exists(repo.Path))
            {
                if (string.IsNullOrWhiteSpace(repo.CloneUrl))
                {
                    Console.WriteLine("Repo folder missing and no clone URL is configured. Skipping.");
                    continue;
                }

                Console.WriteLine($"Cloning {repo.CloneUrl} into {repo.Path}...");
                var parent = Path.GetDirectoryName(repo.Path) ?? repo.Path;
                Directory.CreateDirectory(parent);
                var cloneExit = CommandRunner.Run("git", $"clone {repo.CloneUrl} \"{repo.Path}\"");
                if (cloneExit != 0)
                {
                    Console.WriteLine($"Git clone failed for {repo.Name}.");
                }
                continue;
            }

            if (!Directory.Exists(Path.Combine(repo.Path, ".git")))
            {
                Console.WriteLine("Path exists but is not a Git repository. Skipping.");
                continue;
            }

            Console.WriteLine("Fetching and pulling latest changes...");
            CommandRunner.Run("git", "fetch --all --prune", repo.Path);
            var pullExit = CommandRunner.Run("git", "pull --rebase", repo.Path);
            if (pullExit != 0)
            {
                Console.WriteLine($"Git pull failed for {repo.Name}. Check repository state.");
            }
        }
    }

    public void ShowStatus()
    {
        var repos = ResolveRepos();
        if (repos.Count == 0)
        {
            Console.WriteLine("No repositories found to show status.");
            return;
        }

        foreach (var repo in repos)
        {
            Console.WriteLine($"\n=== Status for {repo.Name} ({repo.Path}) ===");
            if (!Directory.Exists(repo.Path) || !Directory.Exists(Path.Combine(repo.Path, ".git")))
            {
                Console.WriteLine("Repository path is missing or not a git repository.");
                continue;
            }

            CommandRunner.Run("git", "status --short", repo.Path);
        }
    }

    public void PushRepos()
    {
        var repos = ResolveRepos();
        if (repos.Count == 0)
        {
            Console.WriteLine("No repositories found to push.");
            return;
        }

        foreach (var repo in repos)
        {
            Console.WriteLine($"\n=== Push {repo.Name} ({repo.Path}) ===");
            if (!Directory.Exists(repo.Path) || !Directory.Exists(Path.Combine(repo.Path, ".git")))
            {
                Console.WriteLine("Repository path is missing or not a git repository. Skipping.");
                continue;
            }

            var exit = CommandRunner.Run("git", "push", repo.Path);
            if (exit != 0)
            {
                Console.WriteLine($"Git push failed for {repo.Name}. Fix local branch or remote configuration.");
            }
        }
    }

    public void PushReposBranch(string branchName)
    {
        var repos = ResolveRepos();
        if (repos.Count == 0)
        {
            Console.WriteLine("No repositories found to push branch.");
            return;
        }

        foreach (var repo in repos)
        {
            Console.WriteLine($"\n=== Push branch {branchName} in {repo.Name} ({repo.Path}) ===");
            if (!Directory.Exists(repo.Path) || !Directory.Exists(Path.Combine(repo.Path, ".git")))
            {
                Console.WriteLine("Repository path is missing or not a git repository. Skipping.");
                continue;
            }

            var exit = CommandRunner.Run("git", $"push origin \"{branchName}\"", repo.Path);
            if (exit != 0)
            {
                Console.WriteLine($"Git push failed for {repo.Name}. Fix local branch or remote configuration.");
            }
        }
    }

    public void ShowStatusOnlyChanged()
    {
        var repos = ResolveRepos();
        if (repos.Count == 0)
        {
            Console.WriteLine("No repositories found to show status.");
            return;
        }

        foreach (var repo in repos)
        {
            if (!Directory.Exists(repo.Path) || !Directory.Exists(Path.Combine(repo.Path, ".git")))
            {
                continue;
            }

            var output = GetCommandOutput("git", "status --short", repo.Path);
            if (string.IsNullOrWhiteSpace(output))
            {
                continue;
            }

            Console.WriteLine($"\n=== Modified repository: {repo.Name} ({repo.Path}) ===");
            Console.WriteLine(output);
        }
    }

    public void ShowDiffOnlyChanged()
    {
        var repos = ResolveRepos();
        if (repos.Count == 0)
        {
            Console.WriteLine("No repositories found to show diff.");
            return;
        }

        foreach (var repo in repos)
        {
            if (!Directory.Exists(repo.Path) || !Directory.Exists(Path.Combine(repo.Path, ".git")))
            {
                continue;
            }

            var status = GetCommandOutput("git", "status --short", repo.Path);
            if (string.IsNullOrWhiteSpace(status))
            {
                continue;
            }

            Console.WriteLine($"\n=== Diff for modified repository: {repo.Name} ({repo.Path}) ===");
            CommandRunner.Run("git", "diff", repo.Path);
        }
    }

    private static string GetCommandOutput(string fileName, string arguments, string workingDirectory)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return string.Empty;
            }

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output?.Trim() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public void ListBranches()
    {
        var repos = ResolveRepos();
        if (repos.Count == 0)
        {
            Console.WriteLine("No repositories found to list branches.");
            return;
        }

        foreach (var repo in repos)
        {
            Console.WriteLine($"\n=== Branches in {repo.Name} ({repo.Path}) ===");
            if (!Directory.Exists(repo.Path) || !Directory.Exists(Path.Combine(repo.Path, ".git")))
            {
                Console.WriteLine("Repository path is missing or not a git repository. Skipping.");
                continue;
            }

            CommandRunner.Run("git", "branch --all", repo.Path);
        }
    }

    public void CheckoutBranch(string branchName)
    {
        if (string.IsNullOrWhiteSpace(branchName))
        {
            Console.WriteLine("Branch name is required.");
            return;
        }

        var repos = ResolveRepos();
        if (repos.Count == 0)
        {
            Console.WriteLine("No repositories found to checkout branch.");
            return;
        }

        foreach (var repo in repos)
        {
            Console.WriteLine($"\n=== Checkout {branchName} in {repo.Name} ({repo.Path}) ===");
            if (!Directory.Exists(repo.Path) || !Directory.Exists(Path.Combine(repo.Path, ".git")))
            {
                Console.WriteLine("Repository path is missing or not a git repository. Skipping.");
                continue;
            }

            var exit = CommandRunner.Run("git", $"checkout \"{branchName}\"", repo.Path);
            if (exit != 0)
            {
                Console.WriteLine($"Git checkout failed for {repo.Name}. Check if branch exists or if repository is clean.");
            }
        }
    }

    public void MergeBranch(string branchName)
    {
        if (string.IsNullOrWhiteSpace(branchName))
        {
            Console.WriteLine("Branch name is required.");
            return;
        }

        var repos = ResolveRepos();
        if (repos.Count == 0)
        {
            Console.WriteLine("No repositories found to merge branch.");
            return;
        }

        foreach (var repo in repos)
        {
            Console.WriteLine($"\n=== Merge {branchName} into current branch in {repo.Name} ({repo.Path}) ===");
            if (!Directory.Exists(repo.Path) || !Directory.Exists(Path.Combine(repo.Path, ".git")))
            {
                Console.WriteLine("Repository path is missing or not a git repository. Skipping.");
                continue;
            }

            var exit = CommandRunner.Run("git", $"merge \"{branchName}\"", repo.Path);
            if (exit != 0)
            {
                Console.WriteLine($"Git merge failed for {repo.Name}. Resolve conflicts locally and retry if needed.");
            }
        }
    }
}

internal sealed class RepoInfo
{
    public string Name { get; }
    public string Path { get; }
    public string CloneUrl { get; }

    public RepoInfo(string name, string path, string cloneUrl)
    {
        Name = name;
        Path = path;
        CloneUrl = cloneUrl;
    }
}
