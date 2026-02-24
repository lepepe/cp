# cp — Interactive Git Cherry-Pick Helper

A C# CLI tool that makes `git cherry-pick` simple and visual. Browse commits,
select one or many with a checkbox list, choose a target branch, and handle
conflicts — all without leaving your terminal.

Built with [Spectre.Console](https://spectreconsole.net/) on .NET 10.

---

## Features

- **Branch picker** — select any local or remote branch as the source
- **Commit table** — view the last 60 commits with hash, date, author, and message
- **Multi-select** — toggle commits with `Space`, confirm with `Enter`
- **Target branch** — type a new branch name (creates it) or an existing one (checks it out)
- **Conflict handling**
  - Lists every conflicted file
  - Shows a colour-coded diff per file (`+` green, `-` red, `@@` blue)
  - Lets you fix conflicts in your editor, then stage & continue
  - Or skip the commit, or abort the entire session
- **Summary** — applied / skipped count at the end

---

## Prerequisites

| Requirement | Version  |
|-------------|----------|
| .NET SDK    | 10.0+    |
| git         | any      |

---

## Build & Run

```bash
# Clone or enter the project directory
cd cp

# Run directly (development)
dotnet run

# Build a Release binary
dotnet build -c Release
./bin/Release/net10.0/cp

# Publish as a self-contained single-file executable
dotnet publish -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true -p:AssemblyName=cp -o bin/publish

# Optionally put it on your PATH
sudo cp bin/publish/cp /usr/local/bin/cp
```

> Run `cp` from inside any git repository.

---

## Usage

```
$ cp
```

The tool walks you through each step interactively:

```
Step 1  Pick the source branch to cherry-pick from
Step 2  A table shows the last 60 commits on that branch
Step 3  Select one or more commits (Space toggles, Enter confirms)
Step 4  Enter the name of the target/promotion branch
Step 5  The branch is created or checked out automatically
Step 6  Each commit is cherry-picked in chronological order
        On conflict → view diffs, fix, continue / skip / abort
Step 7  A summary shows how many commits were applied or skipped
```

### Conflict resolution options

| Choice | What happens |
|--------|-------------|
| I fixed it manually — stage & continue | Runs `git add -A && git cherry-pick --continue` |
| Skip this commit | Runs `git cherry-pick --skip` |
| Abort all remaining cherry-picks | Runs `git cherry-pick --abort` and exits |

---

## Project Structure

```
cp/
├── cp.csproj          # Project file (.NET 10, Spectre.Console 0.54)
├── Program.cs         # App entry point — UI flow and cherry-pick loop
├── GitService.cs      # Thin wrapper around git CLI commands
└── Models/
    └── CommitInfo.cs  # Record representing a single commit
```

---

## License

MIT
