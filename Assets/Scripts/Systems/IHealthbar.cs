public interface IHealthbar
{
    int Health { get; set; }

    public void Damage();
    public void Heal();
}
