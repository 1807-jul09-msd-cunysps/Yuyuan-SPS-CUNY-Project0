using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestPalindrome
{
    [TestClass]
    public class UnitTestPalindrome
    {
        [TestMethod]
        public void TestMethodPalindrome()
        {
            // Arrange
            string[] testCases = new string[3];
            testCases[0] = "A nut for a jar of tuna.";
            testCases[1] = "Borrow or rob";
            testCases[2] = "343";
            bool[] actualValue = new bool[3];
            bool[] expectedValue = new bool[3] { true, true, true };
            // Act
            for (int i = 0; i < 3; i++)
            {
                actualValue[i] = Palindrome.Palindrome.IsPalindrome(testCases[i]);
            }
            // Assert
            for (int i = 0; i < 3; i++)
            {
                Assert.AreEqual(expectedValue[i], actualValue[i]);
            }
        }
    }
}
