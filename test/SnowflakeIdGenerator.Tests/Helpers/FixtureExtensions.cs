using AutoFixture;

namespace SnowflakeIdGenerator.Tests.Helpers
{
    public static class FixtureExtensions
    {
        public static int CreateRangeForInt(this IFixture fixture, int min, int max)
        {
            return fixture.Create<int>() % (max - min + 1) + min;
        }

        public static long CreateRangeForLong(this IFixture fixture, long min, long max)
        {
            return fixture.Create<long>() % (max - min + 1) + min;
        }
    }
}
