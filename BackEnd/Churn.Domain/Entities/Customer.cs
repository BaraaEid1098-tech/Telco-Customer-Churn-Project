using Churn.Domain.Common;

namespace Churn.Domain.Entities;

public class Customer : BaseEntity
{
    public string CustomerCode { get; set; } = null!;

    public string Gender { get; set; } = null!;

    public bool SeniorCitizen { get; set; }

    public bool Partner { get; set; }
    public bool Dependents { get; set; }

    public int Tenure { get; set; }

    public bool PhoneService { get; set; }

    public string MultipleLines { get; set; } = null!;
    public string InternetService { get; set; } = null!;
    public string OnlineSecurity { get; set; } = null!;
    public string OnlineBackup { get; set; } = null!;
    public string DeviceProtection { get; set; } = null!;
    public string TechSupport { get; set; } = null!;
    public string StreamingTV { get; set; } = null!;
    public string StreamingMovies { get; set; } = null!;

    public string Contract { get; set; } = null!;//

    public bool PaperlessBilling { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public decimal MonthlyCharges { get; set; }

    public decimal? TotalCharges { get; set; }


    public bool? ActualChurn { get; set; }

    public ICollection<Prediction> Predictions { get; set; } = new List<Prediction>();
    public ICollection<CampaignCustomer> CampaignCustomers { get; set; } = new List<CampaignCustomer>();
}
