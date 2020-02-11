namespace Havoc.Reflection
{
    public class HkParameter
    {
        public string Name { get; internal set; }
        public object Value { get; internal set; }

        public bool IsInt => Name[ 0 ] == 'v';
        public bool IsType => Name[ 0 ] == 't';

        public long IntValue => ( long ) Value;
        public HkType TypeValue => ( HkType ) Value;
    }
}