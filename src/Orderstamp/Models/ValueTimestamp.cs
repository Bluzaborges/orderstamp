namespace Orderstamp.Models
{
    public class ValueTimestamp
    {
        public int RTS { get; set; }
        public int WTS { get; set; }

        private ValueTimestamp()
        {
            RTS = -1;
            WTS = -1;
        }

        public static ValueTimestamp Create()
        {
            return new ValueTimestamp();
        }
    }
}
