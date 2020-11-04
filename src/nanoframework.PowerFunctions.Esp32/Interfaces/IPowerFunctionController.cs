using nanoframework.TechnicHub.Esp32.Enums;

namespace nanoframework.TechnicHub.Esp32.Interfaces
{
    public interface IPowerFunctionController
    {
        PowerFunctionsPwm SpeedToPwm(int speed);

        void SinglePwm(PowerFunctionsPort port, PowerFunctionsPwm pwm);
        void SinglePwm(PowerFunctionsPort port, PowerFunctionsPwm pwm, int channel);

        void SingleIncrement(PowerFunctionsPort port);
        void SingleIncrement(PowerFunctionsPort port, byte channel);

        void SingleDecrement(PowerFunctionsPort port);
        void SingleDecrement(PowerFunctionsPort port, byte channel);

        void ComboPwm(PowerFunctionsPwm bluePwm, PowerFunctionsPwm redPwm);
        void ComboPwm(PowerFunctionsPwm bluePwm, PowerFunctionsPwm redPwm, byte channel);
    }
}
