# eventbrite-cli

Look at your **Eventbrite** events, attendees, orders and more from the terminal.

`eventbrite` is a small command line tool for the [Eventbrite API](https://www.eventbrite.com/platform/api). It is **read-only**: it can show your data but can never change or delete anything on your Eventbrite account.

```text
> eventbrite events list
ID            NAME                 START                STATUS  CAPACITY  ONLINE  URL
812345678901  Spring Meetup        2026-04-12T18:30:00  live    120       false   https://www.eventbrite.com/e/...
812345678902  Autumn Workshop      2026-10-03T09:00:00  draft   40        true    https://www.eventbrite.com/e/...
(2 items)
```

---

## Contents

1. [Install .NET](#1-install-net-one-time)
2. [Install eventbrite-cli](#2-install-eventbrite-cli)
3. [Get your Eventbrite private token](#3-get-your-eventbrite-private-token)
4. [Log in](#4-log-in)
5. [Try it](#5-try-it)
6. [Command reference](#command-reference)
7. [Troubleshooting](#troubleshooting)
8. [Update or uninstall](#update-or-uninstall)
9. [Security](#security)
10. [Development](#development)

---

## 1. Install .NET (one time)

eventbrite-cli runs on .NET 10 (Windows, macOS or Linux). Open a terminal (on Windows: **PowerShell** or **Windows Terminal**) and check whether you already have it:

```sh
dotnet --list-sdks
```

If you see a line starting with `10.`, skip to step 2. Otherwise install the **.NET 10 SDK**:

| System | Command |
|---|---|
| Windows | `winget install Microsoft.DotNet.SDK.10` |
| macOS | `brew install --cask dotnet-sdk` |
| Linux | follow <https://learn.microsoft.com/dotnet/core/install/linux> |

Alternatively, download the installer from <https://dotnet.microsoft.com/download/dotnet/10.0>.

> **Close and reopen your terminal** after installing, so it picks up the new `dotnet` command.

## 2. Install eventbrite-cli

Choose **one** of the options below.

### Option A: from the GitHub release (easiest)

1. Open <https://github.com/nicolaparo/eventbrite-cli/releases/latest>.
2. Download the file ending in **`.nupkg`** (for example `EventbriteCli.0.1.1.nupkg`) into your **Downloads** folder.
3. Run the command for your system:

   **Windows (PowerShell)**

   ```powershell
   dotnet tool install --global EventbriteCli --add-source "$HOME\Downloads"
   ```

   **macOS / Linux**

   ```sh
   dotnet tool install --global EventbriteCli --add-source ~/Downloads
   ```

### Option B: with the GitHub CLI (`gh`)

If you use the [GitHub CLI](https://cli.github.com/), this downloads and installs in two lines:

```sh
gh release download --repo nicolaparo/eventbrite-cli --pattern "*.nupkg" --dir eventbrite-cli-package
dotnet tool install --global EventbriteCli --add-source eventbrite-cli-package
```

### Option C: build from source

```sh
gh repo clone nicolaparo/eventbrite-cli      # or: git clone https://github.com/nicolaparo/eventbrite-cli.git
cd eventbrite-cli
dotnet pack src/EventbriteCli -c Release
dotnet tool install --global EventbriteCli --add-source ./nupkg
```

### Check it worked

```sh
eventbrite --version
```

You should see a version number such as `0.1.1`. If you get **"command not found"** or **"not recognized"**, close and reopen the terminal. If that doesn't help, see [Troubleshooting](#troubleshooting).

## 3. Get your Eventbrite private token

The tool needs a **private token** to read your account. Think of it as a password: keep it to yourself.

1. Log in to [eventbrite.com](https://www.eventbrite.com).
2. Open <https://www.eventbrite.com/platform/api-keys>. You can also click your profile picture, then **Account settings**, then **Developer links**, then **API keys**.
3. If you have no API key yet, click **Create API key**, fill in the short form (any app name and URL will do), and submit it.
4. Find your key in the list and click **Show API key**.
5. Copy the value labelled **Private token**. Don't copy the API key, client secret or public token.

## 4. Log in

```sh
eventbrite auth login
```

Paste your private token when asked and press **Enter**. For your safety, **nothing appears on screen while you paste**; this is normal. The tool checks the token with Eventbrite and then shows:

```text
Logged in as Jane Doe <jane@example.com> (user id 1234567890).
```

You only need to do this once. The token is saved in your user profile (see [Security](#security)).

## 5. Try it

```sh
eventbrite me                  # who am I?
eventbrite orgs list           # my organizations
eventbrite events list         # my events
eventbrite events get 812345678901
eventbrite attendees list --event 812345678901
```

Every event, order and attendee has an **ID** (the first column of a list). Use it in the next command to see more detail.

**Getting help:** add `--help` to any command to see everything it can do.

```sh
eventbrite --help
eventbrite events --help
eventbrite attendees list --help
```

---

## Command reference

| What | Command |
|---|---|
| Your account | `eventbrite me` |
| Your organizations / members / roles | `eventbrite orgs list` · `orgs members` · `orgs roles` |
| Events | `eventbrite events list` · `events get <event-id>` |
| Event details | `events description <id>` · `events ticket-classes <id>` · `events ticket-class <id> <ticket-class-id>` · `events questions <id>` · `events canned-questions <id>` · `events capacity <id>` · `events inventory-tiers <id>` · `events display-settings <id>` · `events teams <id>` · `events structured-content <id> --purpose listing` |
| Attendees | `eventbrite attendees list [--event <id>]` · `attendees get <event-id> <attendee-id>` |
| Orders | `eventbrite orders list [--event <id>]` · `orders get <order-id>` |
| Venues | `eventbrite venues list` · `venues get <id>` · `venues events <id>` |
| Discounts / promo codes | `eventbrite discounts list --scope event --event-id <id>` · `discounts get <id>` |
| Webhooks | `eventbrite webhooks list` |
| Reports | `eventbrite reports sales --event-ids <id>` · `reports attendees --event-ids <id>` |
| Categories, formats | `eventbrite catalog categories list` · `catalog subcategories list` · `catalog formats list` |
| Anything else in the API | `eventbrite api get <path>` |
| Login | `eventbrite auth login` · `auth status` · `auth logout` |

### Useful options

| Option | What it does | Example |
|---|---|---|
| `-o json` | Print **all** the data as JSON instead of a summary table | `eventbrite events get 123 -o json` |
| `-n`, `--limit` | Maximum items in a list (default 50, `0` = everything) | `eventbrite attendees list --event 123 -n 0` |
| `--org` | Choose the organization, if you have more than one | `eventbrite events list --org 987654` |
| `--status` | Filter by status | `eventbrite events list --status live` |
| `--time-filter` | `past`, `current_future` or `all` | `eventbrite events list --time-filter past` |
| `--expand` | Include related objects | `eventbrite events get 123 --expand venue,ticket_availability` |
| `--changed-since` | Only items changed since a UTC date and time | `eventbrite orders list --changed-since 2026-01-01T00:00:00Z` |

### Recipes

```sh
# Save every attendee of an event to a file
eventbrite attendees list --event 123 -n 0 -o json > attendees.json

# Past events only
eventbrite events list --time-filter past

# Raw access to any read endpoint (merging all pages of a list)
eventbrite api get events/123/attendees --items attendees -n 0
eventbrite api get events/123 -q expand=venue
```

### Environment variables (optional)

| Variable | Purpose |
|---|---|
| `EVENTBRITE_TOKEN` | Use this token instead of the saved one (handy for scripts and CI) |
| `EVENTBRITE_ORG` | Default organization ID, so you don't have to pass `--org` every time |

---

## Troubleshooting

| Message / problem | Fix |
|---|---|
| `eventbrite: command not found` / `not recognized` | Close and reopen the terminal. If it still fails, add the .NET tools folder to your PATH: `%USERPROFILE%\.dotnet\tools` on Windows, `~/.dotnet/tools` on macOS/Linux. |
| `dotnet: command not found` | .NET is not installed: see [step 1](#1-install-net-one-time). |
| `No Eventbrite token configured` | Run `eventbrite auth login`. |
| `Eventbrite API error 401 INVALID_AUTH` | The token is wrong or was revoked. Copy the **Private token** again (step 3) and rerun `eventbrite auth login`. |
| `You belong to several organizations` | Add `--org <id>` using an ID from the message, or set `EVENTBRITE_ORG`. |
| `Eventbrite API error 404 NOT_FOUND` | Check the ID. You can only see events of organizations you belong to. |
| `Eventbrite API error 429 HIT_RATE_LIMIT` | Eventbrite allows about 2,000 requests per hour. Wait a bit, or use a smaller `--limit`. |
| `The stored token cannot be decrypted` | Happens after changing Windows user or PC. Run `eventbrite auth login` again. |
| `Tool 'eventbritecli' is already installed` | See [Update or uninstall](#update-or-uninstall). |
| A table column is empty | Some fields are optional on Eventbrite. Use `-o json` to see everything that was returned. |

## Update or uninstall

```sh
# Update: download the new .nupkg (step 2), then
dotnet tool update --global EventbriteCli --add-source <folder-with-the-nupkg>

# Uninstall
eventbrite auth logout          # removes the saved token
dotnet tool uninstall --global EventbriteCli
```

## Security

- **The tool only reads.** It never sends anything except `GET` requests to Eventbrite.
- **Where the token is saved:**
  - Windows: `%APPDATA%\eventbrite-cli\token`, encrypted so only your Windows user can read it.
  - macOS/Linux: `~/.config/eventbrite-cli/token`, readable only by you.
- **Where the token goes:** it is sent only to `www.eventbriteapi.com`, never to any other address.
- **No `--token` option:** this keeps the token out of your shell history.
- **If your token leaks:** go to the API keys page (step 3) and regenerate it.

## Development

Requires the .NET 10 SDK.

```sh
dotnet build
dotnet test
dotnet run --project src/EventbriteCli -- events list
```

See [CLAUDE.md](CLAUDE.md) for the architecture, coding rules and Eventbrite API notes.
