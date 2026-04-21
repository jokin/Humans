# In-App Guide Section — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Render `docs/guide/*.md` inside the app at `/Guide` with role-aware section filtering, link/image rewriting, and admin-triggered GitHub refresh — backed by an in-memory cache, no database.

**Architecture:** One controller + three Infrastructure services + a pure-Application filter. `IGuideContentSource` (Octokit) fetches raw markdown from the Humans repo; `IGuideRenderer` turns it into HTML with role-block `<div>` wrappers and rewritten links/images; `IGuideContentService` owns the `IMemoryCache` and orchestrates refreshes; `IGuideRoleResolver` builds the per-request `GuideRoleContext`; `GuideFilter` strips HTML blocks the user can't see. `GuideController` wires it together.

**Tech Stack:** .NET 10 (ASP.NET Core MVC), Markdig, Octokit, `Microsoft.Extensions.Caching.Memory`, xUnit + AwesomeAssertions, EF Core InMemory (tests), NodaTime.

**Reference the spec throughout:** `docs/superpowers/specs/2026-04-21-in-app-guide-design.md`

---

## Conventions & facts to know before starting

1. **Test project layout.** Unit tests for both Application and Infrastructure services live in `tests/Humans.Application.Tests/` — that project already `<ProjectReference>`s `Humans.Infrastructure` and `Humans.Web`. No separate Infrastructure.Tests or Web.Tests project exists; do not create one.
2. **`GitHubSettings` points at the legal repo, not Humans.** Its `Repository` default is `"legal"`. The guide needs a different source, so this plan introduces a separate `GuideSettings` config section with defaults `Owner = "nobodies-collective"`, `Repository = "Humans"`, `Branch = "main"`, `FolderPath = "docs/guide"`. Do not reuse `GitHubSettings`.
3. **Markdig convention.** Per `HtmlHelperExtensions.cs:20-22`, the app uses `new MarkdownPipelineBuilder().UseAdvancedExtensions().Build()`. Mirror that for `GuideRenderer`.
4. **Authorization.** Admin-only routes use `[Authorize(Policy = PolicyNames.AdminOnly)]`. Do NOT use `[Authorize(Roles = "Admin")]` — that bypasses the project's policy pattern.
5. **Role constants.** `Humans.Domain.Constants.RoleNames` holds `Admin`, `Board`, `ConsentCoordinator`, `VolunteerCoordinator`, `TeamsAdmin`, `CampAdmin`, `TicketAdmin`, `NoInfoAdmin`, `FeedbackAdmin`, `HumanAdmin`, `FinanceAdmin`.
6. **Team-coordinator detection.** `TeamMember.Role == TeamMemberRole.Coordinator` with `LeftAt == null`.
7. **Nav link idiom.** Add a `<li class="nav-item">` to `src/Humans.Web/Views/Shared/_Layout.cshtml`, use `asp-controller="Guide" asp-action="Index"`. The Legal link at line 70 is a good template.
8. **Commit after each task passes build + tests.** Small, frequent commits.

---

## File structure (locked)

### New — Application (`src/Humans.Application/`)

| File | Responsibility |
|---|---|
| `Interfaces/IGuideContentService.cs` | Public façade: `GetRenderedAsync(stem)`, `RefreshAllAsync()` |
| `Interfaces/IGuideContentSource.cs` | Abstracts GitHub fetch for testability |
| `Interfaces/IGuideRenderer.cs` | Markdown → annotated HTML |
| `Interfaces/IGuideRoleResolver.cs` | Builds `GuideRoleContext` from a `ClaimsPrincipal` |
| `Models/GuideRoleContext.cs` | `(IsAuthenticated, IsTeamCoordinator, SystemRoles)` record |
| `Services/GuideFilter.cs` | Pure static: strip role-blocks user can't see |
| `Services/GuideRolePrivilegeMap.cs` | Maps display names (`"Camp Admin"`) → `RoleNames.CampAdmin` |
| `Constants/GuideFiles.cs` | The 17 file stems, in sidebar order |

### New — Infrastructure (`src/Humans.Infrastructure/`)

| File | Responsibility |
|---|---|
| `Configuration/GuideSettings.cs` | Owner/Repository/Branch/FolderPath/CacheTtl |
| `Services/GuideContentService.cs` | Owns `IMemoryCache`, orchestrates source+renderer |
| `Services/GuideRenderer.cs` | Markdig pipeline, pre/post-processing |
| `Services/GuideMarkdownPreprocessor.cs` | Wraps `## As a …` blocks in `<div>`s |
| `Services/GuideHtmlPostprocessor.cs` | Rewrites links and images in rendered HTML |
| `Services/GuideRoleResolver.cs` | Reads `User.IsInRole` + `TeamMember` DB check |
| `Services/GitHubGuideContentSource.cs` | Octokit-based implementation |

### New — Web (`src/Humans.Web/`)

| File | Responsibility |
|---|---|
| `Controllers/GuideController.cs` | `/Guide`, `/Guide/{name}`, `POST /Guide/Refresh` |
| `Models/GuideViewModel.cs` | Title, Html, SidebarModel |
| `Models/GuideSidebarModel.cs` | Start/Sections/Appendix entries + active stem |
| `Views/Guide/Index.cshtml` | Renders README at `/Guide` |
| `Views/Guide/Document.cshtml` | Renders named page at `/Guide/{name}` |
| `Views/Guide/NotFound.cshtml` | Unknown file view |
| `Views/Guide/Unavailable.cshtml` | GitHub-down cold-cache view |
| `Views/Shared/_GuideLayout.cshtml` | Sidebar + breadcrumb wrapper |

### New — Tests (`tests/Humans.Application.Tests/`)

| File | Covers |
|---|---|
| `Services/GuideRolePrivilegeMapTests.cs` | Display-name → system-role mapping |
| `Services/GuideMarkdownPreprocessorTests.cs` | `## As a …` → `<div>` wrapping, parenthetical capture |
| `Services/GuideHtmlPostprocessorTests.cs` | Link/image rewriting rules |
| `Services/GuideRendererTests.cs` | End-to-end markdown → annotated HTML |
| `Services/GuideFilterTests.cs` | Per-user visibility decisions |
| `Services/GuideRoleResolverTests.cs` | In-memory DbContext coordinator check |
| `Services/GuideContentServiceTests.cs` | Cache hit/miss, refresh, fault tolerance |

### New — Docs

| File | Responsibility |
|---|---|
| `docs/features/39-in-app-guide.md` | Feature spec (business context, workflows, data model) |
| `docs/sections/Guide.md` | Section invariants (actors, invariants, triggers) |

### Modified

| File | Change |
|---|---|
| `src/Humans.Web/Views/Shared/_Layout.cshtml` | Add "Guide" nav link |
| `src/Humans.Web/Extensions/InfrastructureServiceCollectionExtensions.cs` | Register GuideSettings + 6 new services |
| `src/Humans.Infrastructure/Humans.Infrastructure.csproj` | (No package changes — Markdig and Octokit already present) |
| `docs/guide/Teams.md` | Heading → `## As a Board member / Admin (Teams Admin)` |
| `docs/guide/Profiles.md` | Heading → `## As a Board member / Admin (Human Admin)` |
| `docs/guide/Shifts.md` | Heading → `## As a Board member / Admin (NoInfo Admin)` |
| `docs/guide/Feedback.md` | Heading → `## As a Board member / Admin (Feedback Admin)` |
| `docs/guide/Budget.md` | Heading → `## As a Board member / Admin (Finance Admin)` |
| `docs/guide/Onboarding.md` | Heading → `## As a Board member / Admin (Human Admin)` |

---

## Task 1 — Scaffold contracts (no logic yet)

**Why:** Lock the public shapes so downstream tasks can be written independently.

**Files:**

- Create: `src/Humans.Application/Models/GuideRoleContext.cs`
- Create: `src/Humans.Application/Constants/GuideFiles.cs`
- Create: `src/Humans.Application/Interfaces/IGuideContentService.cs`
- Create: `src/Humans.Application/Interfaces/IGuideContentSource.cs`
- Create: `src/Humans.Application/Interfaces/IGuideRenderer.cs`
- Create: `src/Humans.Application/Interfaces/IGuideRoleResolver.cs`

- [ ] **Step 1: Create `GuideRoleContext.cs`**

```csharp
namespace Humans.Application.Models;

public sealed record GuideRoleContext(
    bool IsAuthenticated,
    bool IsTeamCoordinator,
    IReadOnlySet<string> SystemRoles)
{
    public static readonly GuideRoleContext Anonymous =
        new(false, false, new HashSet<string>(StringComparer.Ordinal));
}
```

- [ ] **Step 2: Create `GuideFiles.cs`**

```csharp
namespace Humans.Application.Constants;

/// <summary>
/// The set of markdown files rendered at /Guide/{stem}. Order mirrors docs/guide/README.md.
/// </summary>
public static class GuideFiles
{
    public const string Readme = "README";
    public const string GettingStarted = "GettingStarted";
    public const string Glossary = "Glossary";

    public static readonly IReadOnlyList<string> Sections =
    [
        "Profiles",
        "Onboarding",
        "LegalAndConsent",
        "Teams",
        "Shifts",
        "Tickets",
        "Camps",
        "Email",
        "Campaigns",
        "Feedback",
        "Governance",
        "Budget",
        "CityPlanning",
        "GoogleIntegration",
        "Admin"
    ];

    public static readonly IReadOnlySet<string> All = BuildAll();

    private static IReadOnlySet<string> BuildAll()
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Readme,
            GettingStarted,
            Glossary
        };
        foreach (var section in Sections)
        {
            set.Add(section);
        }
        return set;
    }
}
```

- [ ] **Step 3: Create `IGuideContentService.cs`**

```csharp
namespace Humans.Application.Interfaces;

/// <summary>
/// Public façade for retrieving rendered guide pages. Owns the memory cache.
/// </summary>
public interface IGuideContentService
{
    /// <summary>
    /// Returns the rendered, role-annotated HTML for a guide file. Triggers a
    /// full refresh if the cache is cold. Throws <see cref="GuideContentUnavailableException"/>
    /// when GitHub is unreachable and no stale content is available.
    /// </summary>
    Task<string> GetRenderedAsync(string fileStem, CancellationToken cancellationToken = default);

    /// <summary>
    /// Evicts all cached entries and re-fetches every known guide file from GitHub.
    /// </summary>
    Task RefreshAllAsync(CancellationToken cancellationToken = default);
}

public sealed class GuideContentUnavailableException : Exception
{
    public GuideContentUnavailableException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}
```

- [ ] **Step 4: Create `IGuideContentSource.cs`**

```csharp
namespace Humans.Application.Interfaces;

/// <summary>
/// Abstracts the GitHub fetch so GuideContentService is testable without network.
/// </summary>
public interface IGuideContentSource
{
    /// <summary>
    /// Fetches the raw markdown for one guide file by stem (e.g. "Profiles" → Profiles.md).
    /// </summary>
    Task<string> GetMarkdownAsync(string fileStem, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 5: Create `IGuideRenderer.cs`**

```csharp
namespace Humans.Application.Interfaces;

/// <summary>
/// Renders guide markdown to HTML with role-section div wrappers and rewritten
/// links/images. Pure function of (markdown, file stem) — safe to cache the output.
/// </summary>
public interface IGuideRenderer
{
    string Render(string markdown, string fileStem);
}
```

- [ ] **Step 6: Create `IGuideRoleResolver.cs`**

```csharp
using System.Security.Claims;
using Humans.Application.Models;

namespace Humans.Application.Interfaces;

/// <summary>
/// Builds a <see cref="GuideRoleContext"/> for the current user: reads system roles
/// from claims and checks the database for any active team-coordinator assignment.
/// </summary>
public interface IGuideRoleResolver
{
    Task<GuideRoleContext> ResolveAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 7: Build the solution, confirm the new files compile**

Run: `dotnet build Humans.slnx`
Expected: `Build succeeded`

- [ ] **Step 8: Commit**

```bash
git add src/Humans.Application/
git commit -m "Scaffold IGuideContentService / IGuideRenderer / IGuideRoleResolver contracts"
```

---

## Task 2 — `GuideSettings` configuration class

**Why:** Guide content comes from the Humans repo, not the legal repo `GitHubSettings` points at. A dedicated config lets operators override owner/branch/TTL separately.

**Files:**

- Create: `src/Humans.Infrastructure/Configuration/GuideSettings.cs`

- [ ] **Step 1: Create `GuideSettings.cs`**

```csharp
namespace Humans.Infrastructure.Configuration;

/// <summary>
/// Configuration for the in-app Guide section. Source location and cache behaviour.
/// </summary>
public class GuideSettings
{
    public const string SectionName = "Guide";

    /// <summary>GitHub owner (defaults to nobodies-collective).</summary>
    public string Owner { get; set; } = "nobodies-collective";

    /// <summary>GitHub repository (defaults to Humans).</summary>
    public string Repository { get; set; } = "Humans";

    /// <summary>Branch to read guide content from.</summary>
    public string Branch { get; set; } = "main";

    /// <summary>Folder inside the repo that contains the guide markdown files.</summary>
    public string FolderPath { get; set; } = "docs/guide";

    /// <summary>Cache TTL in hours for rendered guide pages. Sliding expiration.</summary>
    public int CacheTtlHours { get; set; } = 6;

    /// <summary>Optional personal access token. Falls back to GitHubSettings.AccessToken if empty.</summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Returns the raw.githubusercontent.com URL prefix for image references inside
    /// guide content: "https://raw.githubusercontent.com/{Owner}/{Repository}/{Branch}/{FolderPath}/".
    /// </summary>
    public string RawContentBaseUrl =>
        $"https://raw.githubusercontent.com/{Owner}/{Repository}/{Branch}/{FolderPath.TrimEnd('/')}/";
}
```

- [ ] **Step 2: Build**

Run: `dotnet build Humans.slnx`
Expected: `Build succeeded`

- [ ] **Step 3: Commit**

```bash
git add src/Humans.Infrastructure/Configuration/GuideSettings.cs
git commit -m "Add GuideSettings config class (separate from GitHubSettings)"
```

---

## Task 3 — `GuideRolePrivilegeMap` helper

**Why:** Parenthetical parsing and user-facing display-name ↔ system-role matching appear in multiple components. Centralise the mapping so it's defined once.

**Files:**

- Create: `src/Humans.Application/Services/GuideRolePrivilegeMap.cs`
- Test: `tests/Humans.Application.Tests/Services/GuideRolePrivilegeMapTests.cs`

- [ ] **Step 1: Write the failing test**

```csharp
using AwesomeAssertions;
using Humans.Application.Services;
using Humans.Domain.Constants;
using Xunit;

namespace Humans.Application.Tests.Services;

public class GuideRolePrivilegeMapTests
{
    [Theory]
    [InlineData("Camp Admin", RoleNames.CampAdmin)]
    [InlineData("camp admin", RoleNames.CampAdmin)]
    [InlineData("Teams Admin", RoleNames.TeamsAdmin)]
    [InlineData("NoInfo Admin", RoleNames.NoInfoAdmin)]
    [InlineData("Human Admin", RoleNames.HumanAdmin)]
    [InlineData("Finance Admin", RoleNames.FinanceAdmin)]
    [InlineData("Feedback Admin", RoleNames.FeedbackAdmin)]
    [InlineData("Ticket Admin", RoleNames.TicketAdmin)]
    [InlineData("Consent Coordinator", RoleNames.ConsentCoordinator)]
    [InlineData("Volunteer Coordinator", RoleNames.VolunteerCoordinator)]
    public void TryResolve_KnownDisplayName_ReturnsSystemRole(string displayName, string expectedRole)
    {
        GuideRolePrivilegeMap.TryResolve(displayName, out var role).Should().BeTrue();
        role.Should().Be(expectedRole);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Unknown")]
    [InlineData("Camp Coordinator")] // A team-coordinator, not a system role
    public void TryResolve_UnknownOrEmpty_ReturnsFalse(string input)
    {
        GuideRolePrivilegeMap.TryResolve(input, out var role).Should().BeFalse();
        role.Should().BeNull();
    }

    [Fact]
    public void ParseParenthetical_MultipleCommaSeparated_ReturnsAll()
    {
        var result = GuideRolePrivilegeMap.ParseParenthetical("Camp Admin, Finance Admin");

        result.Should().BeEquivalentTo([RoleNames.CampAdmin, RoleNames.FinanceAdmin]);
    }

    [Fact]
    public void ParseParenthetical_UnknownTokensDropped()
    {
        var result = GuideRolePrivilegeMap.ParseParenthetical("Camp Admin, Gibberish");

        result.Should().BeEquivalentTo([RoleNames.CampAdmin]);
    }

    [Fact]
    public void ParseParenthetical_NullOrEmpty_ReturnsEmpty()
    {
        GuideRolePrivilegeMap.ParseParenthetical(null).Should().BeEmpty();
        GuideRolePrivilegeMap.ParseParenthetical("").Should().BeEmpty();
        GuideRolePrivilegeMap.ParseParenthetical("   ").Should().BeEmpty();
    }
}
```

- [ ] **Step 2: Run test, confirm it fails**

Run: `dotnet test tests/Humans.Application.Tests/Humans.Application.Tests.csproj --filter "FullyQualifiedName~GuideRolePrivilegeMapTests"`
Expected: Compile error or FAIL ("GuideRolePrivilegeMap does not exist").

- [ ] **Step 3: Implement `GuideRolePrivilegeMap.cs`**

```csharp
using System.Diagnostics.CodeAnalysis;
using Humans.Domain.Constants;

namespace Humans.Application.Services;

/// <summary>
/// Maps display names that appear in guide-heading parentheticals (e.g. "Camp Admin",
/// "Consent Coordinator") to the system role constants defined in <see cref="RoleNames"/>.
/// </summary>
public static class GuideRolePrivilegeMap
{
    private static readonly IReadOnlyDictionary<string, string> DisplayToRole =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Admin"] = RoleNames.Admin,
            ["Board"] = RoleNames.Board,
            ["Teams Admin"] = RoleNames.TeamsAdmin,
            ["Camp Admin"] = RoleNames.CampAdmin,
            ["Ticket Admin"] = RoleNames.TicketAdmin,
            ["NoInfo Admin"] = RoleNames.NoInfoAdmin,
            ["No Info Admin"] = RoleNames.NoInfoAdmin,
            ["Feedback Admin"] = RoleNames.FeedbackAdmin,
            ["Human Admin"] = RoleNames.HumanAdmin,
            ["Finance Admin"] = RoleNames.FinanceAdmin,
            ["Consent Coordinator"] = RoleNames.ConsentCoordinator,
            ["Volunteer Coordinator"] = RoleNames.VolunteerCoordinator
        };

    public static bool TryResolve(string displayName, [NotNullWhen(true)] out string? systemRole)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            systemRole = null;
            return false;
        }

        return DisplayToRole.TryGetValue(displayName.Trim(), out systemRole);
    }

    /// <summary>
    /// Parses a guide-heading parenthetical like "Camp Admin, Finance Admin" into the
    /// matching system-role constants. Unknown tokens are skipped (not thrown).
    /// </summary>
    public static IReadOnlyList<string> ParseParenthetical(string? paren)
    {
        if (string.IsNullOrWhiteSpace(paren))
        {
            return [];
        }

        var result = new List<string>();
        foreach (var token in paren.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (TryResolve(token, out var role))
            {
                result.Add(role);
            }
        }
        return result;
    }
}
```

- [ ] **Step 4: Run tests, confirm pass**

Run: `dotnet test tests/Humans.Application.Tests/Humans.Application.Tests.csproj --filter "FullyQualifiedName~GuideRolePrivilegeMapTests"`
Expected: All tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/Humans.Application/Services/GuideRolePrivilegeMap.cs tests/Humans.Application.Tests/Services/GuideRolePrivilegeMapTests.cs
git commit -m "Add GuideRolePrivilegeMap for parenthetical display-name → system-role mapping"
```

---

## Task 4 — `GuideMarkdownPreprocessor` (role-block wrapping)

**Why:** Markdig can't natively identify role-section boundaries from heading text. Pre-processing wraps each `## As a …` block in raw `<div data-guide-role="…">` so the resulting HTML carries the role metadata per block.

**Files:**

- Create: `src/Humans.Infrastructure/Services/GuideMarkdownPreprocessor.cs`
- Test: `tests/Humans.Application.Tests/Services/GuideMarkdownPreprocessorTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
using AwesomeAssertions;
using Humans.Infrastructure.Services;
using Xunit;

namespace Humans.Application.Tests.Services;

public class GuideMarkdownPreprocessorTests
{
    private static readonly GuideMarkdownPreprocessor Preprocessor = new();

    [Fact]
    public void Wrap_VolunteerBlock_WrapsWithDivVolunteerRole()
    {
        const string input = """
            # Profiles

            ## What this section is for

            Intro.

            ## As a Volunteer

            Do volunteer things.

            ## Related sections
            """;

        var result = Preprocessor.Wrap(input);

        result.Should().Contain("<div data-guide-role=\"volunteer\" data-guide-roles=\"\">");
        result.Should().Contain("## As a Volunteer");
        result.Should().Contain("</div>");
    }

    [Fact]
    public void Wrap_CoordinatorWithParenthetical_CapturesRoles()
    {
        const string input = """
            ## As a Coordinator (Consent Coordinator)

            Do consent-coordinator things.

            ## Related sections
            """;

        var result = Preprocessor.Wrap(input);

        result.Should().Contain("<div data-guide-role=\"coordinator\" data-guide-roles=\"ConsentCoordinator\">");
    }

    [Fact]
    public void Wrap_BoardAdminWithParenthetical_CapturesSystemRole()
    {
        const string input = """
            ## As a Board member / Admin (Camp Admin)

            Do camp admin things.
            """;

        var result = Preprocessor.Wrap(input);

        result.Should().Contain("<div data-guide-role=\"boardadmin\" data-guide-roles=\"CampAdmin\">");
    }

    [Fact]
    public void Wrap_HeadingWithGlossaryLink_StillMatches()
    {
        const string input = """
            ## As a [Volunteer](Glossary.md#volunteer)

            Content.
            """;

        var result = Preprocessor.Wrap(input);

        result.Should().Contain("<div data-guide-role=\"volunteer\" data-guide-roles=\"\">");
    }

    [Fact]
    public void Wrap_ClosesDivBeforeNextSectionHeading()
    {
        const string input = """
            ## As a Volunteer

            Volunteer stuff.

            ## As a Coordinator

            Coordinator stuff.
            """;

        var result = Preprocessor.Wrap(input);

        // A closing div must appear before the next As-a heading's opening div.
        var firstOpen = result.IndexOf("<div data-guide-role=\"volunteer\"", StringComparison.Ordinal);
        var firstClose = result.IndexOf("</div>", firstOpen, StringComparison.Ordinal);
        var secondOpen = result.IndexOf("<div data-guide-role=\"coordinator\"", StringComparison.Ordinal);

        firstOpen.Should().BeGreaterThan(-1);
        firstClose.Should().BeGreaterThan(firstOpen);
        secondOpen.Should().BeGreaterThan(firstClose);
    }

    [Fact]
    public void Wrap_ClosesDivAtRelatedSectionsHeading()
    {
        const string input = """
            ## As a Volunteer

            Content.

            ## Related sections

            See other stuff.
            """;

        var result = Preprocessor.Wrap(input);

        var open = result.IndexOf("<div data-guide-role=\"volunteer\"", StringComparison.Ordinal);
        var close = result.IndexOf("</div>", open, StringComparison.Ordinal);
        var related = result.IndexOf("## Related sections", StringComparison.Ordinal);

        open.Should().BeGreaterThan(-1);
        close.Should().BeGreaterThan(open);
        related.Should().BeGreaterThan(close);
    }

    [Fact]
    public void Wrap_NoAsAHeadings_ReturnsInputUnchanged()
    {
        const string input = """
            # Glossary

            ## Admin

            A human with full access.

            ## Board

            The governance body.
            """;

        var result = Preprocessor.Wrap(input);

        result.Should().Be(input);
    }

    [Fact]
    public void Wrap_BlankLineBeforeAndAfterDiv_SoMarkdigRendersInner()
    {
        const string input = """
            Intro paragraph.

            ## As a Volunteer

            Content.
            """;

        var result = Preprocessor.Wrap(input);

        // Markdig requires HTML block tags to be separated from inline markdown by blank lines.
        result.Should().Contain("\n\n<div data-guide-role=\"volunteer\"");
    }

    [Fact]
    public void Wrap_ParentheticalWithUnknownToken_OmitsUnknown()
    {
        const string input = """
            ## As a Board member / Admin (Camp Admin, Mystery Role)

            Content.
            """;

        var result = Preprocessor.Wrap(input);

        result.Should().Contain("data-guide-roles=\"CampAdmin\"");
        result.Should().NotContain("Mystery");
    }
}
```

- [ ] **Step 2: Run tests, confirm they fail**

Run: `dotnet test tests/Humans.Application.Tests/Humans.Application.Tests.csproj --filter "FullyQualifiedName~GuideMarkdownPreprocessorTests"`
Expected: compile error or FAIL.

- [ ] **Step 3: Implement `GuideMarkdownPreprocessor.cs`**

```csharp
using System.Text;
using System.Text.RegularExpressions;
using Humans.Application.Services;

namespace Humans.Infrastructure.Services;

/// <summary>
/// Wraps each "## As a …" block in a raw HTML div carrying role metadata so the
/// rendered HTML can be role-filtered per request. Blocks end at the next "## " heading
/// or EOF.
/// </summary>
public sealed class GuideMarkdownPreprocessor
{
    private static readonly Regex RoleHeading = new(
        @"^##\s+As\s+an?\s+(?:\[)?(?<head>Volunteer|Coordinator|Board)[^\n]*?(?:\((?<paren>[^)]+)\))?\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(250));

    private static readonly Regex AnyH2 = new(
        @"^##\s+",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100));

    public string Wrap(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);

        var lines = markdown.Split('\n');
        var output = new StringBuilder(markdown.Length + 256);
        var inBlock = false;

        for (var i = 0; i < lines.Length; i++)
        {
            var rawLine = lines[i];
            var line = rawLine.EndsWith('\r') ? rawLine[..^1] : rawLine;

            var roleMatch = RoleHeading.Match(line);
            if (roleMatch.Success)
            {
                if (inBlock)
                {
                    AppendDivClose(output);
                }

                var head = roleMatch.Groups["head"].Value.ToLowerInvariant() switch
                {
                    "volunteer" => "volunteer",
                    "coordinator" => "coordinator",
                    "board" => "boardadmin",
                    _ => "volunteer"
                };

                var parenContent = roleMatch.Groups["paren"].Success
                    ? roleMatch.Groups["paren"].Value
                    : null;
                var roles = GuideRolePrivilegeMap.ParseParenthetical(parenContent);
                var rolesAttr = string.Join(",", roles);

                output.Append('\n');
                output.Append($"<div data-guide-role=\"{head}\" data-guide-roles=\"{rolesAttr}\">");
                output.Append('\n');
                output.Append('\n');
                output.Append(rawLine);
                output.Append('\n');
                inBlock = true;
                continue;
            }

            if (inBlock && AnyH2.IsMatch(line))
            {
                AppendDivClose(output);
                inBlock = false;
            }

            output.Append(rawLine);
            if (i < lines.Length - 1)
            {
                output.Append('\n');
            }
        }

        if (inBlock)
        {
            AppendDivClose(output);
        }

        return output.ToString();
    }

    private static void AppendDivClose(StringBuilder sb)
    {
        sb.Append('\n');
        sb.Append("</div>");
        sb.Append('\n');
        sb.Append('\n');
    }
}
```

- [ ] **Step 4: Run tests, confirm pass**

Run: `dotnet test tests/Humans.Application.Tests/Humans.Application.Tests.csproj --filter "FullyQualifiedName~GuideMarkdownPreprocessorTests"`
Expected: All tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/Humans.Infrastructure/Services/GuideMarkdownPreprocessor.cs tests/Humans.Application.Tests/Services/GuideMarkdownPreprocessorTests.cs
git commit -m "Add GuideMarkdownPreprocessor to wrap role-section blocks"
```

---

## Task 5 — `GuideHtmlPostprocessor` (link + image rewriting)

**Why:** Markdig produces standard `<a href>` and `<img src>` markup. Post-processing rewrites sibling `.md` links to `/Guide/…` routes, relative paths deeper than `../` to GitHub web URLs, external links get `target="_blank"`, and image paths get anchored at the `raw.githubusercontent.com` URL.

**Files:**

- Create: `src/Humans.Infrastructure/Services/GuideHtmlPostprocessor.cs`
- Test: `tests/Humans.Application.Tests/Services/GuideHtmlPostprocessorTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
using AwesomeAssertions;
using Humans.Application.Constants;
using Humans.Infrastructure.Configuration;
using Humans.Infrastructure.Services;
using Xunit;

namespace Humans.Application.Tests.Services;

public class GuideHtmlPostprocessorTests
{
    private static readonly GuideSettings Settings = new()
    {
        Owner = "nobodies-collective",
        Repository = "Humans",
        Branch = "main",
        FolderPath = "docs/guide"
    };

    private static readonly GuideHtmlPostprocessor Processor = new();

    [Fact]
    public void Rewrite_SiblingMdLink_BecomesGuideRoute()
    {
        const string html = """<a href="Profiles.md">Profiles</a>""";

        var result = Processor.Rewrite(html, Settings, GuideFiles.All);

        result.Should().Contain("""href="/Guide/Profiles" """.Trim());
    }

    [Fact]
    public void Rewrite_SiblingMdWithFragment_PreservesFragment()
    {
        const string html = """<a href="Glossary.md#coordinator">Coordinator</a>""";

        var result = Processor.Rewrite(html, Settings, GuideFiles.All);

        result.Should().Contain("""href="/Guide/Glossary#coordinator" """.Trim());
    }

    [Fact]
    public void Rewrite_SiblingMdCaseInsensitive_MatchesKnown()
    {
        const string html = """<a href="profiles.md">Profiles</a>""";

        var result = Processor.Rewrite(html, Settings, GuideFiles.All);

        result.Should().Contain("/Guide/Profiles");
    }

    [Fact]
    public void Rewrite_UnknownSiblingMd_LeftAsExternal()
    {
        const string html = """<a href="NonExistent.md">Link</a>""";

        var result = Processor.Rewrite(html, Settings, GuideFiles.All);

        // Unknown siblings fall through to external github.com URL.
        result.Should().Contain("https://github.com/nobodies-collective/Humans/blob/main/docs/guide/NonExistent.md");
        result.Should().Contain("target=\"_blank\"");
    }

    [Fact]
    public void Rewrite_AppPathLink_LeftAsIs()
    {
        const string html = """<a href="/Profile/Me/Edit">Edit</a>""";

        var result = Processor.Rewrite(html, Settings, GuideFiles.All);

        result.Should().Contain("""href="/Profile/Me/Edit" """.Trim());
        result.Should().NotContain("target=\"_blank\"");
    }

    [Fact]
    public void Rewrite_ExternalHttpLink_GetsNewTabAttrs()
    {
        const string html = """<a href="https://example.com/foo">x</a>""";

        var result = Processor.Rewrite(html, Settings, GuideFiles.All);

        result.Should().Contain("target=\"_blank\"");
        result.Should().Contain("rel=\"noopener\"");
    }

    [Fact]
    public void Rewrite_MailtoLink_LeftAsIs()
    {
        const string html = """<a href="mailto:a@b.com">a</a>""";

        var result = Processor.Rewrite(html, Settings, GuideFiles.All);

        result.Should().Contain("""href="mailto:a@b.com" """.Trim());
        result.Should().NotContain("target=\"_blank\"");
    }

    [Fact]
    public void Rewrite_ParentRelativePath_BecomesGitHubBlobUrl()
    {
        const string html = """<a href="../sections/Teams.md">Section invariants</a>""";

        var result = Processor.Rewrite(html, Settings, GuideFiles.All);

        result.Should().Contain("https://github.com/nobodies-collective/Humans/blob/main/docs/sections/Teams.md");
        result.Should().Contain("target=\"_blank\"");
    }

    [Fact]
    public void Rewrite_ImageShortPath_BecomesRawGitHubUrl()
    {
        const string html = """<img src="img/screenshot.png" alt="x" />""";

        var result = Processor.Rewrite(html, Settings, GuideFiles.All);

        result.Should().Contain("""src="https://raw.githubusercontent.com/nobodies-collective/Humans/main/docs/guide/img/screenshot.png" """.Trim());
    }

    [Fact]
    public void Rewrite_ImageWithDocsGuidePrefix_AlsoRewritten()
    {
        const string html = """<img src="docs/guide/img/screenshot.png" alt="x" />""";

        var result = Processor.Rewrite(html, Settings, GuideFiles.All);

        result.Should().Contain("https://raw.githubusercontent.com/nobodies-collective/Humans/main/docs/guide/img/screenshot.png");
    }

    [Fact]
    public void Rewrite_ImageAbsoluteUrl_LeftAsIs()
    {
        const string html = """<img src="https://cdn.example.com/x.png" alt="x" />""";

        var result = Processor.Rewrite(html, Settings, GuideFiles.All);

        result.Should().Contain("""src="https://cdn.example.com/x.png" """.Trim());
    }
}
```

- [ ] **Step 2: Run tests, confirm they fail**

Run: `dotnet test tests/Humans.Application.Tests/Humans.Application.Tests.csproj --filter "FullyQualifiedName~GuideHtmlPostprocessorTests"`
Expected: compile error or FAIL.

- [ ] **Step 3: Implement `GuideHtmlPostprocessor.cs`**

```csharp
using System.Text.RegularExpressions;
using Humans.Infrastructure.Configuration;

namespace Humans.Infrastructure.Services;

/// <summary>
/// Rewrites &lt;a href&gt; and &lt;img src&gt; attributes in rendered guide HTML so sibling
/// .md links become in-app routes, external/parent-relative links open in a new tab,
/// and image references resolve to raw.githubusercontent.com.
/// </summary>
public sealed class GuideHtmlPostprocessor
{
    private static readonly Regex HrefPattern = new(
        """<a\s+([^>]*?)href="(?<url>[^"]+)"(?<rest>[^>]*)>""",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(500));

    private static readonly Regex ImgPattern = new(
        """<img\s+([^>]*?)src="(?<url>[^"]+)"(?<rest>[^>]*)>""",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        TimeSpan.FromMilliseconds(500));

    public string Rewrite(string html, GuideSettings settings, IReadOnlySet<string> knownFileStems)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(knownFileStems);

        var githubRepoBlobBase =
            $"https://github.com/{settings.Owner}/{settings.Repository}/blob/{settings.Branch}/";
        var rawBase = settings.RawContentBaseUrl;
        var guideFolder = settings.FolderPath.TrimEnd('/') + "/";

        html = HrefPattern.Replace(html, match =>
        {
            var before = match.Groups[1].Value;
            var url = match.Groups["url"].Value;
            var after = match.Groups["rest"].Value;

            var rewritten = RewriteHref(url, knownFileStems, githubRepoBlobBase, guideFolder);
            if (rewritten.IsExternal && !after.Contains("target=", StringComparison.OrdinalIgnoreCase))
            {
                after = $" target=\"_blank\" rel=\"noopener\"{after}";
            }

            return $"<a {before}href=\"{rewritten.Href}\"{after}>";
        });

        html = ImgPattern.Replace(html, match =>
        {
            var before = match.Groups[1].Value;
            var url = match.Groups["url"].Value;
            var after = match.Groups["rest"].Value;

            var rewritten = RewriteImgSrc(url, rawBase, guideFolder);
            return $"<img {before}src=\"{rewritten}\"{after}>";
        });

        return html;
    }

    private static (string Href, bool IsExternal) RewriteHref(
        string url,
        IReadOnlySet<string> knownStems,
        string githubRepoBlobBase,
        string guideFolder)
    {
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return (url, true);
        }

        if (url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
        {
            return (url, false);
        }

        if (url.StartsWith('/'))
        {
            return (url, false);
        }

        if (url.StartsWith("../", StringComparison.Ordinal))
        {
            var resolved = ResolveParentRelative(url, guideFolder);
            return ($"{githubRepoBlobBase}{resolved}", true);
        }

        var (path, fragment) = SplitFragment(url);

        if (path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            var stem = path[..^3];
            var known = knownStems.FirstOrDefault(s => s.Equals(stem, StringComparison.OrdinalIgnoreCase));
            if (known is not null)
            {
                var href = fragment is null ? $"/Guide/{known}" : $"/Guide/{known}#{fragment}";
                return (href, false);
            }

            var blobUrl = $"{githubRepoBlobBase}{guideFolder}{path}";
            if (fragment is not null)
            {
                blobUrl = $"{blobUrl}#{fragment}";
            }
            return (blobUrl, true);
        }

        return (url, false);
    }

    private static string RewriteImgSrc(string url, string rawBase, string guideFolder)
    {
        if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return url;
        }

        var trimmed = url.TrimStart('/');
        if (trimmed.StartsWith(guideFolder, StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[guideFolder.Length..];
        }

        return rawBase + trimmed;
    }

    private static string ResolveParentRelative(string url, string guideFolder)
    {
        var segments = guideFolder.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();

        var remainder = url;
        while (remainder.StartsWith("../", StringComparison.Ordinal))
        {
            if (segments.Count > 0)
            {
                segments.RemoveAt(segments.Count - 1);
            }
            remainder = remainder[3..];
        }

        segments.Add(remainder);
        return string.Join('/', segments);
    }

    private static (string Path, string? Fragment) SplitFragment(string url)
    {
        var hashIndex = url.IndexOf('#');
        if (hashIndex < 0)
        {
            return (url, null);
        }
        return (url[..hashIndex], url[(hashIndex + 1)..]);
    }
}
```

- [ ] **Step 4: Run tests, confirm pass**

Run: `dotnet test tests/Humans.Application.Tests/Humans.Application.Tests.csproj --filter "FullyQualifiedName~GuideHtmlPostprocessorTests"`
Expected: All tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/Humans.Infrastructure/Services/GuideHtmlPostprocessor.cs tests/Humans.Application.Tests/Services/GuideHtmlPostprocessorTests.cs
git commit -m "Add GuideHtmlPostprocessor for link and image rewriting"
```

---

## Task 6 — `GuideRenderer` (Markdig orchestration)

**Why:** Glues the preprocessor, Markdig, and post-processor. This is the `IGuideRenderer` impl.

**Files:**

- Create: `src/Humans.Infrastructure/Services/GuideRenderer.cs`
- Test: `tests/Humans.Application.Tests/Services/GuideRendererTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
using AwesomeAssertions;
using Microsoft.Extensions.Options;
using Humans.Infrastructure.Configuration;
using Humans.Infrastructure.Services;
using Xunit;

namespace Humans.Application.Tests.Services;

public class GuideRendererTests
{
    private static readonly GuideSettings Settings = new()
    {
        Owner = "nobodies-collective",
        Repository = "Humans",
        Branch = "main",
        FolderPath = "docs/guide"
    };

    private static GuideRenderer CreateRenderer() => new(
        Options.Create(Settings),
        new GuideMarkdownPreprocessor(),
        new GuideHtmlPostprocessor());

    [Fact]
    public void Render_RoleSection_WrappedWithDiv()
    {
        const string markdown = """
            # Profiles

            Intro.

            ## As a Volunteer

            Do stuff.
            """;

        var html = CreateRenderer().Render(markdown, "Profiles");

        html.Should().Contain("<div data-guide-role=\"volunteer\"");
    }

    [Fact]
    public void Render_SiblingMdLink_RewrittenToGuideRoute()
    {
        const string markdown = "See [Profiles](Profiles.md) for details.";

        var html = CreateRenderer().Render(markdown, "Teams");

        html.Should().Contain("/Guide/Profiles");
    }

    [Fact]
    public void Render_ImageShortPath_RewrittenToRawUrl()
    {
        const string markdown = "![x](img/screenshot.png)";

        var html = CreateRenderer().Render(markdown, "Profiles");

        html.Should().Contain("raw.githubusercontent.com/nobodies-collective/Humans/main/docs/guide/img/screenshot.png");
    }

    [Fact]
    public void Render_ExternalLink_GetsBlankTarget()
    {
        const string markdown = "[ex](https://example.com)";

        var html = CreateRenderer().Render(markdown, "Profiles");

        html.Should().Contain("target=\"_blank\"");
    }

    [Fact]
    public void Render_AppPathLink_LeftAsIs()
    {
        const string markdown = "[Edit](/Profile/Me/Edit)";

        var html = CreateRenderer().Render(markdown, "Profiles");

        html.Should().Contain("/Profile/Me/Edit");
        html.Should().NotContain("target=\"_blank\"");
    }

    [Fact]
    public void Render_GlossaryFile_NoRoleWrappers()
    {
        const string markdown = """
            # Glossary

            ## Admin

            A human with full access.
            """;

        var html = CreateRenderer().Render(markdown, "Glossary");

        html.Should().NotContain("data-guide-role");
    }
}
```

- [ ] **Step 2: Run tests, confirm they fail**

Run: `dotnet test tests/Humans.Application.Tests/Humans.Application.Tests.csproj --filter "FullyQualifiedName~GuideRendererTests"`
Expected: compile error or FAIL.

- [ ] **Step 3: Implement `GuideRenderer.cs`**

```csharp
using Markdig;
using Microsoft.Extensions.Options;
using Humans.Application.Constants;
using Humans.Application.Interfaces;
using Humans.Infrastructure.Configuration;

namespace Humans.Infrastructure.Services;

public sealed class GuideRenderer : IGuideRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    private readonly IOptions<GuideSettings> _settings;
    private readonly GuideMarkdownPreprocessor _preprocessor;
    private readonly GuideHtmlPostprocessor _postprocessor;

    public GuideRenderer(
        IOptions<GuideSettings> settings,
        GuideMarkdownPreprocessor preprocessor,
        GuideHtmlPostprocessor postprocessor)
    {
        _settings = settings;
        _preprocessor = preprocessor;
        _postprocessor = postprocessor;
    }

    public string Render(string markdown, string fileStem)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileStem);

        var wrapped = _preprocessor.Wrap(markdown);
        var rendered = Markdown.ToHtml(wrapped, Pipeline);
        return _postprocessor.Rewrite(rendered, _settings.Value, GuideFiles.All);
    }
}
```

- [ ] **Step 4: Run tests, confirm pass**

Run: `dotnet test tests/Humans.Application.Tests/Humans.Application.Tests.csproj --filter "FullyQualifiedName~GuideRendererTests"`
Expected: All tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/Humans.Infrastructure/Services/GuideRenderer.cs tests/Humans.Application.Tests/Services/GuideRendererTests.cs
git commit -m "Add GuideRenderer orchestrating preprocessor, Markdig, postprocessor"
```

---

## Task 7 — `GuideFilter` (per-request role-block stripping)

**Why:** At request time, the service returns fully annotated HTML. The filter deletes role-block `<div>`s the user isn't entitled to see, using the rules in the spec §Role filtering.

**Files:**

- Create: `src/Humans.Application/Services/GuideFilter.cs`
- Test: `tests/Humans.Application.Tests/Services/GuideFilterTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
using AwesomeAssertions;
using Humans.Application.Models;
using Humans.Application.Services;
using Humans.Domain.Constants;
using Xunit;

namespace Humans.Application.Tests.Services;

public class GuideFilterTests
{
    private const string Sample = """
        <p>Intro, always visible.</p>
        <div data-guide-role="volunteer" data-guide-roles="">
          <h2>As a Volunteer</h2>
          <p>Volunteer content.</p>
        </div>
        <div data-guide-role="coordinator" data-guide-roles="ConsentCoordinator">
          <h2>As a Coordinator (Consent Coordinator)</h2>
          <p>Coord content.</p>
        </div>
        <div data-guide-role="boardadmin" data-guide-roles="TeamsAdmin">
          <h2>As a Board member / Admin (Teams Admin)</h2>
          <p>Teams admin content.</p>
        </div>
        <h2>Related sections</h2>
        <p>Always visible.</p>
        """;

    private static GuideRoleContext Roles(bool isCoord, params string[] systemRoles) =>
        new(IsAuthenticated: true, IsTeamCoordinator: isCoord,
            SystemRoles: new HashSet<string>(systemRoles, StringComparer.Ordinal));

    [Fact]
    public void Apply_Anonymous_KeepsOnlyVolunteerBlock()
    {
        var result = GuideFilter.Apply(Sample, GuideRoleContext.Anonymous);

        result.Should().Contain("Volunteer content.");
        result.Should().Contain("Intro, always visible.");
        result.Should().Contain("Related sections");
        result.Should().NotContain("Coord content.");
        result.Should().NotContain("Teams admin content.");
    }

    [Fact]
    public void Apply_PlainVolunteer_SameAsAnonymous()
    {
        var result = GuideFilter.Apply(Sample, Roles(isCoord: false));

        result.Should().Contain("Volunteer content.");
        result.Should().NotContain("Coord content.");
        result.Should().NotContain("Teams admin content.");
    }

    [Fact]
    public void Apply_TeamCoordinator_SeesVolunteerAndCoordinator()
    {
        var result = GuideFilter.Apply(Sample, Roles(isCoord: true));

        result.Should().Contain("Volunteer content.");
        result.Should().Contain("Coord content.");
        result.Should().NotContain("Teams admin content.");
    }

    [Fact]
    public void Apply_ConsentCoordinatorRoleOnly_SeesCoordinatorBlockByParenthetical()
    {
        var result = GuideFilter.Apply(Sample, Roles(isCoord: false, RoleNames.ConsentCoordinator));

        result.Should().Contain("Coord content.");
        result.Should().NotContain("Teams admin content.");
    }

    [Fact]
    public void Apply_ConsentCoordinatorOnBareCoordinatorHeading_NotVisible()
    {
        const string bareCoord = """
            <div data-guide-role="coordinator" data-guide-roles="">
              <h2>As a Coordinator</h2>
              <p>Bare coord content.</p>
            </div>
            """;

        var result = GuideFilter.Apply(bareCoord, Roles(isCoord: false, RoleNames.ConsentCoordinator));

        result.Should().NotContain("Bare coord content.");
    }

    [Fact]
    public void Apply_TeamsAdmin_SeesCoordinatorAndBoardOnTeamsFile()
    {
        // Within-file superset: seeing Board/Admin via (Teams Admin) implies seeing Coordinator too.
        var result = GuideFilter.Apply(Sample, Roles(isCoord: false, RoleNames.TeamsAdmin));

        result.Should().Contain("Coord content.");
        result.Should().Contain("Teams admin content.");
    }

    [Fact]
    public void Apply_TeamsAdminOnTicketsFile_SeesNothingBeyondVolunteer()
    {
        const string ticketsLike = """
            <div data-guide-role="volunteer" data-guide-roles="">V</div>
            <div data-guide-role="coordinator" data-guide-roles="">C</div>
            <div data-guide-role="boardadmin" data-guide-roles="TicketAdmin">BA</div>
            """;

        var result = GuideFilter.Apply(ticketsLike, Roles(isCoord: false, RoleNames.TeamsAdmin));

        result.Should().Contain("V");
        result.Should().NotContain(">C<");
        result.Should().NotContain("BA");
    }

    [Fact]
    public void Apply_Admin_SeesEverything()
    {
        var result = GuideFilter.Apply(Sample, Roles(isCoord: false, RoleNames.Admin));

        result.Should().Contain("Volunteer content.");
        result.Should().Contain("Coord content.");
        result.Should().Contain("Teams admin content.");
    }

    [Fact]
    public void Apply_Board_SeesAllBoardAdminBlocksRegardlessOfParenthetical()
    {
        const string mixed = """
            <div data-guide-role="boardadmin" data-guide-roles="">Plain</div>
            <div data-guide-role="boardadmin" data-guide-roles="CampAdmin">Camp-scoped</div>
            """;

        var result = GuideFilter.Apply(mixed, Roles(isCoord: false, RoleNames.Board));

        result.Should().Contain("Plain");
        result.Should().Contain("Camp-scoped");
    }

    [Fact]
    public void Apply_NoRoleDivs_ReturnsUnchanged()
    {
        const string plain = "<p>Glossary entries.</p>";

        var result = GuideFilter.Apply(plain, GuideRoleContext.Anonymous);

        result.Should().Be(plain);
    }
}
```

- [ ] **Step 2: Run tests, confirm they fail**

Run: `dotnet test tests/Humans.Application.Tests/Humans.Application.Tests.csproj --filter "FullyQualifiedName~GuideFilterTests"`
Expected: compile error or FAIL.

- [ ] **Step 3: Implement `GuideFilter.cs`**

```csharp
using System.Text.RegularExpressions;
using Humans.Application.Models;
using Humans.Domain.Constants;

namespace Humans.Application.Services;

/// <summary>
/// Strips role-scoped blocks the current user is not entitled to see. Operates on the
/// HTML produced by <c>GuideRenderer</c> (with &lt;div data-guide-role&gt; wrappers).
/// Pure function — returns the filtered HTML.
/// </summary>
public static class GuideFilter
{
    private static readonly Regex BlockPattern = new(
        """<div\s+data-guide-role="(?<role>[^"]+)"\s+data-guide-roles="(?<roles>[^"]*)"\s*>(?<body>.*?)</div>""",
        RegexOptions.Compiled | RegexOptions.Singleline,
        TimeSpan.FromSeconds(1));

    public static string Apply(string html, GuideRoleContext context)
    {
        ArgumentNullException.ThrowIfNull(html);
        ArgumentNullException.ThrowIfNull(context);

        // Two-pass: first pass decides each block's visibility; second pass applies the
        // within-file Coordinator superset (Coordinator inherits from Board/Admin).
        var fileSeesBoardAdmin = false;
        var matches = BlockPattern.Matches(html).ToList();

        foreach (Match match in matches)
        {
            var role = match.Groups["role"].Value;
            var roles = match.Groups["roles"].Value;
            if (role.Equals("boardadmin", StringComparison.Ordinal) &&
                IsVisible(role, roles, context))
            {
                fileSeesBoardAdmin = true;
                break;
            }
        }

        return BlockPattern.Replace(html, match =>
        {
            var role = match.Groups["role"].Value;
            var roles = match.Groups["roles"].Value;

            var visible = IsVisible(role, roles, context);
            if (!visible && role.Equals("coordinator", StringComparison.Ordinal) && fileSeesBoardAdmin)
            {
                visible = true;
            }

            return visible ? match.Value : string.Empty;
        });
    }

    private static bool IsVisible(string role, string rolesAttr, GuideRoleContext context)
    {
        var parenthetical = string.IsNullOrEmpty(rolesAttr)
            ? []
            : (IReadOnlyList<string>)rolesAttr.Split(',', StringSplitOptions.RemoveEmptyEntries);

        return role switch
        {
            "volunteer" => true,
            "coordinator" => IsCoordinatorVisible(parenthetical, context),
            "boardadmin" => IsBoardAdminVisible(parenthetical, context),
            _ => false
        };
    }

    private static bool IsCoordinatorVisible(IReadOnlyList<string> paren, GuideRoleContext ctx)
    {
        if (ctx.IsTeamCoordinator) return true;
        if (ctx.SystemRoles.Contains(RoleNames.Board) || ctx.SystemRoles.Contains(RoleNames.Admin)) return true;
        foreach (var role in paren)
        {
            if (ctx.SystemRoles.Contains(role)) return true;
        }
        return false;
    }

    private static bool IsBoardAdminVisible(IReadOnlyList<string> paren, GuideRoleContext ctx)
    {
        if (ctx.SystemRoles.Contains(RoleNames.Board) || ctx.SystemRoles.Contains(RoleNames.Admin)) return true;
        foreach (var role in paren)
        {
            if (ctx.SystemRoles.Contains(role)) return true;
        }
        return false;
    }
}
```

- [ ] **Step 4: Run tests, confirm pass**

Run: `dotnet test tests/Humans.Application.Tests/Humans.Application.Tests.csproj --filter "FullyQualifiedName~GuideFilterTests"`
Expected: All tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/Humans.Application/Services/GuideFilter.cs tests/Humans.Application.Tests/Services/GuideFilterTests.cs
git commit -m "Add GuideFilter for per-user role-block stripping"
```

---

## Task 8 — `GuideRoleResolver` (claims + DB)

**Why:** Turns a `ClaimsPrincipal` into a `GuideRoleContext`. Reads system roles from claims and checks `TeamMember.Role == Coordinator` in the DB.

**Files:**

- Create: `src/Humans.Infrastructure/Services/GuideRoleResolver.cs`
- Test: `tests/Humans.Application.Tests/Services/GuideRoleResolverTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
using System.Security.Claims;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.Testing;
using Humans.Domain.Constants;
using Humans.Domain.Entities;
using Humans.Domain.Enums;
using Humans.Infrastructure.Data;
using Humans.Infrastructure.Services;
using Xunit;

namespace Humans.Application.Tests.Services;

public class GuideRoleResolverTests : IDisposable
{
    private readonly HumansDbContext _db;
    private readonly FakeClock _clock = new(Instant.FromUtc(2026, 4, 21, 12, 0));

    public GuideRoleResolverTests()
    {
        var options = new DbContextOptionsBuilder<HumansDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new HumansDbContext(options);
    }

    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }

    private ClaimsPrincipal PrincipalWithRoles(Guid? userId, params string[] roles)
    {
        var claims = new List<Claim>();
        if (userId is not null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        }
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        var identity = new ClaimsIdentity(claims, authenticationType: userId is null ? null : "test");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task Resolve_Anonymous_ReturnsAnonymousContext()
    {
        var resolver = new GuideRoleResolver(_db);

        var result = await resolver.ResolveAsync(new ClaimsPrincipal(new ClaimsIdentity()));

        result.IsAuthenticated.Should().BeFalse();
        result.IsTeamCoordinator.Should().BeFalse();
        result.SystemRoles.Should().BeEmpty();
    }

    [Fact]
    public async Task Resolve_AuthWithAdminRole_ReportsSystemRoles()
    {
        var resolver = new GuideRoleResolver(_db);
        var user = PrincipalWithRoles(Guid.NewGuid(), RoleNames.Admin, RoleNames.Board);

        var result = await resolver.ResolveAsync(user);

        result.IsAuthenticated.Should().BeTrue();
        result.SystemRoles.Should().Contain([RoleNames.Admin, RoleNames.Board]);
    }

    [Fact]
    public async Task Resolve_ActiveTeamCoordinator_IsTeamCoordinatorTrue()
    {
        var userId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        _db.Teams.Add(new Team
        {
            Id = teamId, Name = "T", Slug = "t", IsActive = true,
            CreatedAt = _clock.GetCurrentInstant(), UpdatedAt = _clock.GetCurrentInstant()
        });
        _db.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            UserId = userId,
            Role = TeamMemberRole.Coordinator,
            JoinedAt = _clock.GetCurrentInstant()
        });
        await _db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var resolver = new GuideRoleResolver(_db);
        var user = PrincipalWithRoles(userId);

        var result = await resolver.ResolveAsync(user, TestContext.Current.CancellationToken);

        result.IsTeamCoordinator.Should().BeTrue();
    }

    [Fact]
    public async Task Resolve_FormerTeamCoordinator_IsTeamCoordinatorFalse()
    {
        var userId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        _db.Teams.Add(new Team
        {
            Id = teamId, Name = "T", Slug = "t", IsActive = true,
            CreatedAt = _clock.GetCurrentInstant(), UpdatedAt = _clock.GetCurrentInstant()
        });
        _db.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            UserId = userId,
            Role = TeamMemberRole.Coordinator,
            JoinedAt = _clock.GetCurrentInstant(),
            LeftAt = _clock.GetCurrentInstant()
        });
        await _db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var resolver = new GuideRoleResolver(_db);
        var user = PrincipalWithRoles(userId);

        var result = await resolver.ResolveAsync(user, TestContext.Current.CancellationToken);

        result.IsTeamCoordinator.Should().BeFalse();
    }

    [Fact]
    public async Task Resolve_MemberButNotCoordinator_IsTeamCoordinatorFalse()
    {
        var userId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        _db.Teams.Add(new Team
        {
            Id = teamId, Name = "T", Slug = "t", IsActive = true,
            CreatedAt = _clock.GetCurrentInstant(), UpdatedAt = _clock.GetCurrentInstant()
        });
        _db.TeamMembers.Add(new TeamMember
        {
            Id = Guid.NewGuid(),
            TeamId = teamId,
            UserId = userId,
            Role = TeamMemberRole.Member,
            JoinedAt = _clock.GetCurrentInstant()
        });
        await _db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var resolver = new GuideRoleResolver(_db);
        var user = PrincipalWithRoles(userId);

        var result = await resolver.ResolveAsync(user, TestContext.Current.CancellationToken);

        result.IsTeamCoordinator.Should().BeFalse();
    }
}
```

Note: if the existing test project does not use `TestContext.Current.CancellationToken`, replace with `CancellationToken.None`. Check one existing test file first to match the idiom.

- [ ] **Step 2: Run tests, confirm they fail**

Run: `dotnet test tests/Humans.Application.Tests/Humans.Application.Tests.csproj --filter "FullyQualifiedName~GuideRoleResolverTests"`
Expected: compile error or FAIL.

- [ ] **Step 3: Implement `GuideRoleResolver.cs`**

```csharp
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Humans.Application.Interfaces;
using Humans.Application.Models;
using Humans.Domain.Constants;
using Humans.Domain.Enums;
using Humans.Infrastructure.Data;

namespace Humans.Infrastructure.Services;

public sealed class GuideRoleResolver : IGuideRoleResolver
{
    private static readonly IReadOnlyList<string> KnownRoles =
    [
        RoleNames.Admin,
        RoleNames.Board,
        RoleNames.TeamsAdmin,
        RoleNames.CampAdmin,
        RoleNames.TicketAdmin,
        RoleNames.NoInfoAdmin,
        RoleNames.FeedbackAdmin,
        RoleNames.HumanAdmin,
        RoleNames.FinanceAdmin,
        RoleNames.ConsentCoordinator,
        RoleNames.VolunteerCoordinator
    ];

    private readonly HumansDbContext _db;

    public GuideRoleResolver(HumansDbContext db)
    {
        _db = db;
    }

    public async Task<GuideRoleContext> ResolveAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (user.Identity is null || !user.Identity.IsAuthenticated)
        {
            return GuideRoleContext.Anonymous;
        }

        var systemRoles = new HashSet<string>(StringComparer.Ordinal);
        foreach (var role in KnownRoles)
        {
            if (user.IsInRole(role))
            {
                systemRoles.Add(role);
            }
        }

        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var isCoordinator = false;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            isCoordinator = await _db.TeamMembers
                .AsNoTracking()
                .AnyAsync(
                    tm => tm.UserId == userId
                          && tm.Role == TeamMemberRole.Coordinator
                          && tm.LeftAt == null,
                    cancellationToken);
        }

        return new GuideRoleContext(
            IsAuthenticated: true,
            IsTeamCoordinator: isCoordinator,
            SystemRoles: systemRoles);
    }
}
```

- [ ] **Step 4: Run tests, confirm pass**

Run: `dotnet test tests/Humans.Application.Tests/Humans.Application.Tests.csproj --filter "FullyQualifiedName~GuideRoleResolverTests"`
Expected: All tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/Humans.Infrastructure/Services/GuideRoleResolver.cs tests/Humans.Application.Tests/Services/GuideRoleResolverTests.cs
git commit -m "Add GuideRoleResolver: system roles + team-coordinator DB check"
```

---

## Task 9 — `GuideContentService` (cache + orchestrator)

**Why:** Owns the `IMemoryCache`. On miss, triggers a full refresh via `IGuideContentSource`. Handles GitHub-down gracefully.

**Files:**

- Create: `src/Humans.Infrastructure/Services/GuideContentService.cs`
- Test: `tests/Humans.Application.Tests/Services/GuideContentServiceTests.cs`

- [ ] **Step 1: Write the failing tests**

```csharp
using AwesomeAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Humans.Application.Interfaces;
using Humans.Infrastructure.Configuration;
using Humans.Infrastructure.Services;
using Xunit;

namespace Humans.Application.Tests.Services;

public class GuideContentServiceTests
{
    private sealed class FakeSource : IGuideContentSource
    {
        public int Calls { get; private set; }
        public Func<string, string> MarkdownFor { get; set; } = stem => $"# {stem}\n\nContent.";
        public Func<string, Exception?> FailFor { get; set; } = _ => null;

        public Task<string> GetMarkdownAsync(string fileStem, CancellationToken cancellationToken = default)
        {
            Calls++;
            var fail = FailFor(fileStem);
            if (fail is not null) throw fail;
            return Task.FromResult(MarkdownFor(fileStem));
        }
    }

    private sealed class StubRenderer : IGuideRenderer
    {
        public string Render(string markdown, string fileStem) => $"[rendered:{fileStem}]";
    }

    private static GuideContentService CreateService(FakeSource source, out IMemoryCache cache)
    {
        cache = new MemoryCache(new MemoryCacheOptions());
        var settings = Options.Create(new GuideSettings { CacheTtlHours = 6 });
        return new GuideContentService(
            source,
            new StubRenderer(),
            cache,
            settings,
            NullLogger<GuideContentService>.Instance);
    }

    [Fact]
    public async Task GetRenderedAsync_FirstCall_FetchesFromSource()
    {
        var source = new FakeSource();
        var service = CreateService(source, out _);

        var html = await service.GetRenderedAsync("Profiles", TestContext.Current.CancellationToken);

        html.Should().Be("[rendered:Profiles]");
        source.Calls.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetRenderedAsync_SecondCall_ServedFromCache()
    {
        var source = new FakeSource();
        var service = CreateService(source, out _);

        await service.GetRenderedAsync("Profiles", TestContext.Current.CancellationToken);
        var callsAfterFirst = source.Calls;
        await service.GetRenderedAsync("Profiles", TestContext.Current.CancellationToken);

        source.Calls.Should().Be(callsAfterFirst);
    }

    [Fact]
    public async Task RefreshAllAsync_ClearsAndRefetches()
    {
        var source = new FakeSource();
        var service = CreateService(source, out _);
        await service.GetRenderedAsync("Profiles", TestContext.Current.CancellationToken);
        var callsBefore = source.Calls;

        await service.RefreshAllAsync(TestContext.Current.CancellationToken);

        source.Calls.Should().BeGreaterThan(callsBefore);
    }

    [Fact]
    public async Task GetRenderedAsync_UnknownFile_Throws()
    {
        var source = new FakeSource();
        var service = CreateService(source, out _);

        var act = async () => await service.GetRenderedAsync("DoesNotExist", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<FileNotFoundException>();
    }

    [Fact]
    public async Task GetRenderedAsync_ColdCacheGitHubFailure_ThrowsUnavailable()
    {
        var source = new FakeSource { FailFor = _ => new InvalidOperationException("network down") };
        var service = CreateService(source, out _);

        var act = async () => await service.GetRenderedAsync("Profiles", TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<GuideContentUnavailableException>();
    }

    [Fact]
    public async Task GetRenderedAsync_WarmCacheThenSourceFails_ServesStale()
    {
        var source = new FakeSource();
        var service = CreateService(source, out var cache);
        await service.GetRenderedAsync("Profiles", TestContext.Current.CancellationToken);

        // Simulate TTL-expired warm cache by clearing only the sentinel, leaving stale entries.
        // Implementation detail: the service tracks a "populated" flag separately from entries.
        source.FailFor = _ => new InvalidOperationException("flaky");
        await service.RefreshAllAsync(TestContext.Current.CancellationToken); // should NOT throw — stale content present

        var html = await service.GetRenderedAsync("Profiles", TestContext.Current.CancellationToken);

        html.Should().Be("[rendered:Profiles]");
    }
}
```

- [ ] **Step 2: Run tests, confirm they fail**

Run: `dotnet test tests/Humans.Application.Tests/Humans.Application.Tests.csproj --filter "FullyQualifiedName~GuideContentServiceTests"`
Expected: compile error or FAIL.

- [ ] **Step 3: Implement `GuideContentService.cs`**

```csharp
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Humans.Application.Constants;
using Humans.Application.Interfaces;
using Humans.Infrastructure.Configuration;

namespace Humans.Infrastructure.Services;

public sealed class GuideContentService : IGuideContentService
{
    private const string CacheKeyPrefix = "guide:";

    private readonly IGuideContentSource _source;
    private readonly IGuideRenderer _renderer;
    private readonly IMemoryCache _cache;
    private readonly IOptions<GuideSettings> _settings;
    private readonly ILogger<GuideContentService> _logger;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    public GuideContentService(
        IGuideContentSource source,
        IGuideRenderer renderer,
        IMemoryCache cache,
        IOptions<GuideSettings> settings,
        ILogger<GuideContentService> logger)
    {
        _source = source;
        _renderer = renderer;
        _cache = cache;
        _settings = settings;
        _logger = logger;
    }

    public async Task<string> GetRenderedAsync(string fileStem, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileStem);

        var canonical = GuideFiles.All.FirstOrDefault(s => s.Equals(fileStem, StringComparison.OrdinalIgnoreCase))
            ?? throw new FileNotFoundException($"Guide file '{fileStem}' is not in the known set.");

        if (_cache.TryGetValue(CacheKey(canonical), out string? cached) && cached is not null)
        {
            return cached;
        }

        await PopulateAsync(isRefresh: false, cancellationToken);

        if (_cache.TryGetValue(CacheKey(canonical), out string? afterPopulate) && afterPopulate is not null)
        {
            return afterPopulate;
        }

        throw new GuideContentUnavailableException(
            $"Guide content '{canonical}' is not currently available.");
    }

    public Task RefreshAllAsync(CancellationToken cancellationToken = default) =>
        PopulateAsync(isRefresh: true, cancellationToken);

    private async Task PopulateAsync(bool isRefresh, CancellationToken cancellationToken)
    {
        await _refreshLock.WaitAsync(cancellationToken);
        try
        {
            var hasStale = GuideFiles.All.Any(s => _cache.TryGetValue(CacheKey(s), out string? _));

            var settings = _settings.Value;
            var ttl = TimeSpan.FromHours(Math.Max(1, settings.CacheTtlHours));
            var anyFailures = false;
            var newEntries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var stem in GuideFiles.All)
            {
                try
                {
                    var markdown = await _source.GetMarkdownAsync(stem, cancellationToken);
                    var html = _renderer.Render(markdown, stem);
                    newEntries[stem] = html;
                }
                catch (Exception ex)
                {
                    anyFailures = true;
                    _logger.LogWarning(ex,
                        "Failed to fetch or render guide file {FileStem}; {Outcome}",
                        stem,
                        hasStale ? "keeping stale cached copy" : "no stale copy available");
                }
            }

            if (!hasStale && newEntries.Count == 0)
            {
                throw new GuideContentUnavailableException(
                    "Guide content is unavailable and the cache is cold.");
            }

            foreach (var (stem, html) in newEntries)
            {
                _cache.Set(CacheKey(stem), html, new MemoryCacheEntryOptions
                {
                    SlidingExpiration = ttl
                });
            }

            if (anyFailures)
            {
                _logger.LogWarning(
                    "Guide refresh completed with failures (isRefresh={IsRefresh}); stale entries retained.",
                    isRefresh);
            }
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    private static string CacheKey(string stem) => CacheKeyPrefix + stem;
}
```

- [ ] **Step 4: Run tests, confirm pass**

Run: `dotnet test tests/Humans.Application.Tests/Humans.Application.Tests.csproj --filter "FullyQualifiedName~GuideContentServiceTests"`
Expected: All tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/Humans.Infrastructure/Services/GuideContentService.cs tests/Humans.Application.Tests/Services/GuideContentServiceTests.cs
git commit -m "Add GuideContentService: memory cache + graceful GitHub-fail handling"
```

---

## Task 10 — `GitHubGuideContentSource` (Octokit)

**Why:** The production implementation of `IGuideContentSource`. Kept thin; exercised in the QA smoke test rather than unit tests.

**Files:**

- Create: `src/Humans.Infrastructure/Services/GitHubGuideContentSource.cs`

- [ ] **Step 1: Implement `GitHubGuideContentSource.cs`**

```csharp
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Octokit;
using Humans.Application.Interfaces;
using Humans.Infrastructure.Configuration;

namespace Humans.Infrastructure.Services;

public sealed class GitHubGuideContentSource : IGuideContentSource
{
    private readonly IOptions<GuideSettings> _guideSettings;
    private readonly IOptions<GitHubSettings> _gitHubSettings;
    private readonly GitHubClient _client;
    private readonly ILogger<GitHubGuideContentSource> _logger;

    public GitHubGuideContentSource(
        IOptions<GuideSettings> guideSettings,
        IOptions<GitHubSettings> gitHubSettings,
        ILogger<GitHubGuideContentSource> logger)
    {
        _guideSettings = guideSettings;
        _gitHubSettings = gitHubSettings;
        _logger = logger;

        _client = new GitHubClient(new ProductHeaderValue("NobodiesHumansGuide"));
        var token = guideSettings.Value.AccessToken ?? gitHubSettings.Value.AccessToken;
        if (!string.IsNullOrEmpty(token))
        {
            _client.Credentials = new Credentials(token);
        }
    }

    public async Task<string> GetMarkdownAsync(string fileStem, CancellationToken cancellationToken = default)
    {
        var settings = _guideSettings.Value;
        var path = $"{settings.FolderPath.TrimEnd('/')}/{fileStem}.md";

        _logger.LogDebug(
            "Fetching guide file {Path} from {Owner}/{Repository}@{Branch}",
            path, settings.Owner, settings.Repository, settings.Branch);

        var rawBytes = await _client.Repository.Content.GetRawContentByRef(
            settings.Owner,
            settings.Repository,
            path,
            settings.Branch);

        return System.Text.Encoding.UTF8.GetString(rawBytes);
    }
}
```

- [ ] **Step 2: Build**

Run: `dotnet build Humans.slnx`
Expected: `Build succeeded`.

- [ ] **Step 3: Commit**

```bash
git add src/Humans.Infrastructure/Services/GitHubGuideContentSource.cs
git commit -m "Add GitHubGuideContentSource (Octokit-backed IGuideContentSource)"
```

---

## Task 11 — `GuideController` + views

**Why:** HTTP surface. Routes, admin refresh, error views.

**Files:**

- Create: `src/Humans.Web/Controllers/GuideController.cs`
- Create: `src/Humans.Web/Models/GuideViewModel.cs`
- Create: `src/Humans.Web/Models/GuideSidebarModel.cs`
- Create: `src/Humans.Web/Views/Guide/Index.cshtml`
- Create: `src/Humans.Web/Views/Guide/Document.cshtml`
- Create: `src/Humans.Web/Views/Guide/NotFound.cshtml`
- Create: `src/Humans.Web/Views/Guide/Unavailable.cshtml`
- Create: `src/Humans.Web/Views/Shared/_GuideLayout.cshtml`

- [ ] **Step 1: Create `GuideSidebarModel.cs`**

```csharp
namespace Humans.Web.Models;

public sealed record GuideSidebarEntry(string Stem, string DisplayName, string Group);

public sealed class GuideSidebarModel
{
    public required IReadOnlyList<GuideSidebarEntry> Entries { get; init; }
    public required string? ActiveStem { get; init; }
}
```

- [ ] **Step 2: Create `GuideViewModel.cs`**

```csharp
using Microsoft.AspNetCore.Html;

namespace Humans.Web.Models;

public sealed class GuideViewModel
{
    public required string Title { get; init; }
    public required HtmlString Html { get; init; }
    public required GuideSidebarModel Sidebar { get; init; }
    public required string? FileStem { get; init; }
}
```

- [ ] **Step 3: Create `GuideController.cs`**

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Humans.Application.Constants;
using Humans.Application.Interfaces;
using Humans.Application.Services;
using Humans.Web.Authorization;
using Humans.Web.Models;

namespace Humans.Web.Controllers;

[Route("Guide")]
[AllowAnonymous]
public class GuideController : Controller
{
    private readonly IGuideContentService _content;
    private readonly IGuideRoleResolver _roles;

    public GuideController(IGuideContentService content, IGuideRoleResolver roles)
    {
        _content = content;
        _roles = roles;
    }

    [HttpGet("")]
    public Task<IActionResult> Index(CancellationToken cancellationToken) =>
        RenderAsync(GuideFiles.Readme, cancellationToken);

    [HttpGet("{name}")]
    public Task<IActionResult> Document(string name, CancellationToken cancellationToken) =>
        RenderAsync(name, cancellationToken);

    [HttpPost("Refresh")]
    [Authorize(Policy = PolicyNames.AdminOnly)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        try
        {
            await _content.RefreshAllAsync(cancellationToken);
            TempData["GuideRefreshStatus"] = "Guide refreshed from GitHub.";
        }
        catch (GuideContentUnavailableException ex)
        {
            TempData["GuideRefreshStatus"] = $"Refresh failed: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> RenderAsync(string requestedStem, CancellationToken cancellationToken)
    {
        var canonical = GuideFiles.All.FirstOrDefault(s =>
            s.Equals(requestedStem, StringComparison.OrdinalIgnoreCase));

        if (canonical is null)
        {
            return View("NotFound", BuildSidebar(null));
        }

        string rendered;
        try
        {
            rendered = await _content.GetRenderedAsync(canonical, cancellationToken);
        }
        catch (GuideContentUnavailableException)
        {
            return View("Unavailable", BuildSidebar(canonical));
        }

        var roleContext = await _roles.ResolveAsync(User, cancellationToken);
        var filtered = GuideFilter.Apply(rendered, roleContext);

        var viewModel = new GuideViewModel
        {
            Title = DisplayName(canonical),
            Html = new HtmlString(filtered),
            Sidebar = BuildSidebar(canonical),
            FileStem = canonical
        };

        ViewData["Title"] = viewModel.Title;
        return View(canonical.Equals(GuideFiles.Readme, StringComparison.OrdinalIgnoreCase)
            ? "Index"
            : "Document", viewModel);
    }

    private static GuideSidebarModel BuildSidebar(string? activeStem)
    {
        var entries = new List<GuideSidebarEntry>
        {
            new(GuideFiles.GettingStarted, "Getting Started", "Start here")
        };
        foreach (var section in GuideFiles.Sections)
        {
            entries.Add(new GuideSidebarEntry(section, DisplayName(section), "Section guides"));
        }
        entries.Add(new GuideSidebarEntry(GuideFiles.Glossary, "Glossary", "Appendix"));
        return new GuideSidebarModel { Entries = entries, ActiveStem = activeStem };
    }

    private static string DisplayName(string stem) => stem switch
    {
        GuideFiles.Readme => "Guide",
        GuideFiles.GettingStarted => "Getting Started",
        GuideFiles.Glossary => "Glossary",
        "LegalAndConsent" => "Legal & Consent",
        "CityPlanning" => "City Planning",
        "GoogleIntegration" => "Google Integration",
        _ => stem
    };
}
```

- [ ] **Step 4: Create `_GuideLayout.cshtml`**

```html
@model Humans.Web.Models.GuideViewModel

<div class="row">
    <aside class="col-md-3 mb-4">
        <nav class="nav flex-column" aria-label="Guide navigation">
            @foreach (var group in Model.Sidebar.Entries.GroupBy(e => e.Group))
            {
                <div class="text-uppercase text-muted small mt-3 mb-1">@group.Key</div>
                @foreach (var entry in group)
                {
                    var active = string.Equals(entry.Stem, Model.Sidebar.ActiveStem, StringComparison.OrdinalIgnoreCase);
                    <a class="nav-link @(active ? "active fw-bold" : "")"
                       asp-controller="Guide" asp-action="Document" asp-route-name="@entry.Stem">
                        @entry.DisplayName
                    </a>
                }
            }
        </nav>
    </aside>
    <section class="col-md-9">
        @if (!string.IsNullOrEmpty(Model.FileStem) && !string.Equals(Model.FileStem, "README", StringComparison.OrdinalIgnoreCase))
        {
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb">
                    <li class="breadcrumb-item"><a asp-controller="Guide" asp-action="Index">Guide</a></li>
                    <li class="breadcrumb-item active" aria-current="page">@Model.Title</li>
                </ol>
            </nav>
        }

        @if (TempData["GuideRefreshStatus"] is string status)
        {
            <div class="alert alert-info">@status</div>
        }

        <article class="guide-content">
            @Model.Html
        </article>

        @if (User.IsInRole(Humans.Domain.Constants.RoleNames.Admin))
        {
            <form asp-action="Refresh" method="post" class="mt-4">
                @Html.AntiForgeryToken()
                <button type="submit" class="btn btn-outline-secondary btn-sm">
                    Refresh from GitHub
                </button>
            </form>
        }
    </section>
</div>
```

- [ ] **Step 5: Create `Index.cshtml`**

```html
@model Humans.Web.Models.GuideViewModel
@{
    ViewData["Title"] = Model.Title;
}

<h1 class="mb-4">Guide</h1>

@await Html.PartialAsync("_GuideLayout", Model)
```

- [ ] **Step 6: Create `Document.cshtml`**

```html
@model Humans.Web.Models.GuideViewModel
@{
    ViewData["Title"] = Model.Title;
}

@await Html.PartialAsync("_GuideLayout", Model)
```

- [ ] **Step 7: Create `NotFound.cshtml`**

```html
@model Humans.Web.Models.GuideSidebarModel
@{
    ViewData["Title"] = "Guide — Not found";
}

<div class="row">
    <section class="col-md-9 offset-md-3">
        <h1>Page not found</h1>
        <p>That guide page does not exist. <a asp-controller="Guide" asp-action="Index">Return to the Guide home</a>.</p>
    </section>
</div>
```

- [ ] **Step 8: Create `Unavailable.cshtml`**

```html
@model Humans.Web.Models.GuideSidebarModel
@{
    ViewData["Title"] = "Guide — Temporarily unavailable";
}

<div class="row">
    <section class="col-md-9 offset-md-3">
        <h1>Guide temporarily unavailable</h1>
        <p>We couldn't load the guide content right now. Please try again in a few minutes.</p>
    </section>
</div>
```

- [ ] **Step 9: Build**

Run: `dotnet build Humans.slnx`
Expected: `Build succeeded`.

Note: `NotFound.cshtml` and `Unavailable.cshtml` declare `@model GuideSidebarModel` but the controller returns a `GuideSidebarModel` there — confirm the model type matches between controller and view.

- [ ] **Step 10: Commit**

```bash
git add src/Humans.Web/Controllers/GuideController.cs src/Humans.Web/Models/GuideViewModel.cs src/Humans.Web/Models/GuideSidebarModel.cs src/Humans.Web/Views/Guide/ src/Humans.Web/Views/Shared/_GuideLayout.cshtml
git commit -m "Add GuideController, views, and sidebar layout"
```

---

## Task 12 — Nav link

**Why:** Make `/Guide` discoverable.

**Files:**

- Modify: `src/Humans.Web/Views/Shared/_Layout.cshtml`

- [ ] **Step 1: Open `_Layout.cshtml` and locate the `<li class="nav-item">` for Legal at around line 69-71**

- [ ] **Step 2: Insert a new `<li>` above the Legal block (visible to everyone):**

```html
<li class="nav-item">
    <a class="nav-link" asp-controller="Guide" asp-action="Index">Guide</a>
</li>
```

Keep it outside the `@if (!isAuthenticated || !hasProfile)` block so authenticated users see it too.

- [ ] **Step 3: Build**

Run: `dotnet build Humans.slnx`
Expected: `Build succeeded`.

- [ ] **Step 4: Commit**

```bash
git add src/Humans.Web/Views/Shared/_Layout.cshtml
git commit -m "Add Guide link to main nav"
```

---

## Task 13 — DI registrations + config

**Why:** Wire the services into the container.

**Files:**

- Modify: `src/Humans.Web/Extensions/InfrastructureServiceCollectionExtensions.cs`

- [ ] **Step 1: Add `GuideSettings` binding near the other `Configure<…>` calls**

Locate the block in `AddHumansInfrastructure` that configures `GitHubSettings`, `EmailSettings`, etc. (around line 30). Add immediately after the `GitHubSettings` line:

```csharp
services.Configure<GuideSettings>(configuration.GetSection(GuideSettings.SectionName));
```

- [ ] **Step 2: Register the 6 new services**

Locate the service registrations block (where `IDashboardService`, `ISystemTeamSync`, etc. are registered, around line 240-245). Add:

```csharp
services.AddSingleton<GuideMarkdownPreprocessor>();
services.AddSingleton<GuideHtmlPostprocessor>();
services.AddSingleton<Humans.Application.Interfaces.IGuideRenderer, GuideRenderer>();
services.AddSingleton<Humans.Application.Interfaces.IGuideContentSource, GitHubGuideContentSource>();
services.AddSingleton<Humans.Application.Interfaces.IGuideContentService, GuideContentService>();
services.AddScoped<Humans.Application.Interfaces.IGuideRoleResolver, GuideRoleResolver>();
```

Note: `GuideRoleResolver` is scoped because it depends on `HumansDbContext`. Everything else is singleton (stateless).

- [ ] **Step 3: Register config-registry entries if the `configRegistry` block is present**

Inside the `if (configRegistry is not null)` block (around line 49), after the GitHub settings registrations, add:

```csharp
configuration.GetOptionalSetting(configRegistry, "Guide:Owner", "Guide");
configuration.GetOptionalSetting(configRegistry, "Guide:Repository", "Guide");
configuration.GetOptionalSetting(configRegistry, "Guide:Branch", "Guide");
configuration.GetOptionalSetting(configRegistry, "Guide:FolderPath", "Guide");
configuration.GetOptionalSetting(configRegistry, "Guide:CacheTtlHours", "Guide");
configuration.GetOptionalSetting(configRegistry, "Guide:AccessToken", "Guide", isSensitive: true);
```

- [ ] **Step 4: Build**

Run: `dotnet build Humans.slnx`
Expected: `Build succeeded`.

- [ ] **Step 5: Run the full test suite**

Run: `dotnet test Humans.slnx`
Expected: all tests pass.

- [ ] **Step 6: Commit**

```bash
git add src/Humans.Web/Extensions/InfrastructureServiceCollectionExtensions.cs
git commit -m "Register Guide services in DI and bind GuideSettings"
```

---

## Task 14 — Guide content parenthetical updates

**Why:** The role filter needs parentheticals on the "Board member / Admin" heading in 6 guide files to know which domain admin role should see each one.

**Files:**

- Modify: `docs/guide/Teams.md`
- Modify: `docs/guide/Profiles.md`
- Modify: `docs/guide/Shifts.md`
- Modify: `docs/guide/Feedback.md`
- Modify: `docs/guide/Budget.md`
- Modify: `docs/guide/Onboarding.md`

- [ ] **Step 1: Update `Teams.md`**

Replace `## As a Board member / Admin` with `## As a Board member / Admin (Teams Admin)`.

- [ ] **Step 2: Update `Profiles.md`**

Replace `## As a Board member / Admin` with `## As a Board member / Admin (Human Admin)`.

- [ ] **Step 3: Update `Shifts.md`**

Replace `## As a Board member / Admin` with `## As a Board member / Admin (NoInfo Admin)`.

- [ ] **Step 4: Update `Feedback.md`**

Replace `## As a Board member / Admin` with `## As a Board member / Admin (Feedback Admin)`.

- [ ] **Step 5: Update `Budget.md`**

Replace `## As a Board member / Admin` with `## As a Board member / Admin (Finance Admin)`.

- [ ] **Step 6: Update `Onboarding.md`**

Replace `## As a Board member / Admin` with `## As a Board member / Admin (Human Admin)`.

- [ ] **Step 7: Confirm files already-parenthetical are unchanged**

Verify `docs/guide/Camps.md`, `CityPlanning.md`, `Tickets.md` still have their parentheticals. Do NOT touch `Admin.md`, `Governance.md`, `Campaigns.md`, `Email.md`, `GoogleIntegration.md`, `LegalAndConsent.md` — those stay bare (Board/Admin-only).

- [ ] **Step 8: Commit**

```bash
git add docs/guide/Teams.md docs/guide/Profiles.md docs/guide/Shifts.md docs/guide/Feedback.md docs/guide/Budget.md docs/guide/Onboarding.md
git commit -m "Add domain-admin parentheticals to 6 guide files for role filtering"
```

---

## Task 15 — Feature spec and section invariants

**Why:** Per project conventions (CLAUDE.md), every new feature gets a `docs/features/NN-*.md` spec and every new section gets `docs/sections/<Name>.md` invariants. Keeps the architecture docs in sync.

**Files:**

- Create: `docs/features/39-in-app-guide.md`
- Create: `docs/sections/Guide.md`

- [ ] **Step 1: Create `docs/features/39-in-app-guide.md`**

```markdown
# 39 — In-App Guide

## Business context

The Humans app has a comprehensive end-user guide under `docs/guide/` (17
files covering every section of the app). Before this feature, humans read
it on GitHub — one click away from the app itself. Embedding the guide at
`/Guide` makes it immediately available from the nav bar and lets links
inside the guide navigate in-app to the pages they describe.

GitHub remains the authoring source: guide changes go through pull-request
review (no in-app CMS). The app pulls the current content from
`nobodies-collective/Humans:main:docs/guide/` on demand, caches it in
memory, and re-renders it for each request with role-aware filtering.

## User stories

- As a volunteer, I can click "Guide" in the nav bar and read a
  human-friendly explanation of the parts of the app I use, with in-app
  links that take me directly to the right page.
- As a team coordinator, I see coordinator-specific sections in the guide
  that explain the management features I have access to.
- As a domain admin (e.g. TeamsAdmin), I see admin guidance for my domain
  but not for domains I don't manage.
- As an admin, I can click "Refresh from GitHub" after merging a guide
  change, without waiting for the app to redeploy.

## Data model

None. The feature is stateless relative to the database: all content is
cached in-memory from GitHub. No migrations, no new tables.

## Workflows

### Reading a guide page

1. Human navigates to `/Guide/<Page>`.
2. `GuideRoleResolver` builds a `GuideRoleContext` from claims +
   `TeamMember` check.
3. `GuideContentService` returns the fully rendered HTML for the page,
   fetching and rendering the 17 files on cache miss.
4. `GuideFilter` strips role-scoped `<div>` blocks the user can't see.
5. Filtered HTML rendered inside `_GuideLayout.cshtml` with sidebar.

### Refreshing from GitHub (Admin)

1. Admin submits `POST /Guide/Refresh` (CSRF-protected).
2. `GuideContentService.RefreshAllAsync` re-fetches all 17 files and
   re-renders; existing cache entries are overwritten.
3. Admin is redirected back to `/Guide` with a status flash.

## Authorization

- `GET /Guide` / `GET /Guide/{name}` — `[AllowAnonymous]`. Role filtering
  applied server-side; anonymous users see Volunteer sections only.
- `POST /Guide/Refresh` — `[Authorize(Policy = PolicyNames.AdminOnly)]`.

## Role filtering rules

See `docs/superpowers/specs/2026-04-21-in-app-guide-design.md` §Role
filtering for the authoritative rules (per-block visibility, within-file
superset, parenthetical parsing).

## Related features

- Legal documents (feature 04) use a similar GitHub-sync pattern but with
  DB persistence and versioning. Guide content has no such requirement
  and is memory-only.

## Out of scope for v1

- Localization (English-only for now).
- Glossary-term hover tooltips.
- Scheduled background refresh (TTL + manual refresh cover the cases).
- In-app markdown editing.
```

- [ ] **Step 2: Create `docs/sections/Guide.md`**

```markdown
# Guide — Section Invariants

## Concepts

- A **guide page** is one markdown file under `docs/guide/` in the Humans
  repo, rendered at `/Guide/<FileStem>`.
- A **role-scoped block** is a `## As a …` heading and the content under
  it, wrapped in `<div data-guide-role="…" data-guide-roles="…">` by the
  renderer and optionally stripped at request time by `GuideFilter`.

## Actors & Roles

| Actor | Capabilities |
|-------|-------------|
| Anonymous | View Volunteer-scoped content on any guide page |
| Any authenticated human | View Volunteer-scoped content |
| Team coordinator (TeamMember.Role == Coordinator) | Additionally view Coordinator-scoped blocks |
| Domain admin (*Admin or *Coordinator system role named in a parenthetical) | Additionally view blocks whose parenthetical names their role, on those specific files |
| Admin, Board | View all blocks on all pages |
| Admin | Trigger `POST /Guide/Refresh` |

## Invariants

- All content above the first `## As a …` heading in a file is always
  visible regardless of role.
- All content at or below a non-`As a …` `## ` heading (e.g.
  `## Related sections`) is always visible.
- Anonymous users only see Volunteer-scoped blocks. They never see
  Coordinator or Board/Admin blocks.
- Guide content is the 17 files in `docs/guide/` on the current branch;
  nothing is authored in-app.
- Cache key is `guide:<FileStem>`. TTL is sliding, configured via
  `Guide:CacheTtlHours` (default 6).
- Only the `GuideContentService` reads or writes the `guide:*` cache
  entries. No other service touches guide content.

## Triggers

- First `GET /Guide/*` after cold start → full refresh of all 17 cache
  entries.
- `POST /Guide/Refresh` (Admin) → clears and repopulates all entries.
- GitHub fetch failure on warm cache → stale content served; warning
  logged; TTL preserved.
- GitHub fetch failure on cold cache → `GuideContentUnavailableException`
  bubbles; controller renders `Unavailable.cshtml`.

## Cross-Section Dependencies

- Reads `TeamMember.Role` to resolve the `IsTeamCoordinator` flag for the
  current user (see `Teams` section).
- Reads `User.IsInRole` claims for the `SystemRoles` set (see `Admin`
  section for role assignments).
```

- [ ] **Step 3: Commit**

```bash
git add docs/features/39-in-app-guide.md docs/sections/Guide.md
git commit -m "Add Guide feature spec and section invariants"
```

---

## Task 16 — Build + verification + smoke test notes

**Why:** Final full-solution check before local smoke testing.

- [ ] **Step 1: Run full build**

Run: `dotnet build Humans.slnx`
Expected: `Build succeeded` with 0 errors, 0 warnings (or warnings pre-existing to this branch).

- [ ] **Step 2: Run full test suite**

Run: `dotnet test Humans.slnx`
Expected: all tests pass (including the ~7 new test classes from this plan).

- [ ] **Step 3: Run the app locally and smoke-test**

Run: `dotnet run --project src/Humans.Web`
Then in a browser:

1. Unauthenticated → `/` → confirm "Guide" link in nav → click → landing page shows README content.
2. Navigate to `/Guide/Profiles`. Confirm only "What this section is for", "Key pages at a glance", "As a Volunteer", and "Related sections" are visible. Coordinator and Board/Admin sections are absent from the rendered DOM.
3. Log in as a plain volunteer → same visibility.
4. Log in as a team coordinator → Coordinator sections now visible; Board/Admin still hidden.
5. Log in as a `CampAdmin` → on `/Guide/Camps`, Board/Admin block for Camp Admin is visible; on `/Guide/Tickets`, it is not.
6. Log in as `Admin` → all sections visible everywhere. "Refresh from GitHub" button visible; click it, confirm flash message.
7. Click a sibling `.md` link (e.g. any `Glossary.md#...` inside a guide) — navigates in-app with anchor.
8. Click an app-path link (e.g. `/Profile/Me/Edit`) — goes to the real app route.
9. Click an external link — opens in a new tab.
10. Confirm images (if any are present in the committed guide content) load from `raw.githubusercontent.com`.

- [ ] **Step 4: If all smoke tests pass, tag a wrap-up commit**

(Optional — only if further tweaks were made during smoke testing.)

```bash
git status
# If nothing changed, skip. Otherwise:
git add -A
git commit -m "Tweaks after smoke test"
```

---

## Self-review against the spec

- [x] **Spec coverage — routes:** `/Guide`, `/Guide/{name}`, `POST /Guide/Refresh` (Task 11).
- [x] **Spec coverage — role filtering:** parenthetical parser (Task 3), preprocessor wrapping (Task 4), per-request filter (Task 7), including within-file superset.
- [x] **Spec coverage — link rewriting:** sibling `.md`, fragments, external, app-path, mailto, parent-relative (Task 5).
- [x] **Spec coverage — image rewriting:** short paths, `docs/guide/` prefix, absolute URLs (Task 5).
- [x] **Spec coverage — caching:** memory-only, per-file entries, TTL, admin manual refresh, graceful GitHub-down handling (Task 9).
- [x] **Spec coverage — team-coordinator detection:** `TeamMember.Role == Coordinator && LeftAt == null` (Task 8).
- [x] **Spec coverage — nav link + sidebar:** Task 11 view, Task 12 nav.
- [x] **Spec coverage — content updates:** 6 guide files parenthetical-updated (Task 14).
- [x] **Spec coverage — docs:** feature spec + section invariants (Task 15).
- [x] **Spec coverage — authorization:** AllowAnonymous on GET, AdminOnly policy on POST/Refresh (Task 11).
- [x] **Placeholder scan:** no TBDs, every code block is complete, every test has explicit assertions.
- [x] **Type consistency:** `IGuideContentService.GetRenderedAsync` / `RefreshAllAsync` consistent across controller, service, and tests. `GuideRoleContext` record fields (`IsAuthenticated`, `IsTeamCoordinator`, `SystemRoles`) consistent in resolver, filter, and controller. Cache key prefix `guide:` used consistently.
