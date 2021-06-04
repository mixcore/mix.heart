namespace Mix.Heart.Entity
{
    public interface IHasPrimaryKey<TPrimaryKey>
    {
        public TPrimaryKey Id { get; set; }
    }
}
