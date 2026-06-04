using HelperGitDocker;

const string configFile = "config.json";
var rootPath = Directory.GetCurrentDirectory();
var config = AppConfig.Load(Path.Combine(rootPath, configFile));
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
    Console.WriteLine("11 - Запустить Docker Compose");
    Console.WriteLine("12 - Остановить Docker Compose");
    Console.WriteLine("13 - Показать логи Docker Compose");
    Console.WriteLine("14 - Выход");
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
        default:
            Console.WriteLine("Неизвестная команда. Введите число от 1 до 14.");
            break;
    }

    Console.WriteLine();
}

static string AskBranchName(string action)
{
    Console.Write($"Введите имя ветки для {action}: ");
    return Console.ReadLine()?.Trim() ?? string.Empty;
}
