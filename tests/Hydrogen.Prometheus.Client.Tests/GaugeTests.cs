using System;
using Xunit;

namespace Hydrogen.Prometheus.Client.Tests
{
    public class GaugeTests
    {
        private CollectorRegistry _registry = new CollectorRegistry();
        private Gauge _gaugeNoLabels;

        public GaugeTests()
        {
            _gaugeNoLabels = Gauge.Build("name", "help").Register(_registry);
        }

        [Fact]
        public void Increment_ShouldIncrementBy1()
        {
            Assert.Equal(0d, _gaugeNoLabels.Labels().Value);

            _gaugeNoLabels.Labels().Increment();

            Assert.Equal(1d, _gaugeNoLabels.Labels().Value);
        }

        [Fact]
        public void Decrement_ShouldIncrementBy1()
        {
            Assert.Equal(0d, _gaugeNoLabels.Labels().Value);

            _gaugeNoLabels.Labels().Decrement();

            Assert.Equal(-1d, _gaugeNoLabels.Labels().Value);
        }

        [Fact]
        public void Increment_ShouldIncrementByAmount()
        {
            Assert.Equal(0d, _gaugeNoLabels.Labels().Value);

            _gaugeNoLabels.Labels().Increment(5);
            Assert.Equal(5d, _gaugeNoLabels.Labels().Value);

            _gaugeNoLabels.Labels().Increment(0);
            Assert.Equal(5d, _gaugeNoLabels.Labels().Value);

            _gaugeNoLabels.Labels().Increment(-1);
            Assert.Equal(4d, _gaugeNoLabels.Labels().Value);
        }

        [Fact]
        public void Decrement_ShouldIncrementByAmount()
        {
            Assert.Equal(0d, _gaugeNoLabels.Labels().Value);

            _gaugeNoLabels.Labels().Decrement(5);
            Assert.Equal(-5d, _gaugeNoLabels.Labels().Value);

            _gaugeNoLabels.Labels().Decrement(0);
            Assert.Equal(-5d, _gaugeNoLabels.Labels().Value);

            _gaugeNoLabels.Labels().Decrement(-1);
            Assert.Equal(-4d, _gaugeNoLabels.Labels().Value);
        }

        [Fact]
        public void Set_ShouldSetValue()
        {
            Assert.Equal(0d, _gaugeNoLabels.Labels().Value);

            _gaugeNoLabels.Labels().Set(5);
            Assert.Equal(5d, _gaugeNoLabels.Labels().Value);

            _gaugeNoLabels.Labels().Set(0);
            Assert.Equal(0d, _gaugeNoLabels.Labels().Value);

            _gaugeNoLabels.Labels().Set(-1);
            Assert.Equal(-1d, _gaugeNoLabels.Labels().Value);
        }

        [Fact]
        public void SetToCurrentTime_ShouldSetToCurrentTime()
        {
            var nowSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            _gaugeNoLabels.Labels().SetToCurrentTime();

            Assert.Equal(nowSeconds, _gaugeNoLabels.Labels().Value, 01);
        }
    }
}
