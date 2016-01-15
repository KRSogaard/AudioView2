namespace AudioView.Common.Data
{
    public class ReadingData
    {
        public double LAeq { get; set; }

        public string SerializeToOneLine(string splitter)
        {
            return LAeq.ToString();
        }
    }
}
