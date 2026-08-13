using System;
using System.Collections.Generic;
using System.Linq;
using GroveApp.DesignSystem;
using GroveApp.Models;
using GroveApp.Models.Memory;

namespace GroveApp.Engine.Memory;

public enum MemorySearchMatchLevel : byte
{
    None,
    Payload,
    Title,
    Anchor
}

public sealed record MemorySearchResult(
    MemoryRecord Memory,
    MemorySearchMatchLevel MatchLevel,
    double SemanticScore,
    double SpatialScore,
    IReadOnlyList<MemoryAnchor> MatchingAnchors);

/// <summary>
/// Searches semantic Memory records and derives Content-side context and
/// spatial recall ranking without changing the ledger or requiring a viewport
/// field snapshot.
/// </summary>
public sealed class MemorySearchService
{
    private readonly IMemoryLedger _memoryLedger;
    private readonly Func<IReadOnlyList<MemoryAnchor>> _anchors;
    private readonly Func<IReadOnlyList<GridContentItem>> _content;
    private readonly FieldLedgerEngine _fieldLedger;

    public MemorySearchService(
        IMemoryLedger memoryLedger,
        Func<IReadOnlyList<MemoryAnchor>> anchors,
        Func<IReadOnlyList<GridContentItem>> content,
        FieldLedgerEngine fieldLedger)
    {
        _memoryLedger = memoryLedger ?? throw new ArgumentNullException(nameof(memoryLedger));
        _anchors = anchors ?? throw new ArgumentNullException(nameof(anchors));
        _content = content ?? throw new ArgumentNullException(nameof(content));
        _fieldLedger = fieldLedger ?? throw new ArgumentNullException(nameof(fieldLedger));
    }

    public IReadOnlyList<MemorySearchResult> Search(string? query)
    {
        string normalizedQuery = query?.Trim() ?? string.Empty;
        string[] terms = Tokenize(normalizedQuery);
        IReadOnlyList<MemoryAnchor> anchors = _anchors();
        IReadOnlyList<GridContentItem> content = _content();
        var results = new List<MemorySearchResult>();

        foreach (MemoryRecord memory in _memoryLedger.GetAllMemories())
        {
            List<MemoryAnchor> matchingAnchors = anchors
                .Where(anchor => anchor.MemoryId == memory.MemoryId &&
                                 Matches(anchor.ContextLabel, normalizedQuery, terms))
                .ToList();
            (MemorySearchMatchLevel level, double semanticScore) = ResolveSemanticMatch(
                memory,
                matchingAnchors,
                normalizedQuery,
                terms);

            if (terms.Length > 0 && level == MemorySearchMatchLevel.None)
            {
                continue;
            }

            double spatialScore = CalculateSpatialScore(memory, content);
            results.Add(new MemorySearchResult(
                memory,
                level,
                semanticScore,
                spatialScore,
                matchingAnchors));
        }

        return results
            .OrderByDescending(result => result.MatchLevel)
            .ThenByDescending(result => result.SemanticScore)
            .ThenByDescending(result => result.SpatialScore)
            .ThenByDescending(result => result.Memory.CreatedAtTicks)
            .ToArray();
    }

    private (MemorySearchMatchLevel Level, double Score) ResolveSemanticMatch(
        MemoryRecord memory,
        IReadOnlyList<MemoryAnchor> matchingAnchors,
        string query,
        IReadOnlyList<string> terms)
    {
        if (terms.Count == 0)
        {
            return (MemorySearchMatchLevel.None, 0.0);
        }

        if (matchingAnchors.Count > 0)
        {
            return (MemorySearchMatchLevel.Anchor, BestScore(
                matchingAnchors.Select(anchor => anchor.ContextLabel), query, terms));
        }

        if (Matches(memory.Title, query, terms))
        {
            return (MemorySearchMatchLevel.Title, BestScore(new[] { memory.Title }, query, terms));
        }

        string payload = memory.GetUtf8Payload();
        return Matches(payload, query, terms)
            ? (MemorySearchMatchLevel.Payload, BestScore(new[] { payload }, query, terms))
            : (MemorySearchMatchLevel.None, 0.0);
    }

    private double CalculateSpatialScore(MemoryRecord memory, IReadOnlyList<GridContentItem> content)
    {
        GridContentItem[] candidates = content
            .Where(item => item.MemoryId == memory.MemoryId)
            .ToArray();
        if (candidates.Length == 0)
        {
            return 0.0;
        }

        double score = 0.0;
        foreach (GridContentItem candidate in candidates)
        {
            foreach (GridContentItem nearby in content)
            {
                if (ReferenceEquals(candidate, nearby))
                {
                    continue;
                }

                score += ScoreRelationship(candidate, nearby, content);
            }
        }

        return score;
    }

    private double ScoreRelationship(
        GridContentItem candidate,
        GridContentItem nearby,
        IReadOnlyList<GridContentItem> content)
    {
        int minX = Math.Min(candidate.CellX, nearby.CellX) - Tokens.MaxCullingRadiusCells;
        int minY = Math.Min(candidate.CellY, nearby.CellY) - Tokens.MaxCullingRadiusCells;
        int maxX = Math.Max(candidate.CellX + candidate.CellWidth, nearby.CellX + nearby.CellWidth) +
                   Tokens.MaxCullingRadiusCells;
        int maxY = Math.Max(candidate.CellY + candidate.CellHeight, nearby.CellY + nearby.CellHeight) +
                   Tokens.MaxCullingRadiusCells;
        double overlap = 0.0;
        double saturation = 0.0;

        for (int col = minX; col <= maxX; col++)
        {
            for (int row = minY; row <= maxY; row++)
            {
                double candidateEnergy = _fieldLedger.GetSourceEnergyAtCell(
                    candidate, col, row, candidate.LayerId);
                double nearbyEnergy = _fieldLedger.GetSourceEnergyAtCell(
                    nearby, col, row, candidate.LayerId);
                if (candidateEnergy <= 0 || nearbyEnergy <= 0)
                {
                    continue;
                }

                overlap += Math.Min(candidateEnergy, nearbyEnergy);
                foreach (GridContentItem item in content)
                {
                    saturation += _fieldLedger.GetSourceEnergyAtCell(
                        item,
                        col,
                        row,
                        candidate.LayerId);
                }
            }
        }

        if (overlap > 0.0)
        {
            return overlap + saturation * 0.05;
        }

        int gapX = GapDistance(candidate.CellX, candidate.CellX + candidate.CellWidth, nearby.CellX, nearby.CellX + nearby.CellWidth);
        int gapY = GapDistance(candidate.CellY, candidate.CellY + candidate.CellHeight, nearby.CellY, nearby.CellY + nearby.CellHeight);
        double gapDistanceSquared = gapX * gapX + gapY * gapY;
        return 1.0 / (1.0 + 0.4 * gapDistanceSquared);
    }

    private static int GapDistance(int firstMin, int firstMax, int secondMin, int secondMax)
    {
        if (firstMax < secondMin) return secondMin - firstMax;
        if (secondMax < firstMin) return firstMin - secondMax;
        return 0;
    }

    private static bool Matches(string? value, string query, IReadOnlyList<string> terms)
    {
        if (terms.Count == 0) return false;
        string normalized = value?.ToLowerInvariant() ?? string.Empty;
        return (query.Length > 0 && normalized.Contains(query, StringComparison.Ordinal)) ||
               terms.All(normalized.Contains);
    }

    private static double BestScore(IEnumerable<string> values, string query, IReadOnlyList<string> terms) =>
        values.Select(value =>
        {
            string normalized = value?.ToLowerInvariant() ?? string.Empty;
            double exact = query.Length > 0 && normalized.Contains(query, StringComparison.Ordinal) ? 1.0 : 0.0;
            double termCoverage = terms.Count == 0 ? 0.0 : terms.Count(term => normalized.Contains(term, StringComparison.Ordinal)) / (double)terms.Count;
            return exact + termCoverage;
        }).DefaultIfEmpty(0.0).Max();

    private static string[] Tokenize(string query) => query
        .Split(new[] { ' ', '\t', '\r', '\n', ',', '.', ';', ':', '-', '_', '\'', '"' }, StringSplitOptions.RemoveEmptyEntries)
        .Select(term => term.Trim().ToLowerInvariant())
        .Where(term => term.Length > 1)
        .Distinct(StringComparer.Ordinal)
        .ToArray();
}
