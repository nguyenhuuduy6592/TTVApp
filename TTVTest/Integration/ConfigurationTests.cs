using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using TTV.Config;
using Xunit;

namespace TTVTest.Integration
{
    public class ConfigurationTests : IDisposable
    {
        private readonly string _testConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        public ConfigurationTests()
        {
            // Cleanup any existing test config
            if (File.Exists(_testConfigPath))
                File.Delete(_testConfigPath);
        }

        public void Dispose()
        {
            // Cleanup after tests
            if (File.Exists(_testConfigPath))
                File.Delete(_testConfigPath);
        }

        [Fact]
        public void LoadConfig_WithValidConfig_ShouldDeserializeCorrectly()
        {
            // Arrange
            var config = new AppConfig
            {
                UserId = "user123",
                Token = "token123",
                StoryId = "story123",
                OutputFileName = "output.html",
                StartChapter = 1,
                EndChapter = 10,
                Gemini = new GeminiConfig
                {
                    ApiKey = "test-api-key",
                    ModelId = "gemini-2.0-flash"
                }
            };

            File.WriteAllText(_testConfigPath, JsonConvert.SerializeObject(config));

            // Act
            var loadedConfig = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(_testConfigPath));

            // Assert
            Assert.NotNull(loadedConfig);
            Assert.Equal(config.UserId, loadedConfig.UserId);
            Assert.Equal(config.Token, loadedConfig.Token);
            Assert.Equal(config.StoryId, loadedConfig.StoryId);
            Assert.Equal(config.OutputFileName, loadedConfig.OutputFileName);
            Assert.Equal(config.StartChapter, loadedConfig.StartChapter);
            Assert.Equal(config.EndChapter, loadedConfig.EndChapter);
            Assert.NotNull(loadedConfig.Gemini);
            Assert.Equal(config.Gemini.ApiKey, loadedConfig.Gemini.ApiKey);
            Assert.Equal(config.Gemini.ModelId, loadedConfig.Gemini.ModelId);
        }

        [Fact]
        public void LoadConfig_WithMissingApiKey_ShouldLoadWithoutGemini()
        {
            // Arrange
            var config = new AppConfig
            {
                UserId = "user123",
                Token = "token123",
                StoryId = "story123",
                OutputFileName = "output.html",
                StartChapter = 1,
                EndChapter = 10
            };

            File.WriteAllText(_testConfigPath, JsonConvert.SerializeObject(config));

            // Act
            var loadedConfig = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(_testConfigPath));

            // Assert
            Assert.NotNull(loadedConfig);
            Assert.Equal(config.UserId, loadedConfig.UserId);
            Assert.Equal(config.Token, loadedConfig.Token);
            Assert.Null(loadedConfig.Gemini);
        }

        [Fact]
        public void LoadConfig_WithInvalidJson_ShouldThrowException()
        {
            // Arrange
            File.WriteAllText(_testConfigPath, "invalid json content");

            // Act & Assert
            Assert.Throws<JsonReaderException>(() => 
                JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(_testConfigPath)));
        }

        [Fact]
        public void GeminiConfig_WithApiKey_ShouldBeConfigured()
        {
            // Arrange
            var config = new GeminiConfig { ApiKey = "test-key" };

            // Assert
            Assert.True(config.IsConfigured);
        }

        [Fact]
        public void GeminiConfig_WithoutApiKey_ShouldNotBeConfigured()
        {
            // Arrange
            var config = new GeminiConfig { ApiKey = null };
            var emptyConfig = new GeminiConfig { ApiKey = "" };

            // Assert
            Assert.False(config.IsConfigured);
            Assert.False(emptyConfig.IsConfigured);
        }
    }
}
