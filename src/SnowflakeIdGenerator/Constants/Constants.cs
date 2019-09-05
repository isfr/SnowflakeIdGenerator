using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("SnowflakeIdGenerator.Tests")]
namespace SnowflakeIdGenerator.Constants
{
    internal static class IdConstants
    {
        internal static class IdBits
        {
            internal const int TOTAL_BITS = 64;

            internal const int EPOCH_BITS = 42;

            internal const int NODE_ID_BITS = 10;

            internal const int SEQUENCE_BITS = 12;

        }

        internal static class Limits
        {
            internal static readonly int MAX_NODE_ID = (int) (Math.Pow(2, IdBits.NODE_ID_BITS) - 1);

            internal static readonly int MAX_SEQUENCE = (int)(Math.Pow(2, IdBits.SEQUENCE_BITS) - 1);
        }

        /// <summary>
        /// The custom epoch time, measured in miliseconds. Represents 2019-01-01T00:00:00Z
        /// </summary>
        internal const long CUSTOM_EPOCH = 1546300800000L;
        
        internal static readonly DateTime CUSTOM_DATE = new DateTime(2019, 01, 01);
    }
}
