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
using System.Threading;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PhantomHead
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AD5360_TestPanel : Page
    {
        public Boolean LEDon = false;
        private const int LED_PIN = 5;
        private string _SampleFile;
        private Boolean _StopEnabled = false;
        private const int ENABLE_PIN = 18;
        private GpioPin _enable_Timer;
        private Thread PlayBackThread;

        public string SampleFile
        {
            get { return _SampleFile; }
            set { _SampleFile = value; }
        }
        
        byte[] restQuery = new byte[3] { 0x01, 0x80, 0x00 };
        byte[] clearQuery = new byte[3] { 0x01, 0x90, 0x00 };
        byte[] ldacQuery = new byte[3] { 0x01, 0xA0, 0x00 };
        byte[] RestBuffer = new byte[3];
        byte[] ClearBuffer = new byte[3];
        byte[] LDACBuffer = new byte[3];

        AD5360_Controller ad5360;
        Sram_Controller lc1024;

        public AD5360_TestPanel()
        {
            this.InitializeComponent();
            ad5360 = new AD5360_Controller(SpiMode.Mode1, (500 * 1000), 0, "SPI0");
            Init_Enable();
        }

        private void Init_Enable()
        {
            GpioOpenStatus status = new GpioOpenStatus();
            var gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                return;
            }

            var open = gpio.TryOpenPin(ENABLE_PIN, GpioSharingMode.Exclusive, out _enable_Timer, out status);
            if (open == true)
            {
                _enable_Timer.SetDriveMode(GpioPinDriveMode.Output);
                _enable_Timer.Write(GpioPinValue.Low);
            }
        }

        private void EnablePlayback()
        {
            _StopEnabled = false;
            _enable_Timer.Write(GpioPinValue.High);
            ad5360.startPlayBack();
        }

        private void stopPlayBack()
        {
            PlayBackThread.Abort();
            _StopEnabled = true;
            _enable_Timer.Write(GpioPinValue.Low);
            ad5360.stopPlayBack();
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

            EnablePlayback();

            int count = 1000;
            for (int index = 0; index < count; index++)
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
                            var value = (float)0.5;
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
            PlayFile();
        }

        private void PlayFile()
        {
            EnablePlayback();

            var tmp = System.IO.Directory.GetCurrentDirectory();
            tmp += "\\DataFile\\Base_EEG_Data_Play.txt";
            if (File.Exists(tmp))
            {
                using (StreamReader sr = File.OpenText(tmp))
                {
                    _TimeSlice timeslice = new _TimeSlice();

                    while (!sr.EndOfStream && _StopEnabled == false)
                    {
                        var line = sr.ReadLine();
                        string[] values = line.Split(',');
                        var floats = new List<double>();
                        for (int channel = 0; channel < 15; channel++)
                        {
                            try
                            {
                                var value = ((float)Convert.ToDouble(values[channel]) / 10);
                                _writeBuffer buff = new _writeBuffer(channel, value);
                                timeslice.timeSlice.Add(buff);
                            }
                            catch (Exception ex)
                            {
                                var error = ex.Message.ToString();
                            }
                        }
                        ad5360.add_WrtieFrame(timeslice);
                    }

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
                    ad5360.add_WrtieFrame(timeslice);
                }
                while (ad5360.playBackBusy() == true)
                {

                }
            }
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
        //private void Click_write(object sender, RoutedEventArgs e)
        //{
        //    var volts = Convert.ToSingle(txt_Value.Text.ToString());
        //    if (volts >= 5)
        //    {
        //        volts = (float)(5 - .0001);
        //    }
        //    if(volts <= -5)
        //    {
        //        volts = (float)(-5 + .0001);
        //    }
        //    _TimeSlice timeVolt = new _TimeSlice();
        //    for (int index = 0; index < 16; index++)
        //    {
        //        var buff = new _writeBuffer(index, volts);
        //        timeVolt.timeSlice.Add(buff);
        //    }
        //    ad5360.add_WrtieFrame(timeVolt);
        //}

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

        private void Click_Close(object sender, RoutedEventArgs e)
        {

            MainPage mainPage = new MainPage();
            this.Frame.Navigate(typeof(MainPage), mainPage);
        }

        private void Click_Stop(object sender, RoutedEventArgs e)
        {
            stopPlayBack();
            txt_PlayStatus.Text = "Stopped";
            txt_PlayStatus.FontSize = 20;
        }
    }
}
