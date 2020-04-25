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

        public static EquivalencyAssertionOptions<T> ExcludingSubCollectionMember<T>(this EquivalencyAssertionOptions<T> assert, string subMemberPath)
        {
            assert.Excluding(x => x.SubCollectionMember() == subMemberPath);
            return assert;
        }

        public static string SubCollectionMember(this IMemberInfo memberInfo)
        {
            return memberInfo.SelectedMemberPath.SubCollectionMember();
        }

        public static string SubCollectionMember(this string path)
        {
            var i = path.IndexOf(".", StringComparison.Ordinal);
            return path.Substring(i + 1);
        }
    }
}