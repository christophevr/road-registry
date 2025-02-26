namespace RoadRegistry.BackOffice
{
    using System;
    using Albedo;
    using AutoFixture;
    using AutoFixture.Idioms;
    using RoadRegistry.Framework.Assertions;
    using Xunit;

    public class UploadIdTests
    {
        private readonly Fixture _fixture;

        public UploadIdTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeArchiveId();
        }

        [Fact]
        public void VerifyBehavior()
        {
            new CompositeIdiomaticAssertion(
                new ImplicitConversionOperatorAssertion<Guid>(_fixture),
                new EquatableEqualsSelfAssertion(_fixture),
                new EquatableEqualsOtherAssertion(_fixture),
                new EqualityOperatorEqualsSelfAssertion(_fixture),
                new EqualityOperatorEqualsOtherAssertion(_fixture),
                new InequalityOperatorEqualsSelfAssertion(_fixture),
                new InequalityOperatorEqualsOtherAssertion(_fixture),
                new EqualsNewObjectAssertion(_fixture),
                new EqualsNullAssertion(_fixture),
                new EqualsSelfAssertion(_fixture),
                new EqualsOtherAssertion(_fixture),
                new EqualsSuccessiveAssertion(_fixture),
                new GetHashCodeSuccessiveAssertion(_fixture)
            ).Verify(typeof(UploadId));
        }

        [Fact]
        public void CtorValueCanNotBeEmpty()
        {
            new GuardClauseAssertion(
                _fixture,
                new EmptyGuidBehaviorExpectation()
            ).Verify(Constructors.Select(() => new UploadId(Guid.Empty)));
        }

        [Fact]
        public void ToStringReturnsExpectedResult()
        {
            var value = Guid.NewGuid();
            var sut = new UploadId(value);

            Assert.Equal(value.ToString("N"), sut.ToString());
        }

        [Theory]
        [InlineData("00000000000000000000000000000000", false)]
        [InlineData("62bee5951746453da866aef66daa2be7", true)]
        public void AcceptsReturnsExceptedResult(string value, bool expected)
        {
            var result = UploadId.Accepts(Guid.Parse(value));

            Assert.Equal(expected, result);
        }
    }
}
