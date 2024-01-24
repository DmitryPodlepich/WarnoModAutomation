namespace WarnoModeAutomation.Extensions
{
    public static class MathExtensions
    {
        public static int RoundOff(this int i)
        {
            return ((int)Math.Round(i / 10.0)) * 10;
        }
    }
}