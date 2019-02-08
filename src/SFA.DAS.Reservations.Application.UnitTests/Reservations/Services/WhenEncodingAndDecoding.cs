using System;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using FluentAssertions;
using HashidsNet;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Services;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Services
{
    [TestFixture]
    public class WhenEncodingAndDecoding
    {
        private long[] _decodedId;
        private string _encodedId;
        private Mock<IHashids> _mockHashIds;
        private HashingService _hashingService;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture()
                .Customize(new AutoMoqCustomization{ConfigureMembers = true});

            _decodedId = fixture.Create<long[]>();
            _encodedId = fixture.Create<string>();
            
            _mockHashIds = fixture.Freeze<Mock<IHashids>>();
            _mockHashIds
                .Setup(hashids => hashids.DecodeLong(_encodedId))
                .Returns(_decodedId);
            _mockHashIds
                .Setup(hashids => hashids.EncodeLong(_decodedId[0]))
                .Returns(_encodedId);

            _hashingService = fixture.Create<HashingService>();
        }

        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void When_Decode_HashId_IsNullOrEmpty_Should_ThrowException(string valueToDecode)
        {
            //Act
            Action testDelegate = () => _hashingService.DecodeValue(valueToDecode);

            //Assert
            testDelegate.Should().Throw<ArgumentException>();
        }

        [Test, AutoData]
        public void Then_Numeric_HashValue_Should_Equal_DecodeValue()
        {
            //Act
            var actualValue = _hashingService.DecodeValue(_encodedId);

            //Assert
            actualValue.Should().Be(_decodedId[0]);
        }

        [Test, AutoData]
        public void Then_HashValue_Should_Equal_EncodedValue()
        {
            //Act
            var actualValue = _hashingService.HashValue(_decodedId[0]);

            //Assert
            actualValue.Should().Be(_encodedId);
        }
    }
}