# Property Node Contract Replay Design

## Goal

Preserve `PropertyNode` bindings through `export_graph_contract` and
`import_graph_contract` without introducing a new contract version or a new
tool action.

## Scope

- In scope:
  - exporting bound property metadata for `PropertyNode`
  - importing that metadata back into `AddNodeRequest`
  - smoke coverage for graph contract round-trip
  - response metadata coverage for the exported node contract shape
- Out of scope:
  - new `PropertyNode` connection families
  - new MCP request fields
  - non-additive contract version changes

## Design

### Export Shape

`BuildExportedNodeContractData(...)` stays additive. When the exported node is a
`PropertyNode`, the node dictionary gains:

- `propertyName`
- `propertyDisplayName`
- `referenceName`
- `propertyType`

These fields mirror the existing `add_node` property-binding surface. They are
redundant by design so external tooling can replay or inspect the contract
without resolving nested structures.

### Import Replay

`ImportedGraphContractNode` gains matching optional fields. During
`import_graph_contract`, the package-backed path passes them into
`new AddNodeRequest(...)` so `PropertyNode` reuses the same binding flow that
plain `add_node` already uses.

### Compatibility

This stays on `unity-shader-graph-mcp/export-graph-contract-v1` because the new
fields are additive and optional. Older contracts continue to import. Newer
contracts still degrade safely when the node is not a `PropertyNode`.

## Testing

- export smoke:
  - create a `Color` property
  - add a bound `PropertyNode`
  - export and assert the exported node carries the binding fields
- import smoke:
  - build `PropertyNode -> Split`
  - export from source graph
  - import into a blank target graph
  - verify property, node, and connection counts survive round-trip
- metadata test:
  - preserve the new node contract fields in the export envelope

## Acceptance Criteria

- exported `PropertyNode` contract entries include enough data to replay the
  binding
- importing a contract containing `PropertyNode` succeeds through the
  package-backed path
- a bound `PropertyNode` can still participate in a simple connection after
  round-trip import
