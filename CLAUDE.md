# CLAUDE.md

Read-only CLI for the Eventbrite Platform API v3, shipped as a .NET 10 global tool (`eventbrite`).

## Commands

```sh
dotnet build                      # warnings are errors; analyzers + code style run in build
dotnet test                       # xunit.v3 on Microsoft.Testing.Platform (see global.json)
dotnet run --project src/EventbriteCli -- events list
dotnet pack src/EventbriteCli -c Release && dotnet tool update -g EventbriteCli --add-source ./nupkg
```

## Coding rules (non-negotiable)

- **Async all the way.** Never `.GetAwaiter().GetResult()`, `.Result`, `.Wait()`, `Task.WaitAll/WaitAny`. These are banned in `BannedSymbols.txt` (RS0030 build error).
- **Every method returning `Task`, `ValueTask` or `IAsyncEnumerable` ends with `Async`**, tests included. Enforced by `.editorconfig` (IDE1006) and `ConventionTests`.
- **No member name contains `_`.** No `_field` prefixes (use primary constructors and camelCase fields), no `Method_Scenario_Expected` test names. Enforced by IDE1006 and `ConventionTests`.
- Pass `CancellationToken` through every async call.
- SOLID, KISS, DRY: small, single-purpose classes behind interfaces, wired in `ServiceCollectionExtensions`. Reuse the existing factories; don't hand-roll a command.
- Match the existing style: file-scoped namespaces, `internal sealed` classes, collection expressions, XML `<summary>` only where it explains intent.
- Package versions live only in `Directory.Packages.props` (central package management). Shared MSBuild settings live in `Directory.Build.props`.

## Architecture (`src/EventbriteCli`)

- `Program.cs`: builds DI, then adds every `ICommandModule` to the root command.
- `Authentication/`: `ITokenStore` / `FileTokenStore` (DPAPI on Windows, a `0600` file elsewhere) and `TokenProvider` (`EVENTBRITE_TOKEN` env var first, then the store). There is intentionally no `--token` option, to keep tokens out of shell history.
- `Api/`: `EventbriteClient` is a typed `HttpClient` returning raw `JsonElement`.
  - `GetPagedAsync` follows `pagination.continuation`, falling back to `page`.
  - `AuthenticationHandler` adds `Authorization: Bearer`.
  - Non-2xx responses become `EventbriteApiException`.
- `Output/`: `ConsoleOutputWriter` renders `table` (the `Column` list, with dotted JSON paths like `name.text` or `emails.0.email`) or `json` (the full payload).
- `Commands/`:
  - `ResourceCommandFactory.List/Get` + `PathSource` + `QueryOption` build every resource command declaratively. One `*CommandModule` exists per API area.
  - `CommandAction.SetHandler` maps expected exceptions to stderr and exit code 1. stdout carries data only.

### Adding an endpoint

1. Add a `factory.List(...)` or `factory.Get(...)` line to the matching module, or create a new `ICommandModule`.
2. Register it in `ServiceCollectionExtensions`.
3. Give it columns for table output.

Any endpoint is also reachable through `eventbrite api get <path>`.

## Eventbrite API gotchas

- Base URL: `https://www.eventbriteapi.com/v3/`.
  - **Paths must end with `/`.** A redirect would drop the `Authorization` header.
  - Never send the token to another host: `api get` rejects absolute URLs.
- IDs are strings and must be escaped through `PathSource.Escape`.
- Pagination: `{ object_count, page_number, page_size, page_count, has_more_items, continuation }`. List payloads are under a named key (`events`, `attendees`, `orders`, ...).
- Errors: `{ error, error_description, status_code }`.
- Rate limit: 2,000 calls/hour and 48,000/day (HTTP 429 `HIT_RATE_LIMIT`).
- Deprecated endpoints that must not be used: user-scoped `/users/{id}/owned_events/`, `/users/{id}/events/` and the related ones, `/events/search/`, `/batch/`. Use the `/organizations/{id}/...` equivalents.
- `event.description` is deprecated; use `/events/{id}/description/`.
- Scope is **read-only for now**: only GET requests. Do not add write operations unless asked.
