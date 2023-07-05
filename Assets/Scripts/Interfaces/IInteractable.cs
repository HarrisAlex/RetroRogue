public interface IInteractable
{
    bool CurrentlyInteractable { get; set; }

    public void Interact();
}