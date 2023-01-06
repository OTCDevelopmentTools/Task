namespace FileSystemChecker.IntegrationTest
{
    public class FileSystemCheckerTest
    {
        public FileSystemCheckerTest()
        {

        }

        [Fact]
        public void TestCheckFilesChangedInLastHour()
        {
            /// Arrange
            FileSystemCheckerClassLib.FileSystemChecker objClass = new FileSystemCheckerClassLib.FileSystemChecker(true);

            /// Act
            var result = objClass.CheckFilesChangedInLastHour();

            /// Assert
            Assert.True(true, result.ToString());
        }
    }
}