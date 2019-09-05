using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using AutoFixture;
using FluentAssertions;
using SnowflakeIdGenerator.Constants;
using SnowflakeIdGenerator.Core;
using SnowflakeIdGenerator.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace SnowflakeIdGenerator.Tests.Core
{
    public class GeneratorTests : IDisposable
    {
        private const long NODE_ID_MASK = 0x00000000003ff000L;
        private IFixture _fixture;

        private readonly ITestOutputHelper _output;

        public GeneratorTests(ITestOutputHelper output)
        {
            _fixture = new Fixture();
            _output = output;
        }

        [Fact]
        public void ShouldThrowOutOfRangeExceptionForNodeId()
        {
            // Act
            Action result = () => new Generator(_fixture.CreateRangeForInt(IdConstants.Limits.MAX_NODE_ID, int.MaxValue));

            //Assert
            result.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void ShouldReturnValidId()
        {
            // Arrange
            var subjectUnderTest = new Generator(_fixture.CreateRangeForInt(0, IdConstants.Limits.MAX_NODE_ID));
            
            // Act
            var result = subjectUnderTest.GenerateNextId();
            
            //Assert
            result.Should().BeGreaterThan(0);
        }

        [Fact]
        public void ShouldReturnValidIdWithCorrectNodeID()
        {
            // Arrange
            var anyValidNodeId = _fixture.CreateRangeForInt(0, IdConstants.Limits.MAX_NODE_ID);
            var subjectUnderTest = new Generator(anyValidNodeId);
           
            // Act
            var anyValidId = subjectUnderTest.GenerateNextId();
            var result = (anyValidId & NODE_ID_MASK) >> IdConstants.IdBits.SEQUENCE_BITS;

            //Assert
            result.Should().Be(anyValidNodeId);
        }

        [Fact]
        public void ShouldReturnValidIdWithCorrectNodeIDV2()
        {
            // Arrange
            var anyValidNodeId = _fixture.CreateRangeForInt(0, IdConstants.Limits.MAX_NODE_ID);
            var subjectUnderTest = new Generator(anyValidNodeId);

            // Act
            var anyValidId = subjectUnderTest.GenerateNextId();
            var removedTimeShift = BitConverter.GetBytes(anyValidId << IdConstants.IdBits.EPOCH_BITS);
            int result = (removedTimeShift[removedTimeShift.Length - 1] << 8 | removedTimeShift[removedTimeShift.Length - 2]) >> 6;
            
            //Assert
            result.Should().Be(anyValidNodeId);
        }

        [Fact]
        public void ShouldGenerateUniqueIds()
        {
            // Arrange
            var numberOfIds = 2000000;
            var IdCollection = new HashSet<long>(numberOfIds);
            var anyNodeId = _fixture.CreateRangeForInt(0, IdConstants.Limits.MAX_NODE_ID);
            var subjectUnderTest = new Generator(anyNodeId);
            var lockObj = new Object();

            // Act
            Parallel.For(0, numberOfIds, (index) =>
            {
                lock(lockObj)
                {
                    IdCollection.Add(subjectUnderTest.GenerateNextId());
                }
            });

            //Assert
            IdCollection.Count.Should().Be(numberOfIds);
        }

        [Fact]
        public void ShouldGenerateUniqueIdsV2()
        {
            // Arrange
            var numberOfIds = 2000000;
            var IdCollection = new HashSet<long>(numberOfIds);
            var subjectUnderTest = new Generator(_fixture.CreateRangeForInt(0, IdConstants.Limits.MAX_NODE_ID));
            var lockObj = new Object();
            var result = 0;

            // Act
            Parallel.For(0,
                numberOfIds,
                () => new List<long>()
                ,
                (index, loopState, partialResult) =>
                {
                    partialResult.Add(subjectUnderTest.GenerateNextId());
                    return partialResult;
                },
                (loopPartialResult) => {
                    lock (lockObj)
                    {
                        IdCollection.UnionWith(loopPartialResult);
                        result += loopPartialResult.Count;
                    }
                });

            //Assert
            result.Should().Be(numberOfIds);
            result.Should().Be(IdCollection.Count);
        }

        [Fact]
        public void ShouldGenerateSecondIdGreaterThanThePreviousOne()
        {
            // Arrange
            var subjectUnderTest = new Generator(_fixture.CreateRangeForInt(0, IdConstants.Limits.MAX_NODE_ID));

            // Act
            var resultFirstId = subjectUnderTest.GenerateNextId();
            var resultSecondId = subjectUnderTest.GenerateNextId();

            //Assert
            resultSecondId.Should().BeGreaterThan(resultFirstId);
        }

        [Fact]
        public void ShouldGenerateMillionIdsInSeconds()
        {
            // Arrange
            var iterations = 1000000;
            var subjectUnderTest = new Generator(_fixture.CreateRangeForInt(0, IdConstants.Limits.MAX_NODE_ID));

            // Act
            var stopwatch = Stopwatch.StartNew();
            Parallel.For(0, iterations, (index) => subjectUnderTest.GenerateNextId());
            stopwatch.Stop();

            _output.WriteLine("Duration to generate {1:n0} ids: {0:n0} ms", stopwatch.ElapsedMilliseconds, iterations);
            _output.WriteLine("Number of ids generated in 1 ms: {0:n0}", iterations / stopwatch.ElapsedMilliseconds);
            _output.WriteLine("Number of ids generated in 1 s: {0:n0}", (int)(iterations / (stopwatch.ElapsedMilliseconds / 1000.0)));
        }

        [Fact]
        public void PerformanceTestWithTenMillionUniqueIdsWithHundredDifferentGenerators()
        {
            const int number = 1000000;
            const int machines = 100;

            var stopwatch = Stopwatch.StartNew();
            Parallel.For(0, machines, m => GetIds((short)m, number));
            stopwatch.Stop();
                        
            _output.WriteLine("Duration to generate {1:n0} ids: {0:n0} ms", stopwatch.ElapsedMilliseconds, number * machines);
            _output.WriteLine("Number of ids generated in 1 ms: {0:n0}", number * machines / stopwatch.ElapsedMilliseconds);
            _output.WriteLine("Number of ids generated in 1 s: {0:n0}", (int)(number * machines / (stopwatch.ElapsedMilliseconds / 1000.0)));
        }

        [Fact]
        public void PerformanceTestWithTenMillionUniqueIdsWithSingleGenerator()
        {
            const int number = 10000000;

            var stopwatch = Stopwatch.StartNew();
            var ids = GetIds(0, number);
            stopwatch.Stop();

            ids.Count.Should().Be(number);

            _output.WriteLine("Duration to generate {1:n0} ids: {0:n0} ms", stopwatch.ElapsedMilliseconds, ids.Count);
            _output.WriteLine("Number of ids generated in 1 ms: {0:n0}", ids.Count / stopwatch.ElapsedMilliseconds);
            _output.WriteLine("Number of ids generated in 1 s: {0:n0}", (int)(ids.Count / (stopwatch.ElapsedMilliseconds / 1000.0)));
        }

        private ICollection<long> GetIds(short machineId, int number)
        {
            var generator = new Generator(machineId);
            var ids = new HashSet<long>(number);

            for (int i = 0; i < number; i++)
            {
                ids.Add(generator.GenerateNextId());
            }

            return ids;
        }
        
        public void Dispose()
        {
            if (_fixture != null)
                _fixture = null;
        }
    }
}
