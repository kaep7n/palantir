namespace Palantir.Homatic.Mock;

public static class Randomizer
{
    public static VeapMessage VeapMessage(Parameter parameter)
    {
        var value = RandomizeParameterValue(parameter);

        object? minValue = parameter.Type switch
        {
            "ACTION" => parameter.Minimum.GetBoolean(),
            "BOOL" => parameter.Minimum.GetBoolean(),
            "ENUM" => parameter.Minimum.GetInt32(),
            "FLOAT" => parameter.Minimum.GetDouble(),
            "INTEGER" => parameter.Minimum.GetInt32(),
            "STRING" => parameter.Minimum.GetString(),
            _ => throw new InvalidOperationException($"Unexpected type {parameter.Type}.")
        };

        object? maxValue = parameter.Type switch
        {
            "ACTION" => parameter.Maximum.GetBoolean(),
            "BOOL" => parameter.Maximum.GetBoolean(),
            "ENUM" => parameter.Maximum.GetInt32(),
            "FLOAT" => parameter.Maximum.GetDouble(),
            "INTEGER" => parameter.Maximum.GetInt32(),
            "STRING" => parameter.Maximum.GetString(),
            _ => throw new InvalidOperationException($"Unexpected type {parameter.Type}.")
        };

        if (value is IComparable cValue)
        {
            if (cValue.CompareTo(minValue) < 0 || cValue.CompareTo(maxValue) > 0)
                throw new InvalidOperationException($"Value {value} is not between {minValue} and {maxValue}");
        }

        return new VeapMessage(DateTimeOffset.Now.ToUnixTimeMilliseconds(), value, 0);
    }

    public static object RandomizeParameterValue(Parameter parameter)
    {
        return parameter.Identifier switch
        {
            "ACTUAL_HUMIDITY" => ActualHumidity(),
            "ACTUAL_TEMPERATURE" => ActualTemperature(),
            "AES_KEY" => AesKey(),
            "AUTO_MODE" => AutoMode(),
            "BATTERY_STATE" => BatteryState(),
            "BOOST_MODE" => BoostMode(),
            "BOOST_STATE" => BoostState(),
            "COMFORT_MODE" => ComfortMode(),
            "COMMUNICATION_REPORTING" => CommunicationReporting(),
            "CONFIG_PENDING" => ConfigPending(),
            "CONTROL_MODE" => ControlMode(),
            "DECISION_VALUE" => DecisionValue(),
            "DEVICE_IN_BOOTLOADER" => DeviceInBootloader(),
            "DIRECTION" => Direction(),
            "DUTYCYCLE" => Dutycycle(),
            "ERROR" => Error(),
            "ERROR_OVERHEAT" => ErrorOverheat(),
            "ERROR_OVERLOAD" => ErrorOverload(),
            "ERROR_REDUCED" => ErrorReduced(),
            "HUMIDITY" => Humidity(),
            "INHIBIT" => Inhibit(),
            "INSTALL_MODE" => InstallMode(),
            "INSTALL_TEST" => InstallTest(),
            "LEVEL" => Level(),
            "LEVEL_REAL" => LevelReal(),
            "LOWBAT" => LowBat(),
            "LOWBAT_REPORTING" => LowBatReporting(),
            "LOWERING_MODE" => LoweringMode(),
            "MANU_MODE" => ManuMode(),
            "OLD_LEVEL" => OldLevel(),
            "ON_TIME" => OnTime(),
            "PARTY_MODE_SUBMIT" => PartyModeSubmit(),
            "PARTY_START_DAY" => PartyStartDay(),
            "PARTY_START_MONTH" => PartyStartMonth(),
            "PARTY_START_TIME" => PartyStartTime(),
            "PARTY_START_YEAR" => PartyStartYear(),
            "PARTY_STOP_DAY" => PartyStopDay(),
            "PARTY_STOP_MONTH" => PartyStopMonth(),
            "PARTY_STOP_TIME" => PartyStopTime(),
            "PARTY_STOP_YEAR" => PartyStopYear(),
            "PARTY_TEMPERATURE" => PartyTemperature(),
            "PRESS_LONG" => PressLong(),
            "PRESS_SHORT" => PressShort(),
            "RAMP_STOP" => RampStop(),
            "RAMP_TIME" => RampTime(),
            "RSSI_DEVICE" => RssiDevice(),
            "RSSI_PEER" => RssiPeer(),
            "SET_TEMPERATURE" => SetTemperature(),
            "STATE" => State(),
            "STICKY_UNREACH" => StickyUnreach(),
            "STOP" => Stop(),
            "TEMPERATURE" => Temperature(),
            "UNREACH" => Unreach(),
            "UPDATE_PENDING" => UpdatePending(),
            "WINDOW_OPEN_REPORTING" => WindowOpenReporting(),
            "WORKING" => Working(),
            _ => throw new InvalidOperationException($"Unexpected parameter identifier '{parameter.Identifier}'.")
        };
    }

    public static double ActualTemperature() => Double(19.0, 30.0);

    public static double ActualHumidity() => Double(60.0, 90.0);

    internal static bool AutoMode() => Boolean();

    internal static int AesKey() => Integer(0, 127);

    public static double BatteryState() => Double(1.5, 4.6);

    public static bool BoostMode() => Boolean();

    public static int BoostState() => Integer(0, 30);

    public static bool ComfortMode() => Boolean();

    public static bool CommunicationReporting() => Boolean();

    public static bool ConfigPending() => Boolean();

    public static int ControlMode() => Integer(0, 3);

    public static int DecisionValue() => Integer(0, 255);

    public static bool DeviceInBootloader() => Boolean();

    public static int Direction() => Integer(0, 3);

    public static bool Dutycycle() => Boolean();

    public static int Error() => Integer(0, 7);

    public static bool ErrorOverheat() => Boolean();

    public static bool ErrorOverload() => Boolean();

    public static bool ErrorReduced() => Boolean();

    public static double Humidity() => Double(60.0, 90.0);

    public static bool Inhibit() => Boolean();

    public static bool InstallMode() => Boolean();

    public static bool InstallTest() => Boolean();

    public static double Level() => Double(0.0, 1.0);

    public static double LevelReal() => Double(0.0, 1.0);

    public static bool LowBat() => Boolean();

    public static bool LowBatReporting() => Boolean();

    public static bool LoweringMode() => Boolean();

    public static double ManuMode() => Double(4.5, 30.5);

    public static bool OldLevel() => Boolean();

    public static double OnTime() => Double(0.0, 85825945.6);

    public static string PartyModeSubmit() => string.Empty;

    public static int PartyStartDay() => Integer(1, 31);

    public static int PartyStartMonth() => Integer(1, 12);

    public static int PartyStartTime() => Integer(0, 1410);

    public static int PartyStartYear() => Integer(0, 99);

    public static int PartyStopDay() => Integer(1, 31);

    public static int PartyStopMonth() => Integer(1, 12);

    public static int PartyStopTime() => Integer(0, 1410);

    public static int PartyStopYear() => Integer(0, 99);

    public static double PartyTemperature() => Double(5.0, 30.0);

    public static bool PressLong() => Boolean();

    public static bool PressShort() => Boolean();

    public static bool RampStop() => Boolean();

    public static double RampTime() => Double(0.0, 85825945.6);

    public static int RssiDevice() => Integer(int.MinValue, int.MaxValue);

    public static int RssiPeer() => Integer(int.MinValue, int.MaxValue);

    public static double SetTemperature() => Double(20.0, 24.0);

    public static bool State() => Boolean();

    public static bool StickyUnreach() => Boolean();

    public static bool Stop() => Boolean();

    public static double Temperature() => Double(19.0, 30.0);

    public static bool Unreach() => Boolean();

    public static bool UpdatePending() => Boolean();

    public static bool WindowOpenReporting() => Boolean();

    public static bool Working() => Boolean();

    public static double Double(double minimum, double maximum)
        => Math.Round((Random.Shared.NextDouble() * (maximum - minimum)) + minimum, 2);

    public static bool Boolean() => Random.Shared.NextDouble() > 0.5;

    public static int Integer(int minimum, int maximum)
        => Random.Shared.Next(minimum, maximum);


}
