using Havoc.Reflection;

namespace Havoc.Objects
{
    public interface IHkObject
    {
        object Value { get; }
        HkType Type { get; }
    }
}