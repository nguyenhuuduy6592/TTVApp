using dotnet_core;
using System;
using Xunit;

namespace TTVTest
{
    public class StoryControllerTests
    {
        [Fact]
        public void sha256Hash_should_success()
        {
            var storyController = new StoryController("f010b5c4df76c653caa2785343d0495a09b7dbc786b7facee27b1073aafc1a9f", 19954);
            var data = storyController.CalculateHash_GetChapterList();
            var hash = storyController.sha256Hash(data);
            Assert.Equal("3bb4e73fe34d9adfa8955101ab1638c35963a47b85f2d9eae5d7147304089e02", hash);
        }
    }
}
