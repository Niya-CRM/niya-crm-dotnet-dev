using Moq;
using OXDesk.Core.Identity;
using OXDesk.Core.Identity.DTOs;
using OXDesk.Core.Settings;
using OXDesk.Core.Settings.DTOs;
using OXDesk.Identity.Services;
using Shouldly;
using Xunit;

namespace OXDesk.Tests.Unit.Identity
{
    public class UserSignatureServiceTests
    {
        private readonly Mock<IUserSignatureRepository> _mockRepository;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<ISettingService> _mockSettingService;
        private readonly UserSignatureService _service;

        public UserSignatureServiceTests()
        {
            _mockRepository = new Mock<IUserSignatureRepository>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockCurrentUser.SetupGet(u => u.Id).Returns(1);
            _mockSettingService = new Mock<ISettingService>();

            _service = new UserSignatureService(
                _mockRepository.Object,
                _mockCurrentUser.Object,
                _mockSettingService.Object);
        }

        #region GetByUserIdAsync

        [Fact]
        public async Task GetByUserIdAsync_ReturnsEntity_WhenFound()
        {
            var expected = new UserSignature { Id = 10001, UserId = 5 };
            _mockRepository
                .Setup(r => r.GetByUserIdAsync(5))
                .ReturnsAsync(expected);

            var result = await _service.GetByUserIdAsync(5);

            result.ShouldNotBeNull();
            result!.Id.ShouldBe(10001);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsNull_WhenNotFound()
        {
            _mockRepository
                .Setup(r => r.GetByUserIdAsync(999))
                .ReturnsAsync((UserSignature?)null);

            var result = await _service.GetByUserIdAsync(999);

            result.ShouldBeNull();
        }

        #endregion

        #region UpsertAsync – StandardFixed (default)

        [Fact]
        public async Task UpsertAsync_StandardFixed_CreatesNew_WithStructuredFields()
        {
            _mockSettingService
                .Setup(s => s.GetSignatureAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SignatureSettingValue { SignatureType = SettingConstant.SignatureTypes.StandardFixed });

            _mockRepository
                .Setup(r => r.GetByUserIdAsync(5))
                .ReturnsAsync((UserSignature?)null);

            _mockRepository
                .Setup(r => r.AddAsync(It.IsAny<UserSignature>()))
                .ReturnsAsync((UserSignature s) => { s.Id = 10001; return s; });

            var request = new UpsertUserSignatureRequest
            {
                ComplimentaryClose = "Kind regards",
                FullName = "John Doe",
                FreeStyleSignature = "<p>Should be ignored</p>"
            };

            var result = await _service.UpsertAsync(5, request);

            result.ComplimentaryClose.ShouldBe("Kind regards");
            result.FullName.ShouldBe("John Doe");
            result.FreeStyleSignature.ShouldBeNull();
            result.CreatedBy.ShouldBe(1);
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<UserSignature>()), Times.Once);
        }

        [Fact]
        public async Task UpsertAsync_StandardFixed_UpdatesExisting_WithStructuredFields()
        {
            _mockSettingService
                .Setup(s => s.GetSignatureAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SignatureSettingValue { SignatureType = SettingConstant.SignatureTypes.StandardFixed });

            var existing = new UserSignature
            {
                Id = 10001,
                UserId = 5,
                ComplimentaryClose = "Old",
                FreeStyleSignature = "Old free style",
                CreatedBy = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _mockRepository
                .Setup(r => r.GetByUserIdAsync(5))
                .ReturnsAsync(existing);

            _mockRepository
                .Setup(r => r.UpdateAsync(It.IsAny<UserSignature>()))
                .ReturnsAsync((UserSignature s) => s);

            var request = new UpsertUserSignatureRequest
            {
                ComplimentaryClose = "Best wishes",
                FullName = "Jane Doe",
                FreeStyleSignature = "<p>Should be ignored</p>"
            };

            var result = await _service.UpsertAsync(5, request);

            result.ComplimentaryClose.ShouldBe("Best wishes");
            result.FullName.ShouldBe("Jane Doe");
            result.FreeStyleSignature.ShouldBeNull();
            result.UpdatedBy.ShouldBe(1);
            result.CreatedBy.ShouldBe(2);
            _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<UserSignature>()), Times.Once);
        }

        #endregion

        #region UpsertAsync – FreeStyle

        [Fact]
        public async Task UpsertAsync_FreeStyle_CreatesNew_WithFreeStyleOnly()
        {
            _mockSettingService
                .Setup(s => s.GetSignatureAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SignatureSettingValue { SignatureType = SettingConstant.SignatureTypes.FreeStyle });

            _mockRepository
                .Setup(r => r.GetByUserIdAsync(5))
                .ReturnsAsync((UserSignature?)null);

            _mockRepository
                .Setup(r => r.AddAsync(It.IsAny<UserSignature>()))
                .ReturnsAsync((UserSignature s) => { s.Id = 10002; return s; });

            var request = new UpsertUserSignatureRequest
            {
                ComplimentaryClose = "Should be ignored",
                FullName = "Should be ignored",
                FreeStyleSignature = "<p>My custom signature</p>"
            };

            var result = await _service.UpsertAsync(5, request);

            result.FreeStyleSignature.ShouldBe("<p>My custom signature</p>");
            result.ComplimentaryClose.ShouldBeNull();
            result.FullName.ShouldBeNull();
            result.JobTitle.ShouldBeNull();
        }

        [Fact]
        public async Task UpsertAsync_FreeStyle_UpdatesExisting_NullsStructuredFields()
        {
            _mockSettingService
                .Setup(s => s.GetSignatureAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SignatureSettingValue { SignatureType = SettingConstant.SignatureTypes.FreeStyle });

            var existing = new UserSignature
            {
                Id = 10001,
                UserId = 5,
                ComplimentaryClose = "Old close",
                FullName = "Old name",
                CreatedBy = 2,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _mockRepository
                .Setup(r => r.GetByUserIdAsync(5))
                .ReturnsAsync(existing);

            _mockRepository
                .Setup(r => r.UpdateAsync(It.IsAny<UserSignature>()))
                .ReturnsAsync((UserSignature s) => s);

            var request = new UpsertUserSignatureRequest
            {
                FreeStyleSignature = "<p>Updated free style</p>"
            };

            var result = await _service.UpsertAsync(5, request);

            result.FreeStyleSignature.ShouldBe("<p>Updated free style</p>");
            result.ComplimentaryClose.ShouldBeNull();
            result.FullName.ShouldBeNull();
        }

        #endregion

        #region UpsertAsync – GlobalSignature (behaves like StandardFixed)

        [Fact]
        public async Task UpsertAsync_GlobalSignature_BehavesLikeStandardFixed()
        {
            _mockSettingService
                .Setup(s => s.GetSignatureAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SignatureSettingValue { SignatureType = SettingConstant.SignatureTypes.GlobalSignature });

            _mockRepository
                .Setup(r => r.GetByUserIdAsync(5))
                .ReturnsAsync((UserSignature?)null);

            _mockRepository
                .Setup(r => r.AddAsync(It.IsAny<UserSignature>()))
                .ReturnsAsync((UserSignature s) => { s.Id = 10003; return s; });

            var request = new UpsertUserSignatureRequest
            {
                ComplimentaryClose = "Best",
                FullName = "Admin",
                FreeStyleSignature = "<p>Ignored</p>"
            };

            var result = await _service.UpsertAsync(5, request);

            result.ComplimentaryClose.ShouldBe("Best");
            result.FullName.ShouldBe("Admin");
            result.FreeStyleSignature.ShouldBeNull();
        }

        #endregion

        #region UpsertAsync – Null/Empty SignatureType defaults to StandardFixed

        [Fact]
        public async Task UpsertAsync_NullSignatureSetting_DefaultsToStandardFixed()
        {
            _mockSettingService
                .Setup(s => s.GetSignatureAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((SignatureSettingValue?)null);

            _mockRepository
                .Setup(r => r.GetByUserIdAsync(5))
                .ReturnsAsync((UserSignature?)null);

            _mockRepository
                .Setup(r => r.AddAsync(It.IsAny<UserSignature>()))
                .ReturnsAsync((UserSignature s) => { s.Id = 10004; return s; });

            var request = new UpsertUserSignatureRequest
            {
                ComplimentaryClose = "Regards",
                FreeStyleSignature = "<p>Ignored</p>"
            };

            var result = await _service.UpsertAsync(5, request);

            result.ComplimentaryClose.ShouldBe("Regards");
            result.FreeStyleSignature.ShouldBeNull();
        }

        [Fact]
        public async Task UpsertAsync_EmptySignatureType_DefaultsToStandardFixed()
        {
            _mockSettingService
                .Setup(s => s.GetSignatureAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SignatureSettingValue { SignatureType = "" });

            _mockRepository
                .Setup(r => r.GetByUserIdAsync(5))
                .ReturnsAsync((UserSignature?)null);

            _mockRepository
                .Setup(r => r.AddAsync(It.IsAny<UserSignature>()))
                .ReturnsAsync((UserSignature s) => { s.Id = 10005; return s; });

            var request = new UpsertUserSignatureRequest
            {
                ComplimentaryClose = "Cheers",
                FreeStyleSignature = "<p>Ignored</p>"
            };

            var result = await _service.UpsertAsync(5, request);

            result.ComplimentaryClose.ShouldBe("Cheers");
            result.FreeStyleSignature.ShouldBeNull();
        }

        #endregion
    }
}
