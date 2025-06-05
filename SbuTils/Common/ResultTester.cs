using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SbuTils.Common
{
    [TestClass]
    public class ResultTester
    {
        enum TestErrorEnum
        {
            Nope1,
            Nope2
        }

        class DummyObject
        {
            public DummyObject(string name)
            {
                Name = name;
            }

            public string Name { get; init; }
        }

        Result<TestErrorEnum> GenerateResultNoObjectError()
        {
            return Result<TestErrorEnum>.NotOk(TestErrorEnum.Nope1, "Pas bon 1");
        }

        Result<DummyObject, TestErrorEnum> GenerateResultWithObjectError()
        {
            return Result<DummyObject, TestErrorEnum>.NotOk(TestErrorEnum.Nope2, "Pas bon 2");
        }

        Result<TestErrorEnum> GenerateOkResultNoObject()
        {
            return Result<TestErrorEnum>.Ok;
        }

        Result<DummyObject, TestErrorEnum> GenerateOkResultWithObject()
        {
            return Result<DummyObject, TestErrorEnum>.Ok(new DummyObject("Bob"));
        }

        class TestException : Exception, IConvertFromNotOkResultException<TestErrorEnum>
        {
            public TestException(string message)
                : base(message) { }

            public TestErrorEnum Code { get; set; }
            string IConvertFromNotOkResultException<TestErrorEnum>.Message => this.Message;

            public Exception BuildException(string code, string message)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void ThrowOnError_WorksFineOnGenericExceptionWithResultNoObject()
        {
            // Arrange
            //Act
            var ex = Assert.ThrowsExactly<TestException>(() =>
            {
                GenerateResultNoObjectError().ThrowOnError<TestException>();
            });

            //Assert
            Assert.AreEqual(TestErrorEnum.Nope1, ex.Code);
            Assert.AreEqual("Pas bon 1", ex.Message);
        }

        [TestMethod]
        public void ThrowOnError_WorksFineOnGenericExceptionWithResultWithObject()
        {
            // Arrange
            //Act
            var ex = Assert.ThrowsExactly<TestException>(() =>
            {
                GenerateResultWithObjectError().ThrowOnError<TestException>();
            });

            //Assert
            Assert.AreEqual(TestErrorEnum.Nope2, ex.Code);
            Assert.AreEqual("Pas bon 2", ex.Message);
        }

        [TestMethod]
        public void ThrowOnError_DoNotThrowOnOkResultWithoutObject()
        {
            // Arrange
            //Act
            var result = GenerateOkResultNoObject().ThrowOnError<TestException>().ExtractObject();

            //Assert

            Assert.IsNull(result);
        }

        [TestMethod]
        public void ThrowOnError_DoNotThrowOnOkResultWithObject()
        {
            // Arrange
            //Act
            var result = GenerateOkResultWithObject().ThrowOnError<TestException>().ExtractObject();

            //Assert

            Assert.AreEqual("Bob", result?.Name);
        }
    }
}
