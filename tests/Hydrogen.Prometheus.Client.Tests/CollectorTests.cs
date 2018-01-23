using System;
using Xunit;

namespace Hydrogen.Prometheus.Client.Tests
{
    public class CollectorTests
    {
        private CollectorRegistry _registry = new CollectorRegistry();
        private Counter _metric;

        public CollectorTests()
        {
            _metric = Counter.Build("name", "help").WithLabels("l").Register(_registry);
        }

        [Fact]
        public void CorrectLabelCount()
        {
            var c = _metric.Labels("a");
            Assert.NotNull(c);
        }

        [Fact]
        public void TooFewLabelsThrows()
        {
            Assert.Throws<ArgumentException>(() => _metric.Labels());
        }

        [Fact]
        public void NullLabelThrows()
        {
            // TODO: better exception
            Assert.ThrowsAny<Exception>(() => _metric.Labels(new string[] { null }));
        }

        [Fact]
        public void TooManyLabelsThrows()
        {
            Assert.Throws<ArgumentException>(() => _metric.Labels("a", "b"));
        }

        [Fact]
        public void Labels_ShouldReturnSameInstance()
        {
            var a = _metric.Labels("a");
            Assert.Equal(a, _metric.Labels("a"));

            var b = _metric.Labels("b");
            Assert.Equal(b, _metric.Labels("b"));

            Assert.NotEqual(a, b);
        }

        [Fact]
        public void Build_NameIsConcatenated()
        {
            Assert.Equal("a_b_c", Counter.Build().WithName("c").WithSubsystem("b").WithNamespace("a").WithHelp("h").Build().Name);
        }

        [Fact]
        public void Build_NameIsRequired()
        {
            Assert.Throws<ArgumentNullException>("Name", () => Counter.Build().WithHelp("h").Build());
        }

        [Fact]
        public void Build_HelpIsRequired()
        {
            Assert.Throws<ArgumentNullException>("Help", () => Counter.Build().WithName("n").Build());
        }

        [Fact]
        public void Build_InvalidNameThrows()
        {
            Assert.Throws<ArgumentException>(() => Counter.Build("a'''b", "h").Build());
        }

        [Fact]
        public void Build_InvalidLabelNameThrows()
        {
            Assert.Throws<ArgumentException>(() => Counter.Build("a", "h").WithLabels("a$").Build());
        }

        [Fact]
        public void Build_ReservedLabelNameThrows()
        {
            Assert.Throws<ArgumentException>(() => Counter.Build("a", "h").WithLabels("__a").Build());
        }

        [Fact]
        public void Build_ReturnsCollector()
        {
            var counter = Counter.Build("a", "h").WithLabels("a").Build();
            Assert.NotNull(counter);
        }
    }
}
