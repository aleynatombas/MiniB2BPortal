namespace MiniB2B.Business.Validation;

public enum StockLevel
{
    Available,
    Critical,
    Unavailable
}

public static class StockRules
{
    public static StockLevel Evaluate(int quantity, int criticalLevel)
    {
        if (quantity <= 0)
            return StockLevel.Unavailable;
        if (quantity <= criticalLevel)
            return StockLevel.Critical;
        return StockLevel.Available;
    }
}
