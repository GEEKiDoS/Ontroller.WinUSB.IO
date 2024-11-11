using Ontroller.WinUSB.IO;

using var ontroller = new Connection();
ontroller.TryConnect();

byte[] colors = 
[
    127, 127, 0,
    0, 127, 127,
    127, 0, 127,

    255, 0, 0,
    0, 255, 0,
    0, 0, 255,
];

byte[] sideColors =
[
    // left
    255, 255, 0,
    0, 255, 255,
    // right
    255, 0, 0,
    255, 255, 255,
];

while(ontroller.Connected)
{
    ontroller.SetIO4Leds(colors);
    ontroller.SetSideLeds(sideColors);
    ontroller.Write();

    Console.CursorLeft = 0;
    Console.CursorTop = 0;
    Console.WriteLine("Left: {0}                         ", ontroller.Left);
    Console.WriteLine("Right: {0}                        ", ontroller.Right);
    Console.WriteLine("Operation: {0}                    ", ontroller.Operation);
    Console.WriteLine("Lever: {0}                        ", ontroller.Lever);
}
