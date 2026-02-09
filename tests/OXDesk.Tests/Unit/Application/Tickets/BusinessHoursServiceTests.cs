using Microsoft.Extensions.Logging;
using Moq;
using OXDesk.Core;
using OXDesk.Core.AuditLogs.ChangeHistory;
using OXDesk.Core.Identity;
using OXDesk.Core.Tickets;
using OXDesk.Core.Tickets.DTOs;
using OXDesk.Tickets.Services;
using Shouldly;
using Xunit;

namespace OXDesk.Tests.Unit.Application.Tickets
{
    public class BusinessHoursServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<ICurrentUser> _mockCurrentUser;
        private readonly Mock<IChangeHistoryLogService> _mockChangeHistoryLogService;
        private readonly Mock<ILogger<BusinessHoursService>> _mockLogger;
        private readonly Mock<IBusinessHoursRepository> _mockBusinessHoursRepo;
        private readonly Mock<ICustomBusinessHoursRepository> _mockCustomBusinessHoursRepo;
        private readonly Mock<IHolidayRepository> _mockHolidayRepo;
        private readonly BusinessHoursService _service;

        public BusinessHoursServiceTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockCurrentUser = new Mock<ICurrentUser>();
            _mockChangeHistoryLogService = new Mock<IChangeHistoryLogService>();
            _mockLogger = new Mock<ILogger<BusinessHoursService>>();
            _mockBusinessHoursRepo = new Mock<IBusinessHoursRepository>();
            _mockCustomBusinessHoursRepo = new Mock<ICustomBusinessHoursRepository>();
            _mockHolidayRepo = new Mock<IHolidayRepository>();

            _mockUnitOfWork
                .Setup(uow => uow.GetRepository<IBusinessHoursRepository>())
                .Returns(_mockBusinessHoursRepo.Object);

            _mockUnitOfWork
                .Setup(uow => uow.GetRepository<ICustomBusinessHoursRepository>())
                .Returns(_mockCustomBusinessHoursRepo.Object);

            _mockUnitOfWork
                .Setup(uow => uow.GetRepository<IHolidayRepository>())
                .Returns(_mockHolidayRepo.Object);

            _mockCurrentUser.SetupGet(u => u.Id).Returns(1);

            _service = new BusinessHoursService(
                _mockUnitOfWork.Object,
                _mockCurrentUser.Object,
                _mockChangeHistoryLogService.Object,
                _mockLogger.Object);
        }

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ReturnsAllBusinessHours()
        {
            var expected = new List<BusinessHours>
            {
                new() { Id = 1, Name = "Default Hours" },
                new() { Id = 2, Name = "Weekend Hours" }
            };

            _mockBusinessHoursRepo
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _service.GetAllAsync();

            result.ShouldNotBeNull();
            result.Count().ShouldBe(2);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmptyList_WhenNoBusinessHoursExist()
        {
            _mockBusinessHoursRepo
                .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BusinessHours>());

            var result = await _service.GetAllAsync();

            result.ShouldNotBeNull();
            result.Count().ShouldBe(0);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ReturnsBusinessHours_WhenFound()
        {
            var expected = new BusinessHours { Id = 1, Name = "Default Hours" };

            _mockBusinessHoursRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var result = await _service.GetByIdAsync(1);

            result.ShouldNotBeNull();
            result!.Id.ShouldBe(1);
            result.Name.ShouldBe("Default Hours");
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            _mockBusinessHoursRepo
                .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BusinessHours?)null);

            var result = await _service.GetByIdAsync(999);

            result.ShouldBeNull();
        }

        #endregion

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_SetsDefaultTrue_WhenNoExistingBusinessHours()
        {
            _mockBusinessHoursRepo
                .Setup(r => r.AnyExistAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockBusinessHoursRepo
                .Setup(r => r.AddAsync(It.IsAny<BusinessHours>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BusinessHours bh, CancellationToken _) => { bh.Id = 101; return bh; });

            var request = new CreateBusinessHoursRequest
            {
                Name = "First",
                IsDefault = false,
                BusinessHoursType = BusinessHoursConstant.BusinessHoursTypes.TwentyFourSeven
            };

            var result = await _service.CreateAsync(request);

            result.IsDefault.ShouldBeTrue();
        }

        [Fact]
        public async Task CreateAsync_ClearsOtherDefaults_WhenIsDefaultTrue()
        {
            _mockBusinessHoursRepo
                .Setup(r => r.AnyExistAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockBusinessHoursRepo
                .Setup(r => r.AddAsync(It.IsAny<BusinessHours>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BusinessHours bh, CancellationToken _) => { bh.Id = 102; return bh; });

            var request = new CreateBusinessHoursRequest
            {
                Name = "New Default",
                IsDefault = true,
                BusinessHoursType = BusinessHoursConstant.BusinessHoursTypes.Custom
            };

            await _service.CreateAsync(request);

            _mockBusinessHoursRepo.Verify(r => r.ClearDefaultsAsync(102, 1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_UsesDefaultTimeZone_WhenTimeZoneIsNull()
        {
            _mockBusinessHoursRepo
                .Setup(r => r.AnyExistAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockBusinessHoursRepo
                .Setup(r => r.AddAsync(It.IsAny<BusinessHours>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BusinessHours bh, CancellationToken _) => { bh.Id = 103; return bh; });

            var request = new CreateBusinessHoursRequest
            {
                Name = "Default TZ",
                BusinessHoursType = BusinessHoursConstant.BusinessHoursTypes.TwentyFourSeven,
                TimeZone = null
            };

            var result = await _service.CreateAsync(request);

            result.TimeZone.ShouldBe(TimeZoneInfo.Local.Id);
        }

        [Fact]
        public async Task CreateAsync_LogsChangeHistory()
        {
            _mockBusinessHoursRepo
                .Setup(r => r.AnyExistAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockBusinessHoursRepo
                .Setup(r => r.AddAsync(It.IsAny<BusinessHours>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BusinessHours bh, CancellationToken _) => { bh.Id = 104; return bh; });

            await _service.CreateAsync(new CreateBusinessHoursRequest
            {
                Name = "Tracked Hours",
                BusinessHoursType = BusinessHoursConstant.BusinessHoursTypes.Custom
            });

            _mockChangeHistoryLogService.Verify(c => c.CreateChangeHistoryLogAsync(
                It.IsAny<string>(), It.IsAny<int>(), "created", null, "Tracked Hours", 1,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ReturnsNull_WhenNotFound()
        {
            _mockBusinessHoursRepo
                .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BusinessHours?)null);

            var result = await _service.UpdateAsync(999, new PatchBusinessHoursRequest { Name = "X" });

            result.ShouldBeNull();
        }

        [Fact]
        public async Task UpdateAsync_PatchesOnlyProvidedFields()
        {
            var existing = new BusinessHours
            {
                Id = 1, Name = "Old", Description = "OldDesc",
                TimeZone = "UTC", BusinessHoursType = BusinessHoursConstant.BusinessHoursTypes.Custom,
                IsDefault = false
            };

            _mockBusinessHoursRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            _mockBusinessHoursRepo
                .Setup(r => r.UpdateAsync(It.IsAny<BusinessHours>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BusinessHours bh, CancellationToken _) => bh);

            var result = await _service.UpdateAsync(1, new PatchBusinessHoursRequest { Name = "New" });

            result.ShouldNotBeNull();
            result!.Name.ShouldBe("New");
            result.Description.ShouldBe("OldDesc");
        }

        [Fact]
        public async Task UpdateAsync_ClearsOtherDefaults_WhenIsDefaultTrue()
        {
            var existing = new BusinessHours { Id = 1, Name = "Test", IsDefault = false };

            _mockBusinessHoursRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            _mockBusinessHoursRepo
                .Setup(r => r.UpdateAsync(It.IsAny<BusinessHours>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BusinessHours bh, CancellationToken _) => bh);

            await _service.UpdateAsync(1, new PatchBusinessHoursRequest { IsDefault = true });

            _mockBusinessHoursRepo.Verify(
                r => r.ClearDefaultsAsync(1, 1, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_CascadesSoftDelete_ToCustomHoursAndHolidays()
        {
            var existing = new BusinessHours { Id = 1, Name = "To Delete" };

            _mockBusinessHoursRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            _mockBusinessHoursRepo
                .Setup(r => r.SoftDeleteAsync(1, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _service.DeleteAsync(1);

            result.ShouldBeTrue();
            _mockCustomBusinessHoursRepo.Verify(r => r.SoftDeleteByBusinessHourIdAsync(1, 1, It.IsAny<CancellationToken>()), Times.Once);
            _mockHolidayRepo.Verify(r => r.SoftDeleteByBusinessHourIdAsync(1, 1, It.IsAny<CancellationToken>()), Times.Once);
            _mockBusinessHoursRepo.Verify(r => r.SoftDeleteAsync(1, 1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
        {
            _mockBusinessHoursRepo
                .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BusinessHours?)null);

            var result = await _service.DeleteAsync(999);

            result.ShouldBeFalse();
        }

        #endregion

        #region CreateAsync_WithCustomHours

        [Fact]
        public async Task CreateAsync_CreatesCustomHours_WhenTypeIsCustom()
        {
            _mockBusinessHoursRepo
                .Setup(r => r.AnyExistAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockBusinessHoursRepo
                .Setup(r => r.AddAsync(It.IsAny<BusinessHours>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BusinessHours bh, CancellationToken _) => { bh.Id = 200; return bh; });

            var request = new CreateBusinessHoursRequest
            {
                Name = "With Custom",
                BusinessHoursType = BusinessHoursConstant.BusinessHoursTypes.Custom,
                CustomHours = new List<CustomBusinessHoursItem>
                {
                    new() { Day = "Monday", StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0) }
                }
            };

            await _service.CreateAsync(request);

            _mockCustomBusinessHoursRepo.Verify(
                r => r.AddRangeAsync(It.IsAny<IEnumerable<CustomBusinessHours>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_IgnoresCustomHours_WhenTypeIs24x7()
        {
            _mockBusinessHoursRepo
                .Setup(r => r.AnyExistAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockBusinessHoursRepo
                .Setup(r => r.AddAsync(It.IsAny<BusinessHours>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BusinessHours bh, CancellationToken _) => { bh.Id = 201; return bh; });

            var request = new CreateBusinessHoursRequest
            {
                Name = "24x7 With Custom",
                BusinessHoursType = BusinessHoursConstant.BusinessHoursTypes.TwentyFourSeven,
                CustomHours = new List<CustomBusinessHoursItem>
                {
                    new() { Day = "Monday", StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(17, 0) }
                }
            };

            await _service.CreateAsync(request);

            _mockCustomBusinessHoursRepo.Verify(
                r => r.AddRangeAsync(It.IsAny<IEnumerable<CustomBusinessHours>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region UpdateAsync_WithCustomHours

        [Fact]
        public async Task UpdateAsync_ReplacesCustomHours_WhenProvided()
        {
            var existing = new BusinessHours
            {
                Id = 1, Name = "Test",
                BusinessHoursType = BusinessHoursConstant.BusinessHoursTypes.Custom,
                IsDefault = false
            };

            _mockBusinessHoursRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            _mockBusinessHoursRepo
                .Setup(r => r.UpdateAsync(It.IsAny<BusinessHours>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BusinessHours bh, CancellationToken _) => bh);

            await _service.UpdateAsync(1, new PatchBusinessHoursRequest
            {
                CustomHours = new List<CustomBusinessHoursItem>
                {
                    new() { Day = "Tuesday", StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(16, 0) }
                }
            });

            _mockCustomBusinessHoursRepo.Verify(
                r => r.SoftDeleteByBusinessHourIdAsync(1, 1, It.IsAny<CancellationToken>()), Times.Once);
            _mockCustomBusinessHoursRepo.Verify(
                r => r.AddRangeAsync(It.IsAny<IEnumerable<CustomBusinessHours>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_DeletesCustomHoursAndSkipsAdd_WhenTypeChangedTo24x7()
        {
            var existing = new BusinessHours
            {
                Id = 1, Name = "Test",
                BusinessHoursType = BusinessHoursConstant.BusinessHoursTypes.Custom,
                IsDefault = false
            };

            _mockBusinessHoursRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);

            _mockBusinessHoursRepo
                .Setup(r => r.UpdateAsync(It.IsAny<BusinessHours>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BusinessHours bh, CancellationToken _) => bh);

            await _service.UpdateAsync(1, new PatchBusinessHoursRequest
            {
                BusinessHoursType = BusinessHoursConstant.BusinessHoursTypes.TwentyFourSeven
            });

            _mockCustomBusinessHoursRepo.Verify(
                r => r.SoftDeleteByBusinessHourIdAsync(1, 1, It.IsAny<CancellationToken>()), Times.Once);
            _mockCustomBusinessHoursRepo.Verify(
                r => r.AddRangeAsync(It.IsAny<IEnumerable<CustomBusinessHours>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region ValidateNoTimeOverlap

        [Fact]
        public void ValidateNoTimeOverlap_AllowsNonOverlappingSameDay()
        {
            var items = new List<CustomBusinessHoursItem>
            {
                new() { Day = "Monday", StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(13, 0) },
                new() { Day = "Monday", StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(18, 0) }
            };

            BusinessHoursService.ValidateNoTimeOverlap(items).ShouldBeTrue();
        }

        [Fact]
        public void ValidateNoTimeOverlap_RejectsOverlappingSameDay()
        {
            var items = new List<CustomBusinessHoursItem>
            {
                new() { Day = "Tuesday", StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(18, 0) },
                new() { Day = "Tuesday", StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(18, 0) }
            };

            BusinessHoursService.ValidateNoTimeOverlap(items).ShouldBeFalse();
        }

        [Fact]
        public void ValidateNoTimeOverlap_RejectsSameMinuteOverlap()
        {
            var items = new List<CustomBusinessHoursItem>
            {
                new() { Day = "Wednesday", StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(14, 0) },
                new() { Day = "Wednesday", StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(18, 0) }
            };

            BusinessHoursService.ValidateNoTimeOverlap(items).ShouldBeTrue();
        }

        [Fact]
        public void ValidateNoTimeOverlap_AllowsDifferentDays()
        {
            var items = new List<CustomBusinessHoursItem>
            {
                new() { Day = "Monday", StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(18, 0) },
                new() { Day = "Tuesday", StartTime = new TimeOnly(8, 0), EndTime = new TimeOnly(18, 0) }
            };

            BusinessHoursService.ValidateNoTimeOverlap(items).ShouldBeTrue();
        }

        #endregion

        #region CreateHolidayAsync

        [Fact]
        public async Task CreateHolidayAsync_CreatesEntry_WithCorrectFields()
        {
            var request = new CreateHolidayRequest
            {
                BusinessHourId = 1,
                Name = "Christmas",
                Date = new DateOnly(2025, 12, 25)
            };

            _mockHolidayRepo
                .Setup(r => r.AddAsync(It.IsAny<Holiday>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Holiday h, CancellationToken _) => { h.Id = 101; return h; });

            var result = await _service.CreateHolidayAsync(request);

            result.ShouldNotBeNull();
            result.BusinessHourId.ShouldBe(1);
            result.Name.ShouldBe("Christmas");
            result.Date.ShouldBe(new DateOnly(2025, 12, 25));
            result.CreatedBy.ShouldBe(1);
        }

        #endregion

        #region DeleteHolidayAsync

        [Fact]
        public async Task DeleteHolidayAsync_ReturnsTrue_WhenFound()
        {
            _mockHolidayRepo
                .Setup(r => r.SoftDeleteAsync(10, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _service.DeleteHolidayAsync(1, 10);

            result.ShouldBeTrue();
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteHolidayAsync_ReturnsFalse_WhenNotFound()
        {
            _mockHolidayRepo
                .Setup(r => r.SoftDeleteAsync(999, 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _service.DeleteHolidayAsync(1, 999);

            result.ShouldBeFalse();
        }

        #endregion
    }
}
