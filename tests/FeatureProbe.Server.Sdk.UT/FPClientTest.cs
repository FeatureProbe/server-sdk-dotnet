using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using FeatureProbe.Server.Sdk.DataRepositories;
using FeatureProbe.Server.Sdk.Models;
using Xunit.Abstractions;

namespace FeatureProbe.Server.Sdk.UT;

public class FPClientTest
{
    private readonly JsonNode _testCase;
    private readonly ITestOutputHelper _testOutputHelper;

    public FPClientTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;

        var data = File.ReadAllText("resources/test/spec/toggle_simple_spec.json");
        _testCase = JsonSerializer.Deserialize<JsonNode>(data)!;

        _testOutputHelper.WriteLine(data);
    }

    [Fact]
    private void TestConfigSdkKeyNotEmpty()
    {
        Assert.Throws<ArgumentException>(() => new FPConfig.Builder().Build());
        Assert.Throws<ArgumentException>(() => new FPConfig.Builder().ServerSdkKey("").Build());
        Assert.Throws<ArgumentException>(() => new FPConfig.Builder().ServerSdkKey(" \n\t  ").Build());

        new FPConfig.Builder().ServerSdkKey("server-xxx").Build();
    }

    // groovy
    /*
             def tests = testCase.get("tests").asList()
        for (int i = 0; i < tests.size(); i++) {
            def scenario = tests.get(i)
            def name = scenario.get("scenario").asText()
            def fixture = scenario.get("fixture")
            def dataRepository = new MemoryDataRepository()
            def repository = mapper.readValue(fixture.toPrettyString(), Repository.class)
            dataRepository.refresh(repository)
            featureProbe = new FeatureProbe(dataRepository);
            def cases = scenario.get("cases")
            for (int j = 0; j < cases.size(); j++) {
                def testCase = cases.get(j)
                def caseName = testCase.get("name").asText()
                println("starting execute scenario : " + name + ",case : " + caseName)
                def userCase = testCase.get("user")
                FPUser user = new FPUser().stableRollout(userCase.get("key").asText())
                def customValues = userCase.get("customValues").asList()
                for (int x = 0; x < customValues.size(); x++) {
                    def customValue = customValues.get(x)
                    user.with(customValue.get("key").asText(), customValue.get("value").asText())
                }
                def functionCase = testCase.get("function")
                def functionName = functionCase.get("name").asText()
                def toggleKey = functionCase.get("toggle").asText()
                def expectResult = testCase.get("expectResult")
                def defaultValue = functionCase.get("default")
                def expectValue = expectResult.get("value")
                switch (functionName) {
                    case "bool_value":
                        def boolRes = featureProbe.boolValue(toggleKey, user, defaultValue.asBoolean())
                        assert boolRes == expectValue.asBoolean()
                        break
                    case "string_value":
                        def stringRes = featureProbe.stringValue(toggleKey, user, defaultValue.asText())
                        assert stringRes == expectValue.asText()
                        break
                    case "number_value":
                        def numberRes = featureProbe.numberValue(toggleKey, user, defaultValue.asDouble())
                        assert numberRes == expectValue.asDouble()
                        break
                    case "json_value":
                        def jsonDefaultMap = mapper.readValue(defaultValue.toPrettyString(), Map.class)
                        def jsonRres = featureProbe.jsonValue(toggleKey, user, jsonDefaultMap, Map.class)
                        def jsonExpectString = mapper.writeValueAsString(expectValue);
                        def jsonResString = mapper.writeValueAsString(jsonRres)
                        assert jsonExpectString == jsonResString
                        break
                    case "bool_detail":
                        def boolDetailRes = featureProbe.boolDetail(toggleKey, user,
                                defaultValue.asBoolean())
                        def detailStr = boolDetailRes.toString()
                        assert boolDetailRes.value == expectValue.asBoolean()
                        break
                    case "number_detail":
                        def numberDetailRes = featureProbe.numberDetail(toggleKey, user,
                                defaultValue.asDouble())
                        assert numberDetailRes.value == expectValue.asDouble()
                        break
                    case "json_detail":
                        def jsonDetailDefaultMap = mapper.readValue(defaultValue.toPrettyString(), Map.class)
                        def jsonDetailRes = featureProbe.jsonDetail(toggleKey, user,
                                jsonDetailDefaultMap, Map.class)
                        def jsonExpectString = mapper.writeValueAsString(expectValue)
                        def jsonResString = mapper.writeValueAsString(jsonDetailRes.value)
                        assert jsonExpectString == jsonResString
                        break
                    case "string_detail":
                        def stringDetailRes = featureProbe.stringDetail(toggleKey, user,
                                defaultValue.asText())
                        assert stringDetailRes.value == expectValue.asText()
                        break
                }
            }
        }

     */


    [Fact]
    private void TestFeatureProbeCases()
    {
        var tests = _testCase["tests"]!.AsArray();
        foreach (var scenario in tests)
        {
            var name = scenario!["scenario"]!.ToString();
            var fixture = scenario["fixture"]!;

            var dataRepository = new MemoryDataRepository();
            var repository = JsonSerializer.Deserialize<Repository>(fixture.ToString())!;
            dataRepository.Refresh(repository);

            var fpClient = new FPClient("server-xxx");
            var dataRepoField =
                typeof(FPClient).GetField("_dataRepository", BindingFlags.NonPublic | BindingFlags.Instance)!;
            dataRepoField.SetValue(fpClient, dataRepository);

            var cases = scenario["cases"]!.AsArray();
            foreach (var testCase in cases)
            {
                var caseName = testCase!["name"]!.ToString();
                _testOutputHelper.WriteLine($"starting execute scenario: {name}, case: {caseName}");

                var userCase = testCase["user"]!;
                var user = new FPUser().StableRollout(userCase["key"]!.ToString());
                var customValues = userCase["customValues"]!.AsArray();
                foreach (var customValue in customValues)
                {
                    user.With(customValue!["key"]!.ToString(), customValue["value"]!.ToString());
                }

                var functionCase = testCase["function"]!;
                var functionName = functionCase["name"]!.ToString();
                var toggleKey = functionCase["toggle"]!.ToString();
                var expectResult = testCase["expectResult"]!;
                var defaultValue = functionCase["default"]!;
                var expectValue = expectResult["value"]!;
                switch (functionName)
                {
                    case "bool_value":
                    {
                        var boolRes = fpClient.BoolValue(toggleKey, user, defaultValue.GetValue<bool>());
                        Assert.Equal(expectValue.GetValue<bool>(), boolRes);
                        break;
                    }
                    case "string_value":
                    {
                        var stringRes = fpClient.StringValue(toggleKey, user, defaultValue.GetValue<string>());
                        Assert.Equal(expectValue.GetValue<string>(), stringRes);
                        break;
                    }
                    case "number_value":
                    {
                        var numberRes = fpClient.NumberValue(toggleKey, user, defaultValue.GetValue<double>());
                        Assert.Equal(expectValue.GetValue<double>(), numberRes);
                        break;
                    }
                    case "json_value":
                    {
                        var jsonDefaultMap =
                            JsonSerializer.Deserialize<Dictionary<string, object>>(defaultValue.ToString())!;
                        var jsonRes = fpClient.JsonValue(toggleKey, user, jsonDefaultMap);
                        var jsonExpectString = JsonSerializer.Serialize(expectValue);
                        var jsonResString = JsonSerializer.Serialize(jsonRes);
                        Assert.Equal(jsonExpectString, jsonResString);
                        break;
                    }
                    // case "bool_detail":
                }
            }
        }
    }
}
