// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

// https://github.com/Azure/azure-iot-sdk-csharp/blob/cd93e75fd8914b6247f23eb6a5cfd7aea459676f/iothub/service/src/Common/EventHubPartitionKeyResolver.cs

// Microsoft Azure IoT SDKs 
// Copyright (c) Microsoft Corporation
// All rights reserved. 
// MIT License
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the ""Software""), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE.

using System;
using System.Globalization;

namespace NickDarvey.ServiceFabric.EventHubs
{
    public static class EventHubPartitionKeyResolver
    {
        const short DefaultLogicalPartitionCount = short.MaxValue;

        public static string ResolveToPartition(string partitionKey, int partitionCount)
        {
            if (string.IsNullOrWhiteSpace(partitionKey))
            {
                throw new ArgumentNullException("partitionKey");
            }

            if (partitionCount < 1 || partitionCount > DefaultLogicalPartitionCount)
            {
                throw new ArgumentOutOfRangeException("partitionCount", partitionCount, string.Format(CultureInfo.InvariantCulture, "Should be between {0} and {1}", 1, DefaultLogicalPartitionCount));
            }

            var logicalPartition = Math.Abs((short)(PerfectHash.HashToShort(partitionKey) % DefaultLogicalPartitionCount));

            var shortRangeWidth = (int)Math.Floor((decimal)DefaultLogicalPartitionCount / (decimal)(partitionCount));
            var remainingLogicalPartitions = DefaultLogicalPartitionCount - (partitionCount * shortRangeWidth);
            var largeRangeWidth = shortRangeWidth + 1;
            var largeRangesLogicalPartitions = largeRangeWidth * remainingLogicalPartitions;
            var partitionIndex = logicalPartition < largeRangesLogicalPartitions
                ? logicalPartition / largeRangeWidth
                : remainingLogicalPartitions + ((logicalPartition - largeRangesLogicalPartitions) / shortRangeWidth);

            return partitionIndex.ToString(NumberFormatInfo.InvariantInfo);
        }
    }
}
