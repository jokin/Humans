using Humans.Domain.Enums;
using NodaTime;

namespace Humans.Application.DTOs;

/// <summary>Aggregated ticket dashboard statistics.</summary>
public class TicketDashboardStats
{
    public int TicketsSold { get; init; }
    public decimal Revenue { get; init; }
    public decimal TotalStripeFees { get; init; }
    public decimal TotalApplicationFees { get; init; }
    public decimal NetRevenue { get; init; }
    public decimal AveragePrice { get; init; }
    public int UnmatchedOrderCount { get; init; }

    public List<FeeBreakdownByMethod> FeesByPaymentMethod { get; init; } = [];
    public List<DailySales> DailySalesPoints { get; init; } = [];
    public List<RecentOrder> RecentOrders { get; init; } = [];

    // Sync state
    public TicketSyncStatus SyncStatus { get; init; }
    public string? SyncError { get; init; }
    public Instant? LastSyncAt { get; init; }

    // Volunteer ticket coverage
    public int TotalActiveVolunteers { get; init; }
    public int VolunteersWithTickets { get; init; }
    public decimal VolunteerCoveragePercent { get; init; }
}

public class FeeBreakdownByMethod
{
    public string PaymentMethod { get; init; } = string.Empty;
    public int OrderCount { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal TotalStripeFees { get; init; }
    public decimal TotalApplicationFees { get; init; }
    public decimal EffectiveRate { get; init; }
}

public class DailySales
{
    public string Date { get; init; } = string.Empty;
    public int TicketsSold { get; init; }
    public decimal? RollingAverage { get; init; }
}

public class RecentOrder
{
    public Guid Id { get; init; }
    public string BuyerName { get; init; } = string.Empty;
    public int TicketCount { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "EUR";
    public Instant PurchasedAt { get; init; }
    public bool IsMatched { get; init; }
}

/// <summary>Weekly and quarterly sales aggregates for reporting.</summary>
public class TicketSalesAggregates
{
    public List<WeeklySalesAggregate> WeeklySales { get; init; } = [];
    public List<QuarterlySalesAggregate> QuarterlySales { get; init; } = [];
}

public class WeeklySalesAggregate
{
    public string WeekLabel { get; init; } = string.Empty;
    public int TicketsSold { get; init; }
    public decimal GrossRevenue { get; init; }
    public int OrderCount { get; init; }
}

public class QuarterlySalesAggregate
{
    public string QuarterLabel { get; init; } = string.Empty;
    public int Year { get; init; }
    public int Quarter { get; init; }
    public int TicketsSold { get; init; }
    public decimal GrossRevenue { get; init; }
    public int OrderCount { get; init; }
}
