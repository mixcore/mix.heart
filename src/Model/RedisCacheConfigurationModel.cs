namespace Mix.Heart.Models
{
    public class RedisCacheConfigurationModel
    {
        public string ConnectionString { get; set; }
        public int SlidingExpirationInMinute { get; set; } = 20;
        public int? AbsoluteExpirationInMinute { get; set; } = 20;
        public int? AbsoluteExpirationRelativeToNowInMinute { get; set; } = 20;

        public RedisCacheConfigurationModel()
        {

        }
    }
}
