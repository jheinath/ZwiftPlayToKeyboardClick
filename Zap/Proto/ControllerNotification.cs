using Google.Protobuf;
using ZwiftPlayConsoleApp.BLE;

namespace ZwiftPlayConsoleApp.Zap.Proto;

public class ControllerNotification
{
    private const int BtnPressed = 0;

    private const string ShoulderName = "Shoulder";
    private const string PowerName = "Power";
    private const string SteerName = "Steer/Brake";
    private const string UnknownName = "???";

    private readonly bool _isRightController;

    public bool ButtonYPressed;// or up on left controller
    public bool ButtonZPressed; // or left on left controller
    public bool ButtonAPressed;// or right on left controller
    public bool ButtonBPressed; // or down on left controller

    public bool ShoulderButtonPressed;
    public bool PowerButtonPressed;

    // on the left this will be negative when steering and positive when breaking and vice versa on right
    public int SteerBrakeValue;

    public int SomethingValue;

    public ControllerNotification(byte[] messageBytes)
    {
        var input = new CodedInputStream(messageBytes);
        while (true)
        {
            var tag = input.ReadTag();
            var type = WireFormat.GetTagWireType(tag);
            if (tag == 0 || type == WireFormat.WireType.EndGroup)
            {
                break;
            }

            var number = WireFormat.GetTagFieldNumber(tag);

            switch (type)
            {
                case WireFormat.WireType.Varint:
                    var value = input.ReadInt64();
                    switch (number)
                    {
                        case 1:
                            _isRightController = value == BtnPressed;
                            break;
                        case 2:
                            ButtonYPressed = value == BtnPressed;
                            break;
                        case 3:
                            ButtonZPressed = value == BtnPressed;
                            break;
                        case 4:
                            ButtonAPressed = value == BtnPressed;
                            break;
                        case 5:
                            ButtonBPressed = value == BtnPressed;
                            break;
                        case 6:
                            ShoulderButtonPressed = value == BtnPressed;
                            break;
                        case 7:
                            PowerButtonPressed = value == BtnPressed;
                            break;
                        case 8:
                            SteerBrakeValue = ProtoUtils.GetSignedValue((int)value);
                            break;
                        case 9:
                            SomethingValue = (int)value;
                            break;

                        default:
                            throw new ArgumentException("Unexpected tag");
                    }
                    break;
                default:
                    throw new ArgumentException("Unexpected wire type");
                    break;
            }
        }
    }

    public string Diff(ControllerNotification previousNotification)
    {
        var diff = "";
        diff += Diff(NameY(), ButtonYPressed, previousNotification.ButtonYPressed);
        diff += Diff(NameZ(), ButtonZPressed, previousNotification.ButtonZPressed);
        diff += Diff(NameA(), ButtonAPressed, previousNotification.ButtonAPressed);
        diff += Diff(NameB(), ButtonBPressed, previousNotification.ButtonBPressed);
        diff += Diff(ShoulderName, ShoulderButtonPressed, previousNotification.ShoulderButtonPressed);
        diff += Diff(PowerName, PowerButtonPressed, previousNotification.PowerButtonPressed);
        diff += Diff(SteerName, SteerBrakeValue, previousNotification.SteerBrakeValue);
        diff += Diff(UnknownName, SomethingValue, previousNotification.SomethingValue);
        return diff;
    }

    private string Diff(string title, bool pressedValue, bool oldPressedValue)
    {
        if (pressedValue != oldPressedValue)
            return $"{title}={(pressedValue ? "Pressed" : "Released")} ";
        return "";
    }

    private string Diff(string title, int newValue, int oldValue)
    {
        if (newValue != oldValue)
            return $"{title}={newValue} ";
        return "";
    }

    private string NameController() => _isRightController ? "Right" : "Left";

    private string NameY() => _isRightController? "Y" : "Up";

    private string NameZ() => _isRightController? "Z" : "Left";

    private string NameA() => _isRightController? "A" : "Right";

    private string NameB() => _isRightController ? "B" : "Down";

    public override string ToString()
    {
        var text = "ControllerNotification(";

        text += $"{NameController()} ";

        text += ButtonYPressed ? NameY() : "";
        text += ButtonZPressed ? NameZ() : "";
        text += ButtonAPressed ? NameA() : "";
        text += ButtonBPressed ? NameB() : "";

        text += ShoulderButtonPressed ? ShoulderName : "";
        text += PowerButtonPressed ? PowerName : "";

        text += SteerBrakeValue != 0 ? $"{SteerName}: {SteerBrakeValue}" : "";

        text += SomethingValue != 0 ? $"{UnknownName}: {SomethingValue}" : "";

        text += ")";
        return text;
    }

    public ButtonChange[] DiffChange(ControllerNotification? previousNotification)
    {
        var diffList = new List<ButtonChange>();
        DiffChange(diffList, _isRightController ? ZwiftPlayButton.Y : ZwiftPlayButton.Up, ButtonYPressed, previousNotification?.ButtonYPressed ?? false);
        DiffChange(diffList, _isRightController ? ZwiftPlayButton.Z : ZwiftPlayButton.Left, ButtonZPressed, previousNotification?.ButtonZPressed ?? false);
        DiffChange(diffList, _isRightController ? ZwiftPlayButton.A : ZwiftPlayButton.Right, ButtonAPressed, previousNotification?.ButtonAPressed ?? false);
        DiffChange(diffList, _isRightController ? ZwiftPlayButton.B : ZwiftPlayButton.Down, ButtonBPressed, previousNotification?.ButtonBPressed ?? false);
        DiffChange(diffList, _isRightController ? ZwiftPlayButton.RightShoulder : ZwiftPlayButton.LeftShoulder, ShoulderButtonPressed, previousNotification?.ShoulderButtonPressed ?? false);
        DiffChange(diffList, _isRightController ? ZwiftPlayButton.RightPower: ZwiftPlayButton.LeftPower, PowerButtonPressed, previousNotification?.PowerButtonPressed ?? false);
        //diff += Diff(STEER_NAME, steerBrakeValue, previousNotification.steerBrakeValue);
        //diff += Diff(UNKNOWN_NAME, somethingValue, previousNotification.somethingValue);
        return diffList.ToArray();
    }

    private void DiffChange(List<ButtonChange> changes, ZwiftPlayButton button, bool pressedValue, bool oldPressedValue)
    {
        if (pressedValue != oldPressedValue)
        {
            changes.Add(new ButtonChange { Button = button, IsPressed = pressedValue });
        }
    }

}

public class ProtoUtils
{

    public static int GetSignedValue(int value)
    {
        var negativeBit = value & 0b1;
        var num = value >> 1;
        return negativeBit == 1 ? -num : num;
    }

}