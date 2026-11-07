using Churn.Application.DTOs.Customers;
using Churn.Application.Interfaces;
using Churn.Domain.Entities;
using Microsoft.Extensions.Caching.Hybrid;

namespace Churn.Application.Services;

public sealed class CustomerService(
    IUnitOfWork unitOfWork,
    HybridCache cache)
{
    private const string AllCustomersCacheKey = "customers:all";

    private static string CustomerCacheKey(int id)
        => $"customers:{id}";

    public async Task<IReadOnlyList<CustomerResponse>> GetAllAsync(
        CancellationToken ct = default)
    {
        return await cache.GetOrCreateAsync(
            AllCustomersCacheKey,
            async cancellationToken =>
            {
                var customers =
                    await unitOfWork.Customers.GetAllAsync(cancellationToken);

                return customers.Select(Map).ToList();
            },
            cancellationToken: ct);
    }

    public async Task<CustomerResponse?> GetByIdAsync(
        int id,
        CancellationToken ct = default)
    {
        return await cache.GetOrCreateAsync(
            CustomerCacheKey(id),
            async cancellationToken =>
            {
                var customer =
                    await unitOfWork.Customers.GetByIdAsync(id, cancellationToken);

                return customer is null ? null : Map(customer);
            },
            cancellationToken: ct);
    }

    public async Task<CustomerResponse> CreateAsync(
        CreateCustomerRequest request,
        CancellationToken ct = default)
    {
        Validate(
            request.CustomerCode,
            request.Gender,
            request.Tenure,
            request.MonthlyCharges,
            request.TotalCharges);

        var code = request.CustomerCode.Trim();

        if (await unitOfWork.Customers.GetByCodeAsync(code, ct) is not null)
            throw new InvalidOperationException(
                $"A customer with code '{code}' already exists.");

        var entity = new Customer
        {
            CustomerCode = code
        };

        Apply(
            entity,
            request.Gender,
            request.SeniorCitizen,
            request.Partner,
            request.Dependents,
            request.Tenure,
            request.PhoneService,
            request.MultipleLines,
            request.InternetService,
            request.OnlineSecurity,
            request.OnlineBackup,
            request.DeviceProtection,
            request.TechSupport,
            request.StreamingTV,
            request.StreamingMovies,
            request.Contract,
            request.PaperlessBilling,
            request.PaymentMethod,
            request.MonthlyCharges,
            request.TotalCharges,
            request.ActualChurn);

        await unitOfWork.Customers.AddAsync(entity, ct);
        await unitOfWork.SaveChangesAsync(ct);

        // Invalidate customers list cache
        await cache.RemoveAsync(AllCustomersCacheKey, ct);

        return Map(entity);
    }

    public async Task<CustomerResponse?> UpdateAsync(
        int id,
        UpdateCustomerRequest request,
        CancellationToken ct = default)
    {
        var entity = await unitOfWork.Customers.GetByIdAsync(id, ct);

        if (entity is null)
            return null;

        Validate(
            entity.CustomerCode,
            request.Gender,
            request.Tenure,
            request.MonthlyCharges,
            request.TotalCharges);

        Apply(
            entity,
            request.Gender,
            request.SeniorCitizen,
            request.Partner,
            request.Dependents,
            request.Tenure,
            request.PhoneService,
            request.MultipleLines,
            request.InternetService,
            request.OnlineSecurity,
            request.OnlineBackup,
            request.DeviceProtection,
            request.TechSupport,
            request.StreamingTV,
            request.StreamingMovies,
            request.Contract,
            request.PaperlessBilling,
            request.PaymentMethod,
            request.MonthlyCharges,
            request.TotalCharges,
            request.ActualChurn);

        entity.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Customers.Update(entity);

        await unitOfWork.SaveChangesAsync(ct);

        await cache.RemoveAsync(CustomerCacheKey(id), ct);
        await cache.RemoveAsync(AllCustomersCacheKey, ct);

        return Map(entity);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken ct = default)
    {
        var entity = await unitOfWork.Customers.GetByIdAsync(id, ct);

        if (entity is null)
            return false;

        unitOfWork.Customers.Delete(entity);

        await unitOfWork.SaveChangesAsync(ct);

        // Invalidate deleted customer cache
        await cache.RemoveAsync(CustomerCacheKey(id), ct);
        await cache.RemoveAsync(AllCustomersCacheKey, ct);

        return true;
    }

    private static void Validate(
        string code,
        string gender,
        int tenure,
        decimal monthly,
        decimal? total)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length > 20)
            throw new ArgumentException(
                "Customer code is required and must be at most 20 characters.");

        if (string.IsNullOrWhiteSpace(gender) || gender.Length > 10)
            throw new ArgumentException(
                "Gender is required and must be at most 10 characters.");

        if (tenure < 0 || tenure > 1200)
            throw new ArgumentException(
                "Tenure must be between 0 and 1200 months.");

        if (monthly < 0 || total < 0)
            throw new ArgumentException("Charges cannot be negative.");

        if (monthly > 99999999.99m || total > 99999999.99m)
            throw new ArgumentException(
                "Charges exceed the supported amount.");
    }

    private static void Apply(
        Customer c,
        string gender,
        bool senior,
        bool partner,
        bool dependents,
        int tenure,
        bool phone,
        string multiple,
        string internet,
        string security,
        string backup,
        string protection,
        string support,
        string tv,
        string movies,
        string contract,
        bool paperless,
        string payment,
        decimal monthly,
        decimal? total,
        bool? churn)
    {
        c.Gender = gender.Trim();
        c.SeniorCitizen = senior;
        c.Partner = partner;
        c.Dependents = dependents;
        c.Tenure = tenure;
        c.PhoneService = phone;

        c.MultipleLines = Required(multiple, nameof(multiple), 30);
        c.InternetService = Required(internet, nameof(internet), 30);
        c.OnlineSecurity = Required(security, nameof(security), 30);
        c.OnlineBackup = Required(backup, nameof(backup), 30);
        c.DeviceProtection = Required(protection, nameof(protection), 30);
        c.TechSupport = Required(support, nameof(support), 30);
        c.StreamingTV = Required(tv, nameof(tv), 30);
        c.StreamingMovies = Required(movies, nameof(movies), 30);
        c.Contract = Required(contract, nameof(contract), 30);

        c.PaperlessBilling = paperless;
        c.PaymentMethod = Required(payment, nameof(payment), 50);

        c.MonthlyCharges = monthly;
        c.TotalCharges = total;
        c.ActualChurn = churn;
    }

    private static string Required(
        string value,
        string field,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{field} is required.");

        if (value.Trim().Length > maxLength)
            throw new ArgumentException(
                $"{field} must be at most {maxLength} characters.");

        return value.Trim();
    }

    private static CustomerResponse Map(Customer c)
    {
        return new CustomerResponse(
            c.Id,
            c.CustomerCode,
            c.Gender,
            c.SeniorCitizen,
            c.Partner,
            c.Dependents,
            c.Tenure,
            c.PhoneService,
            c.MultipleLines,
            c.InternetService,
            c.OnlineSecurity,
            c.OnlineBackup,
            c.DeviceProtection,
            c.TechSupport,
            c.StreamingTV,
            c.StreamingMovies,
            c.Contract,
            c.PaperlessBilling,
            c.PaymentMethod,
            c.MonthlyCharges,
            c.TotalCharges,
            c.ActualChurn,
            c.CreatedAt);
    }
}