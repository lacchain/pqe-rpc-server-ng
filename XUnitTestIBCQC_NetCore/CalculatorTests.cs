
using IBCQC_NetCore.xUnitTestEx;
using System;
using Xunit;

namespace XUnitTestIBCQC_NetCore
{
    public class CalculatorTests
    {

        //very basic just add two numbers and test
        [Fact]
        public void AddTwoNumbers()
        {
            int a = 1;
            int b = 2;

            //sut  is Subject under Test
            var sut = new Calculator();

            //send test data
            var result = sut.Add(a, b);

            //see if we got what we expected
            //Assert.Equal(expected,actual returned)

            Assert.Equal(3, result);

        }

        ///example of data driven test  


        [Theory]
        [InlineData(1, 2, 3)]
        [InlineData(-4, -6, -10)]
        [InlineData(-2, 2, 0)]
        public void Can_Add_Two_Numbers_Data_Driven(int a, int b, int expectedResult)
        {
            // Arrange
            var sut = new Calculator();

            // Act
            var actualResult = sut.Add(a, b);

            // Assert
            Assert.Equal(expectedResult, actualResult);
    }
    }
}
