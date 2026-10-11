
namespace SrpLab;

/// <summary>
/// Coordinates warehouse pick lines, stock allocation, routing, and output.
/// </summary>
public sealed class WarehousePickList
{
    private readonly WarehousePickLines _pickLines = new();
    private readonly StockAllocator _stockAllocator = new();
    private readonly WarehouseRoutePlanner _routePlanner = new();
    private readonly PickerInstructionFormatter _instructionFormatter = new();
    private readonly WmsXmlFormatter _xmlFormatter = new();

    public void AddNeed(
        string sku,
        string aisle,
        int bin,
        int qtyNeeded,
        int qtyOnHand)
    {
        _pickLines.AddNeed(sku, aisle, bin, qtyNeeded, qtyOnHand);
    }

    public IReadOnlyList<(string Sku, int Allocated)> Allocate()
    {
        return _stockAllocator.Allocate(_pickLines.Lines);
    }

    public IReadOnlyList<(string Aisle, int Bin, string Sku, int Qty)> WalkingOrder()
    {
        return _routePlanner.GetWalkingOrder(_pickLines.Lines);
    }

    public string PickerScript()
    {
        return _instructionFormatter.Format(
            WalkingOrder(),
            Allocate(),
            _pickLines.Lines);
    }

    public string WmsXmlBatch(string batchId)
    {
        return _xmlFormatter.Format(batchId, Allocate());
    }
}


public sealed class WarehousePickLines
{
    private readonly List<(string Sku, string Aisle, int Bin, int QtyNeeded, int QtyOnHand)> _lines = new();

    public IReadOnlyList<(string Sku, string Aisle, int Bin, int QtyNeeded, int QtyOnHand)> Lines
        => _lines;

    public void AddNeed(
        string sku,
        string aisle,
        int bin,
        int qtyNeeded,
        int qtyOnHand)
    {
        _lines.Add((sku, aisle, bin, qtyNeeded, qtyOnHand));
    }
}



public sealed class StockAllocator
{
    public IReadOnlyList<(string Sku, int Allocated)> Allocate(
        IReadOnlyList<(string Sku, string Aisle, int Bin, int QtyNeeded, int QtyOnHand)> lines)
    {
        var result = new List<(string Sku, int Allocated)>();

        foreach (var line in lines)
        {
            var allocated = Math.Min(line.QtyNeeded, line.QtyOnHand);
            result.Add((line.Sku, allocated));
        }

        return result;
    }
}




public sealed class WarehouseRoutePlanner
{
    public IReadOnlyList<(string Aisle, int Bin, string Sku, int Qty)> GetWalkingOrder(
        IReadOnlyList<(string Sku, string Aisle, int Bin, int QtyNeeded, int QtyOnHand)> lines)
    {
        return lines
            .OrderBy(line => line.Aisle)
            .ThenBy(line => line.Bin)
            .Select(line => (
                line.Aisle,
                line.Bin,
                line.Sku,
                Math.Min(line.QtyNeeded, line.QtyOnHand)))
            .Where(item => item.Item4 > 0)
            .ToList();
    }
}



public sealed class PickerInstructionFormatter
{
    public string Format(
        IReadOnlyList<(string Aisle, int Bin, string Sku, int Qty)> walkingOrder,
        IReadOnlyList<(string Sku, int Allocated)> allocations,
        IReadOnlyList<(string Sku, string Aisle, int Bin, int QtyNeeded, int QtyOnHand)> lines)
    {
        var steps = walkingOrder
            .Select((step, index) =>
                $"{index + 1}. Go aisle {step.Aisle} bin {step.Bin}: pick {step.Qty} × {step.Sku}");

        var shortfalls = allocations.Where(allocation =>
        {
            var need = lines.First(line => line.Sku == allocation.Sku).QtyNeeded;
            return allocation.Allocated < need;
        });

        var warning = shortfalls.Any()
            ? "SHORTAGES: " + string.Join(", ", shortfalls.Select(item => item.Sku))
            : "SHORTAGES: none";

        return string.Join('\n', steps) + "\n" + warning;
    }
}



public sealed class WmsXmlFormatter
{
    public string Format(
        string batchId,
        IReadOnlyList<(string Sku, int Allocated)> allocations)
    {
        var parts = allocations
            .Select(item => $"<line sku=\"{item.Sku}\" qty=\"{item.Allocated}\" />");

        return $"<batch id=\"{batchId}\">{string.Join("", parts)}</batch>";
    }
}
