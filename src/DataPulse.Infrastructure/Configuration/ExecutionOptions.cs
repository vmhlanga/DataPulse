namespace DataPulse.Infrastructure.Configuration
{
    public class ExecutionOptions
    {
        public bool AllowParallel { get; set; }
        public bool ContinueOnError { get; set; }
        public SsisOptions SSIS { get; set; } = new();
        public AdfOptions ADF { get; set; } = new();
    }

    public class SsisOptions
    {
        public string Mode { get; set; } = "Catalog"; // Catalog | SqlAgent | Dtexec
        public string CatalogServer { get; set; } = string.Empty;
        public string Folder { get; set; } = string.Empty;
    }

    public class AdfOptions
    {
        public string TenantId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string SubscriptionId { get; set; } = string.Empty;
        public string ResourceGroup { get; set; } = string.Empty;
        public string FactoryName { get; set; } = string.Empty;
    }
}
