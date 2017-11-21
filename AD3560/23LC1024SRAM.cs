using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;

namespace SPIController
{


    static class SRAM_GPIO_Pins
    {

        //public const int ad5360_BUSY = 23; // pin 16 connect to pin 12 on eval board
        //public const int ad5360_LDAC = 25; // pin 22 connect to pin 8 on eval board
        //public const int ad5360_CLR = 24; // pin 18 connect to pin 10 on eval board
        //public const int ad5360_RESET = 22; // pin 15 connect to pin 14 on eval board

    }

    public class SRAM_23LC1024
    {
        #region PRIVATE Members

        int cs_Pin;
        int spi_Speed;
        SpiDevice _23LC1024_Sram;
        private string errorMsgs;
        String Sram_Name;
        private GpioPin GPIO_CS;
        private bool _setupSuccess = false;
        private SpiMode _mode;
        DeviceInformationCollection _deviceInfo;
        DeviceInformation _Info;
        #endregion

        #region PUBLIC Members

        public bool setupSuccess
        {
            get { return _setupSuccess; }
            set { _setupSuccess = value; }
        }

        #endregion

        #region Constructor

        public SRAM_23LC1024( SpiMode mode, int clkFreq, int ChipSelectLine, string spiDeviceSelection)
        {
            cs_Pin = ChipSelectLine;
            spi_Speed = clkFreq;
            Sram_Name = spiDeviceSelection;
            _mode = mode;
            _23LC1024_Setup();

            var gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                return;
            }

            GPIO_CS = gpio.OpenPin(25);
            GPIO_CS.Write(GpioPinValue.High);
            GPIO_CS.SetDriveMode(GpioPinDriveMode.Output);
            
        }

        #endregion

        #region Private Methods

        internal async void _23LC1024_Setup()
        {
            try
            {
                String spiDeviceSelector = SpiDevice.GetDeviceSelector(Sram_Name);

                var devices = await DeviceInformation.FindAllAsync(spiDeviceSelector);

                var deviceSettings = new SpiConnectionSettings(cs_Pin)
                {
                    ClockFrequency = spi_Speed,
                    Mode = _mode,
                };

                _23LC1024_Sram = await SpiDevice.FromIdAsync(devices[0].Id, deviceSettings);

                if (_23LC1024_Sram == null)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Could not get device {0}, with settings {1}", devices[0].Id, deviceSettings));
                }
                setupSuccess = true;
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        #endregion

        #region Public Methods

        public Byte readByte(UInt32 address, ref byte[] data)
        {
            byte[] cmd = new byte[4];
            byte[] result = new byte[1];
            cmd[0] = SRAMConsts.READ;
            cmd[1] = (byte)(address >> 16);
            cmd[2] = (byte)(address >> 8);
            cmd[3] = (byte)(address);
            GPIO_CS.Write(GpioPinValue.Low);
            _23LC1024_Sram.TransferSequential(cmd, result);
            GPIO_CS.Write(GpioPinValue.High);
            data = result;
            return result[0];
        }

        public void writeByte(UInt32 address, byte data)
        {
            byte[] cmd = new byte[5];
            cmd[0] = SRAMConsts.WRITE;
            cmd[1] = (byte)(address >> 16);
            cmd[2] = (byte)(address >> 8);
            cmd[3] = (byte)(address);
            cmd[4] = data;
            GPIO_CS.Write(GpioPinValue.Low);
            _23LC1024_Sram.Write(cmd);
            GPIO_CS.Write(GpioPinValue.High);
        }

        public void readPage(UInt32 address, ref byte[] data)
        {
            byte[] cmd = new byte[4];
            byte[] result = new byte[32];
            cmd[0] = SRAMConsts.READ;
            cmd[1] = (byte)(address >> 16);
            cmd[2] = (byte)(address >> 8);
            cmd[3] = (byte)(address);
            GPIO_CS.Write(GpioPinValue.Low);
            _23LC1024_Sram.TransferSequential(cmd, result);
            //_23LC1024_Sram.Write(cmd);
            //_23LC1024_Sram.Read(result);
            GPIO_CS.Write(GpioPinValue.High);
            data = result;
        }

        public void writePage(UInt32 address, ref byte[] data)
        {
            byte[] read = new byte[1];
            data[0] = SRAMConsts.WRITE;
            data[1] = (byte)(address >> 16);
            data[2] = (byte)(address >> 8);
            data[3] = (byte)(address);
            GPIO_CS.Write(GpioPinValue.Low);
            _23LC1024_Sram.TransferSequential(data, read);
            //_23LC1024_Sram.Write(cmd);
            //_23LC1024_Sram.Write(data);
            GPIO_CS.Write(GpioPinValue.High);
        }

        public void readBuffer(UInt32 address, ref byte[] data, int length)
        {
            byte[] cmd = new byte[4];
            byte[] result = new byte[length];
            cmd[0] = SRAMConsts.READ;
            cmd[1] = (byte)(address >> 16);
            cmd[2] = (byte)(address >> 8);
            cmd[3] = (byte)(address);
            GPIO_CS.Write(GpioPinValue.Low);
            _23LC1024_Sram.TransferSequential(cmd, result);
            GPIO_CS.Write(GpioPinValue.High);
            data = result;
        }

        public void writeBuffer(UInt32 address, ref byte[] data, int length)
        {
            byte[] cmd = new byte[4];
            cmd[0] = SRAMConsts.WRITE;
            cmd[1] = (byte)(address >> 16);
            cmd[2] = (byte)(address >> 8);
            cmd[3] = (byte)(address);
            GPIO_CS.Write(GpioPinValue.Low);
            //_23LC1024_Sram.TransferSequential(cmd, data);
            _23LC1024_Sram.Write(cmd);
            _23LC1024_Sram.Write(data);
            GPIO_CS.Write(GpioPinValue.High);
        }

        public byte readRegisterMode()
        {
            Byte[] modeData = new byte[1];
            Byte[] Command = { SRAMConsts.RDMR };
            GPIO_CS.Write(GpioPinValue.Low);
            _23LC1024_Sram.TransferSequential(Command, modeData);
            GPIO_CS.Write(GpioPinValue.High);
            return modeData[0];
        }

        public void writeRegisterMode(byte mode)
        { 
            Byte[] Command = { SRAMConsts.WRMR, mode };
            GPIO_CS.Write(GpioPinValue.Low);
            _23LC1024_Sram.Write(Command);
            GPIO_CS.Write(GpioPinValue.High);
        }

        #endregion
    }
}
