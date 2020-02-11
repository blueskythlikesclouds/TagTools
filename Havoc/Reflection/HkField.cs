namespace Havoc.Reflection
{
    public class HkField
    {
        public string Name { get; internal set; }
        public HkFieldFlags Flags { get; internal set; }
        public int ByteOffset { get; internal set; }
        public HkType Type { get; internal set; }
    }
}