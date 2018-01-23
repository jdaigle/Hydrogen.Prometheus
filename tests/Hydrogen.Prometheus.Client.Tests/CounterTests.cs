using System;
using Xunit;

namespace Hydrogen.Prometheus.Client.Tests
{
    public class CounterTests
    {
        private CollectorRegistry _registry = new CollectorRegistry();
        private Counter _counterNoLabels;

        public CounterTests()
        {
            _counterNoLabels = Counter.Build("name", "help").Register(_registry);
        }

        [Fact]
        public void Increment_ShouldIncrementBy1()
        {
            Assert.Equal(0d, _counterNoLabels.Labels().Value);

            _counterNoLabels.Labels().Increment();

            Assert.Equal(1d, _counterNoLabels.Labels().Value);
        }

        [Fact]
        public void Increment_ShouldIncrementByAmount()
        {
            Assert.Equal(0d, _counterNoLabels.Labels().Value);

            _counterNoLabels.Labels().Increment(5);
            Assert.Equal(5d, _counterNoLabels.Labels().Value);

            _counterNoLabels.Labels().Increment(0);
            Assert.Equal(5d, _counterNoLabels.Labels().Value);
        }

        [Fact]
        public void Increment_ShouldRequireNonNegative()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _counterNoLabels.Labels().Increment(-1));
        }
    }
}
