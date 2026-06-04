# HelperGitDocker

Консольное приложение для упрощения работы с Git и Docker в проекте `WebParser`.

## Возможности

- `1` - Обновить/склонировать репозитории
- `2` - Показать статус репозиториев
- `3` - Push всех репозиториев
- `4` - Построить Docker образы
- `5` - Запустить Docker Compose
- `6` - Выход

## Как использовать

1. Откройте терминал в папке `HelperGitDocker`.
2. При необходимости поправьте `config.json`:
   - `Organization` — имя организации GitHub/GitLab.
   - `GitHost` — `github.com` или `gitlab.com`.
   - `GitRootPath` — путь до родительской директории с репозиториями (по умолчанию `..`).
   - `DockerComposeFile` — путь к `docker-compose.yml`.
   - `Repositories` — список репозиториев, которые нужно клонировать/обновлять.
3. Запустите:

```powershell
cd e:\Projects\Orders\WebParser\HelperGitDocker
dotnet run
```

## Примечания по DevOps

Для полноценной DevOps-реализации полезно:

- Развернуть репозитории как отдельные проекты в организации GitHub/ GitLab.
- Настроить `docker-compose.yml` для всех сервисов внутри `HelperGitDocker`.
- Использовать отдельные pipelines / workflows для каждого сервиса:
  - `build` + `test` для backend и frontend,
  - `docker build` и `push` в реестр (GitHub Packages, Docker Hub, GitLab Container Registry),
  - `deploy` на Kubernetes, Docker Swarm или виртуальный хост.
- Настроить секреты для доступа к репозиториям и Docker Registry.
- Определить единый путь к версиям и тегам образов.

## Что уже сделано

- Реализован helper в `HelperGitDocker`.
- Добавлена базовая поддержка Git:
  - `clone`, `fetch`, `pull`, `push`, `status`.
- Добавлена базовая поддержка Docker:
  - сборка образов с Dockerfile,
  - запуск `docker compose up -d`, если указан compose-файл.

## Что ещё можно сделать

- Добавить команды для `git branch`, `git checkout`, `git merge`.
- Добавить поддержку `git push` по имени ветки.
- Поддержать автоматическое создание `docker-compose.yml` для всех сервисов.
- Добавить CI/CD pipeline (GitHub Actions / GitLab CI) для автоматизации сборки и деплоя.

## Docker Compose

В папке `HelperGitDocker` создан `docker-compose.yml` со следующими сервисами:

- `api` — сборка из `Api/WebParserApi/WebApi/Dockerfile`
- `client` — сборка из `Client/web-parser-client/Dockerfile`
- `redis` — официальный Redis
- `postgres` — PostgreSQL с томом `postgres_data`
- `camoufox` — служебный контейнер внутри helper-а, сборка из `HelperGitDocker/docker/Dockerfile`

`Camoufox` теперь хранится в helper-е и не требует отдельного репозитория.

Запуск из папки helper-а:

```powershell
cd e:\Projects\Orders\WebParser\HelperGitDocker
docker compose up -d
```

Если позже вы захотите сделать `Camoufox` полноценным проектом, просто перенесите содержимое `HelperGitDocker/docker` в отдельный репозиторий и обновите `docker-compose.yml`.
