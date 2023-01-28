namespace GUI.Data
{
    public class DataTag
    {
        public DataOrigin DataOrigin;
        public string Name { get; set; }
        public string Address { get; set; }
        public int ScanClass { get; set; }
        public string Type { get; set; }
        public int ID { get; set; }
        public string DefaultValue { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is DataTag tag &&
                   ID == tag.ID;
        }

        public override string? ToString()
        {
            return Name + "|" + Address + "|" + Type + "|" + ScanClass + "|" + DefaultValue + ".";
        }


    }
}
