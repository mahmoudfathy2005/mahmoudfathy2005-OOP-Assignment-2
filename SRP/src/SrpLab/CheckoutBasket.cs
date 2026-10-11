

namespace SrpLab;

public sealed class CheckoutBasket
{
    private readonly BasketLines _lines = new();
    private readonly CouponDiscountCalculator _couponCalculator = new();
    private readonly BasketPricing _pricing = new();
    private readonly GiftMessageCardFormatter _giftFormatter = new();
    private readonly PaymentAuthorizationStub _paymentStub = new();

    private string? _couponRaw;
    private bool _giftWrap;

    public void AddLine(string sku, decimal price, int qty)
    {
        _lines.AddLine(sku, price, qty);
    }

    public void ApplyCouponText(string? couponText)
    {
        _couponRaw = couponText;
    }

    public void EnableGiftWrap()
    {
        _giftWrap = true;
    }

    public decimal SubTotal()
    {
        return _lines.SubTotal();
    }

    public decimal DiscountAmount()
    {
        return _couponCalculator.Calculate(
            _couponRaw,
            SubTotal());
    }

    public decimal GrandTotal()
    {
        return _pricing.CalculateGrandTotal(
            SubTotal(),
            DiscountAmount(),
            _giftWrap);
    }

    public string GiftMessageCard(string fromName)
    {
        return _giftFormatter.Format(
            fromName,
            _lines.GetSkuList(),
            GrandTotal());
    }

    public string AuthorizePaymentStub(string cardLast4)
    {
        return _paymentStub.Generate(
            GrandTotal(),
            cardLast4,
            _lines.Count);
    }
}




public sealed class BasketLines
{
    private readonly List<(string Sku, decimal Price, int Qty)> _lines = new();

    public int Count => _lines.Count;

    public void AddLine(string sku, decimal price, int qty)
    {
        if (qty <= 0)
            throw new ArgumentOutOfRangeException(nameof(qty));

        _lines.Add((sku, price, qty));
    }

    public decimal SubTotal()
    {
        return _lines.Sum(line => line.Price * line.Qty);
    }

    public string GetSkuList()
    {
        return string.Join(", ", _lines.Select(line => line.Sku));
    }
}





public sealed class CouponDiscountCalculator
{
    public decimal Calculate(string? couponText, decimal subtotal)
    {
        if (string.IsNullOrWhiteSpace(couponText))
            return 0m;

        var text = couponText.Trim().ToUpperInvariant();

        if (text.StartsWith("SAVE")
            && int.TryParse(text[4..], out var percentage)
            && percentage is > 0 and <= 50)
        {
            return Math.Round(subtotal * percentage / 100m, 2);
        }

        if (text.Contains("FREESHIP"))
            return 0m;

        if (text == "WELCOME10")
            return Math.Min(10m, subtotal);

        return 0m;
    }
}





public sealed class BasketPricing
{
    private const decimal GiftWrapFee = 4.99m;

    public decimal CalculateGrandTotal(
        decimal subtotal,
        decimal discount,
        bool giftWrapEnabled)
    {
        var total = subtotal - discount;

        if (giftWrapEnabled)
            total += GiftWrapFee;

        return Math.Max(0m, total);
    }
}




public sealed class GiftMessageCardFormatter
{
    public string Format(
        string fromName,
        string itemList,
        decimal grandTotal)
    {
        return $"Dear friend,\nA gift from {fromName} awaits ({itemList}).\n" +
               $"Total surprise value: {grandTotal:C}\n";
    }
}






public sealed class PaymentAuthorizationStub
{
    public string Generate(
        decimal grandTotal,
        string cardLast4,
        int lineCount)
    {
        var payload =
            $"{grandTotal:0.00}|{cardLast4}|{lineCount}";

        var hash = payload.GetHashCode();

        return $"AUTH-{Math.Abs(hash):X8}";
    }
}
