

namespace SrpLab;

/// <summary>
/// Coordinates kitchen order, allergen detection, ETA calculation, and ticket output.
/// </summary>
public sealed class KitchenTicket
{
    private readonly KitchenOrder _order = new();
    private readonly AllergenDetector _allergenDetector = new();
    private readonly KitchenEtaCalculator _etaCalculator = new();
    private readonly KitchenTicketFormatter _formatter = new();

    public void AddItem(string item, IEnumerable<string> ingredients, int prepMinutes)
    {
        _order.AddItem(item, ingredients, prepMinutes);
    }

    public IReadOnlyList<string> DetectAllergens()
    {
        return _allergenDetector.Detect(_order.Items);
    }

    public int EstimatedReadyMinutes(int openStations)
    {
        return _etaCalculator.Calculate(
            _order.Items,
            openStations,
            DetectAllergens().Count);
    }

    public string RenderThermalTicket(int orderNumber)
    {
        return _formatter.Render(
            orderNumber,
            EstimatedReadyMinutes(2),
            _order.Items,
            DetectAllergens());
    }

    public string ExpoLaneHint()
    {
        return DetectAllergens().Count > 0
            ? "LANE-ALLERGY"
            : EstimatedReadyMinutes(2) > 20
                ? "LANE-SLOW"
                : "LANE-FAST";
    }
}


public sealed class KitchenOrder
{
    private readonly List<(string Item, List<string> Ingredients, int PrepMinutes)> _items = new();

    public IReadOnlyList<(string Item, List<string> Ingredients, int PrepMinutes)> Items
        => _items;

    public void AddItem(string item, IEnumerable<string> ingredients, int prepMinutes)
    {
        _items.Add((
            item,
            ingredients.Select(i => i.Trim().ToLowerInvariant()).ToList(),
            prepMinutes));
    }
}



public sealed class AllergenDetector
{
    public IReadOnlyList<string> Detect(
        IReadOnlyList<(string Item, List<string> Ingredients, int PrepMinutes)> items)
    {
        var hits = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var (_, ingredients, _) in items)
        {
            foreach (var ing in ingredients)
            {
                if (ing.Contains("milk") || ing.Contains("cheese") || ing.Contains("butter"))
                    hits.Add("dairy");

                if (ing.Contains("wheat") || ing.Contains("flour") || ing.Contains("bread"))
                    hits.Add("gluten");

                if (ing.Contains("peanut") || ing.Contains("almond") || ing.Contains("cashew"))
                    hits.Add("nuts");

                if (ing.Contains("shrimp") || ing.Contains("prawn") || ing.Contains("crab"))
                    hits.Add("shellfish");
            }
        }

        return hits.OrderBy(x => x).ToList();
    }
}



public sealed class KitchenEtaCalculator
{
    public int Calculate(
        IReadOnlyList<(string Item, List<string> Ingredients, int PrepMinutes)> items,
        int openStations,
        int allergenCount)
    {
        if (openStations <= 0)
            openStations = 1;

        var sequential = items.Sum(i => i.PrepMinutes);
        var parallel = (int)Math.Ceiling(sequential / (double)openStations);

        if (allergenCount > 0)
            parallel += 3;

        var longest = items.Count == 0 ? 0 : items.Max(i => i.PrepMinutes);

        return Math.Max(parallel, longest);
    }
}



public sealed class KitchenTicketFormatter
{
    public string Render(
        int orderNumber,
        int etaMinutes,
        IReadOnlyList<(string Item, List<string> Ingredients, int PrepMinutes)> items,
        IReadOnlyList<string> allergens)
    {
        var width = 32;
        var line = new string('=', width);

        var body = string.Join(
            '\n',
            items.Select(i => $"* {i.Item.ToUpperInvariant()} ({i.PrepMinutes}m)"));

        var allergyLine = allergens.Count == 0
            ? "ALLERGENS: none"
            : "ALLERGENS: " + string.Join(",", allergens);

        return $"{line}\nORDER #{orderNumber}\nETA {etaMinutes} MIN\n" +
               $"{body}\n{allergyLine}\n{line}\n";
    }
}
