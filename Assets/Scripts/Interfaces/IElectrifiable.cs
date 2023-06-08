public interface IElectrifiable
{
    bool Electrified { get; set; }

    public void Electrify();
    public void Deelectrify();
}
