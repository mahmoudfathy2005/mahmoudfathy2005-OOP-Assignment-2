
namespace SrpLab;

/// <summary>
/// Coordinates loan risk, eligibility, document requirements, and output.
/// </summary>
public sealed class LoanDesk
{
    private readonly LoanRiskCalculator _riskCalculator = new();
    private readonly LoanEligibilityPolicy _eligibilityPolicy = new();
    private readonly LoanDocumentChecklist _documentChecklist = new();
    private readonly LoanDocumentFormatter _formatter = new();

    public decimal RequestedAmount { get; }
    public int CreditScore { get; }
    public int EmploymentMonths { get; }
    public bool HasCollateral { get; }

    public LoanDesk(
        decimal requestedAmount,
        int creditScore,
        int employmentMonths,
        bool hasCollateral)
    {
        RequestedAmount = requestedAmount;
        CreditScore = creditScore;
        EmploymentMonths = employmentMonths;
        HasCollateral = hasCollateral;
    }

    public decimal RiskScore()
    {
        return _riskCalculator.Calculate(
            RequestedAmount,
            CreditScore,
            EmploymentMonths,
            HasCollateral);
    }

    public bool IsEligible()
    {
        return _eligibilityPolicy.IsEligible(RiskScore(), CreditScore);
    }

    public IReadOnlyList<string> RequiredDocuments()
    {
        return _documentChecklist.GetRequiredDocuments(
            RequestedAmount,
            HasCollateral,
            EmploymentMonths,
            IsEligible());
    }

    public string DecisionLetter(string applicantName)
    {
        return _formatter.DecisionLetter(
            applicantName,
            RequestedAmount,
            RiskScore(),
            IsEligible(),
            RequiredDocuments());
    }

    public string UnderwriterCsvRow(string applicationId)
    {
        return _formatter.UnderwriterCsvRow(
            applicationId,
            CreditScore,
            EmploymentMonths,
            HasCollateral,
            RiskScore(),
            IsEligible());
    }
}


public sealed class LoanRiskCalculator
{
    public decimal Calculate(
        decimal requestedAmount,
        int creditScore,
        int employmentMonths,
        bool hasCollateral)
    {
        decimal score = 100m;

        score -= Math.Max(0, 700 - creditScore) * 0.15m;

        if (employmentMonths < 6)
            score -= 20m;

        if (requestedAmount > 50_000m && !hasCollateral)
            score -= 25m;

        if (requestedAmount > 150_000m)
            score -= 10m;

        return Math.Clamp(score, 0m, 100m);
    }
}



public sealed class LoanEligibilityPolicy
{
    public bool IsEligible(decimal riskScore, int creditScore)
    {
        return riskScore >= 55m && creditScore >= 580;
    }
}




public sealed class LoanDocumentChecklist
{
    public IReadOnlyList<string> GetRequiredDocuments(
        decimal requestedAmount,
        bool hasCollateral,
        int employmentMonths,
        bool isEligible)
    {
        var docs = new List<string>
        {
            "National ID",
            "Proof of income (3 months)"
        };

        if (requestedAmount > 40_000m)
            docs.Add("Bank statements (6 months)");

        if (hasCollateral)
            docs.Add("Collateral ownership deed");

        if (employmentMonths < 12)
            docs.Add("Employer letter");

        if (!isEligible)
            docs.Add("Manual underwriter referral form");

        return docs;
    }
}



public sealed class LoanDocumentFormatter
{
    public string DecisionLetter(
        string applicantName,
        decimal requestedAmount,
        decimal riskScore,
        bool isEligible,
        IReadOnlyList<string> requiredDocuments)
    {
        if (isEligible)
        {
            return $"Dear {applicantName},\nYour request for {requestedAmount:C} is pre-approved (risk {riskScore:0}).\n" +
                   $"Please upload: {string.Join("; ", requiredDocuments)}.\n";
        }

        return $"Dear {applicantName},\nWe are unable to approve {requestedAmount:C} at this time.\n" +
               $"Reference risk={riskScore:0}. You may reapply after improving documentation.\n";
    }

    public string UnderwriterCsvRow(
        string applicationId,
        int creditScore,
        int employmentMonths,
        bool hasCollateral,
        decimal riskScore,
        bool isEligible)
    {
        return $"{applicationId},{creditScore},{employmentMonths}," +
               $"{(hasCollateral ? 1 : 0)},{riskScore:0.00}," +
               $"{(isEligible ? "Y" : "N")}";
    }
}
