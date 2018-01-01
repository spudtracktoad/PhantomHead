using System;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;

namespace SPIController
{
    static class ad5360_GPIO_Pins
    {

        public const int ad5360_BUSY = 23; // pin 16 connect to pin 12 on eval board
        public const int ad5360_LDAC = 25; // pin 22 connect to pin 8 on eval board
        public const int ad5360_CLR = 24; // pin 18 connect to pin 10 on eval board
        public const int ad5360_RESET = 22; // pin 15 connect to pin 14 on eval board

    }
    class ad5360
    {
        private SpiDevice ad_5360;
        private GpioPin GPIO_LDAC;
        private GpioPin GPIO_Busy;
        private GpioPin GPIO_Clear;
        private GpioPin GPIO_Reset;
        private string errorMsgs;
        private bool _Active = false;

        public bool IsActive
        {
            get
            {
                return _Active;
            }
            set
            {
                _Active = value;
            }
        }

        public ad5360(SpiMode spi_Mode, int clkFreq, int chipSelectLine, string spiDeviceSelection)
        {
            IsActive = InitGPIO();
            if (IsActive)
            {
                this.ad5360_Setup(spi_Mode, clkFreq, chipSelectLine, spiDeviceSelection);
            }
        }

        internal async void ad5360_Setup(SpiMode spi_Mode, int clkFreq, int chipSelectLine, string spiDeviceSelection)
        {
            if (clkFreq > 50000000) //Max write frequency 50 mHz
            {
                errorMsgs = "Clock Speed greater than 50 mHz";
                return;
            }
            try
            {
                var settings = new SpiConnectionSettings(chipSelectLine)
                {
                    ClockFrequency = clkFreq,
                    Mode = spi_Mode,
                };

                string spiAqs = SpiDevice.GetDeviceSelector(spiDeviceSelection);

                var deviceInfo = await DeviceInformation.FindAllAsync(spiAqs);

                ad_5360 = await SpiDevice.FromIdAsync(deviceInfo[0].Id, settings);
                System.Diagnostics.Debug.WriteLine("InitSpi seccessful");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("InitSpi threw " + ex);
            }

        }
        
        public void Write(Byte[] writeBuffer)
        {
            ad_5360.Write(writeBuffer);
        }

        public void pulse_AD5360_Reset()
        {
            setPinStatus(GPIO_Reset, GpioPinValue.Low);
            setPinStatus(GPIO_Reset, GpioPinValue.High);
        }

        public void pulse_AD5360_Clear()
        {
            setPinStatus(GPIO_Clear, GpioPinValue.Low);
            setPinStatus(GPIO_Clear, GpioPinValue.High);
        }

        public void pulse_AD5360_LDAC()
        {
            setPinStatus(GPIO_LDAC, GpioPinValue.Low);
            setPinStatus(GPIO_LDAC, GpioPinValue.High);
        }

        private void setPinStatus(GpioPin pin, GpioPinValue value)
        {
            pin.Write(value);
        }

        private Boolean InitGPIO()

        {
            var gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                return false;
            }

            GpioOpenStatus status = new GpioOpenStatus();

            var open = gpio.TryOpenPin(ad5360_GPIO_Pins.ad5360_LDAC, GpioSharingMode.Exclusive, out GPIO_LDAC, out status);
            if(open == true)
            {
                GPIO_LDAC.Write(GpioPinValue.Low);
                GPIO_LDAC.SetDriveMode(GpioPinDriveMode.Output);
            }

            open = gpio.TryOpenPin(ad5360_GPIO_Pins.ad5360_BUSY, GpioSharingMode.Exclusive, out GPIO_Busy, out status);
            if (open == true)
            {
                GPIO_Busy.SetDriveMode(GpioPinDriveMode.Input);
                GpioPinValue tmp = GPIO_Busy.Read();
                if (tmp == GpioPinValue.Low)
                {
                    System.Diagnostics.Debug.WriteLine("BUSY LOW ");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("BUSY HIGH ");
                }
            }

            open = gpio.TryOpenPin(ad5360_GPIO_Pins.ad5360_RESET, GpioSharingMode.Exclusive, out GPIO_Reset, out status);
            if (open == true)
            {
                GPIO_Reset.Write(GpioPinValue.High);
                GPIO_Reset.SetDriveMode(GpioPinDriveMode.Output);
            }

            open = gpio.TryOpenPin(ad5360_GPIO_Pins.ad5360_CLR, GpioSharingMode.Exclusive, out GPIO_Clear, out status);
            if (open == true)
            {
                GPIO_Clear.Write(GpioPinValue.High);
                GPIO_Clear.SetDriveMode(GpioPinDriveMode.Output);
            }

            return true;
        }
    }
}
