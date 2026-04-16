# Property Node AddNode Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Extend `add_node` so `PropertyNode` can be created through the package-backed backend by binding it to an existing graph property.

**Architecture:** Keep `add_node` as the single mutation entrypoint, widen its request model with optional property-binding query fields, and special-case `PropertyNode` binding inside the package-backed add-node flow. Promote `Property` into the supported-node catalog by making the probe property-aware instead of adding a separate authoring action.

**Tech Stack:** Python 3.12 MCP server, Unity Editor C#, NUnit EditMode tests, package-backed Shader Graph reflection helpers.

---

### Task 1: Add the failing request-normalization tests

**Files:**
- Modify: `server/tests/test_shadergraph_asset.py`

- [ ] **Step 1: Write the failing test**

```python
def test_request_normalization_accepts_add_node_property_binding_aliases(self) -> None:
    request = normalize_shadergraph_asset_request(
        {
            "action": "add_node",
            "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
            "nodeType": "Property",
            "propertyName": "Tint",
            "propertyDisplayName": "Base Tint",
            "propertyReferenceName": "_BaseTint",
            "propertyType": "Color",
        }
    )

    self.assertEqual(request.action, "add_node")
    self.assertEqual(request.payload["propertyName"], "Tint")
    self.assertEqual(request.payload["propertyDisplayName"], "Base Tint")
    self.assertEqual(request.payload["referenceName"], "_BaseTint")
    self.assertEqual(request.payload["propertyType"], "Color")
```

- [ ] **Step 2: Run test to verify it fails**

Run: `python3.12 -m unittest server.tests.test_shadergraph_asset.ShaderGraphAssetRequestNormalizationTests.test_request_normalization_accepts_add_node_property_binding_aliases`
Expected: FAIL because `propertyDisplayName` / `propertyReferenceName` are not normalized into the request payload.

- [ ] **Step 3: Write minimal implementation**

Update `server/src/unity_shader_graph_mcp/tools/shadergraph_asset.py` so `add_node` normalization accepts:

- `propertyDisplayName` / `property_display_name`
- `propertyReferenceName` / `property_reference_name`

and stores the canonical bridge-facing keys in `request.payload`.

- [ ] **Step 4: Run test to verify it passes**

Run: `python3.12 -m unittest server.tests.test_shadergraph_asset.ShaderGraphAssetRequestNormalizationTests.test_request_normalization_accepts_add_node_property_binding_aliases`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add server/src/unity_shader_graph_mcp/tools/shadergraph_asset.py server/tests/test_shadergraph_asset.py
git commit -m "test: normalize add_node property binding aliases"
```

### Task 2: Add the failing Unity request-parser test

**Files:**
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/ShaderGraphBatchmodeBridgeTests.cs`
- Modify: `packages/unity-shader-graph-mcp/Editor/Models/ShaderGraphRequests.cs`
- Modify: `packages/unity-shader-graph-mcp/Editor/Tools/ShaderGraphBatchmodeBridge.cs`

- [ ] **Step 1: Write the failing test**

```csharp
[Test]
public void TryParseRequest_ReturnsAddNodeRequest_WithPropertyBindingFields()
{
    string json = @"{
        ""tool"": ""shadergraph_asset"",
        ""action"": ""add_node"",
        ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
        ""nodeType"": ""Property"",
        ""propertyName"": ""Tint"",
        ""propertyDisplayName"": ""Base Tint"",
        ""referenceName"": ""_BaseTint"",
        ""propertyType"": ""Color""
    }";

    Assert.That(
        ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
        Is.True,
        errorMessage);

    var addNodeRequest = request as AddNodeRequest;
    Assert.That(addNodeRequest, Is.Not.Null);
    Assert.That(addNodeRequest.PropertyName, Is.EqualTo("Tint"));
    Assert.That(addNodeRequest.PropertyDisplayName, Is.EqualTo("Base Tint"));
    Assert.That(addNodeRequest.ReferenceName, Is.EqualTo("_BaseTint"));
    Assert.That(addNodeRequest.PropertyType, Is.EqualTo("Color"));
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `Unity Test Runner EditMode -> ShaderGraphBatchmodeBridgeTests`
Expected: compile/test failure because `AddNodeRequest` does not yet expose the property-binding members.

- [ ] **Step 3: Write minimal implementation**

Extend `AddNodeRequest` and `ShaderGraphBatchmodeBridge.TryCreateAddNodeRequest(...)` with the new optional property-binding fields.

- [ ] **Step 4: Run test to verify it passes**

Run: `Unity Test Runner EditMode -> ShaderGraphBatchmodeBridgeTests`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add packages/unity-shader-graph-mcp/Editor/Models/ShaderGraphRequests.cs packages/unity-shader-graph-mcp/Editor/Tools/ShaderGraphBatchmodeBridge.cs packages/unity-shader-graph-mcp/Tests/Editor/ShaderGraphBatchmodeBridgeTests.cs
git commit -m "test: parse property-bound add_node requests"
```

### Task 3: Add the failing package-backed smoke test

**Files:**
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/ShaderGraphPackageBackedSmokeTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
[Test]
public void BlankGraph_AddPropertyNodeBoundToExistingProperty_StaysPackageBacked()
{
    string assetPath = CreateBlankGraph("BlankGraphAddPropertyNodeBound", out _);

    ShaderGraphResponse addPropertyResponse = ShaderGraphAssetTool.HandleAddProperty(
        assetPath,
        "Tint",
        "Color",
        "#FFFFFFFF");
    ShaderGraphTestAssets.RequirePackageReady(addPropertyResponse);

    ShaderGraphResponse addNodeResponse = ShaderGraphAssetTool.Handle(
        new AddNodeRequest(
            assetPath,
            "Property",
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            "Tint",
            null,
            null,
            "Color"));
    ShaderGraphTestAssets.RequirePackageReady(addNodeResponse);

    var addedNode = ShaderGraphTestAssets.RequireDictionary(addNodeResponse.Data, "addedNode");
    Assert.That(ShaderGraphTestAssets.GetString(addedNode, "resolvedNodeType"), Is.EqualTo("Property"));

    var propertyBinding = ShaderGraphTestAssets.RequireDictionary(addNodeResponse.Data, "propertyBinding");
    var boundProperty = ShaderGraphTestAssets.RequireDictionary(propertyBinding, "boundProperty");
    Assert.That(ShaderGraphTestAssets.GetString(boundProperty, "displayName"), Is.EqualTo("Tint"));
    Assert.That(ShaderGraphTestAssets.GetString(boundProperty, "resolvedPropertyType"), Is.EqualTo("Color"));
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `Unity Test Runner EditMode -> ShaderGraphPackageBackedSmokeTests.BlankGraph_AddPropertyNodeBoundToExistingProperty_StaysPackageBacked`
Expected: FAIL because `Property` is not yet supported by `add_node` and no property-binding metadata exists.

- [ ] **Step 3: Write minimal implementation**

In `ShaderGraphPackageBackend.cs`:

- resolve/bind `PropertyNode`
- add `propertyBinding` response metadata
- promote `Property` into the supported-node catalog through a property-aware probe

- [ ] **Step 4: Run test to verify it passes**

Run: `Unity Test Runner EditMode -> ShaderGraphPackageBackedSmokeTests.BlankGraph_AddPropertyNodeBoundToExistingProperty_StaysPackageBacked`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add packages/unity-shader-graph-mcp/Editor/Adapters/ShaderGraphPackageBackend.cs packages/unity-shader-graph-mcp/Tests/Editor/ShaderGraphPackageBackedSmokeTests.cs
git commit -m "feat: support property-bound add_node requests"
```

### Task 4: Verify the full touched surface

**Files:**
- Modify: `docs/superpowers/specs/2026-04-16-property-node-add-node-design.md`
- Modify: `docs/superpowers/plans/2026-04-16-property-node-add-node-plan.md`

- [ ] **Step 1: Run Python server tests**

Run: `python3.12 -m unittest discover -s server/tests -p 'test_*.py'`
Expected: PASS

- [ ] **Step 2: Run Unity EditMode coverage**

Run: `Unity Test Runner EditMode`
Expected: PASS, including the new `PropertyNode` smoke and parser coverage.

- [ ] **Step 3: Run a whitespace sanity check**

Run: `git diff --check`
Expected: no output

- [ ] **Step 4: Commit documentation follow-up if needed**

```bash
git add docs/superpowers/specs/2026-04-16-property-node-add-node-design.md docs/superpowers/plans/2026-04-16-property-node-add-node-plan.md
git commit -m "docs: capture property node add_node slice"
```
