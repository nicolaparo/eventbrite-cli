# eventbrite-cli

A read-only command line client for the [Eventbrite Platform API v3](https://www.eventbrite.com/platform/api), packaged as a .NET 10 global tool.

## Install

```sh
dotnet pack src/EventbriteCli -c Release
dotnet tool install -g EventbriteCli --add-source ./nupkg
# later: dotnet tool update -g EventbriteCli --add-source ./nupkg
```

## Authenticate

Create a **private token** at <https://www.eventbrite.com/platform/api-keys> (Account settings → Developer links → API keys).

```sh
eventbrite auth login                 # hidden prompt; the token is checked against /users/me/ before it is kept
echo "$TOKEN" | eventbrite auth login # or pipe it (CI/scripts)
eventbrite auth status                # shows where the token comes from and who it authenticates
eventbrite auth logout                # deletes the stored token
```

The token is resolved in this order:

1. The `EVENTBRITE_TOKEN` environment variable.
2. The stored token at `%APPDATA%\eventbrite-cli\token` (Windows, encrypted with DPAPI for the current user) or `~/.config/eventbrite-cli/token` (Linux/macOS, file mode `0600`).

There is deliberately no `--token` option, so tokens never end up in shell history.

## Usage

```sh
eventbrite me
eventbrite orgs list
eventbrite events list --status live --time-filter current_future
eventbrite events get 123456789 --expand venue,ticket_availability
eventbrite events description 123456789
eventbrite events ticket-classes 123456789
eventbrite attendees list --event 123456789 --limit 0      # 0 = every page
eventbrite orders list --changed-since 2026-01-01T00:00:00Z
eventbrite orders get 987654321 --expand attendees
eventbrite venues list
eventbrite discounts list --scope event --event-id 123456789
eventbrite reports sales --event-ids 123456789
eventbrite catalog categories list
eventbrite api get events/123456789/attendees/ --items attendees --limit 0   # any other GET endpoint
```

* `-o json` prints the complete API payload (tables show summary columns only), so it can be piped to `jq`.
* Commands under an organization use `--org`, then `EVENTBRITE_ORG`, then your only organization.
* Lists follow pagination automatically up to `--limit` (default 50).
* Errors go to stderr with exit code `1`.

Run `eventbrite --help` or `eventbrite <command> --help` for every option.
