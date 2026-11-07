namespace Churn.Application.DTOs.Imports;

public sealed record ImportResultResponse(int Imported, int Skipped, IReadOnlyList<string> Errors);
