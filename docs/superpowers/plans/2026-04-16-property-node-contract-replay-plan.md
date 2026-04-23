# Property Node Contract Replay Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Keep `PropertyNode` bindings intact when exporting and re-importing graph contracts through the package-backed backend.

**Architecture:** Extend the exported node contract with additive property-binding fields and thread those fields back through `ImportedGraphContractNode` into the existing `AddNodeRequest` property-binding path. Reuse the already-shipped `add_node` `PropertyNode` binding logic instead of creating a separate import-only path.

**Tech Stack:** Unity Editor C#, NUnit EditMode tests, package-backed Shader Graph reflection helpers.

---

### Task 1: Add the failing export/import smoke tests

**Files:**
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/ShaderGraphPackageBackedSmokeTests.cs`

- [ ] **Step 1: Write the failing export smoke test**

```csharp
[Test]
public void BlankGraph_ExportGraphContract_IncludesPropertyNodeBinding_StaysPackageBacked()
{
    string assetPath = CreateBlankGraph("BlankGraphExportPropertyNodeContract", out _);

    ShaderGraphTestAssets.RequirePackageReady(
        ShaderGraphAssetTool.HandleAddProperty(assetPath, "Tint", "Color", "#FFFFFFFF"));

    ShaderGraphResponse addPropertyNodeResponse = ShaderGraphAssetTool.Handle(
        new AddNodeRequest(assetPath, "Property", null, null, null, null, null, null, null, null, null, "Tint", null, null, "Color"));
    ShaderGraphTestAssets.RequirePackageReady(addPropertyNodeResponse);

    ShaderGraphResponse exportResponse = ShaderGraphAssetTool.HandleExportGraphContract(assetPath);
    ShaderGraphTestAssets.RequirePackageReady(exportResponse);

    var exportedGraphContract = ShaderGraphTestAssets.RequireDictionary(exportResponse.Data, "exportedGraphContract");
    var nodes = ShaderGraphTestAssets.RequireList(exportedGraphContract, "nodes");
    var exportedPropertyNode = ShaderGraphTestAssets.RequireDictionary(nodes[0]);
    Assert.That(ShaderGraphTestAssets.GetString(exportedPropertyNode, "nodeType"), Is.EqualTo("Property"));
    Assert.That(ShaderGraphTestAssets.GetString(exportedPropertyNode, "propertyName"), Is.EqualTo("Tint"));
    Assert.That(ShaderGraphTestAssets.GetString(exportedPropertyNode, "propertyType"), Is.EqualTo("Color"));
}
```

- [ ] **Step 2: Write the failing import smoke test**

```csharp
[Test]
public void BlankGraph_ImportGraphContract_ReplaysPropertyNodeBinding_StaysPackageBacked()
{
    string sourceAssetPath = CreateBlankGraph("BlankGraphImportPropertyNodeContractSource", out _);

    ShaderGraphTestAssets.RequirePackageReady(
        ShaderGraphAssetTool.HandleAddProperty(sourceAssetPath, "Tint", "Color", "#FFFFFFFF"));

    ShaderGraphResponse addPropertyNodeResponse = ShaderGraphAssetTool.Handle(
        new AddNodeRequest(sourceAssetPath, "Property", null, null, null, null, null, null, null, null, null, "Tint", null, null, "Color"));
    ShaderGraphTestAssets.RequirePackageReady(addPropertyNodeResponse);
    string propertyNodeId = ShaderGraphTestAssets.GetAddedNodeId(addPropertyNodeResponse);

    ShaderGraphResponse addSplitResponse = ShaderGraphAssetTool.HandleAddNode(sourceAssetPath, "Split", "Imported Split");
    ShaderGraphTestAssets.RequirePackageReady(addSplitResponse);
    string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSplitResponse);

    ShaderGraphTestAssets.RequirePackageReady(
        ShaderGraphAssetTool.HandleConnectPorts(sourceAssetPath, propertyNodeId, "Out", splitNodeId, "In"));

    IReadOnlyDictionary<string, object> exportedGraphContract =
        ShaderGraphTestAssets.RequireDictionary(
            ShaderGraphAssetTool.HandleExportGraphContract(sourceAssetPath).Data,
            "exportedGraphContract");

    string targetAssetPath = CreateBlankGraph("BlankGraphImportPropertyNodeContractTarget", out _);
    ShaderGraphResponse importResponse = ShaderGraphAssetTool.HandleImportGraphContract(
        targetAssetPath,
        ShaderGraphTestAssets.SerializeToJson(exportedGraphContract));
    ShaderGraphTestAssets.RequirePackageReady(importResponse);

    ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(targetAssetPath);
    ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
    Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "propertyCount"), Is.EqualTo(1));
    Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(2));
    Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(1));
}
```

- [ ] **Step 3: Run tests to verify they fail**

Run: `Unity Test Runner EditMode -> ShaderGraphPackageBackedSmokeTests`
Expected: FAIL because export does not yet include `PropertyNode` binding fields and import does not replay them.

- [ ] **Step 4: Write minimal implementation**

Update:

- `ImportedGraphContractNode`
- `BuildExportedNodeContractData(...)`
- `ImportGraphContract(...)`

so bound `PropertyNode` fields round-trip into `AddNodeRequest`.

- [ ] **Step 5: Run the targeted smoke tests again**

Run: `Unity Test Runner EditMode -> ShaderGraphPackageBackedSmokeTests`
Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add packages/unity-shader-graph-mcp/Editor/Adapters/ShaderGraphPackageBackend.cs packages/unity-shader-graph-mcp/Editor/Models/ShaderGraphRequests.cs packages/unity-shader-graph-mcp/Tests/Editor/ShaderGraphPackageBackedSmokeTests.cs
git commit -m "feat: replay property node bindings in graph contracts"
```

### Task 2: Add metadata coverage

**Files:**
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/ShaderGraphResponseMetadataTests.cs`

- [ ] **Step 1: Write the failing metadata test**

```csharp
[Test]
public void Ok_PreservesPropertyNodeBindingInExportGraphContractEnvelope()
{
    var response = ShaderGraphResponse.Ok(
        "Exported package-backed Shader Graph contract.",
        new Dictionary<string, object>
        {
            ["operation"] = "export_graph_contract",
            ["exportedGraphContract"] = new Dictionary<string, object>
            {
                ["nodes"] = new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["nodeType"] = "Property",
                        ["propertyName"] = "Tint",
                        ["propertyDisplayName"] = "Tint",
                        ["referenceName"] = "_Tint",
                        ["propertyType"] = "Color",
                    },
                },
            },
        });

    var exportedGraphContract = (IReadOnlyDictionary<string, object>)response.Data["exportedGraphContract"];
    var nodes = (object[])exportedGraphContract["nodes"];
    var propertyNode = (IReadOnlyDictionary<string, object>)nodes[0];
    Assert.That(propertyNode["propertyName"], Is.EqualTo("Tint"));
    Assert.That(propertyNode["referenceName"], Is.EqualTo("_Tint"));
}
```

- [ ] **Step 2: Run the metadata test to verify it fails first**

Run: `Unity Test Runner EditMode -> ShaderGraphResponseMetadataTests`
Expected: FAIL until the envelope fixture includes the new fields.

- [ ] **Step 3: Update the response metadata fixture**

Add the new property-binding fields to the export envelope fixture for `PropertyNode`.

- [ ] **Step 4: Run the metadata test again**

Run: `Unity Test Runner EditMode -> ShaderGraphResponseMetadataTests`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add packages/unity-shader-graph-mcp/Tests/Editor/ShaderGraphResponseMetadataTests.cs
git commit -m "test: lock property node contract metadata"
```

### Task 3: Final verification

**Files:**
- Modify: `docs/superpowers/specs/2026-04-16-property-node-contract-replay-design.md`
- Modify: `docs/superpowers/plans/2026-04-16-property-node-contract-replay-plan.md`

- [ ] **Step 1: Run Unity EditMode suite**

Run: `Unity Test Runner EditMode`
Expected: PASS

- [ ] **Step 2: Run whitespace sanity**

Run: `git diff --check`
Expected: no output

- [ ] **Step 3: Commit docs follow-up if needed**

```bash
git add docs/superpowers/specs/2026-04-16-property-node-contract-replay-design.md docs/superpowers/plans/2026-04-16-property-node-contract-replay-plan.md
git commit -m "docs: capture property node contract replay slice"
```
