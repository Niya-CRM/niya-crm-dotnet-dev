using Moq;
using OXDesk.Core.Identity;
using OXDesk.Core.Settings;
using OXDesk.Core.Settings.DTOs;
using OXDesk.Identity.Factories;
using Shouldly;
using Xunit;

namespace OXDesk.Tests.Unit.Identity
{
    public class UserSignatureFactoryTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ISettingService> _mockSettingService;
        private readonly UserSignatureFactory _factory;

        public UserSignatureFactoryTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockUserService
                .Setup(s => s.GetUserNameByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("Test User");

            _mockSettingService = new Mock<ISettingService>();

            _factory = new UserSignatureFactory(_mockUserService.Object, _mockSettingService.Object);
        }

        [Fact]
        public async Task BuildResponseAsync_MapsAllFields()
        {
            var entity = new UserSignature
            {
                Id = 10001,
                UserId = 5,
                ComplimentaryClose = "Kind regards",
                FullName = "John Doe",
                JobTitle = "Developer",
                Company = "Acme",
                Department = "Engineering",
                AddressLine1 = "123 Main St",
                AddressLine2 = "Suite 100",
                AddressLine3 = "Floor 2",
                Telephone = "555-1234",
                Mobile = "555-5678",
                Email = "john@example.com",
                Website = "https://example.com",
                FreeStyleSignature = "<p>My sig</p>",
                CreatedBy = 1,
                UpdatedBy = 2,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockSettingService
                .Setup(s => s.GetSignatureAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SignatureSettingValue { SignatureType = "standard-fixed" });

            var result = await _factory.BuildResponseAsync(entity);

            result.Id.ShouldBe(10001);
            result.UserId.ShouldBe(5);
            result.ComplimentaryClose.ShouldBe("Kind regards");
            result.FullName.ShouldBe("John Doe");
            result.FreeStyleSignature.ShouldBe("<p>My sig</p>");
        }

        [Fact]
        public async Task BuildResponseAsync_PopulatesSignatureSetting()
        {
            var entity = new UserSignature
            {
                Id = 10001,
                UserId = 5,
                CreatedBy = 1,
                UpdatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var settingValue = new SignatureSettingValue
            {
                SignatureType = SettingConstant.SignatureTypes.FreeStyle,
                SignatureContent = ""
            };

            _mockSettingService
                .Setup(s => s.GetSignatureAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(settingValue);

            var result = await _factory.BuildResponseAsync(entity);

            result.SignatureSetting.ShouldNotBeNull();
            result.SignatureSetting!.SignatureType.ShouldBe(SettingConstant.SignatureTypes.FreeStyle);
        }

        [Fact]
        public async Task BuildResponseAsync_SignatureSettingNull_WhenNoSetting()
        {
            var entity = new UserSignature
            {
                Id = 10001,
                UserId = 5,
                CreatedBy = 1,
                UpdatedBy = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockSettingService
                .Setup(s => s.GetSignatureAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((SignatureSettingValue?)null);

            var result = await _factory.BuildResponseAsync(entity);

            result.SignatureSetting.ShouldBeNull();
        }

        [Fact]
        public async Task BuildResponseAsync_ResolvesAuditNames()
        {
            var entity = new UserSignature
            {
                Id = 10001,
                UserId = 5,
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

            _mockSettingService
                .Setup(s => s.GetSignatureAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((SignatureSettingValue?)null);

            var result = await _factory.BuildResponseAsync(entity);

            result.CreatedByText.ShouldBe("Alice");
            result.UpdatedByText.ShouldBe("Bob");
        }
    }
}
