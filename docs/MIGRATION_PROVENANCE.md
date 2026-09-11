# Public Repository Migration Provenance

This repository intentionally starts with fresh public Git history rather than publishing the historical private development repository.

## Legacy source

- Private legacy repository: `666drjekyll666-cloud/Move-Gamepad-Tooltips-legacy-private`
- Legacy public/mod name: **Move Gamepad Tooltips**
- Accepted legacy version: **1.2.0**
- Accepted freeze branch: `baseline/1.2.0-accepted`
- Accepted freeze commit: `fbb2f7e9d44a70410c3396028409ffce77265e25`
- Accepted DLL SHA-256: `05e5d957c82b1a828584ffa9b93db67b7146410cbeda5a2898fa5efc7bf64869`
- Accepted player test date recorded in the legacy repository: 2026-09-05

Later legacy-main commits only normalized repository policy/documentation; the accepted runtime behavior remained the 1.2.0 baseline.

## Public migration

The public repository uses the new name **Gamepad Tooltip Position Fix** and technical identity `GamepadTooltipPositionFix` for the project/assembly/DLL.

The BepInEx GUID remains `nikich.gyk.movegamepadtooltips` intentionally. This preserves plugin identity while avoiding any need to publish the old private Git history.

Version 1.3.0 is used for the renamed public package so that the immutable accepted 1.2.0 binary is never silently replaced by different bytes under the same version.

## Material intentionally not migrated

The new public history does not import old branches, diagnostic experiments, GitHub Actions history, or other private-repository bookkeeping. No Graveyard Keeper assemblies, extracted game assets, or decompiled game source are required by this production project.
