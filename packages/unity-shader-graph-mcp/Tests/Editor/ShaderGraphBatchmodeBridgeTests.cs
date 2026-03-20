using NUnit.Framework;
using ShaderGraphMcp.Editor.Models;
using ShaderGraphMcp.Editor.Tools;

namespace ShaderGraphMcp.Editor.Tests
{
    public sealed class ShaderGraphBatchmodeBridgeTests
    {
        [Test]
        public void TryParseInvocation_AcceptsSeparateAndEqualsArguments()
        {
            string[] args =
            {
                "/Applications/Unity/Unity.app",
                "-batchmode",
                "-executeMethod",
                "ShaderGraphMcp.Editor.Batchmode.ShaderGraphBatchEntryPoint.Run",
                "-shaderGraphMcpRequestPath",
                "/tmp/shadergraph-request.json",
                "-shaderGraphMcpResponsePath=/tmp/shadergraph-response.json",
            };

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseInvocation(args, out var invocation, out string errorMessage),
                Is.True,
                errorMessage);
            Assert.That(invocation.RequestFilePath, Is.EqualTo("/tmp/shadergraph-request.json"));
            Assert.That(invocation.ResponseFilePath, Is.EqualTo("/tmp/shadergraph-response.json"));
        }

        [Test]
        public void TryParseInvocation_AcceptsLegacyBatchmodeArgumentNames()
        {
            string[] args =
            {
                "/Applications/Unity/Unity.app",
                "--shadergraph-mcp-request-file=/tmp/legacy-request.json",
                "--shadergraph-mcp-response-file",
                "/tmp/legacy-response.json",
            };

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseInvocation(args, out var invocation, out string errorMessage),
                Is.True,
                errorMessage);
            Assert.That(invocation.RequestFilePath, Is.EqualTo("/tmp/legacy-request.json"));
            Assert.That(invocation.ResponseFilePath, Is.EqualTo("/tmp/legacy-response.json"));
        }

        [Test]
        public void TryParseRequest_ReturnsCreateGraphRequest_FromAssetPathPayload()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""create_graph"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""template"": ""blank""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var createGraphRequest = request as CreateGraphRequest;
            Assert.That(createGraphRequest, Is.Not.Null);
            Assert.That(createGraphRequest.Name, Is.EqualTo("ExampleLitGraph"));
            Assert.That(createGraphRequest.Path, Is.EqualTo("Assets/ShaderGraphs"));
            Assert.That(createGraphRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(createGraphRequest.Template, Is.EqualTo("blank"));
        }

        [Test]
        public void TryParseRequest_ReturnsReadGraphSummaryRequest_FromAssetPathPayload()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""read_graph_summary"",
                ""path"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var summaryRequest = request as ReadGraphSummaryRequest;
            Assert.That(summaryRequest, Is.Not.Null);
            Assert.That(summaryRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
        }

        [Test]
        public void TryParseRequest_ReturnsConnectPortsRequest_FromAliasFields()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""connect_ports"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""sourceNodeId"": ""source-node"",
                ""sourcePort"": ""Out"",
                ""targetNodeId"": ""target-node"",
                ""targetPort"": ""X""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var connectPortsRequest = request as ConnectPortsRequest;
            Assert.That(connectPortsRequest, Is.Not.Null);
            Assert.That(connectPortsRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(connectPortsRequest.OutputNodeId, Is.EqualTo("source-node"));
            Assert.That(connectPortsRequest.OutputPort, Is.EqualTo("Out"));
            Assert.That(connectPortsRequest.InputNodeId, Is.EqualTo("target-node"));
            Assert.That(connectPortsRequest.InputPort, Is.EqualTo("X"));
        }

        [Test]
        public void SerializeResponse_WritesStableOrderedJson()
        {
            var response = ShaderGraphResponse.Ok(
                "done",
                new System.Collections.Generic.Dictionary<string, object>
                {
                    ["z"] = 3,
                    ["a"] = new System.Collections.Generic.Dictionary<string, object>
                    {
                        ["b"] = 2,
                        ["a"] = "x",
                    },
                    ["list"] = new object[] { "one", 2, true },
                });

            string json = ShaderGraphBatchmodeBridge.SerializeResponse(response);

            Assert.That(
                json,
                Is.EqualTo("{\"success\":true,\"message\":\"done\",\"data\":{\"a\":{\"a\":\"x\",\"b\":2},\"list\":[\"one\",2,true],\"z\":3}}"));
        }
    }
}
