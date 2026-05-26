# Scripts

Common build workflow helpers for this repo.

## Build

```powershell
.\scripts\build.ps1
.\scripts\build.ps1 -Configuration Release
```

`build` publishes `ManifestTask` to a per-run cache and regenerates `build/generated/ManifestVariables.g.props` before building the solution.
It also produces product zips under `bin/Products` after the solution build completes.
Debug product zips keep `.pdb` and `.deps.json` files and are tagged with `[DEBUG]` in the zip name. Release product zips use the same project payloads but strip those files through the central product exclusion list.

## Clean

```powershell
.\scripts\clean.ps1
.\scripts\clean.ps1 -DryRun
.\scripts\clean.ps1 -IncludeIdeCaches
.\scripts\clean.ps1 -PurgeManifestTaskCache
```

`clean` removes:
- all `bin` and `obj` folders in the repo
- `build/generated`
- `build/sdk/SmapiManifestSdk`
- `build/sdk/SmapiManifestSdk.zip`
- optional `.idea` cache with `-IncludeIdeCaches`
- optional user cache with `-PurgeManifestTaskCache`

If a stale MSBuild host is still holding a build artifact open, `clean` will stop the relevant `dotnet` build process and retry the removal.

## Rebuild

```powershell
.\scripts\rebuild.ps1
.\scripts\rebuild.ps1 -Configuration Release
```

This runs clean, then builds the solution.

## ManifestTask Cache

`ManifestTask` is published to a per-user cache under the local application data folder. The build scripts publish a fresh cache path on each run, then pass that path into the solution build so `UsingTask` never points at the same file that later rebuilds or cleans need to replace.
