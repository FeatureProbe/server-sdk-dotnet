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

using System.Text.Json.Serialization;
using FeatureProbe.Server.Sdk.Results;

namespace FeatureProbe.Server.Sdk.Models;

public class Serve
{
    [JsonPropertyName("select")] public int? Select { get; set; }

    [JsonPropertyName("split")] public Split Split { get; set; }

    public HitResult EvalIndex(FPUser user, String toggleKey)
    {
        if (Select is not null)
        {
            return new HitResult(true, Select);
        }

        return Split.FindIndex(user, toggleKey);
    }
}
