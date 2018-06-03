// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

// https://github.com/Azure/azure-iot-sdk-csharp/blob/cd93e75fd8914b6247f23eb6a5cfd7aea459676f/iothub/service/src/Common/PerfectHash.cs

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
using System.Text;

namespace NickDarvey.ServiceFabric.EventHubs
{
    internal static class PerfectHash
    {
        public static long HashToLong(string data)
        {
#if NETSTANDARD1_3
            var upper = data.ToUpper();
#else
            var upper = data.ToUpper(CultureInfo.InvariantCulture);
#endif
            ComputeHash(Encoding.ASCII.GetBytes(upper), seed1: 0, seed2: 0, hash1: out var hash1, hash2: out var hash2);
            var hashedValue = ((long)hash1 << 32) | hash2;

            return hashedValue;
        }

        public static short HashToShort(string data)
        {
#if NETSTANDARD1_3
            var upper = data.ToUpper();
#else
            string upper = data.ToUpper(CultureInfo.InvariantCulture);
#endif
            PerfectHash.ComputeHash(ASCIIEncoding.ASCII.GetBytes(upper), seed1: 0, seed2: 0, hash1: out var hash1, hash2: out var hash2);
            long hashedValue = hash1 ^ hash2;

            return (short)hashedValue;
        }

        // Perfect hashing implementation. source: distributed cache team
        static void ComputeHash(byte[] data, uint seed1, uint seed2, out uint hash1, out uint hash2)
        {
            uint a, b, c;

            a = b = c = (uint)(0xdeadbeef + data.Length + seed1);
            c += seed2;

            int index = 0, size = data.Length;
            while (size > 12)
            {
                a += BitConverter.ToUInt32(data, index);
                b += BitConverter.ToUInt32(data, index + 4);
                c += BitConverter.ToUInt32(data, index + 8);

                a -= c;
                a ^= (c << 4) | (c >> 28);
                c += b;

                b -= a;
                b ^= (a << 6) | (a >> 26);
                a += c;

                c -= b;
                c ^= (b << 8) | (b >> 24);
                b += a;

                a -= c;
                a ^= (c << 16) | (c >> 16);
                c += b;

                b -= a;
                b ^= (a << 19) | (a >> 13);
                a += c;

                c -= b;
                c ^= (b << 4) | (b >> 28);
                b += a;

                index += 12;
                size -= 12;
            }

            switch (size)
            {
                case 12:
                    a += BitConverter.ToUInt32(data, index);
                    b += BitConverter.ToUInt32(data, index + 4);
                    c += BitConverter.ToUInt32(data, index + 8);
                    break;
                case 11:
                    c += ((uint)data[index + 10]) << 16;
                    goto case 10;
                case 10:
                    c += ((uint)data[index + 9]) << 8;
                    goto case 9;
                case 9:
                    c += (uint)data[index + 8];
                    goto case 8;
                case 8:
                    b += BitConverter.ToUInt32(data, index + 4);
                    a += BitConverter.ToUInt32(data, index);
                    break;
                case 7:
                    b += ((uint)data[index + 6]) << 16;
                    goto case 6;
                case 6:
                    b += ((uint)data[index + 5]) << 8;
                    goto case 5;
                case 5:
                    b += ((uint)data[index + 4]);
                    goto case 4;
                case 4:
                    a += BitConverter.ToUInt32(data, index);
                    break;
                case 3:
                    a += ((uint)data[index + 2]) << 16;
                    goto case 2;
                case 2:
                    a += ((uint)data[index + 1]) << 8;
                    goto case 1;
                case 1:
                    a += (uint)data[index];
                    break;
                case 0:
                    hash1 = c;
                    hash2 = b;
                    return;
            }

            c ^= b;
            c -= (b << 14) | (b >> 18);

            a ^= c;
            a -= (c << 11) | (c >> 21);

            b ^= a;
            b -= (a << 25) | (a >> 7);

            c ^= b;
            c -= (b << 16) | (b >> 16);

            a ^= c;
            a -= (c << 4) | (c >> 28);

            b ^= a;
            b -= (a << 14) | (a >> 18);

            c ^= b;
            c -= (b << 24) | (b >> 8);

            hash1 = c;
            hash2 = b;
        }
    }

}
