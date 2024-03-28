using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Services
{
    public class WhenStoringSessionData
    {
        [Test, MoqAutoData]
        public void Then_The_Object_Is_Serialized_And_Saved_To_The_Session(
            TestModel testModel,
            [Frozen]Mock<ISession> mockSession,
            [Frozen]Mock<IHttpContextAccessor> mockAccessor,
            SessionStorageService<TestModel> sessionStore)
        {
            var expectedModel = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(testModel));
            mockAccessor.Setup(x => x.HttpContext.Session).Returns(mockSession.Object);

            sessionStore.Store(testModel);

            mockSession.Verify(x=>x.Set(nameof(TestModel),expectedModel), Times.Once);
        }

        [Test, MoqAutoData]
        public void Then_The_Object_Is_Returned_From_Session_If_It_Exists(
            TestModel testModel,
            [Frozen]Mock<ISession> mockSession,
            [Frozen]Mock<IHttpContextAccessor> mockAccessor,
            SessionStorageService<TestModel> sessionStore)
        {
            var value = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(testModel));
            mockSession.Setup(x => x.TryGetValue(nameof(TestModel), out value)).Returns(true);
            mockAccessor.Setup(x => x.HttpContext.Session).Returns(mockSession.Object);

            var actual = sessionStore.Get();

            actual.Should().BeEquivalentTo(testModel);
        }

        [Test, MoqAutoData]
        public void Then_Null_Is_Returned_If_The_Object_Does_Not_Exist_In_Session(
            [Frozen]Mock<IHttpContextAccessor> mockAccessor,
            [Frozen]Mock<ISession> mockSession,
            SessionStorageService<TestModel> sessionStore)
        {
            byte[] bytes = null;
            mockSession.Setup(x => x.TryGetValue(nameof(TestModel), out bytes)).Returns(false);
            mockAccessor.Setup(x => x.HttpContext.Session).Returns(mockSession.Object);
            
            var actual = sessionStore.Get();

            Assert.IsNull(actual);
        }

        [Test, MoqAutoData]
        public void Then_The_Object_Is_Deleted_From_Session(
            [Frozen]Mock<IHttpContextAccessor> mockAccessor,
            SessionStorageService<TestModel> sessionStore)
        {
            sessionStore.Delete();

            mockAccessor.Verify(x=>x.HttpContext.Session.Remove(nameof(TestModel)), Times.Once);
        }
    }

    public class TestModel
    {
        public string TestString { get; set; }
        public int TestInt { get; set; }
    }
}
