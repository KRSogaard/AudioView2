namespace AudioView.Common.Data
{
    public class Reading
    {
        public System.Guid Id { get; set; }
        public System.Guid Project { get; set; }
        public System.DateTime Time { get; set; }
        public bool Major { get; set; }
        public double LAeq { get; set; }
    }
}