namespace ENet.Managed
{
    internal sealed class ENetUserDataContainer<T> : IENetUserDataContainer
    {
        public T Data { get; set; }

        public ENetUserDataContainer(T state)
        {
            Data = state;
        }

        public object? GetData() => Data;
    }
}
