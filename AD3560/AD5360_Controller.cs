using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Spi;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Xaml;

namespace SPIController
{
    static class Constants
    {
        public const byte ad5360_Mode = 0xc0;
        public const byte ad5360_A5 = 0x00;
        public const byte ad5360_Group1 = 0x08;
        public const byte ad5360_Group2 = 0x10;
        
    }

    public class AD5360_Controller
    {
        #region Private Members

        private ad5360 ad5360_Board = new ad5360();
        // Construct a ConcurrentStack
        private ConcurrentStack<_TimeSlice> ad5360_Stack = new ConcurrentStack<_TimeSlice>();
        IAsyncAction ad5360_Timer = null;
        IAsyncAction ad5360_TimeControl = null;
        //System.Threading.CancellationToken ad5360_TimerEnd = new System.Threading.CancellationToken();
        private Boolean _Active = false;
        //Thread ad5360_Timer;
        private byte[] ch1_min10V = new byte[3] { 0xC8, 0x00, 0x00 }; //-12.00v
        private byte[] ch1_min5V = new byte[3] { 0xC8, 0x3F, 0xFF }; //-6.000v
        private byte[] ch1_min7_5V = new byte[3] { 0xC8, 0x20, 0x00 }; //-9.0v
        private byte[] ch1_9_9V = new byte[3] { 0xC8, 0xFF, 0xFF }; //11.999v
        private byte[] ch1_TEST = new byte[3] { 0xC8, 0xAA, 0xAA }; //3.32v
        private byte[] ch1_READ = new byte[3] { 0x05, 0x0B, 0x80 };
        private byte[] writeBuffer = new byte[3] { 0x00, 0x00, 0x00 };
        private byte[] responseBuffer = new byte[3];
        private String errorMsgs;

        #endregion // Private Members

        #region Public Properties

        public Boolean active
        {
            get
            {
                return _Active;
            }
        }

        #endregion //Public Properties

        #region constructors
        public AD5360_Controller(SpiMode spi_Mode, int clkFreq, int ChipSelectLine, string spiDeviceSelection)
        {
            if (ad5360_Board.IsActive)
            {
                ad5360_Board.ad5360_Setup(spi_Mode, clkFreq, ChipSelectLine, spiDeviceSelection);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        #endregion //constructors

        #region Public Functions

        public void pulse_AD5360_Reset()
        {
            ad5360_Stack.Clear();
            ad5360_Board.pulse_AD5360_Reset();
        }

        public void pulse_AD5360_Clear()
        {
            ad5360_Board.pulse_AD5360_Clear();
        }

        public void pulse_AD5360_LDAC()
        {
            ad5360_Board.pulse_AD5360_LDAC();
        }

        public void add_WrtieFrame(_TimeSlice timeSlice)
        {
            ad5360_Stack.Push(timeSlice);
            if (ad5360_Timer == null)
            {
                ad5360_Timer = ThreadPool.RunAsync(
                    (workItem) =>
                     {
                         Stopwatch sw = new Stopwatch();
                         while (true)
                         {
                             sw.Start();
                             if (sw.ElapsedTicks > 50)
                             {
                                 ad5360_Write();
                                 sw.Stop();
                                 sw.Reset();
                             }
                         }

                     }
                     , WorkItemPriority.High);
                ad5360_TimeControl = ad5360_Timer;
            }
        }

        #endregion // public functions

        #region private functions
        //private void ad5360_Write(object sender, object e)
        private void ad5360_Write()
        {
            if (!ad5360_Stack.IsEmpty)
            {
                _TimeSlice buffer;
                ad5360_Stack.TryPop(out buffer);
                foreach (_writeBuffer buff in buffer.timeSlice)
                {
                    ad5360_Board.Write(buff.buffer);
                }
            }
        }

        private void InitTimer()
        {
            // old code removed Oct 10 2017
            //DispatcherTimer timer = new DispatcherTimer()
            //{
            //    Interval = TimeSpan.FromMilliseconds(3.9)
            //};

            //timer.Tick += ad5360_Write;
            //timer.Start();
            //New code added 10 Oct 2017
            Stopwatch sw = new Stopwatch();
            while(!ad5360_Stack.IsEmpty)
            {
                ad5360_Write();
                sw.Start();
                while(sw.ElapsedTicks < 500)
                {

                }
                sw.Stop();
                sw.Reset();
            }
        }

         #endregion // private functions
    }
}
