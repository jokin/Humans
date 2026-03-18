using Humans.Application.Interfaces;
using Humans.Domain.Constants;
using Humans.Domain.Entities;
using Humans.Domain.Enums;
using Humans.Infrastructure.Data;
using Humans.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Humans.Web.Controllers;

[Authorize]
[Route("Camps/{slug}/Events")]
[Route("Barrios/{slug}/Events")]
public class CampEventsController : Controller
{
    private readonly HumansDbContext _dbContext;
    private readonly ICampService _campService;
    private readonly UserManager<User> _userManager;
    private readonly IClock _clock;
    private readonly ILogger<CampEventsController> _logger;

    public CampEventsController(
        HumansDbContext dbContext,
        ICampService campService,
        UserManager<User> userManager,
        IClock clock,
        ILogger<CampEventsController> logger)
    {
        _dbContext = dbContext;
        _campService = campService;
        _userManager = userManager;
        _clock = clock;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string slug)
    {
        var ctx = await ResolveAsync(slug);
        if (ctx == null) return NotFound();
        if (!ctx.CanManage) return Forbid();

        var guideSettings = await _dbContext.GuideSettings
            .Include(g => g.EventSettings)
            .FirstOrDefaultAsync();

        var now = _clock.GetCurrentInstant();
        var isSubmissionOpen = guideSettings != null &&
                               now >= guideSettings.SubmissionOpenAt &&
                               now <= guideSettings.SubmissionCloseAt;

        DateTimeZone? tz = guideSettings?.EventSettings != null
            ? DateTimeZoneProviders.Tzdb.GetZoneOrNull(guideSettings.EventSettings.TimeZoneId)
            : null;

        var events = await _dbContext.GuideEvents
            .Include(e => e.Category)
            .Where(e => e.CampId == ctx.Camp.Id)
            .OrderByDescending(e => e.SubmittedAt)
            .ToListAsync();

        var model = new CampEventsTabViewModel
        {
            CampId = ctx.Camp.Id,
            CampName = ctx.CampName,
            CampSlug = slug,
            IsSubmissionOpen = isSubmissionOpen,
            SubmissionOpenAt = guideSettings != null ? ToLocalDateTime(guideSettings.SubmissionOpenAt, tz) : null,
            SubmissionCloseAt = guideSettings != null ? ToLocalDateTime(guideSettings.SubmissionCloseAt, tz) : null,
            TimeZoneId = guideSettings?.EventSettings?.TimeZoneId,
            SubmittedCount = events.Count,
            ApprovedCount = events.Count(e => e.Status == GuideEventStatus.Approved),
            PendingCount = events.Count(e => e.Status == GuideEventStatus.Pending),
            Events = events.Select(e => new CampEventRowViewModel
            {
                Id = e.Id,
                Title = e.Title,
                CategoryName = e.Category.Name,
                StartAt = ToLocalDateTime(e.StartAt, tz),
                DurationMinutes = e.DurationMinutes,
                Status = e.Status,
                PriorityRank = e.PriorityRank,
                CanEdit = e.Status is GuideEventStatus.Draft or GuideEventStatus.Rejected or GuideEventStatus.ResubmitRequested,
                CanWithdraw = e.Status is GuideEventStatus.Draft or GuideEventStatus.Pending
            }).ToList()
        };

        return View(model);
    }

    [HttpGet("New")]
    public async Task<IActionResult> New(string slug)
    {
        var ctx = await ResolveAsync(slug);
        if (ctx == null) return NotFound();
        if (!ctx.CanManage) return Forbid();

        var (open, guideSettings) = await CheckSubmissionWindowAsync();
        if (!open)
        {
            TempData["ErrorMessage"] = "The submission window is not currently open.";
            return RedirectToAction(nameof(Index), new { slug });
        }

        var model = await BuildFormAsync(slug, ctx, guideSettings!);
        return View("CampEventForm", model);
    }

    [HttpPost("New")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string slug, CampEventFormViewModel model)
    {
        var ctx = await ResolveAsync(slug);
        if (ctx == null) return NotFound();
        if (!ctx.CanManage) return Forbid();

        var (open, guideSettings) = await CheckSubmissionWindowAsync();
        if (!open)
        {
            TempData["ErrorMessage"] = "The submission window is not currently open.";
            return RedirectToAction(nameof(Index), new { slug });
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(model, guideSettings!);
            return View("CampEventForm", model);
        }

        var tz = GetTimeZone(guideSettings!);
        var now = _clock.GetCurrentInstant();

        var guideEvent = new GuideEvent
        {
            Id = Guid.NewGuid(),
            CampId = ctx.Camp.Id,
            SubmitterUserId = ctx.UserId,
            CategoryId = model.CategoryId,
            Title = model.Title,
            Description = model.Description,
            LocationNote = model.LocationNote,
            StartAt = ToInstant(model.StartDate.Add(model.StartTime), tz),
            DurationMinutes = model.DurationMinutes,
            IsRecurring = model.IsRecurring,
            RecurrenceDays = model.IsRecurring ? model.RecurrenceDays : null,
            PriorityRank = model.PriorityRank,
            Status = GuideEventStatus.Pending,
            SubmittedAt = now,
            LastUpdatedAt = now
        };

        _dbContext.GuideEvents.Add(guideEvent);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("User {UserId} submitted event '{Title}' for camp {CampId}",
            ctx.UserId, model.Title, ctx.Camp.Id);

        TempData["SuccessMessage"] = $"Event \"{model.Title}\" submitted for review.";
        return RedirectToAction(nameof(Index), new { slug });
    }

    [HttpGet("{eventId:guid}/Edit")]
    public async Task<IActionResult> Edit(string slug, Guid eventId)
    {
        var ctx = await ResolveAsync(slug);
        if (ctx == null) return NotFound();
        if (!ctx.CanManage) return Forbid();

        var guideEvent = await _dbContext.GuideEvents
            .FirstOrDefaultAsync(e => e.Id == eventId && e.CampId == ctx.Camp.Id);
        if (guideEvent == null) return NotFound();

        if (guideEvent.Status is not (GuideEventStatus.Draft or GuideEventStatus.Rejected or GuideEventStatus.ResubmitRequested))
        {
            TempData["ErrorMessage"] = "This event cannot be edited in its current state.";
            return RedirectToAction(nameof(Index), new { slug });
        }

        var guideSettings = await _dbContext.GuideSettings
            .Include(g => g.EventSettings)
            .FirstOrDefaultAsync();
        if (guideSettings == null)
        {
            TempData["ErrorMessage"] = "Guide settings not configured.";
            return RedirectToAction(nameof(Index), new { slug });
        }

        var tz = GetTimeZone(guideSettings);
        var localStart = ToLocalDateTime(guideEvent.StartAt, tz);

        var model = await BuildFormAsync(slug, ctx, guideSettings);
        model.Id = guideEvent.Id;
        model.Title = guideEvent.Title;
        model.Description = guideEvent.Description;
        model.CategoryId = guideEvent.CategoryId;
        model.StartDate = localStart.Date;
        model.StartTime = localStart.TimeOfDay;
        model.DurationMinutes = guideEvent.DurationMinutes;
        model.LocationNote = guideEvent.LocationNote;
        model.IsRecurring = guideEvent.IsRecurring;
        model.RecurrenceDays = guideEvent.RecurrenceDays;
        model.PriorityRank = guideEvent.PriorityRank;
        model.IsResubmit = guideEvent.Status is GuideEventStatus.Rejected or GuideEventStatus.ResubmitRequested;

        return View("CampEventForm", model);
    }

    [HttpPost("{eventId:guid}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string slug, Guid eventId, CampEventFormViewModel model)
    {
        var ctx = await ResolveAsync(slug);
        if (ctx == null) return NotFound();
        if (!ctx.CanManage) return Forbid();

        var guideEvent = await _dbContext.GuideEvents
            .FirstOrDefaultAsync(e => e.Id == eventId && e.CampId == ctx.Camp.Id);
        if (guideEvent == null) return NotFound();

        if (guideEvent.Status is not (GuideEventStatus.Draft or GuideEventStatus.Rejected or GuideEventStatus.ResubmitRequested))
        {
            TempData["ErrorMessage"] = "This event cannot be edited in its current state.";
            return RedirectToAction(nameof(Index), new { slug });
        }

        var guideSettings = await _dbContext.GuideSettings
            .Include(g => g.EventSettings)
            .FirstOrDefaultAsync();
        if (guideSettings == null)
        {
            TempData["ErrorMessage"] = "Guide settings not configured.";
            return RedirectToAction(nameof(Index), new { slug });
        }

        if (!ModelState.IsValid)
        {
            model.Id = eventId;
            await PopulateDropdownsAsync(model, guideSettings);
            return View("CampEventForm", model);
        }

        var tz = GetTimeZone(guideSettings);

        guideEvent.Title = model.Title;
        guideEvent.Description = model.Description;
        guideEvent.CategoryId = model.CategoryId;
        guideEvent.StartAt = ToInstant(model.StartDate.Add(model.StartTime), tz);
        guideEvent.DurationMinutes = model.DurationMinutes;
        guideEvent.LocationNote = model.LocationNote;
        guideEvent.IsRecurring = model.IsRecurring;
        guideEvent.RecurrenceDays = model.IsRecurring ? model.RecurrenceDays : null;
        guideEvent.PriorityRank = model.PriorityRank;

        // Resubmit: reset to Pending
        guideEvent.Submit(_clock);

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("User {UserId} updated event '{Title}' ({EventId})",
            ctx.UserId, model.Title, eventId);

        TempData["SuccessMessage"] = $"Event \"{model.Title}\" resubmitted for review.";
        return RedirectToAction(nameof(Index), new { slug });
    }

    [HttpPost("{eventId:guid}/Withdraw")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(string slug, Guid eventId)
    {
        var ctx = await ResolveAsync(slug);
        if (ctx == null) return NotFound();
        if (!ctx.CanManage) return Forbid();

        var guideEvent = await _dbContext.GuideEvents
            .FirstOrDefaultAsync(e => e.Id == eventId && e.CampId == ctx.Camp.Id);
        if (guideEvent == null) return NotFound();

        if (guideEvent.Status is not (GuideEventStatus.Draft or GuideEventStatus.Pending))
        {
            TempData["ErrorMessage"] = "This event cannot be withdrawn in its current state.";
            return RedirectToAction(nameof(Index), new { slug });
        }

        guideEvent.Withdraw(_clock);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("User {UserId} withdrew event '{Title}' ({EventId})",
            ctx.UserId, guideEvent.Title, eventId);

        TempData["SuccessMessage"] = $"Event \"{guideEvent.Title}\" withdrawn.";
        return RedirectToAction(nameof(Index), new { slug });
    }

    // ─── Helpers ──────────────────────────────────────────────────

    private sealed record ResolvedContext(Camp Camp, string CampName, Guid UserId, bool CanManage);

    private async Task<ResolvedContext?> ResolveAsync(string slug)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return null;

        var camp = await _campService.GetCampBySlugAsync(slug);
        if (camp == null) return null;

        var isLead = await _campService.IsUserCampLeadAsync(user.Id, camp.Id);
        var isAdmin = User.IsInRole(RoleNames.Admin);
        var isCampAdmin = User.IsInRole(RoleNames.CampAdmin);

        if (!isLead && !isAdmin && !isCampAdmin)
            return null;

        // Get camp name from current season, falling back to slug
        var currentSeason = camp.Seasons
            .OrderByDescending(s => s.Year)
            .FirstOrDefault();
        var campName = currentSeason?.Name ?? camp.Slug;

        return new ResolvedContext(camp, campName, user.Id, isLead || isAdmin || isCampAdmin);
    }

    private async Task<(bool IsOpen, GuideSettings? Settings)> CheckSubmissionWindowAsync()
    {
        var guideSettings = await _dbContext.GuideSettings
            .Include(g => g.EventSettings)
            .FirstOrDefaultAsync();

        if (guideSettings == null) return (false, null);

        var now = _clock.GetCurrentInstant();
        var isOpen = now >= guideSettings.SubmissionOpenAt && now <= guideSettings.SubmissionCloseAt;
        return (isOpen, guideSettings);
    }

    private async Task<CampEventFormViewModel> BuildFormAsync(string slug, ResolvedContext ctx, GuideSettings guideSettings)
    {
        var model = new CampEventFormViewModel
        {
            CampId = ctx.Camp.Id,
            CampName = ctx.CampName,
            CampSlug = slug,
            TimeZoneId = guideSettings.EventSettings.TimeZoneId
        };

        await PopulateDropdownsAsync(model, guideSettings);
        return model;
    }

    private async Task PopulateDropdownsAsync(CampEventFormViewModel model, GuideSettings guideSettings)
    {
        model.Categories = await _dbContext.EventCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new CategoryOptionViewModel { Id = c.Id, Name = c.Name })
            .ToListAsync();

        model.TimeZoneId = guideSettings.EventSettings.TimeZoneId;

        var es = guideSettings.EventSettings;
        var tz = DateTimeZoneProviders.Tzdb.GetZoneOrNull(es.TimeZoneId);

        // Build event day options from gate opening + offsets
        model.EventDays = [];
        for (var offset = 0; offset <= es.EventEndOffset; offset++)
        {
            var date = es.GateOpeningDate.PlusDays(offset);
            var dt = tz != null
                ? date.AtStartOfDayInZone(tz).ToDateTimeUnspecified()
                : new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);

            model.EventDays.Add(new EventDayOptionViewModel
            {
                DayOffset = offset,
                Label = date.ToString("ddd d MMM", null),
                Date = dt
            });
        }
    }

    private static DateTimeZone? GetTimeZone(GuideSettings guideSettings)
    {
        return DateTimeZoneProviders.Tzdb.GetZoneOrNull(guideSettings.EventSettings.TimeZoneId);
    }

    private static DateTime ToLocalDateTime(Instant instant, DateTimeZone? tz)
    {
        if (tz == null)
            return instant.ToDateTimeUtc();
        return instant.InZone(tz).ToDateTimeUnspecified();
    }

    private static Instant ToInstant(DateTime dateTime, DateTimeZone? tz)
    {
        if (tz == null)
            return Instant.FromDateTimeUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
        var local = LocalDateTime.FromDateTime(dateTime);
        return local.InZoneLeniently(tz).ToInstant();
    }
}
