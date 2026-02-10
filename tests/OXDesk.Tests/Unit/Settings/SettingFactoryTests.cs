using Moq;
using OXDesk.Core.Identity;
using OXDesk.Core.Settings;
using OXDesk.Settings.Factories;
using Shouldly;
using Xunit;

namespace OXDesk.Tests.Unit.Settings
{
    public class SettingFactoryTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ISettingService> _mockSettingService;
        private readonly SettingFactory _factory;

        public SettingFactoryTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockUserService
                .Setup(s => s.GetUserNameByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Test User");

            _mockSettingService = new Mock<ISettingService>();
            _mockSettingService
                .Setup(s => s.CastValue(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string v, string vt) =>
                {
                    if (string.IsNullOrEmpty(v)) return v;
                    return vt switch
                    {
                        SettingConstant.ValueTypes.Int when int.TryParse(v, out var i) => i,
                        SettingConstant.ValueTypes.Bool when bool.TryParse(v, out var b) => b,
                        _ => v
                    };
                });

            _factory = new SettingFactory(_mockUserService.Object, _mockSettingService.Object);
        }

        [Fact]
        public async Task BuildResponseAsync_CastsIntValue()
        {
            var entity = new Setting
            {
                Id = 101,
                Key = "TicketMaxAttachments",
                Value = "10",
                ValueType = SettingConstant.ValueTypes.Int,
                CreatedBy = 1,
                UpdatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _factory.BuildResponseAsync(entity);

            result.Value.ShouldBeOfType<int>();
            ((int)result.Value!).ShouldBe(10);
        }

        [Fact]
        public async Task BuildResponseAsync_CastsBoolValue()
        {
            var entity = new Setting
            {
                Id = 102,
                Key = "EnableFeatureX",
                Value = "true",
                ValueType = SettingConstant.ValueTypes.Bool,
                CreatedBy = 1,
                UpdatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _factory.BuildResponseAsync(entity);

            result.Value.ShouldBeOfType<bool>();
            ((bool)result.Value!).ShouldBeTrue();
        }

        [Fact]
        public async Task BuildResponseAsync_ReturnsStringAsIs()
        {
            var entity = new Setting
            {
                Id = 103,
                Key = "CustomerPortalUrlBase",
                Value = "https://portal.example.com",
                ValueType = SettingConstant.ValueTypes.String,
                CreatedBy = 1,
                UpdatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _factory.BuildResponseAsync(entity);

            result.Value.ShouldBeOfType<string>();
            ((string)result.Value!).ShouldBe("https://portal.example.com");
        }

        [Fact]
        public async Task BuildResponseAsync_ReturnsJsonAsIs()
        {
            var json = "{\"signatureType\":\"free-style\",\"signatureContent\":\"<p>Hello</p>\"}";
            var entity = new Setting
            {
                Id = 104,
                Key = SettingConstant.Keys.Signature,
                Value = json,
                ValueType = SettingConstant.ValueTypes.Json,
                CreatedBy = 1,
                UpdatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _factory.BuildResponseAsync(entity);

            result.Value.ShouldBeOfType<string>();
            ((string)result.Value!).ShouldBe(json);
        }

        [Fact]
        public async Task BuildResponseAsync_ResolvesCreatedByAndUpdatedByText()
        {
            var entity = new Setting
            {
                Id = 105,
                Key = "test",
                Value = "val",
                ValueType = SettingConstant.ValueTypes.String,
                CreatedBy = 1,
                UpdatedBy = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockUserService
                .Setup(s => s.GetUserNameByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync("Alice");
            _mockUserService
                .Setup(s => s.GetUserNameByIdAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync("Bob");

            var result = await _factory.BuildResponseAsync(entity);

            result.CreatedByText.ShouldBe("Alice");
            result.UpdatedByText.ShouldBe("Bob");
        }

        [Fact]
        public async Task BuildResponseAsync_ReturnsEmptyStringAsIs_WhenValueIsEmpty()
        {
            var entity = new Setting
            {
                Id = 106,
                Key = "empty",
                Value = "",
                ValueType = SettingConstant.ValueTypes.String,
                CreatedBy = 1,
                UpdatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _factory.BuildResponseAsync(entity);

            result.Value.ShouldBe("");
        }
    }
}
