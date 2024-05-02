namespace WarnoModeAutomation.Extensions
{
    public static class MathExtensions
    {
        public static int RoundOff(this int i)
        {
            return ((int)Math.Round(i / 10.0)) * 10;
        }

        public static float GetNumberPercentage(int percent, float fromNumber)
        {
            return (float)percent / 100 * fromNumber;
        }

        public static float GetPercentageAfromB(float a, float b)
        {
            return a / b * 100;
        }

        public static float ConverToWarnoDistance(float value, int warnoMetters)
        {
            return value / 1000 * warnoMetters;
        }
    }
}