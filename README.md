# ArtClass

Мобильное приложение для управления расписанием художественной студии: группы, ученики, двухнедельный цикл занятий и календарь.

Стек: **.NET 9**, **.NET MAUI**, **Entity Framework Core** (SQLite), **MVVM** (CommunityToolkit.Mvvm).

---

## Содержание

- [Требования](#требования)
- [Быстрый старт](#быстрый-старт)
- [Структура решения](#структура-решения)
- [Архитектура](#архитектура)
- [Экраны и навигация](#экраны-и-навигация)
- [База данных](#база-данных)
- [Бизнес-логика расписания](#бизнес-логика-расписания)
- [Кэширование](#кэширование)
- [Платформенные особенности](#платформенные-особенности)
- [Разработка](#разработка)
- [Сборка и публикация](#сборка-и-публикация)

---

## Требования

| Компонент | Версия |
|-----------|--------|
| .NET SDK | 9.0+ |
| .NET MAUI workload | для целевой платформы |
| IDE | Visual Studio 2022 17.12+ (рекомендуется) или VS Code + C# Dev Kit |

### Установка workload (если ещё не установлен)

```bash
dotnet workload install maui
```

Для Android дополнительно нужен Android SDK (ставится через Visual Studio Installer или Android Studio).

### Поддерживаемые платформы

| Платформа | Target Framework | Мин. версия ОС |
|-----------|------------------|----------------|
| Android | `net9.0-android` | API 23 (Android 6.0) |
| iOS | `net9.0-ios` | iOS 15 |
| macOS (Mac Catalyst) | `net9.0-maccatalyst` | macOS 15 |
| Windows | `net9.0-windows10.0.19041.0` | Windows 10 1809 |

> Windows-сборка доступна только при разработке на Windows.

---

## Быстрый старт

```bash
# Клонировать репозиторий
git clone <url-репозитория>
cd ArtClass

# Восстановить зависимости
dotnet restore ArtClass.sln

# Собрать решение
dotnet build ArtClass.sln

# Запустить на Android-эмуляторе
dotnet build ArtClass/ArtClass.csproj -t:Run -f net9.0-android

# Запустить на Windows (только на Windows)
dotnet build ArtClass/ArtClass.csproj -t:Run -f net9.0-windows10.0.19041.0
```

В Visual Studio: открыть `ArtClass.sln`, выбрать целевое устройство (эмулятор / физическое устройство) и нажать **F5**.

При первом запуске приложение автоматически:
1. Создаёт SQLite-базу в `FileSystem.AppDataDirectory/artclass.db`
2. Применяет EF Core миграции
3. Заполняет демо-данными (группы, ученики, расписание), если база пустая

---

## Структура решения

```
ArtClass.sln
├── ArtClass/                    # UI-слой (.NET MAUI)
│   ├── Views/                   # XAML-страницы
│   ├── ViewModels/              # ViewModel'и (MVVM)
│   ├── Converters/              # XAML-конвертеры
│   ├── Services/                # UI-сервисы (TabBootstrapService)
│   ├── Resources/               # Стили, шрифты, иконки
│   └── Platforms/               # Платформенный код (Android, iOS, Windows, Mac)
│
├── ArtClass.Application/        # Слой приложения (use cases)
│   ├── Services/                # Бизнес-сервисы
│   ├── Dtos/                    # Data Transfer Objects
│   ├── Caching/                 # In-memory кэш
│   └── Data/                    # UnitOfWorkExecutor
│
├── ArtClass.Domain/             # Доменный слой
│   ├── Entities/                # Сущности
│   ├── Repositories/            # Интерфейсы репозиториев
│   └── Services/                # Доменная логика (ScheduleCycle)
│
└── ArtClass.Infrastructure/     # Инфраструктура
    ├── Data/                    # EF Core DbContext, миграции, инициализация
    └── Repositories/            # Реализации репозиториев (EF)
```

### Зависимости между проектами

```
ArtClass  →  ArtClass.Application  →  ArtClass.Domain
          →  ArtClass.Infrastructure →  ArtClass.Domain
```

UI-слой не ссылается на Domain напрямую — только через Application-сервисы.

---

## Архитектура

Проект построен по принципам **Clean Architecture** с разделением на 4 слоя.

### ArtClass.Domain

Чистый домен без внешних зависимостей.

| Сущность | Назначение |
|----------|------------|
| `StudyGroup` | Учебная группа (название, цвет, тип повторения) |
| `Lesson` | Занятие (день недели, время, преподаватель, кабинет) |
| `Student` | Ученик |
| `StudentStudyGroup` | Связь многие-ко-многим: ученик ↔ группа |
| `Teacher` | Преподаватель |
| `Subject` | Предмет |
| `Classroom` | Кабинет |
| `ScheduleSettings` | Настройки цикла (дата начала 2-недельного цикла) |

Ключевая доменная логика — `ScheduleCycle`: вычисление текущей недели цикла (1 или 2) по дате.

### ArtClass.Application

Оркестрация бизнес-операций. Сервисы работают через `UnitOfWorkExecutor` — обёртку, создающую DI-scope на каждый запрос к БД.

| Сервис | Ответственность |
|--------|-----------------|
| `IScheduleService` | Календарь, расписание на день/неделю/месяц |
| `IScheduleRollService` | Сдвиг 2-недельного цикла вперёд |
| `IGroupService` | CRUD групп и их занятий |
| `IStudentService` | CRUD учеников и привязка к группам |
| `IReferenceDataService` | Справочники (преподаватели, предметы, кабинеты) |

### ArtClass.Infrastructure

- `ArtClassDbContext` — EF Core контекст
- `Ef*Repository` — реализации репозиториев
- `DatabaseBootstrap` — инициализация БД при старте (миграции + seed)
- `SchemaCompatibility` — идемпотентные патчи схемы для старых баз
- `ArtClassDbContextFactory` — фабрика для design-time миграций

### ArtClass (UI)

- **MVVM**: ViewModel'и используют `[ObservableProperty]` и `[RelayCommand]` из CommunityToolkit.Mvvm
- **DI**: все зависимости регистрируются в `MauiProgram.cs`
- **Навигация**: `AppShell` с TabBar + зарегистрированные маршруты для детальных страниц

---

## Экраны и навигация

### Вкладки (TabBar)

| Вкладка | Страница | ViewModel | Описание |
|---------|----------|-----------|----------|
| Календарь | `CalendarPage` | `CalendarViewModel` | Месячный календарь с цветными метками занятий |
| Группы | `GroupsPage` | `GroupsViewModel` | Список учебных групп |
| Ученики | `StudentsPage` | `StudentsViewModel` | Список учеников |

> Вкладка «Ещё» (`SettingsPage`) закомментирована в `AppShell.xaml.cs`, но страница и ViewModel зарегистрированы в DI — можно вернуть одной строкой.

### Детальные страницы (push-навигация)

| Маршрут | Страница | Описание |
|---------|----------|----------|
| `DayDetailPage` | Расписание на конкретный день |
| `GroupDetailPage` | Детали группы, список занятий и учеников |
| `GroupEditorPage` | Создание / редактирование группы |
| `StudentDetailPage` | Карточка ученика |
| `StudentEditorPage` | Создание / редактирование ученика |

### Жизненный цикл загрузки данных

1. `App.CreateWindow` → создаёт `AppShell`
2. Фоновый `TabBootstrapService.WarmUpAsync()` предзагружает данные для всех вкладок
3. ViewModel'и используют `EnsureLoadedAsync()` — загружают данные только при первом показе или после инвалидации кэша

---

## База данных

### Расположение файла

```
{FileSystem.AppDataDirectory}/artclass.db
```

Путь задаётся в `MauiProgram.cs`:

```csharp
var dbPath = Path.Combine(FileSystem.AppDataDirectory, "artclass.db");
```

На Android это обычно:
```
/data/data/com.vinishk0.artclass/files/artclass.db
```

### Инициализация

При старте (`MauiProgram.CreateMauiApp`):
1. `InitializeDatabaseAsync()` → `DatabaseBootstrap.InitializeAsync`
2. EF Core `MigrateAsync()` — применяет все миграции
3. `SchemaCompatibility.EnsureAsync()` — патчи для старых баз
4. Если таблица `Lessons` пуста → `SeedAsync()` с демо-данными

При ошибке инициализации база пересоздаётся автоматически.

### Миграции

Миграции лежат в `ArtClass.Infrastructure/Data/Migrations/`.

```bash
# Добавить новую миграцию (из корня репозитория)
dotnet ef migrations add <ИмяМиграции> \
  --project ArtClass.Infrastructure \
  --startup-project ArtClass \
  --context ArtClassDbContext

# Применить миграции вручную (обычно не нужно — приложение делает это само)
dotnet ef database update \
  --project ArtClass.Infrastructure \
  --startup-project ArtClass
```

> Для design-time команд нужен пакет `dotnet-ef`:
> `dotnet tool install --global dotnet-ef`

### Сброс базы

Через `SettingsViewModel.ResetDatabaseAsync()` (экран настроек) или программно:

```csharp
await services.ResetDatabaseAsync();
```

Удаляет файлы `.db`, `.db-wal`, `.db-shm` и пересоздаёт базу с демо-данными.

---

## Бизнес-логика расписания

### Двухнедельный цикл

Расписание повторяющихся групп работает в **2-недельном цикле**:

- `ScheduleSettings.CycleStartDate` — понедельник начала текущего цикла
- Каждое занятие (`Lesson`) привязано к `CycleWeek` (1 или 2)
- `ScheduleCycle.GetCycleWeek(date, cycleStart)` вычисляет, какая неделя цикла сейчас

### Типы групп

| `IsRepeating` | `IsBiWeekly` | Поведение |
|---------------|--------------|-----------|
| `true` | `true` | Занятия повторяются в неделю 1 или 2 цикла |
| `true` | `false` | Занятия повторяются каждую календарную неделю (`CycleWeek = null`) |
| `false` | — | Разовое занятие на конкретную дату (`SpecificDate`) |

### Сдвиг цикла (Roll)

`IScheduleRollService.RollAsync()`:
1. Удаляет просроченные разовые занятия
2. Сдвигает `CycleStartDate` на 2 недели вперёд
3. Инвалидирует кэш расписания

На Android сдвиг также запускается автоматически по расписанию (каждое воскресенье в 23:00 UTC) через `ScheduleRollScheduler` + `ScheduleRollReceiver`.

---

## Кэширование

`IAppDataCache` (`AppDataCache`) — потокобезопасный in-memory кэш с версионированием.

| Метод | Когда вызывается |
|-------|-----------------|
| `InvalidateSchedule()` | После изменения расписания или roll |
| `InvalidateGroups()` | После CRUD групп (+ каскадно расписание) |
| `InvalidateStudents()` | После CRUD учеников (+ каскадно группы и расписание) |
| `InvalidateAll()` | Полная очистка |

ViewModel'и отслеживают `cache.Version` — при изменении версии перезагружают данные.

---

## Платформенные особенности

### Android

- `MainApplication.OnCreate()` — планирует еженедельный roll расписания
- `ScheduleRollReceiver` — BroadcastReceiver для фонового сдвига цикла
- В Debug-сборке отключён линкер (`AndroidLinkMode=None`) для ускорения сборки
- В Release — AOT-компиляция и `SdkOnly` линкинг

### Стили

Стили разбиты на файлы в `Resources/Styles/`:
- `Colors.xaml` — палитра
- `Typography.xaml` — шрифты и размеры текста
- `Components.xaml` — стили кнопок, карточек и т.д.
- `Styles.xaml` — глобальные стили MAUI

---

## Разработка

### Добавление нового экрана

1. Создать `Views/MyPage.xaml` + `.xaml.cs`
2. Создать `ViewModels/MyViewModel.cs` (наследник `ObservableObject`)
3. Зарегистрировать в `MauiProgram.cs`:
   ```csharp
   builder.Services.AddTransient<MyViewModel>();
   builder.Services.AddTransient<MyPage>();
   ```
4. Если нужна push-навигация — зарегистрировать маршрут в `AppShell.xaml.cs`:
   ```csharp
   Routing.RegisterRoute(nameof(MyPage), typeof(MyPage));
   ```

### Добавление новой сущности

1. Создать класс в `ArtClass.Domain/Entities/`
2. Добавить интерфейс репозитория в `ArtClass.Domain/Repositories/`
3. Добавить `DbSet` и конфигурацию в Infrastructure
4. Создать EF-реализацию репозитория
5. Зарегистрировать в `EfUnitOfWork`
6. Добавить сервис в Application
7. Создать миграцию

### Полезные NuGet-пакеты

| Пакет | Где используется |
|-------|------------------|
| `CommunityToolkit.Maui` | UI-хелперы |
| `CommunityToolkit.Mvvm` | MVVM (ObservableProperty, RelayCommand) |
| `Microsoft.EntityFrameworkCore.Sqlite` | База данных |
| `Microsoft.Extensions.DependencyInjection` | DI |

---

## Сборка и публикация

```bash
# Debug-сборка для Android
dotnet build ArtClass/ArtClass.csproj -f net9.0-android -c Debug

# Release APK
dotnet publish ArtClass/ArtClass.csproj -f net9.0-android -c Release

# Release для Windows
dotnet publish ArtClass/ArtClass.csproj -f net9.0-windows10.0.19041.0 -c Release
```

Идентификатор приложения: `com.vinishk0.artclass` (задаётся в `ArtClass.csproj` → `ApplicationId`).

---

## Частые вопросы

**Приложение падает при старте с ошибкой БД**
База пересоздаётся автоматически. Если проблема повторяется — удалите `artclass.db` на устройстве или используйте сброс через настройки.

**Как посмотреть базу на Android?**
Через Android Studio → Device Explorer → `data/data/com.vinishk0.artclass/files/artclass.db`. Или используйте `adb pull`.

**Где включить вкладку «Настройки»?**
Раскомментируйте строку в `AppShell.xaml.cs`:
```csharp
CreateTab(services.GetRequiredService<SettingsPage>(), "Ещё", "SettingsPage"),
```

**Как добавить новую миграцию?**
См. раздел [Миграции](#миграции). После добавления миграции приложение применит её автоматически при следующем запуске.
