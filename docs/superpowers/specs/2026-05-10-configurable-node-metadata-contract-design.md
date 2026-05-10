# Configurable Node Metadata Contract Design

## Goal

Define the request, initializer, catalog, and contract replay shape for
configuration-heavy Shader Graph nodes before promoting them from
diagnostic-only status.

Target nodes for the first 1.4.0 configurable-node slice:

- `Dropdown`
- `Keyword`
- `CustomFunction`

This design keeps node addability separate from port compatibility. Promoting a
node through this path does not add connection support unless a later slice also
adds explicit `supportedConnectionRules`.

## Current Constraints

- Python request normalization preserves arbitrary payload keys, so external
  clients can send richer metadata without changing the tool action.
- Unity batchmode parsing currently uses `JsonUtility` into
  `ShaderGraphBatchmodeRequestPayload`, so arbitrary nested dictionaries are not
  a safe direct bridge shape.
- `AddNodeRequest` is the single transport-neutral Unity request model for
  `add_node`.
- `PropertyNode` already uses explicit top-level query fields because its
  configuration is just a property binding query.
- `export_graph_contract` imports through `JsonUtility` into typed
  `ImportedGraphContractNode` fields, so replay metadata must have a typed or
  string-backed representation.

## Request Surface

Keep `add_node` as the only action. Add one generic configurable-node field:

- `nodeConfigJson`

`nodeConfigJson` is a compact JSON string whose schema is selected by
`nodeType`. It is optional for ordinary graph-addable nodes and required for
configurable nodes once they are promoted.

External MCP clients may also send a structured `nodeConfig` object. The Python
normalizer should serialize it to `nodeConfigJson` before forwarding requests to
Unity. Direct Unity batchmode callers should send `nodeConfigJson`.

Do not add large sets of top-level fields for Dropdown, Keyword, or
CustomFunction. Keeping those details inside `nodeConfigJson` avoids
collisions with existing fields such as `displayName`, `propertyName`,
`referenceName`, and future SubGraph asset-binding fields.

## AddNodeRequest Shape

`AddNodeRequest` should gain:

- `NodeConfigJson`

The constructor should normalize it to an empty string when omitted. Existing
constructor call sites should continue to compile by using an optional trailing
parameter.

Node-specific parsing should happen inside the matching node initializer, not
in `ShaderGraphBatchmodeBridge.TryCreateAddNodeRequest(...)`. The bridge should
only copy `payload.nodeConfigJson` into `AddNodeRequest`.

## Config Schemas

All schemas are versioned independently from the graph contract version.

### Dropdown

Initial supported mode:

```json
{
  "kind": "Dropdown",
  "version": 1,
  "entries": ["Low", "Medium", "High"],
  "defaultValue": "Medium"
}
```

Required fields:

- `kind = "Dropdown"`
- `version = 1`
- `entries` with at least one non-empty item

Optional fields:

- `defaultValue`
- `defaultIndex`

Validation rules:

- `defaultValue`, when present, must match an entry.
- `defaultIndex`, when present, must be within the entries array.
- If both default fields are absent, the initializer should use the first
  entry.
- If both are present and disagree, fail before graph mutation.

Deferred modes:

- non-string values
- dynamic entries
- package-specific dropdown behaviors that cannot be serialized portably

### Keyword

Initial supported modes:

```json
{
  "kind": "Keyword",
  "version": 1,
  "keywordType": "Boolean",
  "displayName": "Use Detail",
  "referenceName": "_USE_DETAIL",
  "definition": "ShaderFeature",
  "scope": "Graph",
  "defaultValue": false
}
```

```json
{
  "kind": "Keyword",
  "version": 1,
  "keywordType": "Enum",
  "displayName": "Quality",
  "referenceName": "_QUALITY",
  "definition": "ShaderFeature",
  "scope": "Graph",
  "entries": ["Low", "Medium", "High"],
  "defaultValue": "Medium"
}
```

Required fields:

- `kind = "Keyword"`
- `version = 1`
- `keywordType`
- `displayName`
- `referenceName`

Optional fields:

- `definition`
- `scope`
- `entries` for enum keywords
- `defaultValue`

Validation rules:

- Supported `keywordType` values should be explicit allowlist values, starting
  with `Boolean` and `Enum`.
- Enum keywords require at least one entry.
- Boolean defaults must parse as booleans.
- Enum defaults must match an entry.
- Unknown definition or scope values should fail before graph mutation.

Deferred modes:

- keyword variants whose package API requires version-specific serialized
  objects that cannot be recreated from the config above
- global/material scope modes until the package surface is verified

### CustomFunction

Initial supported mode:

```json
{
  "kind": "CustomFunction",
  "version": 1,
  "sourceType": "String",
  "functionName": "MyFunction",
  "functionBody": "void MyFunction_float(float In, out float Out) { Out = In; }",
  "ports": {
    "inputs": [
      { "name": "In", "type": "Vector1" }
    ],
    "outputs": [
      { "name": "Out", "type": "Vector1" }
    ]
  }
}
```

Required fields:

- `kind = "CustomFunction"`
- `version = 1`
- `sourceType`
- `functionName`
- `ports.outputs`

Required for `sourceType = "String"`:

- `functionBody`

Validation rules:

- Start with `sourceType = "String"` only.
- Function name must be non-empty and identifier-like.
- At least one output port is required.
- Port names must be non-empty and unique within their direction.
- Port types must be drawn from a small allowlist such as `Vector1`,
  `Vector2`, `Vector3`, `Vector4`, `Boolean`, and `Texture2D` only after the
  package surface is verified.

Deferred modes:

- external file-backed functions
- arbitrary include paths
- dynamic precision or function-signature generation beyond the verified
  package surface
- custom port types not verified by package-backed smoke tests

## Initializer Flow

Each configurable node should use a named initializer registry entry:

- `DropdownNode`
- `KeywordNode`
- `CustomFunctionNode`

The initializer should:

1. Check that `NodeConfigJson` is present for the supported node type.
2. Parse it into a typed config object.
3. Validate all required fields before calling `GraphData.AddNode`.
4. Apply package-backed node fields through reflection.
5. Return `nodeInitializerData` and `nodeConfigurationData` in success and
   failure responses where possible.
6. Let the existing add-node path handle layout, `AddNode`, `ValidateGraph`,
   save, and summary metadata.

Validation failures should use messages of the form:

```text
Unable to configure <NodeType> node in '<assetPath>': missing required nodeConfigJson field '<fieldName>'.
```

or:

```text
Unable to configure <NodeType> node in '<assetPath>': unsupported <fieldName> '<value>'. Supported values: ...
```

## Catalog Classification

`nodeCatalogClassification.configurableNodeClassification` should continue to
show the whole configurable family.

When the first supported mode for a node is promoted:

- include the canonical node type in a supported configurable list
- include supported mode labels such as `Dropdown:static-string-entries`,
  `Keyword:boolean`, `Keyword:enum`, or `CustomFunction:string-body`
- keep deferred modes visible with stable diagnostics
- keep metadata-required unsupported diagnostics for nodes that still have no
  supported mode

This avoids implying that every mode of a configurable node is supported just
because one safe mode was promoted.

## Export Contract Shape

`BuildExportedNodeContractData(...)` should add configurable-node metadata only
when it can be reconstructed.

Recommended additive fields:

- `nodeConfigKind`
- `nodeConfigVersion`
- `nodeConfigJson`

The first implementation should rely on `nodeConfigJson` for replay because the
current import path is `JsonUtility`-typed. A later response-shape polish can
also include a structured `nodeConfig` object if needed for readability, but
`nodeConfigJson` should remain the replay source of truth.

The graph contract version can stay
`unity-shader-graph-mcp/export-graph-contract-v1` because these fields are
optional and additive.

## Import Replay

`ImportedGraphContractNode` should gain:

- `nodeConfigKind`
- `nodeConfigVersion`
- `nodeConfigJson`

During `import_graph_contract`, pass `nodeConfigJson` into `AddNodeRequest` so
replay uses the same initializer path as direct `add_node`.

If a contract contains a configurable node without required config metadata,
import should fail with a clear `add_node` step failure rather than silently
creating an unconfigured node.

## Python Normalization

The Python server should accept both forms:

```json
{
  "action": "add_node",
  "nodeType": "Dropdown",
  "nodeConfig": {
    "kind": "Dropdown",
    "version": 1,
    "entries": ["Low", "Medium", "High"]
  }
}
```

and:

```json
{
  "action": "add_node",
  "nodeType": "Dropdown",
  "nodeConfigJson": "{\"kind\":\"Dropdown\",\"version\":1,\"entries\":[\"Low\",\"Medium\",\"High\"]}"
}
```

Normalization should:

- serialize `nodeConfig` to `nodeConfigJson` when `nodeConfigJson` is absent
- preserve `nodeConfigJson` when already supplied
- fail if both are supplied but do not represent the same object
- avoid deep semantic validation that depends on Unity package internals

## Testing Plan

Shared tests:

- Python normalization accepts structured `nodeConfig` and emits
  `nodeConfigJson`.
- Python normalization rejects conflicting `nodeConfig` and `nodeConfigJson`.
- Unity batchmode parse test maps `nodeConfigJson` into `AddNodeRequest`.

Per-node implementation tests:

- missing config fails before graph mutation
- invalid config names the exact field
- supported config adds the node through the package-backed path
- `read_graph_summary` reports the node
- `save_graph` succeeds
- exported contract contains `nodeConfigKind`, `nodeConfigVersion`, and
  `nodeConfigJson`
- import replay succeeds for portable supported modes

## Follow-Up Ownership

- #28 owns Dropdown and Keyword implementation on top of this contract.
- #29 owns CustomFunction implementation on top of this contract.
- #32 owns registry expansion mechanics shared by promoted configurable nodes.
- #33 owns export/import replay coverage for the promoted configurable nodes.
- #34 owns any connection routes and must keep them separate from node
  addability.
