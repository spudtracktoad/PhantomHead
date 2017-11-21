using SPIController;
using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.Spi;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.IO;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.System;
using Windows.Media.Devices.Core;

namespace PhantomHead
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public Boolean LEDon = false;
        private const int LED_PIN = 5;
        private string _SampleFile;

        public string SampleFile
        {
            get { return _SampleFile; }
            set { _SampleFile = value; }
        }

        private GpioPin pin;
        SpiDevice ADC;
        // 0000 0001 1000 0000 0000
        // 0000 0001 1001 0000 0000
        // 0000 0001 1010 0000 0000
        byte[] restQuery = new byte[3] { 0x01, 0x80, 0x00 };
        byte[] clearQuery = new byte[3] { 0x01, 0x90, 0x00 };
        byte[] ldacQuery = new byte[3] { 0x01, 0xA0, 0x00 };
        byte[] RestBuffer = new byte[3];
        byte[] ClearBuffer = new byte[3];
        byte[] LDACBuffer = new byte[3];

        AD5360_Controller ad5360;
        Sram_Controller lc1024;
        
        public MainPage()
        {
            this.InitializeComponent();
            ad5360 = new AD5360_Controller(SpiMode.Mode1, (500 * 1000), 0, "SPI0");
            //lc1024 = new Sram_Controller(SpiMode.Mode0, 1000000);
            //if (ad5360.active == true)
            //{
            //    InitGPIO();
            //    InitSPI();
            //    InitTimer();
            //}
        }

        private void MainPage_Unloaded(object sender, object args)

        {
            pin.Dispose();
        }

        private void Click_Reset(object sender, RoutedEventArgs e)
        {
            ad5360.pulse_AD5360_Reset();
        }

        private void Click_Clear(object sender, RoutedEventArgs e)
        {
            ad5360.pulse_AD5360_Clear();
        }

        private void Click_LDAC(object sender, RoutedEventArgs e)
        {
            ad5360.pulse_AD5360_LDAC();
        }

        private void PinStatusUpdate()
        {
            RestStatusUpdate();
            LdacStatusUpdate();
            ClearStatusUpdate();
        }

        private void RestStatusUpdate()
        {
            int tmp = (RestBuffer[0] << 16) + (RestBuffer[1] << 8) + RestBuffer[2];
            double volts = ((tmp * 3.3) / 1024);
            //if (volts > 2.8)
            //{
            //    ad5360_Rest_Light.Content = "HIGH";
            //}
            //else if (volts < 1.0)
            //{
            //    ad5360_Rest_Light.Content = "LOW";
            //}
            //else
            //{
            //    ad5360_Rest_Light.Content = "UNK";
            //}

        }
        private void LdacStatusUpdate()
        {
            int tmp = (LDACBuffer[0] << 16) + (LDACBuffer[1] << 8) + LDACBuffer[2];
            double volts = ((tmp * 3.3) / 1024);
            //if (volts > 2.8)
            //{
            //    ad5360_LDAC_Light.Content = "HIGH";
            //}
            //else if (volts < 1.0)
            //{
            //    ad5360_LDAC_Light.Content = "LOW";
            //}
            //else
            //{
            //    ad5360_LDAC_Light.Content = "UNK";
            //}
        }
        private void ClearStatusUpdate()
        {
            int tmp = (ClearBuffer[0] << 16) + (ClearBuffer[1] << 8) + ClearBuffer[2];
            double volts = ((tmp * 3.3) / 1024);
            //if (volts > 2.8)
            //{
            //    ad5360_Clr_Light.Content = "HIGH";
            //}
            //else if (volts < 1.0)
            //{
            //    ad5360_Clr_Light.Content = "LOW";
            //}
            //else
            //{
            //    ad5360_Clr_Light.Content = "UNK";
            //}
        }

        private void Click_write(object sender, RoutedEventArgs e)
        {
            //var channel = 0;//Convert.ToInt32(ad5360_Channel.Text.ToString());
            var volts = 5;// Convert.ToSingle(ad5360_OutVoltage.Text.ToString());
            _TimeSlice timeVolt = new _TimeSlice();
            for (int index = 0; index < 16; index++)
            {
                var buff = new _writeBuffer(index, volts);
                timeVolt.timeSlice.Add(buff);
            }
            ad5360.add_WrtieFrame(timeVolt);
        }

        //private void Click_Read(object sender, RoutedEventArgs e)
        //{
        //    ad5360.ad5360_Read(1, (float)5.5);
        //}

        private void click_VoltUp(object sender, RoutedEventArgs e)
        {
            //var value = Convert.ToDouble(ad5360_OutVoltage.Text.ToString());
            //value += .01;
            //if (value > 4.99)
            //    value = 4.99;
            //ad5360_OutVoltage.Text = value.ToString();
        }

        private void click_VoltDown(object sender, RoutedEventArgs e)
        {
            //var value = Convert.ToDouble(ad5360_OutVoltage.Text.ToString());
            //value -= .01;
            //if (value < -4.99)
            //    value = -4.99;
            //ad5360_OutVoltage.Text = value.ToString();
        }

        private void ad5360_channelup_Click(object sender, RoutedEventArgs e)
        {
            //var value = Convert.ToDouble(ad5360_Channel.Text.ToString());
            //value += 1;
            //if (value > 15)
            //    value = 15;
            //ad5360_Channel.Text = value.ToString();

        }

        private void ad5360_channeldown_Click(object sender, RoutedEventArgs e)
        {
            //var value = Convert.ToDouble(ad5360_Channel.Text.ToString());
            //value -= 1;
            //if (value < 0)
            //    value = 0;
            //ad5360_Channel.Text = value.ToString();

        }

        private void ad5360_Write_Sine_Click(object sender, RoutedEventArgs e)
        {
            double value = -5;
            Random rnd1 = new Random();
            for (int slice = 0; slice < 3200; slice++)
            {
                _TimeSlice timeVolt = new _TimeSlice();
                for (int index = 0; index < 16; index++)
                {
                    value = rnd1.NextDouble();
                    value = (value - .5) * 10;
                    var buff = new _writeBuffer(index, (float)(value));//(float)Math.Sin(tmp));
                    timeVolt.timeSlice.Add(buff);
                }
                ad5360.add_WrtieFrame(timeVolt);
            }
        }

        private void btnShutodown_Click(object sender, RoutedEventArgs e)
        {
            //shutdown the device
            ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromSeconds(2));

        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            //var picker = new Windows.Storage.Pickers.FileOpenPicker();
            //picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            //picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            //picker.FileTypeFilter.Add(".jpg");
            //picker.FileTypeFilter.Add(".jpeg");
            //picker.FileTypeFilter.Add(".png");

            //var file = await picker.PickSingleFileAsync();
            //if (file != null)
            //{
            //    // Application now has read/write access to the picked file
            //    this.txtFileName.Text = "Picked photo: " + file.Name;
            //}
            //else
            //{
            //    this.txtFileName.Text = "Operation cancelled.";
            //}
        }

        private async void btnSquareWave_Click(object sender, RoutedEventArgs e)
        {
            int count = 1000;
            for(int index = 0; index < count; index++)
            {
                _TimeSlice timeslice = new _TimeSlice();
                if (index % 2 == 0)
                {
                    for (int channel = 0; channel < 15; channel++)
                    {
                        try
                        {
                            var value = 1;
                            _writeBuffer buff = new _writeBuffer(channel, value);
                            timeslice.timeSlice.Add(buff);
                        }
                        catch (Exception ex)
                        {
                            var error = ex.Message.ToString();
                        }
                    }
                }
                else
                {
                    for (int channel = 0; channel < 15; channel++)
                    {
                        try
                        {
                            var value = 0;
                            _writeBuffer buff = new _writeBuffer(channel, value);
                            timeslice.timeSlice.Add(buff);
                        }
                        catch (Exception ex)
                        {
                            var error = ex.Message.ToString();
                        }
                    }
                }
                ad5360.add_WrtieFrame(timeslice);

            }
     
        }

        private async void btnReadFile_Click(object sender, RoutedEventArgs e)
        {
            //try to use the physical application location or the relative file location from the 
            //location where the application is running
            var tmp = System.IO.Directory.GetCurrentDirectory();
            tmp += "\\DataFile\\Base_EEG_Data_Play.txt";
            if(File.Exists(tmp))
            {
                using (StreamReader sr = File.OpenText(tmp))
                {
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine();
                        string[] values = line.Split(',');
                        var floats = new List<double>();
                        _TimeSlice timeslice = new _TimeSlice();
                        for (int channel = 0; channel < 15; channel++)
                        {
                            try
                            {
                                var value = ((float)Convert.ToDouble(values[channel])/10);
                                _writeBuffer buff = new _writeBuffer(channel, value);
                                timeslice.timeSlice.Add(buff);
                            }
                            catch(Exception ex)
                            {
                                var error = ex.Message.ToString();
                            }
                        }
                        ad5360.add_WrtieFrame(timeslice);
                    }
                }
            }

        }

        private void btnRestart_Click(object sender, RoutedEventArgs e)
        {
            //Restart Device
            ShutdownManager.BeginShutdown(ShutdownKind.Restart, TimeSpan.FromSeconds(3));
        }

        private async void btnRndFile_Click(object sender, RoutedEventArgs e)
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            double output;
            StorageFolder storagefold = KnownFolders.MusicLibrary;
            StorageFile randomFile = await storagefold.CreateFileAsync("EEGRnd.txt", CreationCollisionOption.ReplaceExisting);
            var Stream = await randomFile.OpenAsync(FileAccessMode.ReadWrite);
            using (var outEEGStream = Stream.GetOutputStreamAt(0))
            {
                using (var outEEG = new Windows.Storage.Streams.DataWriter(outEEGStream))
                {

                    // create file
                    int count = 0;
                    int step = 0;
                    for (int index = 0; index < 512; index++)
                    {
                        count++;
                        for (int channel = 0; channel <= 15; channel++)
                        {
                            output = rnd.Next(-10, 9);
                            output += rnd.NextDouble();
                            if (count > 15)
                            {
                                count = 0;
                                step++;
                            }
                            if (step == 20)
                                step = 0;
                            if (channel < 15)
                                outEEG.WriteString(string.Format("{0:0.0000}, ", output));
                            else
                                outEEG.WriteString(string.Format("{0:0.0000}", output));

                        }
                    }
                    await outEEG.StoreAsync();
                    await outEEGStream.FlushAsync();
                }
            }
            Stream.Dispose();
        }

        private void SramWriteByteMode_Click(object sender, RoutedEventArgs e)
        {
            lc1024.SramOneWriteByteMode();
             txtSramMode.Text = String.Format("#{0:x}", lc1024.SramOneReadRegistermode());
        }

        private void SramWritePageMode_Click(object sender, RoutedEventArgs e)
        {
            lc1024.SramOneWritePageMode();
            txtSramMode.Text = String.Format("#{0:x}", lc1024.SramOneReadRegistermode());

        }

        private void SramWriteSequentialMode_Click(object sender, RoutedEventArgs e)
        {
            lc1024.SramOneWriteSequentialMode();
            txtSramMode.Text = String.Format("#{0:x}", lc1024.SramOneReadRegistermode());

        }

        private void SramReadModeRegister_Click(object sender, RoutedEventArgs e)
        {
            //txtSramMode.Text = String.Format("#{0:x}", 128);
            byte data = lc1024.SramOneReadRegistermode();
            txtSramMode.Text = String.Format("#{0:x}", data);
        }

        private void SramWriteByte_Click(object sender, RoutedEventArgs e)
        {
            Byte[] data = new byte[1];
            data[0] = Convert.ToByte(txt_SramWriteData.Text);
            uint address = Convert.ToUInt32(txt_SramAddress.Text);
            lc1024.SramOneWriteByteData(address, data[0]);
        }

        private void SramReadByte_Click(object sender, RoutedEventArgs e)
        {
            Byte[] data = new byte[1];
            Byte[] result = new Byte[1];
            uint address =Convert.ToUInt32(txt_SramAddress.Text);
            data[0] = lc1024.SramOneReadByteData(address, ref result);
            txt_SramReadData.Text = data[0].ToString();
        }
        //Test Write Page
        private void SramWritePage_Click(object sender, RoutedEventArgs e)
        {
            Byte[] data = new byte[36];
            for(int i = 3; i < 36; i++)
            {
                data[i] = 0xAA;
            }
            lc1024.SramOneWritePageData(0, ref data);
        }

        private void SramReadPage_Click(object sender, RoutedEventArgs e)
        {
            Byte[] data = new byte[32];
            lc1024.SramOneReadPageData(0, ref data);
            String outdata = "";
            for(int i = 0; i < 32; i++)
            {
                outdata += string.Format("{0}", data[i]);
            }
            txtBlk_LargeData.Text = outdata;
        }
        //Test Write Page
        private void SramWrite_Click(object sender, RoutedEventArgs e)
        {
            Byte[] data = new byte[32];
            for (int i = 0; i < 32; i++)
            {
                data[i] = 0xAA;
            }
            lc1024.SramOneWriteData(0, ref data, 32);
        }

        private void SramRead_Click(object sender, RoutedEventArgs e)
        {
            Byte[] data = new byte[32];
            lc1024.SramOneReadData(0, ref data, 32);
            String outdata = "";
            for (int i = 0; i < 32; i++)
            {
                outdata += string.Format("{0}", data[i], 32);
            }
            txtBlk_LargeData.Text = outdata;
        }
    }
}
