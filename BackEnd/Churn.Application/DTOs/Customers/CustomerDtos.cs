namespace Churn.Application.DTOs.Customers;

public sealed record CreateCustomerRequest(
    string CustomerCode, string Gender, bool SeniorCitizen, bool Partner, bool Dependents,
    int Tenure, bool PhoneService, string MultipleLines, string InternetService,
    string OnlineSecurity, string OnlineBackup, string DeviceProtection, string TechSupport,
    string StreamingTV, string StreamingMovies, string Contract, bool PaperlessBilling,
    string PaymentMethod, decimal MonthlyCharges, decimal? TotalCharges, bool? ActualChurn);

public sealed record UpdateCustomerRequest(
    string Gender, bool SeniorCitizen, bool Partner, bool Dependents, int Tenure,
    bool PhoneService, string MultipleLines, string InternetService, string OnlineSecurity,
    string OnlineBackup, string DeviceProtection, string TechSupport, string StreamingTV,
    string StreamingMovies, string Contract, bool PaperlessBilling, string PaymentMethod,
    decimal MonthlyCharges, decimal? TotalCharges, bool? ActualChurn);

public sealed record CustomerResponse(
    int Id, string CustomerCode, string Gender, bool SeniorCitizen, bool Partner,
    bool Dependents, int Tenure, bool PhoneService, string MultipleLines,
    string InternetService, string OnlineSecurity, string OnlineBackup,
    string DeviceProtection, string TechSupport, string StreamingTV,
    string StreamingMovies, string Contract, bool PaperlessBilling,
    string PaymentMethod, decimal MonthlyCharges, decimal? TotalCharges,
    bool? ActualChurn, DateTime CreatedAt);
