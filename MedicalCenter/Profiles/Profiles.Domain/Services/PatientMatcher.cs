using Profiles.Domain.ValueObjects;

namespace Profiles.Domain.Services;

public static class PatientMatcher
{
    public const int MatchThreshold = 13;

    private const int NameWeight = 5;
    private const int DateOfBirthWeight = 3;

    public static Patient? FindBestMatch(PersonName name, DateOnly dateOfBirth, IEnumerable<Patient> candidates)
    {
        Patient? bestMatch = null;
        var bestScore = 0;

        foreach (var candidate in candidates)
        {
            var score = Score(name, dateOfBirth, candidate);

            if (score >= MatchThreshold && score > bestScore)
            {
                bestScore = score;
                bestMatch = candidate;
            }
        }

        return bestMatch;
    }

    private static int Score(PersonName name, DateOnly dateOfBirth, Patient candidate)
    {
        var score = 0;

        if (Matches(candidate.Name.FirstName, name.FirstName))
            score += NameWeight;

        if (Matches(candidate.Name.LastName, name.LastName))
            score += NameWeight;

        if (candidate.Name.MiddleName is not null && Matches(candidate.Name.MiddleName, name.MiddleName))
            score += NameWeight;

        if (candidate.DateOfBirth == dateOfBirth)
            score += DateOfBirthWeight;

        return score;
    }

    private static bool Matches(string candidateValue, string? searchValue)
        => searchValue is not null && string.Equals(candidateValue, searchValue, StringComparison.OrdinalIgnoreCase);
}
