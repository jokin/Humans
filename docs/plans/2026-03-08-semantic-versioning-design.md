# Semantic Versioning

## Summary

Replace hardcoded `0.0.1.0` version with semantic versioning derived from git tags using [MinVer](https://github.com/adamralph/minver). The footer displays the version linked to GitHub release notes, falling back to the commit hash for dev/QA builds.

## How It Works

- **MinVer** reads git tags (prefixed with `v`) at build time to set `Version`, `FileVersion`, and `InformationalVersion`
- On a tagged commit (e.g., `v0.8.0`): version = `0.8.0`, `InformationalVersion` = `0.8.0+<hash>`
- Between tags: version = `0.8.1-alpha.0.N` (N = commits since tag), `InformationalVersion` = `0.8.1-alpha.0.N+<hash>`
- `SourceRevisionId` target still provides the `+<hash>` suffix via the existing MSBuild target

## Footer Display

| Build type | Display | Links to |
|------------|---------|----------|
| Release (clean semver) | `v0.8.0` | `github.com/.../releases/tag/v0.8.0` |
| Dev/QA (pre-release) | `91a0a01` | `github.com/.../commit/91a0a01...` |
| No version data | Nothing | — |

## Release Process

After merging a PR to `upstream` (production):

```bash
gh release create v0.9.0 -R nobodies-collective/Humans --title "v0.9.0" --generate-notes
```

This creates both the git tag and GitHub Release page (which the footer links to). The `--generate-notes` flag auto-generates release notes from PR titles since the last tag.

## Changes Made

- `Directory.Packages.props`: Added `MinVer 7.0.0`
- `Directory.Build.props`: Removed manual `Version`/`FileVersion`/`AssemblyVersion`, added `MinVerTagPrefix` and MinVer package reference
- `_Layout.cshtml`: Footer shows version with release link (release builds) or commit hash (dev builds)
- Tagged `v0.8.0` on current HEAD
