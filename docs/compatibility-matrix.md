# Compatibility Matrix

This repository uses a lightweight, snapshot-driven compatibility matrix rather than a large hand-maintained version table.

The matrix is intended to answer three questions:

1. Which Unity / Shader Graph combination was last verified?
2. Which backend mode did the editor choose for that combination?
3. What should a user do when the package surface degrades or falls back?

## Current Verified Baseline

| Unity | Shader Graph | Backend kind | Core mutation surface | Notes |
| --- | --- | --- | --- | --- |
| Unity 2022.3 | Shader Graph 17.3.0 | `PackageReady` | `Yes` | Current package-backed baseline used for the first `1.0.0` cut. |

## Backend Modes

| Backend kind | What it means | Practical behavior |
| --- | --- | --- |
| `Scaffold` | No Shader Graph package assembly was detected. | The server stays on scaffold-backed behavior and writes the sidecar manifest flow instead of touching real Unity graph internals. |
| `PackageDetectedButIncomplete` | Shader Graph is present, but the reflective core mutation surface is incomplete. | The report captures the missing surface so the next editor upgrade can be triaged before package-backed mutations are trusted. |
| `PackageReady` | The reflective `GraphData` surface exposes the verified core mutation methods. | Package-backed graph creation, summary reads, and the confirmed mutation paths remain enabled. |

## How To Capture A Snapshot

1. Open the project in Unity.
2. Run `Tools > Shader Graph MCP > Write Compatibility Report`.
3. Inspect the generated file under `Assets/ShaderGraphMcpDiagnostics/`.
4. Record `unityVersion`, `backendKind`, `graphTypeName`, `baseTypeName`, `hasCoreMutationSurface`, and the missing method names when the surface is incomplete.
5. Regenerate the report after upgrading Unity or the Shader Graph package.

## What To Update When Versions Change

- If the report stays `PackageReady`, keep the current package-backed paths and update this table only when the observed baseline changes.
- If the report drops to `PackageDetectedButIncomplete`, use the fallback notes and missing methods to decide whether the next release should remain scaffold-only or trim the supported matrix.
- If the package is not detected, keep the matrix row as scaffold-only and avoid claiming package-backed compatibility.

