namespace NiyaCRM.Infrastructure.Configuration
{
    /// <summary>
    /// Represents the settings required to configure a PostgreSQL database connection for the NiyaCRM application.
    /// </summary>
    public class PostgreSqlSettings
    {
        /// <summary>
        /// Gets or sets the AWS secret key used for accessing secrets related to PostgreSQL configuration.
        /// </summary>
        public string AwsSecretKey { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the connection string for the PostgreSQL database.
        /// </summary>
        public string ConnectionString { get; set; } = string.Empty;
    }
}
