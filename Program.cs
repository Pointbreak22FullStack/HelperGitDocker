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
    Console.WriteLine("2 - Показать статус репозиториев");
    Console.WriteLine("3 - Push всех репозиториев");
    Console.WriteLine("4 - Построить Docker образы");
    Console.WriteLine("5 - Запустить Docker Compose");
    Console.WriteLine("6 - Выход");
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
            gitHelper.PushRepos();
            break;
        case "4":
            dockerHelper.BuildAll();
            break;
        case "5":
            dockerHelper.UpAll();
            break;
        case "6":
            return;
        default:
            Console.WriteLine("Неизвестная команда. Введите число от 1 до 6.");
            break;
    }

    Console.WriteLine();
}
