/*
 * Copyright 2023 FeatureProbe
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

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
    }

    [Fact]
    private void TestConfigSdkKeyNotEmpty()
    {
        Assert.Throws<ArgumentException>(() => new FPConfig.Builder().Build());
        Assert.Throws<ArgumentException>(() => new FPConfig.Builder().ServerSdkKey("").Build());
        Assert.Throws<ArgumentException>(() => new FPConfig.Builder().ServerSdkKey(" \n\t  ").Build());

        new FPConfig.Builder().ServerSdkKey("server-xxx").Build();
    }

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
                _testOutputHelper.WriteLine($"[started] scenario: {name}, case: {caseName}");

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
                    case "bool_detail":
                    {
                        var boolDetailRes = fpClient.BoolDetail(toggleKey, user, defaultValue.GetValue<bool>());
                        Assert.Equal(expectValue.GetValue<bool>(), boolDetailRes.Value);
                        break;
                    }
                    case "number_detail":
                    {
                        var numberDetailRes = fpClient.NumberDetail(toggleKey, user, defaultValue.GetValue<double>());
                        Assert.Equal(expectValue.GetValue<double>(), numberDetailRes.Value);
                        break;
                    }
                    case "json_detail":
                    {
                        var jsonDetailDefaultMap =
                            JsonSerializer.Deserialize<Dictionary<string, object>>(defaultValue.ToString())!;
                        var jsonDetailRes = fpClient.JsonDetail(toggleKey, user, jsonDetailDefaultMap);
                        var jsonExpectString = JsonSerializer.Serialize(expectValue);
                        var jsonResString = JsonSerializer.Serialize(jsonDetailRes.Value);
                        Assert.Equal(jsonExpectString, jsonResString);
                        break;
                    }
                    case "string_detail":
                    {
                        var stringDetailRes = fpClient.StringDetail(toggleKey, user, defaultValue.GetValue<string>());
                        _testOutputHelper.WriteLine(JsonSerializer.Serialize(stringDetailRes));
                        Assert.Equal(expectValue.GetValue<string>(), stringDetailRes.Value);
                        break;
                    }
                }

                _testOutputHelper.WriteLine($"[passed] scenario: {name}, case: {caseName}");
            }
        }
    }
}
