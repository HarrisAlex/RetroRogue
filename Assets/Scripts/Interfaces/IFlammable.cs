public interface IFlammable
{
    bool OnFire { get; set; }

    public void Ignite();
    public void Extinguish();
}
