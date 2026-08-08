namespace Inventory.Services
{
    /// <summary>
    /// Wave B: status locks. Never disable via tenant capabilities.
    /// </summary>
    public static class DocumentLock
    {
        public static bool IsEditable(string documentType, string? status)
        {
            var s = (status ?? "").Trim();
            return documentType switch
            {
                "PurchaseOrder" => IsOneOf(s, "Draft", "Pending"),
                "Sale" => IsOneOf(s, "Draft", "Pending", "Confirmed"),
                "PurchaseRequisition" => IsOneOf(s, "Pending"),
                "StockTransfer" => IsOneOf(s, "Pending", "In Transit"),
                // Posted create-only documents: never editable via Update
                "GRN" => false,
                "DeliveryNote" => false,
                // Invoice: structural fields locked; payment status handled separately
                "SalesInvoice" => false,
                _ => true
            };
        }

        public static bool IsPaymentStatusEditable(string? status)
        {
            var s = (status ?? "").Trim();
            return IsOneOf(s, "Due", "Partial", "Paid");
        }

        public static void EnsureEditable(string documentType, string? status)
        {
            if (!IsEditable(documentType, status))
            {
                throw new InvalidOperationException(
                    $"{documentType} with status '{status ?? "(none)"}' is locked and cannot be edited. Create a correcting document if needed.");
            }
        }

        public static void EnsureDeletable(string documentType, string? status)
        {
            if (!IsEditable(documentType, status))
            {
                throw new InvalidOperationException(
                    $"Cannot delete a locked {documentType} (status: '{status ?? "(none)"}'). Finalized transactions must be kept for auditing.");
            }
        }

        public static void EnsurePostedImmutable(string documentType)
        {
            throw new InvalidOperationException(
                $"Posted {documentType} records cannot be modified. Create a new document for corrections.");
        }

        private static bool IsOneOf(string status, params string[] allowed)
        {
            foreach (var a in allowed)
            {
                if (string.Equals(status, a, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
