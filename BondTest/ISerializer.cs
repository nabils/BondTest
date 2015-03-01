namespace BondTest
{
    public interface ISerializer<T>
    {
        byte[] Serialize(Data obj);
        Data Deserialize(byte[] bytes);
    }
}