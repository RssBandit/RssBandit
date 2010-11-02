namespace RssBandit.WinGui.Behaviors
{
    public interface IShellLayoutManager
    {
        void LoadLayout();  // from disk
        void SaveLayout();  // to disk
        void ResetLayout(); // to designer default
    }
}