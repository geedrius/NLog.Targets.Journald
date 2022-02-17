using NLog.Config;

namespace NLog.Targets.Journald
{
    /// <summary>
    /// Additional Journal field included in every emitted event 
    /// </summary>
    [NLogConfigurationItem]
    public class StaticField
    {
        /// <summary>
        /// Name of additional Journal field
        /// </summary>
        [RequiredParameter]
        public string Key { get; set; }

        /// <summary>
        /// Value of additional Journal field
        /// </summary>
        [RequiredParameter]
        public string Value { get; set; } 
    }
}