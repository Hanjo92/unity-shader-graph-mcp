# Property Node AddNode Design

## Goal

Extend `add_node` so the package-backed path can create a `PropertyNode` bound to an existing Shader Graph property without introducing a separate action.

## Scope

This slice only covers direct `add_node` authoring for `PropertyNode`.

- In scope:
  - `add_node` request fields for property binding queries
  - Python request normalization
  - Unity batchmode bridge parsing
  - package-backed `PropertyNode` creation and binding
  - supported-node catalog/probe promotion for `Property`
  - response metadata that shows which property was bound
  - smoke and parser tests
- Out of scope:
  - new `add_property_node` action
  - connection-matrix expansion for `PropertyNode`
  - import/export contract replay for `PropertyNode`

## Design

### Request Surface

`add_node` keeps its existing shape and gains optional property-binding query fields:

- `propertyName`
- `propertyDisplayName`
- `referenceName`
- `propertyType`

These mirror the existing `find_property` matching rules as closely as possible. `displayName` remains the node display name, so property display-name queries use `propertyDisplayName` to avoid ambiguity.

When `nodeType` resolves to `Property`, at least one property-binding query field must be present. For all other node types, the new fields are ignored.

### Normalization and Parsing

The Python MCP tool accepts the new aliases and forwards them unchanged to the Unity bridge. The Unity batchmode bridge parses the fields into `AddNodeRequest` so the package-backed implementation receives one transport-neutral request model.

### Package-Backed Binding Flow

The package-backed `AddNode(...)` path gains a `PropertyNode` specialization:

1. Resolve the requested property from `graphData.properties` using the same display-name, reference-name, and property-type matching logic already used by `find_property`.
2. Fail with query metadata when zero or multiple properties match.
3. Bind the resolved property to the newly created `PropertyNode`.
4. Default the node display name to the bound property display name when the caller does not provide `displayName`.
5. Continue through the existing layout, add-node, validate, and save flow.

### Supported Node Catalog

`PropertyNode` cannot be safely probed as an unbound empty node, so the catalog probe needs a special case:

- create a temporary graph property
- instantiate `PropertyNode`
- bind the temporary property
- run `AddNode -> ValidateGraph`

That lets `Property` appear in the supported node list instead of remaining discoverable-only.

### Response Metadata

Successful `add_node` responses for `PropertyNode` include binding metadata with:

- the property query
- match strategy
- the resolved bound property lookup data

Failure responses for `PropertyNode` reuse the same binding metadata so callers can understand why binding failed.

## Testing

- Python normalization test for `add_node` property-binding aliases
- Unity batchmode bridge parse test for the new `AddNodeRequest` fields
- Unity smoke test that:
  - creates a property
  - adds a `PropertyNode` bound to that property
  - verifies package-backed execution and binding metadata

## Acceptance Criteria

- `add_node` accepts `nodeType=Property` with property query fields.
- The supported-node surface reports `Property`.
- A property-bound `PropertyNode` can be added through the package-backed backend without falling back.
- The response clearly identifies the bound property.
