namespace Mix.Heart.Entities
{
public interface IHasPrimaryKey<TPrimaryKey>
{
    public TPrimaryKey Id {
        get;
        set;
    }
}
}
