namespace GUI.Data
{
    public class DataOrigin
    {
        public string Name { get; set; }
        public string Ip { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public List<DataTag> Tags { get; set; }

        public DataOrigin(string name, string ip, string path, string type)
        {
            Name = name;
            Ip = ip;
            Path = path;
            Type = type;
            Tags = new List<DataTag>();
        }

        public override bool Equals(object? obj)
        {
            return obj is DataOrigin origin &&
                   Ip == origin.Ip &&
                   Path == origin.Path;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Ip, Path);
        }

        public override string? ToString()
        {
            return "Name:" + Name + "\t IP:" + Ip + "\t Path:" + Path + "\t Type:" + Type;
        }
    }
}
