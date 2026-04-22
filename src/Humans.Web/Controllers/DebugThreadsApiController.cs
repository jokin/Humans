using System.Globalization;
using System.Text;
using Humans.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Diagnostics.Runtime;

namespace Humans.Web.Controllers;

/// <summary>
/// Non-Production-only diagnostic endpoint that dumps every managed thread's
/// call stack using ClrMD's self-attach. Intended for debugging hangs/deadlocks
/// in QA/preview environments where external tooling (createdump, gdb, dotnet-dump)
/// is blocked by container ptrace restrictions.
///
/// Gated by two checks: <see cref="IWebHostEnvironment.IsProduction"/> returns 404
/// in Production regardless of any other config, and <see cref="LogApiKeyAuthFilter"/>
/// requires the same API key as /api/logs.
/// </summary>
[ApiController]
[Route("api/debug/threads")]
[ServiceFilter(typeof(LogApiKeyAuthFilter))]
public class DebugThreadsApiController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public DebugThreadsApiController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet]
    public IActionResult Get()
    {
        if (_env.IsProduction())
            return NotFound();

        var inv = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        sb.Append(inv, $"pid={Environment.ProcessId}").AppendLine();
        sb.Append(inv, $"threadpool-threads={System.Threading.ThreadPool.ThreadCount}").AppendLine();
        sb.Append(inv, $"threadpool-pending={System.Threading.ThreadPool.PendingWorkItemCount}").AppendLine();
        sb.Append(inv, $"threadpool-completed={System.Threading.ThreadPool.CompletedWorkItemCount}").AppendLine();
        sb.AppendLine();

        try
        {
            using var target = DataTarget.AttachToProcess(Environment.ProcessId, suspend: false);
            using var runtime = target.ClrVersions[0].CreateRuntime();
            foreach (var thread in runtime.Threads)
            {
                sb.Append(inv, $"--- thread os={thread.OSThreadId} managed={thread.ManagedThreadId} state={thread.State} ---").AppendLine();
                foreach (var frame in thread.EnumerateStackTrace())
                {
                    sb.Append(inv, $"  {frame}").AppendLine();
                }
                sb.AppendLine();
            }
        }
        catch (Exception ex)
        {
            sb.Append(inv, $"CLRMD FAILED: {ex.GetType().FullName}: {ex.Message}").AppendLine();
            sb.AppendLine(ex.StackTrace);
            sb.AppendLine();
            sb.AppendLine("Likely cause: container is missing CAP_SYS_PTRACE. Add it in Coolify (Advanced → Capabilities) and restart.");
        }

        return Content(sb.ToString(), "text/plain");
    }
}
