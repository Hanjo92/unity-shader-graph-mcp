# Roadmap

## Milestone 1 / 1.0.0

Deliver a narrow but real Shader Graph editing path:

- create graph with a blank-only package-backed path
- read summary
- add property
- add node
- connect ports for the verified package-backed paths
- save through the package-backed validate + write + refresh flow
- keep contracts/examples ahead of implementation changes

This milestone is shipped and closed.

## Milestone 2 / 1.1.0

Expand the supported surface on top of the shipped `1.0.0` base:

- broaden the remaining package-backed connection matrix
- promote broader property and graph-addable node coverage carefully
- harden the supported subgraph composition paths
- keep diagnostics, compatibility notes, and metadata aligned with the real runtime boundary

This milestone is shipped and closed. See [1.1.0-plan.md](/Users/song/Projects/unity-shader-graph-mcp/docs/1.1.0-plan.md) for the completed target cut.

## Milestone 3 / 1.2.0

Production hardening:

- advanced Boolean and property-node routing beyond the verified `1.1.0` matrix
- graph-addable catalog expansion from probe/discovery data into runtime support
- deeper subgraph composition beyond the current safe output and contract replay paths
- compatibility matrix by Unity version and Shader Graph package version; see [compatibility-matrix.md](/Users/song/Projects/unity-shader-graph-mcp/docs/compatibility-matrix.md)

This milestone is shipped and closed.

## Milestone 4 / 1.3.0

Full-node-support foundation:

- setup panel for package status, reports, docs, and smoke entrypoints
- node catalog classification for supported, filtered, probe-failed, initializer-backed, metadata-required, externally asset-bound, package-specific, version-sensitive, and render-pipeline-sensitive buckets
- verified graph-addable promotion batches across math/value/vector, texture/sample, coordinate/utility, normal/lighting/rendering, and portable default specialized nodes
- promoted-node contract replay coverage
- documentation that separates `supportedNodeTypes`, `discoveredNodeTypes`, `nodeCatalogClassification`, and `supportedConnectionRules`

This milestone is shipped and closed.

## Milestone 5 / 1.4.0 Candidate

Move from foundation to explicit full-node completion:

- configuration serialization for metadata-heavy nodes such as `Dropdown`, `Keyword`, and string-body `CustomFunction`
- explicit `.shadersubgraph` asset binding fixtures for `SubGraphNode`
- package-context handling for UI, sprite, VFX, deformation, and custom-interpolator nodes where portable support is realistic
- contract replay for each newly promoted configuration-heavy or asset-bound family
- no universal port promise unless the route is represented in `supportedConnectionRules`

The first `1.4.0` cut promotes the explicit metadata-backed families above and
keeps package-context UI/sprite/VFX/deformation/custom-interpolator families as
diagnostic follow-up scope unless they gain similarly explicit fixtures.
