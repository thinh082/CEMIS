namespace CEMIS.Common;

public static class UnitConversionHelper
{
    private enum UnitCategory
    {
        Mass,
        Volume,
        Energy,
        Other
    }

    private class UnitDefinition
    {
        public string Code { get; set; } = string.Empty;
        public UnitCategory Category { get; set; }
        public decimal RatioToBase { get; set; } // Base: Mass = kg, Volume = lit, Energy = kWh
    }

    private static readonly Dictionary<string, UnitDefinition> SupportedUnits = new(StringComparer.OrdinalIgnoreCase)
    {
        // Khối lượng (Base: kg)
        ["kg"] = new() { Code = "kg", Category = UnitCategory.Mass, RatioToBase = 1m },
        ["kilogram"] = new() { Code = "kg", Category = UnitCategory.Mass, RatioToBase = 1m },
        ["g"] = new() { Code = "g", Category = UnitCategory.Mass, RatioToBase = 0.001m },
        ["gram"] = new() { Code = "g", Category = UnitCategory.Mass, RatioToBase = 0.001m },
        ["tan"] = new() { Code = "tan", Category = UnitCategory.Mass, RatioToBase = 1000m },
        ["tấn"] = new() { Code = "tan", Category = UnitCategory.Mass, RatioToBase = 1000m },
        ["t"] = new() { Code = "tan", Category = UnitCategory.Mass, RatioToBase = 1000m },
        ["ton"] = new() { Code = "tan", Category = UnitCategory.Mass, RatioToBase = 1000m },
        ["lb"] = new() { Code = "lb", Category = UnitCategory.Mass, RatioToBase = 0.45359237m },
        ["pound"] = new() { Code = "lb", Category = UnitCategory.Mass, RatioToBase = 0.45359237m },

        // Thể tích (Base: lit)
        ["lit"] = new() { Code = "lit", Category = UnitCategory.Volume, RatioToBase = 1m },
        ["lít"] = new() { Code = "lit", Category = UnitCategory.Volume, RatioToBase = 1m },
        ["l"] = new() { Code = "lit", Category = UnitCategory.Volume, RatioToBase = 1m },
        ["ml"] = new() { Code = "ml", Category = UnitCategory.Volume, RatioToBase = 0.001m },
        ["m3"] = new() { Code = "m3", Category = UnitCategory.Volume, RatioToBase = 1000m },
        ["m³"] = new() { Code = "m3", Category = UnitCategory.Volume, RatioToBase = 1000m },
        ["gal"] = new() { Code = "gal", Category = UnitCategory.Volume, RatioToBase = 3.78541m },
        ["gallon"] = new() { Code = "gal", Category = UnitCategory.Volume, RatioToBase = 3.78541m },

        // Năng lượng (Base: kWh)
        ["kwh"] = new() { Code = "kWh", Category = UnitCategory.Energy, RatioToBase = 1m },
        ["mwh"] = new() { Code = "MWh", Category = UnitCategory.Energy, RatioToBase = 1000m },
        ["gwh"] = new() { Code = "GWh", Category = UnitCategory.Energy, RatioToBase = 1000000m },
        ["mj"] = new() { Code = "MJ", Category = UnitCategory.Energy, RatioToBase = 1m / 3.6m },
        ["gj"] = new() { Code = "GJ", Category = UnitCategory.Energy, RatioToBase = 1000m / 3.6m },
        ["tj"] = new() { Code = "TJ", Category = UnitCategory.Energy, RatioToBase = 1000000m / 3.6m },
        ["btu"] = new() { Code = "BTU", Category = UnitCategory.Energy, RatioToBase = 1m / 3412.142m },
        ["mmbtu"] = new() { Code = "MMBTU", Category = UnitCategory.Energy, RatioToBase = 1000000m / 3412.142m }
    };

    /// <summary>
    /// Tính toán hệ số quy đổi tự động từ donViNhap sang donViChuan
    /// </summary>
    public static (decimal HeSoQuyDoi, decimal SoLuongQuyChuan) Convert(decimal soLuongNhap, string? donViNhap, string donViChuan)
    {
        if (string.IsNullOrWhiteSpace(donViNhap) || string.IsNullOrWhiteSpace(donViChuan))
        {
            return (1.0m, soLuongNhap);
        }

        var inputClean = donViNhap.Trim();
        var targetClean = donViChuan.Trim();

        // Cùng đơn vị
        if (string.Equals(inputClean, targetClean, StringComparison.OrdinalIgnoreCase))
        {
            return (1.0m, soLuongNhap);
        }

        bool hasInput = SupportedUnits.TryGetValue(inputClean, out var inputDef);
        bool hasTarget = SupportedUnits.TryGetValue(targetClean, out var targetDef);

        if (hasInput && hasTarget)
        {
            if (inputDef!.Category != targetDef!.Category)
            {
                throw new InvalidOperationException(
                    $"Không thể quy đổi giữa đơn vị nhập '{inputClean}' ({inputDef.Category}) và đơn vị chuẩn '{targetClean}' ({targetDef.Category}). Vui lòng nhập đúng đại lượng đo lường.");
            }

            decimal heSo = Math.Round(inputDef.RatioToBase / targetDef.RatioToBase, 6);
            decimal soLuongQuyChuan = Math.Round(soLuongNhap * heSo, 4);
            return (heSo, soLuongQuyChuan);
        }

        // Nếu là đơn vị đặc thù (người, p.km, lượt) không nằm trong ma trận vật lý, giữ nguyên hệ số 1
        return (1.0m, soLuongNhap);
    }
}
