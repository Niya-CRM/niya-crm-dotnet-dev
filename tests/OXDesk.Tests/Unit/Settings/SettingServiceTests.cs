using Moq;
using OXDesk.Core.Identity;
using OXDesk.Core.Settings;
using OXDesk.Core.Settings.DTOs;
using OXDesk.Settings.Services;
using Shouldly;
using Xunit;

namespace OXDesk.Tests.Unit.Settings
{
    public class SettingServiceTests
    {
        private readonly Mock<ISettingRepository> _mockRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly SettingService _service;

        public SettingServiceTests()
        {
            _mockRepository = new Mock<ISettingRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockCurrentUser.SetupGet(u => u.Id).Returns(1);

            _service = new SettingService(_mockRepository.Object, _mockCurrentUser.Object);
        }

        #region GetByKeyAsync

        [Fact]
        public async Task GetByKeyAsync_ReturnsEntity_WhenFound()
        {
            var expected = new Setting { Id = 101, Key = "signature", Value = "{}", ValueType = SettingConstant.ValueTypes.Json };

            _mockRepository
                .Setup(r => r.GetByKeyAsync("signature", It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _service.GetByKeyAsync("signature");

            result.ShouldNotBeNull();
            result!.Id.ShouldBe(101);
            result.Key.ShouldBe("signature");
        }

        [Fact]
        public async Task GetByKeyAsync_ReturnsNull_WhenNotFound()
        {
            _mockRepository
                .Setup(r => r.GetByKeyAsync("unknown", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Setting?)null);

            var result = await _service.GetByKeyAsync("unknown");

            result.ShouldBeNull();
        }

        #endregion

        #region UpsertAsync

        [Fact]
        public async Task UpsertAsync_CreatesNew_WhenNotExisting()
        {
            _mockRepository
                .Setup(r => r.GetByKeyAsync("signature", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Setting?)null);

            _mockRepository
                .Setup(r => r.AddAsync(It.IsAny<Setting>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Setting s, CancellationToken _) => { s.Id = 101; return s; });

            var request = new UpsertSettingRequest
            {
                Value = "{}",
                ValueType = SettingConstant.ValueTypes.Json
            };

            var result = await _service.UpsertAsync("signature", request);

            result.ShouldNotBeNull();
            result.Key.ShouldBe("signature");
            result.Value.ShouldBe("{}");
            result.ValueType.ShouldBe(SettingConstant.ValueTypes.Json);
            result.CreatedBy.ShouldBe(1);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<Setting>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpsertAsync_UpdatesExisting_WhenFound()
        {
            var existing = new Setting
            {
                Id = 101,
                Key = "signature",
                Value = "old",
                ValueType = SettingConstant.ValueTypes.String,
                CreatedBy = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _mockRepository
                .Setup(r => r.GetByKeyAsync("signature", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            _mockRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Setting>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Setting s, CancellationToken _) => s);

            var request = new UpsertSettingRequest
            {
                Value = "new",
                ValueType = SettingConstant.ValueTypes.String
            };

            var result = await _service.UpsertAsync("signature", request);

            result.ShouldNotBeNull();
            result.Value.ShouldBe("new");
            result.UpdatedBy.ShouldBe(1);
            result.CreatedBy.ShouldBe(2);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Setting>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpsertAsync_SetsAuditFields_OnCreate()
        {
            _mockRepository
                .Setup(r => r.GetByKeyAsync("signature", It.IsAny<CancellationToken>()))
                .ReturnsAsync((Setting?)null);

            _mockRepository
                .Setup(r => r.AddAsync(It.IsAny<Setting>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Setting s, CancellationToken _) => { s.Id = 102; return s; });

            var before = DateTime.UtcNow;

            var result = await _service.UpsertAsync("signature", new UpsertSettingRequest
            {
                Value = "test",
                ValueType = SettingConstant.ValueTypes.String
            });

            result.CreatedBy.ShouldBe(1);
            result.UpdatedBy.ShouldBe(1);
            result.CreatedAt.ShouldBeGreaterThanOrEqualTo(before);
            result.UpdatedAt.ShouldBeGreaterThanOrEqualTo(before);
        }

        [Fact]
        public async Task UpsertAsync_SetsAuditFields_OnUpdate()
        {
            var existing = new Setting
            {
                Id = 101,
                Key = "signature",
                Value = "old",
                ValueType = SettingConstant.ValueTypes.String,
                CreatedBy = 5,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedBy = 5,
                UpdatedAt = DateTime.UtcNow.AddDays(-7)
            };

            _mockRepository
                .Setup(r => r.GetByKeyAsync("signature", It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            _mockRepository
                .Setup(r => r.UpdateAsync(It.IsAny<Setting>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Setting s, CancellationToken _) => s);

            var before = DateTime.UtcNow;

            var result = await _service.UpsertAsync("signature", new UpsertSettingRequest
            {
                Value = "updated",
                ValueType = SettingConstant.ValueTypes.String
            });

            result.UpdatedBy.ShouldBe(1);
            result.UpdatedAt.ShouldBeGreaterThanOrEqualTo(before);
            result.CreatedBy.ShouldBe(5);
        }

        #endregion

        #region GetSignatureAsync

        [Fact]
        public async Task GetSignatureAsync_ReturnsNull_WhenNotFound()
        {
            _mockRepository
                .Setup(r => r.GetByKeyAsync(SettingConstant.Keys.Signature, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Setting?)null);

            var result = await _service.GetSignatureAsync();

            result.ShouldBeNull();
        }

        [Fact]
        public async Task GetSignatureAsync_ReturnsNull_WhenValueIsEmpty()
        {
            var entity = new Setting
            {
                Id = 101,
                Key = SettingConstant.Keys.Signature,
                Value = "",
                ValueType = SettingConstant.ValueTypes.Json
            };

            _mockRepository
                .Setup(r => r.GetByKeyAsync(SettingConstant.Keys.Signature, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            var result = await _service.GetSignatureAsync();

            result.ShouldBeNull();
        }

        [Fact]
        public async Task GetSignatureAsync_ReturnsTypedValue_WhenValid()
        {
            var json = "{\"signatureType\":\"free-style\",\"signatureContent\":\"<p>Hello</p>\"}";
            var entity = new Setting
            {
                Id = 101,
                Key = SettingConstant.Keys.Signature,
                Value = json,
                ValueType = SettingConstant.ValueTypes.Json
            };

            _mockRepository
                .Setup(r => r.GetByKeyAsync(SettingConstant.Keys.Signature, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            var result = await _service.GetSignatureAsync();

            result.ShouldNotBeNull();
            result!.SignatureType.ShouldBe("free-style");
            result.SignatureContent.ShouldBe("<p>Hello</p>");
        }

        #endregion

        #region CastValue

        [Fact]
        public void CastValue_ReturnsInt_WhenTypeIsInt()
        {
            var result = _service.CastValue("42", SettingConstant.ValueTypes.Int);
            result.ShouldBeOfType<int>();
            ((int)result!).ShouldBe(42);
        }

        [Fact]
        public void CastValue_ReturnsBool_WhenTypeIsBool()
        {
            var result = _service.CastValue("true", SettingConstant.ValueTypes.Bool);
            result.ShouldBeOfType<bool>();
            ((bool)result!).ShouldBeTrue();
        }

        [Fact]
        public void CastValue_ReturnsString_WhenTypeIsString()
        {
            var result = _service.CastValue("hello", SettingConstant.ValueTypes.String);
            result.ShouldBeOfType<string>();
            ((string)result!).ShouldBe("hello");
        }

        [Fact]
        public void CastValue_ReturnsString_WhenTypeIsJson()
        {
            var json = "{\"key\":\"value\"}";
            var result = _service.CastValue(json, SettingConstant.ValueTypes.Json);
            result.ShouldBeOfType<string>();
            ((string)result!).ShouldBe(json);
        }

        [Fact]
        public void CastValue_ReturnsEmptyString_WhenValueIsEmpty()
        {
            var result = _service.CastValue("", SettingConstant.ValueTypes.Int);
            result.ShouldBe("");
        }

        #endregion
    }
}
