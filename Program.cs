using HelperGitDocker;
using System.Reflection;

const string configFile = "config.json";

// Получаем директорию, где находится исполняемый файл HelperGitDocker
var applicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory();
var configFilePath = Path.Combine(applicationPath, configFile);

// Если config.json не найден в директории приложения, ищем в текущей рабочей директории
if (!File.Exists(configFilePath))
{
    var alternativeConfigPath = Path.Combine(Directory.GetCurrentDirectory(), configFile);
    if (File.Exists(alternativeConfigPath))
    {
        configFilePath = alternativeConfigPath;
    }
}

var rootPath = Directory.GetCurrentDirectory();
var config = AppConfig.Load(configFilePath);
var gitHelper = new GitHelper(config, rootPath);
var dockerHelper = new DockerHelper(config, rootPath);

Console.WriteLine("HelperGitDocker — Docker и Git helper");
Console.WriteLine("Рабочая директория: " + rootPath);
Console.WriteLine("Конфигурация: " + Path.Combine(rootPath, configFile));
Console.WriteLine();

while (true)
{
    Console.WriteLine("Выберите команду:");
    Console.WriteLine("1 - Обновить/склонировать репозитории");
    Console.WriteLine("2 - Показать статус всех репозиториев");
    Console.WriteLine("3 - Показать статус только изменённых репозиториев");
    Console.WriteLine("4 - Показать diff только для изменённых репозиториев");
    Console.WriteLine("5 - Push всех репозиториев");
    Console.WriteLine("6 - Push ветки во всех репозиториях");
    Console.WriteLine("7 - Показать ветки репозиториев");
    Console.WriteLine("8 - Checkout ветки во всех репозиториях");
    Console.WriteLine("9 - Merge ветки во всех репозиториях");
    Console.WriteLine("10 - Построить Docker образы");
    Console.WriteLine("11 - Запустить Docker Compose (dev)");
    Console.WriteLine("12 - Остановить Docker Compose");
    Console.WriteLine("13 - Показать логи Docker Compose");
    Console.WriteLine("14 - Выход");
    Console.WriteLine("15 - Запустить Docker Compose (prod — с nginx и SSL)");
    Console.WriteLine("16 - Пересобрать и перезапустить один сервис [dev]");
    Console.WriteLine("17 - Пересобрать и перезапустить один сервис [prod]");
    Console.Write("Команда: ");

    var input = Console.ReadLine()?.Trim();
    Console.WriteLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    switch (input)
    {
        case "1":
            gitHelper.CloneOrUpdateRepos();
            break;
        case "2":
            gitHelper.ShowStatus();
            break;
        case "3":
            gitHelper.ShowStatusOnlyChanged();
            break;
        case "4":
            gitHelper.ShowDiffOnlyChanged();
            break;
        case "5":
            gitHelper.PushRepos();
            break;
        case "6":
            {
                var branchName = AskBranchName("push");
                if (!string.IsNullOrEmpty(branchName))
                {
                    gitHelper.PushReposBranch(branchName);
                }
                break;
            }
        case "7":
            gitHelper.ListBranches();
            break;
        case "8":
            {
                var branchName = AskBranchName("checkout");
                if (!string.IsNullOrEmpty(branchName))
                {
                    gitHelper.CheckoutBranch(branchName);
                }
                break;
            }
        case "9":
            {
                var branchName = AskBranchName("merge");
                if (!string.IsNullOrEmpty(branchName))
                {
                    gitHelper.MergeBranch(branchName);
                }
                break;
            }
        case "10":
            dockerHelper.BuildAll();
            break;
        case "11":
            dockerHelper.UpAll();
            break;
        case "12":
            dockerHelper.DownAll();
            break;
        case "13":
            dockerHelper.LogsAll();
            break;
        case "14":
            return;
        case "15":
            dockerHelper.UpProd();
            break;
        case "16":
            {
                var svc = AskServiceName();
                if (!string.IsNullOrEmpty(svc))
                    dockerHelper.RebuildService(svc, isProd: false);
                break;
            }
        case "17":
            {
                var svc = AskServiceName();
                if (!string.IsNullOrEmpty(svc))
                    dockerHelper.RebuildService(svc, isProd: true);
                break;
            }
        default:
            Console.WriteLine("Неизвестная команда. Введите число от 1 до 17.");
            break;
    }

    Console.WriteLine();
}

static string AskBranchName(string action)
{
    Console.Write($"Введите имя ветки для {action}: ");
    return Console.ReadLine()?.Trim() ?? string.Empty;
}

static string AskServiceName()
{
    Console.Write("Сервис (api / client / identity / camoufox): ");
    var svc = Console.ReadLine()?.Trim().ToLowerInvariant() ?? string.Empty;
    var allowed = new[] { "api", "client", "identity", "camoufox" };
    if (!allowed.Contains(svc))
    {
        Console.WriteLine($"Неизвестный сервис '{svc}'. Допустимые: {string.Join(", ", allowed)}");
        return string.Empty;
    }
    return svc;
}
