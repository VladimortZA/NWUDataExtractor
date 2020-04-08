namespace NWUDataExtractor.Core.Model
{
    public class Module
    {
        public Module(string code)
        {
            Code = code;
        }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
