# MasterBooking
 fear

Веб-приложение для мастеров красоты.

## Как запустить проект локально с помощью Docker
Для запуска приложения и базы данных выполните следующую команду в корневой директории проекта:

```bash
docker-compose up -d
```

После этого приложение будет доступно по адресу: [http://localhost:8080](http://localhost:8080)

### 🔑 Демо-аккаунт (Админка)
При первом запуске база данных автоматически наполняется тестовыми данными.

- **Email**: `admin@demo.com`
- **Пароль**: `Password123!`

### Основные команды:

- **Запуск**: `docker-compose up -d`
- **Остановка**: `docker-compose down`
- **Просмотр логов**: `docker-compose logs -f`
- **Пересборка**: `docker-compose up -d --build`
- **Сброс данных (полная очистка)**: `docker-compose down -v`

## Технологический стек

- ASP.NET Core (Razor Pages)
- Entity Framework Core
- PostgreSQL
- Docker
