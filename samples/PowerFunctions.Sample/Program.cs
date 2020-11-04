using nanoframework.TechnicHub.Esp32;
using nanoframework.TechnicHub.Esp32.Enums;
using System.Diagnostics;
using System.Threading;

namespace PowerFunctions.Sample
{
    public class Program
    {
        public static void Main()
        {
            Debug.WriteLine("Sample for PowerFunction.");

            var pfc = new PowerFunctionController(12);
            var speed = pfc.SpeedToPwm(100);
            pfc.SinglePwm(PowerFunctionsPort.RED, speed);

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
