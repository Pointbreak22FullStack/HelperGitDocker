# HelperGitDocker

Консольное приложение для упрощения работы с Git и Docker в проекте `WebParser`.

## Возможности

- `1` - Обновить/склонировать репозитории
- `2` - Показать статус всех репозиториев
- `3` - Показать статус только изменённых репозиториев
- `4` - Показать diff только для изменённых репозиториев
- `5` - Push всех репозиториев
- `6` - Push ветки во всех репозиториях
- `7` - Показать ветки репозиториев
- `8` - Checkout ветки во всех репозиториях
- `9` - Merge ветки во всех репозиториях
- `10` - Построить Docker образы
- `11` - Запустить Docker Compose
- `12` - Остановить Docker Compose
- `13` - Показать логи Docker Compose
- `14` - Выход

## Как использовать

1. Откройте терминал в папке `HelperGitDocker`.
2. При необходимости поправьте `config.json`:
   - `Organization` — имя организации, например `Pointbreak22FullStack`.
   - `GitHost` — `github.com` или `gitlab.com`.
   - `GitRootPath` — путь до родительской директории с репозиториями (по умолчанию `..`).
   - `DockerComposeFile` — путь к `docker-compose.yml`.
   - `Repositories` — список репозиториев, которые нужно клонировать/обновлять. Сейчас в конфиг добавлены:
     - `Api` — `WebParserApi`
     - `Client` — `WebParserClient`
     - `HelperGitDocker` — сам helper в организации `Pointbreak22FullStack`
3. Запустите:

```powershell
cd e:\Projects\Orders\WebParser\HelperGitDocker
dotnet run
```

## HelperGitDocker в организации Pointbreak22FullStack

Этот проект является инфраструктурным репозиторием для организации `Pointbreak22FullStack`.

В `config.json` уже прописаны три репозитория:

- `WebParserApi` — бэкенд API
- `WebParserClient` — фронтенд Angular
- `HelperGitDocker` — сам helper для управления Git и Docker

`HelperGitDocker` хранит и управляет всеми необходимыми файлами для локальной разработки и запуска через `docker compose`.

### Важные файлы в helper-е

- `.gitattributes` — фиксирует LF для `*.sh` и других текстовых файлов.
- `.gitignore` — исключает `bin/`, `obj/`, IDE/системные файлы и сборочные артефакты.
- `docker-compose.override.yml` — локальный override файл для `docker compose`.

## Примечания по DevOps

Для полноценной DevOps-реализации полезно:

- Развернуть репозитории как отдельные проекты в организации GitHub/ GitLab.
- Настроить `docker-compose.yml` для всех сервисов внутри `HelperGitDocker`.
- Использовать отдельные pipelines / workflows для каждого сервиса:
  - `build` + `test` для backend и frontend,
  - `docker build` и `push` в реестр (GitHub Packages, Docker Hub, GitLab Container Registry),
  - `deploy` на Kubernetes, Docker Swarm или виртуальный хост.
- Настроить CI для `HelperGitDocker` в `Pointbreak22FullStack`, чтобы проверять сборку и `docker compose config`.
- Настроить CI для `WebParserApi` и `WebParserClient` через GitHub Actions.
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

Для локальных изменений можно использовать `docker-compose.override.yml`, который находится в той же папке `HelperGitDocker`. Он накладывается на базовый `docker-compose.yml` и позволяет, например, отключить автоматический перезапуск и задать локальные переменные окружения.

Запуск из папки helper-а:

```powershell
cd e:\Projects\Orders\WebParser\HelperGitDocker
docker compose up -d
```

Запуск с явным override-файлом:

```powershell
docker compose -f docker-compose.yml -f docker-compose.override.yml up -d
```

Если позже вы захотите сделать `Camoufox` полноценным проектом, просто перенесите содержимое `HelperGitDocker/docker` в отдельный репозиторий и обновите `docker-compose.yml`.
