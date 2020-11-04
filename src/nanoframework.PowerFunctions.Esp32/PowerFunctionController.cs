using nanoframework.TechnicHub.Esp32.Enums;
using nanoframework.TechnicHub.Esp32.Interfaces;
using nanoFramework.Hardware.Esp32.Rmt;
using System;

namespace nanoframework.TechnicHub.Esp32
{
    public class PowerFunctionController : IPowerFunctionController
    {
        public const int PF_COMBO_DIRECT_MODE = 0x01;
        public const int PF_SINGLE_PIN_CONTINUOUS = 0x2;
        public const int PF_SINGLE_PIN_TIMEOUT = 0x3;
        public const int PF_SINGLE_OUTPUT = 0x4;
        public const int PF_SINGLE_EXT = 0x6;
        public const int PF_ESCAPE = 0x4;

        private TransmitterChannel transmitterChannel;
        private int channel;
        private int pin;
        private byte nib1;
        private byte nib2;
        private byte nib3;
        private byte toggle;
        public double PF_START_STOP = 20;
        ////#define PF_START_STOP PF_IR_CYCLES(39)
        public double PF_HIGH_PAUSE = 21;
        ////#define PF_HIGH_PAUSE PF_IR_CYCLES(21)
        public double PF_LOW_PAUSE = 10;
        ////#define PF_LOW_PAUSE PF_IR_CYCLES(10)
        public double PF_HALF_PERIOD = 0.5;
        ////#define PF_HALF_PERIOD PF_IR_CYCLES(0.5)
        public uint PF_MAX_MESSAGE_LENGTH = 522;
        ////#define PF_MAX_MESSAGE_LENGTH PF_IR_CYCLES(522) // 2 * 45 + 16 * 27

        public PowerFunctionController(int pin)
           : this(pin, 0) { }

        public PowerFunctionController(int pin, int channel)
        {
            this.pin = pin;
            this.channel = channel;
            this.toggle = 0;
        }

        // PF_IR_CYCLES(num) (uint16_t)((1.0 / 38000.0) * 1000 * 1000 * num)
        private UInt16 IrCycle(double input)
        {
            return (UInt16)((1.0 / 38000.0) * 1000 * 1000 * input);
        }

        private void Pause(byte count, int channel)
        {
            byte pause = 0;

            if (count == 0)
            {
                pause = (byte)(4 - (channel + 1));
            }
            else if (count < 3)
            { // 1, 2
                pause = 5;
            }
            else
            { // 3, 4, 5
                pause = (byte)(5 + (channel + 1) * 2);
            }

            AddDelayCommand((ushort)(pause * 77));
        }

        private void StartStopBit()
        {
            SendBit();
            AddDelayCommand(PF_START_STOP);
        }

        private void SendBit()
        {
            RmtCommand[] _txPulse = new RmtCommand[6];

            for (var i = 0; i < 6; i++)
            {
                _txPulse[i] = new RmtCommand(IrCycle(PF_HALF_PERIOD), true, IrCycle(PF_HALF_PERIOD), false);
                transmitterChannel.AddCommand(_txPulse[i]);
            }
        }

        private void Send(int channel)
        {
            using (transmitterChannel = new TransmitterChannel(pin))
            {
                transmitterChannel.ClockDivider = 80;
                transmitterChannel.CarrierEnabled = false;
                transmitterChannel.IdleLevel = false;

                byte i;
                byte j;
                ushort message = (ushort)(nib1 << 12 | nib2 << 8 | nib3 << 4 | PfChecksum());

                for (i = 0; i < 6; i++)
                {
                    Pause(i, channel);
                    StartStopBit();
                    for (j = 0; j < 16; j++)
                    {
                        SendBit();
                        AddDelayCommand(((0x8000 & (message << j)) != 0 ? PF_HIGH_PAUSE : PF_LOW_PAUSE));
                    }

                    StartStopBit();
                }

                transmitterChannel.Send(true);
            }
        }

        private void AddDelayCommand(double delay)
        {
            var delayCommand = new RmtCommand(IrCycle(delay), true, 0, false);
            transmitterChannel.AddCommand(delayCommand);
        }

        #region To implement
        public void ComboPwm(PowerFunctionsPwm bluePwm, PowerFunctionsPwm redPwm)
        {
            throw new System.NotImplementedException();
        }

        public void ComboPwm(PowerFunctionsPwm bluePwm, PowerFunctionsPwm redPwm, byte channel)
        {
            throw new System.NotImplementedException();
        }

        public void SingleDecrement(PowerFunctionsPort port)
        {
            throw new System.NotImplementedException();
        }

        public void SingleDecrement(PowerFunctionsPort port, byte channel)
        {
            throw new System.NotImplementedException();
        }

        public void SingleIncrement(PowerFunctionsPort port)
        {
            throw new System.NotImplementedException();
        }

        public void SingleIncrement(PowerFunctionsPort port, byte channel)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        public void SinglePwm(PowerFunctionsPort port, PowerFunctionsPwm pwm)
        {
            SinglePwm(port, pwm, channel);
        }

        public void SinglePwm(PowerFunctionsPort port, PowerFunctionsPwm pwm, int channel)
        {
            nib1 = (byte)(toggle | channel);
            nib2 = (byte)((byte)PF_SINGLE_OUTPUT | (byte)port);
            nib3 = (byte)pwm;
            Send(channel);
            Toggle();
        }

        public PowerFunctionsPwm SpeedToPwm(int speed)
        {
            byte pwm;
            if (speed == 0)
            {
                pwm = 0x08;
            }
            else if (speed <= 100)
            {
                pwm = (byte)((speed >> 4) + 1);
            }
            else
            {
                pwm = (byte)(speed >> 4);
            }

            return (PowerFunctionsPwm)pwm;
        }

        private void Toggle()
        {
            toggle ^= 0x8;
        }

        private int PfChecksum()
        {
            return (0xf ^ nib1 ^ nib2 ^ nib3);
        }
    }
}
