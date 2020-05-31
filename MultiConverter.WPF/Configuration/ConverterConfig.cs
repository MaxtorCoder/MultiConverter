using MultiConverter.WPF.Constants;

namespace MultiConverter.WPF.Configuration
{
    public class ConverterConfig
    {
        public CascLoadType LoadType { get; set; }
        public string LocalStorage { get; set; }
        public string LocalBranch { get; set; }
        public string OnlineBranch { get; set; }
    }
}
