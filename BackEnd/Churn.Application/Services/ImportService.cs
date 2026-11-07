using System.Globalization;
using Microsoft.VisualBasic.FileIO;
using Churn.Application.DTOs.Customers;
using Churn.Application.DTOs.Imports;

namespace Churn.Application.Services;

public sealed class ImportService(CustomerService customers)
{
    private static readonly string[] Headers = { "customerID", "gender", "SeniorCitizen", "Partner", "Dependents", "tenure", "PhoneService", "MultipleLines", "InternetService", "OnlineSecurity", "OnlineBackup", "DeviceProtection", "TechSupport", "StreamingTV", "StreamingMovies", "Contract", "PaperlessBilling", "PaymentMethod", "MonthlyCharges", "TotalCharges", "Churn" };

    public async Task<ImportResultResponse> ImportCsvAsync(Stream stream, CancellationToken ct = default)
    {
        var imported = 0; var skipped = 0; var errors = new List<string>(); var row = 0;
        using var parser = new TextFieldParser(stream);
        parser.TextFieldType = FieldType.Delimited; parser.SetDelimiters(","); parser.HasFieldsEnclosedInQuotes = true;
        if (parser.EndOfData) throw new ArgumentException("The CSV file is empty.");
        var header = parser.ReadFields();
        if (header is null || !Headers.SequenceEqual(header.Select(x => x.Trim()), StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException("CSV headers do not match the Telco churn dataset format.");
        while (!parser.EndOfData)
        {
            ct.ThrowIfCancellationRequested(); row++;
            try
            {
                var f = parser.ReadFields();
                if (f is null || f.Length != Headers.Length) throw new FormatException("Expected 21 columns.");
                bool YesNo(int i) => f[i].Equals("Yes", StringComparison.OrdinalIgnoreCase) ? true :
                    f[i].Equals("No", StringComparison.OrdinalIgnoreCase) ? false : throw new FormatException($"Invalid Yes/No value in {Headers[i]}.");
                int Int(int i) => int.Parse(f[i], CultureInfo.InvariantCulture);
                decimal Money(int i) => decimal.Parse(f[i], NumberStyles.Number, CultureInfo.InvariantCulture);
                decimal? Total() => string.IsNullOrWhiteSpace(f[19]) ? null : Money(19);
                bool? Churn() => string.IsNullOrWhiteSpace(f[20]) ? null : YesNo(20);
                var request = new CreateCustomerRequest(f[0].Trim(), f[1].Trim(), Int(2) == 1,
                    YesNo(3), YesNo(4), Int(5), YesNo(6), f[7], f[8], f[9], f[10], f[11], f[12], f[13], f[14],
                    f[15], YesNo(16), f[17], Money(18), Total(), Churn());
                await customers.CreateAsync(request, ct); imported++;
            }
            catch (Exception ex) when (ex is FormatException or ArgumentException or InvalidOperationException or OverflowException)
            {
                skipped++;
                if (errors.Count < 100) errors.Add($"Row {row + 1}: {ex.Message}");
            }
        }
        return new(imported, skipped, errors);
    }
}
