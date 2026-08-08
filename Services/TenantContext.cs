namespace Inventory.Services
{
    public interface ITenantScoped
    {
        int TenantId { get; set; }
    }

    public interface ITenantContext
    {
        int TenantId { get; }
        bool HasTenant { get; }
        void SetTenant(int tenantId);
        void Clear();
    }

    /// <summary>Request-scoped current shop. Empty during migrations / host startup.</summary>
    public class TenantContext : ITenantContext
    {
        public int TenantId { get; private set; }
        public bool HasTenant { get; private set; }

        public void SetTenant(int tenantId)
        {
            TenantId = tenantId;
            HasTenant = tenantId > 0;
        }

        public void Clear()
        {
            TenantId = 0;
            HasTenant = false;
        }
    }
}
