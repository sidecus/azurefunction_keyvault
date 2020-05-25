namespace zyin.Function
{

    /// <summary>
    /// Sample app config
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// Gets or sets an integer config
        /// </summary>
        public int IntConfig { get; set; }

        /// <summary>
        /// Gets or sets a string config
        /// </summary>
        public string StringConfig { get; set; }

        /// <summary>
        /// Secret value in config bound from KeyVault
        /// </summary>
        /// <value></value>
        public string AppSecret { get; set; }
    }
}