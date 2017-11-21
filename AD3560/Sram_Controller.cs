using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Spi;

namespace SPIController
{
    static class SRAMConsts
    {
        //SRAM Instruction
        public const Byte RDMR =    0x5;
        public const Byte WRMR =    0x1;
        public const Byte WRITE =   0x2;
        public const Byte READ =    0x3;

        //SRAM Modes
        public const Byte BYTE_MODE =       0x00;
        public const Byte PAGE_MODE =       0x80;
        public const Byte SEQUENTIAL_MODE = 0x40;

        //SRAM Page size
        public const Byte PAGE_SIZE = 0x20;

        //SRAM Max Speed
        public const int Spi_Max_Speed = 0x1312d00;  // 20MHz

        //SRAM Spi Mode
        public const SpiMode Mode0 = SpiMode.Mode0;
    }
    public class Sram_Controller
    {
        private SRAM_23LC1024 _Sram0;
        private SRAM_23LC1024 _Sram1;


        public bool IsActive { get; set; }

        public Sram_Controller(SpiMode spi_Mode, int clkFreq)
        {
            _Sram0 = new SRAM_23LC1024(spi_Mode, clkFreq, 0, "SPI0");
            // _Sram1 = new SRAM_23LC1024(spi_Mode, clkFreq, 1,"SPI1");
        }

        public Byte SramOneReadRegistermode()
        {
            var result = 0 ;
            return _Sram0.readRegisterMode();
        }
        public void SramOneWriteByteMode( )
        {
            _Sram0.writeRegisterMode(SRAMConsts.BYTE_MODE);
        }

        public void SramOneWritePageMode()
        {
            _Sram0.writeRegisterMode(SRAMConsts.PAGE_MODE);
        }

        public void SramOneWriteSequentialMode()
        {
            _Sram0.writeRegisterMode(SRAMConsts.SEQUENTIAL_MODE);
        }

        public void SramOneWriteByteData(uint address, byte data)
        {
            _Sram0.writeByte(address, data);
        }

        public Byte SramOneReadByteData(uint address, ref byte[] data)
        {
            return _Sram0.readByte(address, ref data);
        }

        public void SramOneWritePageData(uint address, ref byte[] data)
        {
            _Sram0.writePage(address, ref data);
        }

        public void SramOneReadPageData(uint address, ref byte[] data)
        {
            _Sram0.readPage(address, ref data);
        }

        public void SramOneWriteData(uint address, ref byte[] data, int length)
        {
            _Sram0.writeBuffer(address, ref data, length);
        }

        public void SramOneReadData(uint address, ref byte[] data, int length)
        {
            _Sram0.readBuffer(address, ref data, length);
        }
    }
}
