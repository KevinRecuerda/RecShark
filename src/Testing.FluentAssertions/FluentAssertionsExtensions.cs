using FluentAssertions;
using FluentAssertions.Equivalency;
using System;

namespace RecShark.Extensions.Testing.FluentAssertions
{
    public static class FluentAssertionsExtensions
    {
        public const double DefaultPrecision = 1E-8;

        public static void UsePrecision(double precision = DefaultPrecision)
        {
            AssertionOptions.AssertEquivalencyUsing(
                options => options
                    .Using<double>(ctx => ctx.Subject.Should().BeApproximately(ctx.Expectation, precision))
                    .WhenTypeIs<double>());
        }

        public static bool IsEquivalentTo<T>(
            this T actual,
            T expected,
            Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> config = null)
        {
            try
            {
                config = config ?? (x => x);
                actual.Should().BeEquivalentTo(expected, config);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}