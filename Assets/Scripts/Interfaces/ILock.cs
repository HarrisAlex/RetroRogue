public interface ILock
{
    int LockDifficulty { get; set; }
    bool Locked { get; set; }

    public void PickLock();
    public void Unlock();
    public void Lock();
}
