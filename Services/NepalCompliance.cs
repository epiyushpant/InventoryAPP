namespace Inventory.Services
{
    /// <summary>
    /// Nepal fiscal year helpers. FY starts ~16 July (Shrawan) and ends ~15 July.
    /// </summary>
    public static class NepalFiscalYear
    {
        public const int StartMonth = 7;
        public const int StartDay = 16;

        public static (DateTime StartUtc, DateTime EndUtc, string Label) GetBounds(DateTime utcDate)
        {
            var d = utcDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(utcDate, DateTimeKind.Utc).Date
                : utcDate.ToUniversalTime().Date;

            int startYear = d.Month > StartMonth || (d.Month == StartMonth && d.Day >= StartDay)
                ? d.Year
                : d.Year - 1;

            var start = new DateTime(startYear, StartMonth, StartDay, 0, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(startYear + 1, StartMonth, StartDay, 0, 0, 0, DateTimeKind.Utc).AddTicks(-1);
            var label = $"{startYear % 100:D2}/{(startYear + 1) % 100:D2}";
            return (start, end, label);
        }

        public static string FormatInvoiceNumber(string fyLabel, int sequence)
            => $"INV-{fyLabel}-{sequence:D4}";
    }

    public static class NepalPan
    {
        /// <summary>Nepal PAN is 9 digits when provided.</summary>
        public static bool IsValid(string? pan)
        {
            if (string.IsNullOrWhiteSpace(pan)) return true; // optional unless taxable B2B
            var digits = pan.Trim();
            return digits.Length == 9 && digits.All(char.IsDigit);
        }

        public static void EnsureValidOrEmpty(string? pan, string fieldName = "PAN")
        {
            if (!IsValid(pan))
                throw new InvalidOperationException($"{fieldName} must be exactly 9 digits (Nepal PAN).");
        }

        public static void RequireForTaxableInvoice(string? pan)
        {
            if (string.IsNullOrWhiteSpace(pan) || !IsValid(pan))
                throw new InvalidOperationException(
                    "Taxable B2B invoice requires a valid 9-digit customer PAN.");
        }
    }

    public static class NepalVat
    {
        public const decimal Rate = 0.13m;

        public static (decimal Taxable, decimal NonTaxable, decimal Tax, decimal Grand) FromLines(
            IEnumerable<(decimal LineTotal, bool IsTaxable)> lines)
        {
            decimal taxable = 0;
            decimal nonTaxable = 0;
            foreach (var (lineTotal, isTaxable) in lines)
            {
                if (isTaxable) taxable += lineTotal;
                else nonTaxable += lineTotal;
            }

            taxable = Math.Round(taxable, 2, MidpointRounding.AwayFromZero);
            nonTaxable = Math.Round(nonTaxable, 2, MidpointRounding.AwayFromZero);
            var tax = Math.Round(taxable * Rate, 2, MidpointRounding.AwayFromZero);
            var grand = Math.Round(taxable + nonTaxable + tax, 2, MidpointRounding.AwayFromZero);
            return (taxable, nonTaxable, tax, grand);
        }
    }
}
