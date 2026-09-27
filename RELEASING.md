# Releasing ShellIcons

How to ship a new version of `ShellIcons.Blazor` to NuGet.org and create the matching GitHub Release.

**Who can release:** anyone who can approve the `release` environment on GitHub. Contributors without that access open a PR; a maintainer does the release.

**What's released:** only `ShellIcons.Blazor`. `ShellIcons.Maui` is in preview — `release.yml` doesn't pack it yet, and publishing it will need a Windows or macOS job with the MAUI workload. The docs site isn't part of a release either: it deploys to [shellicons.shellui.dev](https://shellicons.shellui.dev) on every push to `main` (`docs.yml`).

Publishing uses **NuGet Trusted Publishing** — GitHub Actions authenticates via OIDC and gets a short-lived (~1 hour) API key per run. No long-lived key lives in the repo.

## One-time setup (repo owner — already done)

Skip to [Every release](#every-release) unless you're setting up the repo or a new package ID. These steps need the nuget.org package owner and repo admin rights.

### 1. Reserve the package on nuget.org (if not already)

Trusted Publishing policies can only be attached to packages you own. For the first release the package ID doesn't exist yet — you need to reserve it OR create the policy with your account as owner and rely on first-push claiming.

- Sign in to <https://www.nuget.org>
- Preferred: reserve the ID prefix `ShellIcons.` under your account so any `ShellIcons.*` push is yours
  - Profile menu → **Package IDs** → **Reserve** → `ShellIcons.*`
  - (Needs review by the NuGet team — takes 1–2 days)
- Alternate: skip the reservation, first-push will claim the ID under whoever publishes

### 2. Create the Trusted Publishing policy on nuget.org

- Signed in on nuget.org → click your username → **Trusted Publishing**
  (direct link: <https://www.nuget.org/account/TrustedPublishing>)
- **Add Trusted Publishing Policy** — GitHub Actions

Fill in exactly these values:

| Field                | Value                              |
|----------------------|------------------------------------|
| Policy Name          | `shell-icons release`              |
| Package Owner        | *your nuget.org username*          |
| Package Glob         | `ShellIcons.*`                     |
| Repository Owner     | `shellui-dev`                      |
| Repository           | `shell-icons`                      |
| Workflow File        | `release.yml`                      |
| Environment          | `release`                          |

**Every field matters.** If the workflow filename or environment name in the policy doesn't match the actual workflow, the OIDC exchange refuses. The `environment: release` line in `release.yml` and the environment in the policy must be identical.

- **Enable Policy** → **Add**

### 3. Add the `NUGET_USER` secret to the GitHub repo

The workflow needs your nuget.org **username** (not email, not the GH org name) to negotiate the OIDC exchange. It stays a secret so it doesn't leak in workflow logs.

- Repo → **Settings** → **Secrets and variables** → **Actions**
- **New repository secret**
  - Name: `NUGET_USER`
  - Value: your nuget.org profile name — the one that appears in your profile URL (`https://www.nuget.org/profiles/<this-part>`)

That's the only secret you need. There is **no `NUGET_API_KEY`** — it's minted per-run.

### 4. Create the `release` environment

The workflow uses `environment: release`, and the Trusted Publishing policy pins to that name. Gating the environment makes every release require an approver click, so no rogue tag push auto-publishes.

- Repo → **Settings** → **Environments** → **New environment** → name it exactly `release`
- Under **Deployment protection rules** → **Required reviewers** — add the maintainers who may release
- Save

## Every release

### 1. Decide the version

Follow SemVer:

- `0.1.0-alpha` → `0.1.0-alpha.1` → `0.1.0-alpha.2` — same alpha line, iterating
- `0.1.0-alpha` → `0.1.0-beta` — moving toward stable
- `0.1.0-alpha` → `0.1.0-rc.1` → `0.1.0` — release candidate then stable
- `0.1.0` → `0.1.1` — patch fix
- `0.1.0` → `0.2.0` — additive change
- `0.1.0` → `1.0.0` — breaking change

Prerelease suffixes (`-alpha`, `-beta`, `-rc`) automatically mark the GitHub Release as prerelease and NuGet lists it under "Include prerelease" only.

### 2. Bump the version and the CHANGELOG

In one PR:

- [`Directory.Build.props`](Directory.Build.props) — set `<Version>` to the new version, e.g. `0.2.0-alpha`.
- [`CHANGELOG.md`](CHANGELOG.md) — rename `## [Unreleased]` to `## [0.2.0-alpha] — YYYY-MM-DD` and add a fresh, empty `## [Unreleased]` above it.

Merge it to `main`.

### 3. Sanity check locally (optional)

```bash
dotnet test ShellIcons.slnx --configuration Release
dotnet pack src/ShellIcons.Blazor/ShellIcons.Blazor.csproj --configuration Release --output nupkgs
```

Should produce `nupkgs/ShellIcons.Blazor.<version>.nupkg` and `.snupkg`.

### 4. Tag the release

From an up-to-date `main`. The tag **must match** `<Version>` in `Directory.Build.props`, prefixed with `v` — the workflow refuses to publish if they disagree.

```bash
git tag v0.2.0-alpha -m "ShellIcons 0.2.0-alpha"
git push origin v0.2.0-alpha
```

### 5. Approve the run

Pushing the tag triggers `.github/workflows/release.yml`. Because of the `release` environment gate, the workflow pauses at the "Push to NuGet" step waiting for you to approve.

- Watch the run at <https://github.com/shellui-dev/shell-icons/actions>
- When it says "Waiting for review", click **Review deployments** → check `release` → **Approve and deploy**

### 6. Verify

- **NuGet.org**: <https://www.nuget.org/packages/ShellIcons.Blazor> — should show the new version within a couple of minutes (indexing) and be installable after ~10 minutes.
- **GitHub Release**: <https://github.com/shellui-dev/shell-icons/releases> — auto-generated release notes based on merged PRs since the last tag, with `.nupkg` + `.snupkg` attached.

## What the workflow does, step by step

```
push tag v<version>
        │
        ▼
   [checkout with full history]
        │
        ▼
   [restore + build (Release) + test]
        │
        ▼
   [dotnet pack → nupkgs/ShellIcons.Blazor.<v>.nupkg + .snupkg]
        │
        ▼
   [verify tag == Directory.Build.props version]     ← fails if mismatch
        │
        ▼
   [verify version not already on nuget.org]         ← fails if already published
        │
        ▼
   *** environment: release — waits for reviewer ***
        │
        ▼
   [NuGet/login@v1 — OIDC exchange with nuget.org]   ← uses NUGET_USER
   [dotnet nuget push --skip-duplicate]              ← uses short-lived key
        │
        ▼
   [softprops/action-gh-release]                     ← creates GitHub Release,
                                                       attaches .nupkg + .snupkg,
                                                       generates release notes,
                                                       marks prerelease if -alpha/-beta/-rc
```

## Dry runs

Manual `workflow_dispatch` supports a `dry_run` boolean (defaults to `true`) — packs, verifies, but skips the actual NuGet push and GitHub Release. Useful for confirming the pipeline is healthy without shipping anything.

- Actions → Release → **Run workflow** → leave `dry_run` checked → **Run**

## Recovering from mistakes

### Version already published, needs a fix

**You cannot unpublish or overwrite** a published NuGet version. If a broken `0.2.0-alpha` shipped, don't re-tag `v0.2.0-alpha` — the "already on nuget.org" check fails anyway.

Instead:

1. Fix the bug on `main`
2. Bump `Directory.Build.props` (e.g. `0.2.1-alpha`) and add a CHANGELOG entry
3. Tag `v0.2.1-alpha` and push
4. Optional: the package owner marks the broken version as deprecated on nuget.org (Package → Manage → Deprecate)

### Tag pushed with the wrong version

The workflow will fail at the "tag matches props version" check before any publish happens. Delete the tag and re-tag:

```bash
git tag -d v0.2.0-alpha
git push origin :refs/tags/v0.2.0-alpha
```

Fix `Directory.Build.props` if needed, merge, then tag again.

## Secrets used

| Name           | Where set        | What for                                                           | Rotation |
|----------------|------------------|--------------------------------------------------------------------|----------|
| `NUGET_USER`   | Repo secret      | The package owner's nuget.org username — feeds the OIDC exchange for a short-lived key | Only if the owner's nuget.org handle changes |
| `GITHUB_TOKEN` | Auto-provided    | Creates the GitHub Release + OIDC identity claims                  | Never — regenerated per run by GitHub |

**No long-lived NuGet API key.** Trusted Publishing mints one per run, valid ~1 hour, scoped to this workflow + environment. If the repo is ever compromised, an attacker can't steal a persistent NuGet credential — because there isn't one.

## Troubleshooting

**`NuGet/login@v1` fails with "no matching trusted publishing policy"**

The Trusted Publishing policy on nuget.org doesn't match what the workflow claims. Check every field:

- Repository owner exactly `shellui-dev` (case sensitive)
- Repository exactly `shell-icons`
- Workflow file exactly `release.yml` (not `.github/workflows/release.yml`, just the filename)
- Environment exactly `release`
- Policy is **Enabled** (there's a toggle after creating)

**`dotnet nuget push` fails with 403 after successful OIDC login**

The short-lived key was minted but doesn't authorize the package glob. Check the policy's **Package Glob** matches `ShellIcons.*` (or is broader). If you renamed the package ID and the glob is now stale, edit the policy.

**Workflow says "Waiting for review" and never proceeds**

The `release` environment gate is doing its job — a required reviewer has to click. Actions → Release → **Review deployments** → check `release` → **Approve and deploy**. A reviewer can approve their own run.
