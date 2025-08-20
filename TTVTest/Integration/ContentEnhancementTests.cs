using System;
using System.Threading.Tasks;
using Moq;
using TTV;
using TTV.Config;
using TTV.Error;
using TTV.Services;
using Xunit;

namespace TTVTest.Integration
{
    public class ContentEnhancementTests
    {
        private readonly Mock<IGeminiService> _mockGeminiService;
        private const string SampleContent = "Tôi đi một bước, cảm thấy đau đớn xuyên qua cơ thể.";
        private const string EnhancedContent = "Vừa bước đi một bước, cơn đau như muôn nghìn mũi kim đâm xuyên qua toàn thân tôi.";

        public ContentEnhancementTests()
        {
            _mockGeminiService = new Mock<IGeminiService>();
        }

        [Fact]
        public async Task GetChapterContent_WithValidApiKey_ShouldEnhanceContent()
        {
            // Arrange
            _mockGeminiService.Setup(x => x.IsConfigured).Returns(true);
            _mockGeminiService.Setup(x => x.EnhanceContentAsync(SampleContent))
                .ReturnsAsync(EnhancedContent);

            var storyController = new StoryController(1, "token", 1, new GeminiConfig { ApiKey = "valid-key" });
            var chapter = new ChapterModel { Id = 1, Content = SampleContent };

            // Act
            var result = await storyController.GetChapterContent(1);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.IsEnhancedWithAI);
            Assert.Equal(EnhancedContent, result.EnhancedContent);
        }

        [Fact]
        public async Task GetChapterContent_WithoutApiKey_ShouldReturnOriginalContent()
        {
            // Arrange
            _mockGeminiService.Setup(x => x.IsConfigured).Returns(false);
            var storyController = new StoryController(1, "token", 1, null);
            var chapter = new ChapterModel { Id = 1, Content = SampleContent };

            // Act
            var result = await storyController.GetChapterContent(1);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsEnhancedWithAI);
            Assert.Equal(result.Content, result.GetDisplayContent());
        }

        [Fact]
        public async Task GetChapterContent_WithInvalidApiKey_ShouldThrowException()
        {
            // Arrange
            _mockGeminiService.Setup(x => x.IsConfigured).Returns(true);
            _mockGeminiService.Setup(x => x.EnhanceContentAsync(SampleContent))
                .ThrowsAsync(ErrorMessages.Api.CreateInvalidApiKey());

            var storyController = new StoryController(1, "token", 1, new GeminiConfig { ApiKey = "invalid-key" });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<EnhancementException>(() => storyController.GetChapterContent(1));
            Assert.Equal(ErrorType.ApiKeyInvalid, ex.Type);
        }

        [Fact]
        public async Task GetChapterContent_WithQuotaExceeded_ShouldThrowException()
        {
            // Arrange
            _mockGeminiService.Setup(x => x.IsConfigured).Returns(true);
            _mockGeminiService.Setup(x => x.EnhanceContentAsync(SampleContent))
                .ThrowsAsync(ErrorMessages.Api.CreateQuotaExceeded());

            var storyController = new StoryController(1, "token", 1, new GeminiConfig { ApiKey = "valid-key" });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<EnhancementException>(() => storyController.GetChapterContent(1));
            Assert.Equal(ErrorType.QuotaExceeded, ex.Type);
        }

        [Fact]
        public async Task GetChapterContent_WithNetworkError_ShouldThrowException()
        {
            // Arrange
            _mockGeminiService.Setup(x => x.IsConfigured).Returns(true);
            _mockGeminiService.Setup(x => x.EnhanceContentAsync(SampleContent))
                .ThrowsAsync(ErrorMessages.Api.CreateNetworkError());

            var storyController = new StoryController(1, "token", 1, new GeminiConfig { ApiKey = "valid-key" });

            // Act & Assert
            var ex = await Assert.ThrowsAsync<EnhancementException>(() => storyController.GetChapterContent(1));
            Assert.Equal(ErrorType.NetworkError, ex.Type);
        }
    }
}
