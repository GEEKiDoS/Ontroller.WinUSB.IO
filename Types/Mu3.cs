namespace Ontroller.WinUSB.IO.Types
{
    [Flags]
    public enum OperationButton : byte
    {
        None = 0,
        Test = 0x01,
        Service = 0x02,
        Coin = 0x04,
    };

    [Flags]
    public enum GameButton : byte
    {
        None = 0,
        A = 0x01,
        B = 0x02,
        C = 0x04,
        Side = 0x08,
        Menu = 0x10,
    };
}
